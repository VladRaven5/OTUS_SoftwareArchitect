using System.Threading.Tasks;
using Dapper;
using Shared;

namespace TasksService
{
    public class ListsRepository : BaseDapperRepository
    {
        protected override string _tableName => "lists"; 

        public ListsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<ListModel> GetListAsync(string listId)
        {
            return GetModelByIdAsync<ListModel>(listId);
        }

        public async Task CreateOrUpdateListAsync(ListModel list)
        {
            var existingList = await GetModelByIdAsync<ListModel>(list.Id);

            if(existingList == null)
            {
                await CreateListAsync(list);
            }
            else
            {
                await UpdateListAsync(list);
            }
        }
        
        public async Task CreateListAsync(ListModel newList)
        {
            string insertQuery = $"insert into {_tableName} (id, title, projectid) "
                + $"values('{newList.Id}', '{newList.Title}', '{newList.ProjectId}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create list failed");
            }
        }

        private async Task UpdateListAsync(ListModel list)
        {
            string updateQuery = $"update {_tableName} set title = '{list.Title}' where id = '{list.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update list failed");
            }
        }

        public async Task<ListModelAggregate> GetListWithProjectAsync(string listId)
        {
            var query = "select l.id, l.title, l.projectid, p.title as projecttitle " +
                $"from {_tableName} l " +
                "left join projects p on l.projectid = p.id " + 
                $"where l.id = '{listId}' limit 1 ;";

            var list = await _connection.QueryFirstOrDefaultAsync<ListModelAggregate>(query);
            if(list == null)
            {
                throw new NotFoundException($"List with id {listId} not found");
            }

            return list;
        }

        public Task DeleteListAsync(string listId)
        {
            return DeleteModelAsync(listId);
        }
    }
}