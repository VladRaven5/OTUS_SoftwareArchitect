using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace ProjectMembersService
{
    public class ProjectMembersRepository : BaseDapperRepository
    {
        protected override string _tableName => "project_members";

        public ProjectMembersRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        public Task<ProjectMemberAggregate> GetProjectMemberAsync(string projectId, string userId)
        {
            string query = GetMergedSelectQuery($"where userId = '{userId}' and projectid = '{projectId}' limit 1");
            return _connection.QueryFirstOrDefaultAsync<ProjectMemberAggregate>(query);
        }

        public Task<IEnumerable<ProjectMemberAggregate>> GetProjectsMembersAsync(string projectId = null, string userId = null)
        {
            string AddStatement(string target, string statement)
            {
                if(string.IsNullOrWhiteSpace(target))
                {
                    target = $"where {statement}";
                }
                else
                {
                    target += $" and {statement}";
                }

                return target;
            }

            string whereStatement = string.Empty;

            if(!string.IsNullOrWhiteSpace(projectId))
            {
                whereStatement = AddStatement(whereStatement, $"projectid = '{projectId}'");
            }

            if(!string.IsNullOrWhiteSpace(userId))
            {
                whereStatement = AddStatement(whereStatement, $"userId = '{userId}'");
            }

            return _connection.QueryAsync<ProjectMemberAggregate>(
                GetMergedSelectQuery(whereStatement));
        }

        private string GetMergedSelectQuery(string whereStatement)
        {
            return "select projectid, userid, role, p.title as projecttitle, u.username " +
                $"from {_tableName} pm " +
                $"join users u on u.id = pm.userid " +
                $"join projects p on p.id = pm.projectid {whereStatement} ;";
        }

        public async Task AddMemberToProjectAsync(ProjectMemberModel newProjectMember, OutboxMessageModel outboxMessage)
        {
            string insertQuery = $"insert into {_tableName} (userid, projectid, role) "
                + $"values('{newProjectMember.UserId}', '{newProjectMember.ProjectId}', '{(int)newProjectMember.Role}');";

            string insertOutboxMessageQuery = TakeInsertMessageQuery(outboxMessage);
            insertQuery += insertOutboxMessageQuery;

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Add member to project failed");
            }            
        }

        public async Task UpdateProjectMemberAsync(ProjectMemberModel updatingProjectMember, OutboxMessageModel outboxMessage)
        {
            string updateQuery = $"update {_tableName} set " + 
                $"role = '{(int)updatingProjectMember.Role}' " +
                $"where userid = '{updatingProjectMember.UserId}' and projectId = '{updatingProjectMember.ProjectId}';";

            string insertOutboxMessageQuery = TakeInsertMessageQuery(outboxMessage);
            updateQuery += insertOutboxMessageQuery;

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update project member failed");
            }
        }

        public Task DeleteMemberFromProjectAsync(string projectId, string userId, OutboxMessageModel outboxMessage)
        {        
            string deleteQuery = $"delete from {_tableName} where userid = '{userId}' and projectId = '{projectId}';";
            string insertOutboxMessageQuery = TakeInsertMessageQuery(outboxMessage);
            deleteQuery += insertOutboxMessageQuery;
            
            return _connection.ExecuteAsync(deleteQuery);    
        }
    }
}