using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace WorkingHoursService
{
    public class RequestsRepository : IDisposable
    {
        private const string _requestsTableName = "handled_requests";
                
        private readonly IDbConnection _connection;

        public RequestsRepository(DbConnectionManager connectionManager)
        {
            _connection = connectionManager.GetConnection();
        }

        public async Task<bool> IsRequestIdHandledAsync(string requestId)
        {
            string query = $"select * from {_requestsTableName} where requestid = '{requestId}' limit 1;";
            
            return (await _connection.QueryFirstOrDefaultAsync<HandledRequest>(query)) != null;
        }   

        public async Task SaveRequestIdAsync(string requestId, DateTimeOffset invalidateAt)
        {
            string insertQuery = $"insert into {_requestsTableName} (requestid, invalidateat) "
                    + $"values('{requestId}', '{invalidateAt}');";

            var result = await _connection.ExecuteAsync(insertQuery);

            if(result <= 0)
            {
                throw new DatabaseException("Request id insertion failed");
            }          
        }   

        public async Task DeleteRequestIdAsync(string requestId)
        {
            string deleteQuery = $"delete from {_requestsTableName} where requestid = '{requestId}';";
            int result = await _connection.ExecuteAsync(deleteQuery);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}