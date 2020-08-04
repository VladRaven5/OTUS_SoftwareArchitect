using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace WorkingHoursService
{
    public class WorkingHoursRepository : IDisposable
    {
        private const string _workingHoursRecordsTableName = "working_hours_records";
        private readonly IDbConnection _connection;

        public WorkingHoursRepository(DbConnectionManager connectionManager)
        {
            _connection = connectionManager.GetConnection();
        }

        public Task<IEnumerable<TaskUserWorkingHoursRecord>> GetWorkingHoursRecordsAsync()
        {
            string query = $"select * from {_workingHoursRecordsTableName};";
            return _connection.QueryAsync<TaskUserWorkingHoursRecord>(query);
        }

        public Task<TaskUserWorkingHoursRecord> GetWorkingHoursRecordByIdAsync(string recordId)
        {
            string query = $"select * from {_workingHoursRecordsTableName} where id = '{recordId}' limit 1;";
            return _connection.QueryFirstOrDefaultAsync<TaskUserWorkingHoursRecord>(query);
        }

        public async Task<TaskUserWorkingHoursRecord> AddWorkingHoursRecordAsync(TaskUserWorkingHoursRecord newRecord)
        {
            string id = Guid.NewGuid().ToString();
            DateTimeOffset createdDate = DateTimeOffset.UtcNow;
            int version = 1;

            string insertQuery = $"insert into {_workingHoursRecordsTableName} (id, taskid, userid, description, hours, createddate, version) "
                + $"values('{id}', '{newRecord.TaskId}', '{newRecord.UserId}', '{newRecord.Description}', {newRecord.Hours}, '{createdDate}', {version});";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Working hours record create failed");
            }            

            return await GetWorkingHoursRecordByIdAsync(id);
        }

        public async Task<TaskUserWorkingHoursRecord> UpdateWorkingHoursRecordAsync(TaskUserWorkingHoursRecord updatedRecord)
        {
            int newVersion = updatedRecord.Version + 1;

            string updateQuery = $"update {_workingHoursRecordsTableName} set " + 
                $"taskid = '{updatedRecord.TaskId}', " +
                $"userid = '{updatedRecord.UserId}', " +
                $"description = '{updatedRecord.Description}', " +
                $"hours = {updatedRecord.Hours}, " +
                $"version = {newVersion} " +
                $"where id = '{updatedRecord.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update failed");
            }

            return await GetWorkingHoursRecordByIdAsync(updatedRecord.Id);
        }

        public async Task DeleteWorkingHoursRecordAsync(string recordId)
        {
            string deleteQuery = $"delete from {_workingHoursRecordsTableName} where id = '{recordId}';";
            await _connection.ExecuteAsync(deleteQuery);              
        }
        

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}