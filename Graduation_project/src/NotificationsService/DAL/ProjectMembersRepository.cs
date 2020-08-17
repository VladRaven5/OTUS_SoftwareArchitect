using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace NotificationsService
{
    public class ProjectMembersRepository : BaseDapperRepository
    {
        protected override string _tableName => "project_members";        

        public ProjectMembersRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<IEnumerable<ProjectMemberModel>> GetProjectMembersAsync(string projectId)
        {
            string query = $"select * from {_tableName} where projectId = '{projectId}';";
            return _connection.QueryAsync<ProjectMemberModel>(query);
        }

        public Task<IEnumerable<string>> GetProjectMembersIdsAsync(string projectId)
        {
            string query = $"select userId from {_tableName} where projectId = '{projectId}';";
            return _connection.QueryAsync<string>(query);
        }

        public Task<ProjectMemberModel> GetProjectMemberAsync(string userId, string projectId)
        {
            string query = $"select * from {_tableName} where userId = '{userId}' and projectId = '{projectId}' limit 1;";
            return _connection.QueryFirstOrDefaultAsync<ProjectMemberModel>(query);
        }

        public async Task CreateProjectMemberAsync(string userId, string projectId)
        {
            var existingMember = await GetProjectMemberAsync(userId, projectId);

            if(existingMember != null)
            {
                return;                
            }
            
            string insertQuery = $"insert into {_tableName} (userid, projectid) "
                + $"values('{userId}', '{projectId}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create member failed");
            }
        }

        public Task DeleteProjectMemberAsync(string userId, string projectId)
        {
            string query = $"delete from {_tableName} where userid = '{userId}' and projectid = '{projectId}';";
            return _connection.ExecuteAsync(query);
        }
    }
}