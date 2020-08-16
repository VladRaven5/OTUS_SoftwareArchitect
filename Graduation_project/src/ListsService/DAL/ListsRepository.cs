using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace ListsService
{
    public class ListsRepository : BaseDapperRepository
    {
        protected override string _tableName => "lists";

        public ListsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<IEnumerable<ListProjectAggregate>> GetListsAsync()
        {
            var mergingQuery = GetMergedSelectQuery("");
            return _connection.QueryAsync<ListProjectAggregate>(mergingQuery);
        }

        public Task<IEnumerable<ListProjectAggregate>> GetProjectListsAsyn(string projectId)
        {
            var mergingQuery = GetMergedSelectQuery($" where p.id = '{projectId}' ");
            return _connection.QueryAsync<ListProjectAggregate>(mergingQuery);
        }

        public Task<ListProjectAggregate> GetListByIdAsync(string listId)
        {
            var mergingQuery = GetMergedSelectQuery($"where l.id = '{listId}' limit 1");
            return _connection.QueryFirstOrDefaultAsync<ListProjectAggregate>(mergingQuery);
        }

        public async Task<ListProjectAggregate> CreateListAsync(ListModel newList, OutboxMessageModel message)
        {
            string insertQuery = $"insert into {_tableName} (id, title, projectId, createddate, version) "
                + $"values('{newList.Id}', '{newList.Title}', '{newList.ProjectId}', '{newList.CreatedDate}', {newList.Version});";

            string insertMessageQuery = ConstructInsertMessageQuery(message);

            insertQuery += insertMessageQuery;

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create list failed");
            }            

            return await GetListByIdAsync(newList.Id);
        }        

        public async Task<ListProjectAggregate> UpdateListAsync(ListModel updatedList, OutboxMessageModel message)
        {
            int newVersion = updatedList.Version + 1;

            string updateQuery = $"update {_tableName} set " + 
                $"title = '{updatedList.Title}', " +                
                $"version = {newVersion} " +
                $"where id = '{updatedList.Id}';";

            string insertMessageQuery = ConstructInsertMessageQuery(message);
            updateQuery += insertMessageQuery;

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update list failed");
            }

            return await GetListByIdAsync(updatedList.Id);
        }

        public Task DeleteListAsync(string listId, OutboxMessageModel message)
        {
            return DeleteModelAsync(listId, message);          
        }

        private string GetMergedSelectQuery(string whereStatement)
        {
            return "select l.id as id, l.title as title, projectid, p.title as projecttitle, createddate, version " +
                $"from {_tableName} l " +
                $"join projects p on p.id = l.projectid {whereStatement} ;";
        }
    }
}