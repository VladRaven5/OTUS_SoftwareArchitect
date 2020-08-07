using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace WorkingHoursService
{
    public class WorkingHoursRepository : BaseDapperRepository
    {
        protected override string _tableName => "working_hours_records";

        public WorkingHoursRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }

        // public Task<IEnumerable<UserWorkingHoursAggregate>> GetProjectWorkingHours(string projectId)
        // {
        //     return _connection.QueryAsync<UserWorkingHoursAggregate>(
        //         GetMergedSelectQuery($"where projectId = {projectId}"));
        // }

        // public Task<IEnumerable<UserWorkingHoursAggregate>> GetTaskMemberWorkingHours(string taskId, string userId)
        // {
        //     return _connection.QueryAsync<UserWorkingHoursAggregate>(
        //         GetMergedSelectQuery($"where taskid = {taskId} and userid = {userId}"));
        // }

        // public Task<IEnumerable<UserWorkingHoursAggregate>> GetMemberWorkingHours(string userId)
        // {
        //     return _connection.QueryAsync<UserWorkingHoursAggregate>(
        //         GetMergedSelectQuery($"where userid = {userId}"));
        // }

        public Task<IEnumerable<MemberWorkingHoursAggregate>> GetProjectTaskMemberWorkingHoursAsync(string projectId = null,
            string taskId = null, string userId = null)
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
                AddStatement(whereStatement, $"projectid = '{projectId}'");
            }

            if(!string.IsNullOrWhiteSpace(taskId))
            {
                AddStatement(whereStatement, $"taskid = '{taskId}'");
            }

            if(!string.IsNullOrWhiteSpace(userId))
            {
                AddStatement(whereStatement, $"userid = '{userId}'");
            }

            return _connection.QueryAsync<MemberWorkingHoursAggregate>(
                GetMergedSelectQuery(whereStatement));
        }


        public Task<MemberWorkingHoursAggregate> GetWorkingHoursRecordByIdAsync(string recordId)
        {
            return _connection.QueryFirstOrDefaultAsync<MemberWorkingHoursAggregate>(
                GetMergedSelectQuery($"where wh.id = '{recordId}' limit 1"));
        }

        private string GetMergedSelectQuery(string whereStatement)
        {
            return "select wh.id, p.id as projectid, p.title as projecttitle, userid, u.username, taskid, t.title as tasktitle, description, hours, createddate, version " +
                $"from {_tableName} wh " +
                $"join users u on u.id = wh.userid " +
                $"join tasks t on t.id = wh.taskid " +  
                $"join projects p on p.id = t.projectid " +
                $"{whereStatement} ;";
        }


        public async Task<TaskUserWorkingHoursRecord> AddWorkingHoursRecordAsync(TaskUserWorkingHoursRecord newRecord)
        {
            newRecord.Init();

            string insertQuery = $"insert into {_tableName} (id, taskid, userid, description, hours, createddate, version) "
                + $"values('{newRecord.Id}', '{newRecord.TaskId}', '{newRecord.UserId}', '{newRecord.Description}', '{newRecord.Hours}', '{newRecord.CreatedDate}', {newRecord.Version});";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Working hours record create failed");
            }            

            return await GetWorkingHoursRecordByIdAsync(newRecord.Id);
        }

        public async Task<TaskUserWorkingHoursRecord> UpdateWorkingHoursRecordAsync(TaskUserWorkingHoursRecord updatedRecord)
        {
            int newVersion = updatedRecord.Version + 1;

            string updateQuery = $"update {_tableName} set " + 
                $"taskid = '{updatedRecord.TaskId}', " +
                $"userid = '{updatedRecord.UserId}', " +
                $"description = '{updatedRecord.Description}', " +
                $"hours = {updatedRecord.Hours}, " +
                $"version = {newVersion} " +
                $"where id = '{updatedRecord.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update record failed");
            }

            return await GetWorkingHoursRecordByIdAsync(updatedRecord.Id);
        }

        public Task DeleteWorkingHoursRecordAsync(string recordId)
        {
            return DeleteModelAsync(recordId);           
        }    
    }
}