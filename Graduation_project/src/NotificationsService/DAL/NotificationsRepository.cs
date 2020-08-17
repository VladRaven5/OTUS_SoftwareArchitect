using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace NotificationsService
{
    public class NotificationsRepository : BaseDapperRepository
    {
        protected override string _tableName => "notifications";  

        public NotificationsRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {            
        }   

        public Task<IEnumerable<NotificationModel>> GetNotificationsAsync()
        {
            return GetModelsAsync<NotificationModel>();
        }

        public Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string userId)
        {
            string query = $"select * from {_tableName} where userid = '{userId}' or userid is null;";
            return _connection.QueryAsync<NotificationModel>(query);             
        }

        public async Task AddNotificationsToUsersAsync(string text, IEnumerable<string> usersIds)
        {
            if(usersIds == null || !usersIds.Any())
            {
                return;
            }

            string insertQuery = $"insert into {_tableName} (id, userid, text, createddate) values";

            DateTimeOffset createddate = DateTimeOffset.UtcNow;

            bool isFirstRecord = true;

            foreach(var userId in usersIds)
            {
                string notificationId = Guid.NewGuid().ToString();

                if(isFirstRecord)
                {
                    isFirstRecord = false;
                }
                else
                {
                    insertQuery += ",";
                }

                insertQuery += $" ('{notificationId}', '{userId}', '{text}', '{createddate}')";
            }

            insertQuery += ";";

            var res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create notifications failed");
            }
        }

        public async Task AddNotificationsToAllUsersAsync(string text)
        {
            string insertQuery = $"insert into {_tableName} (id, userid, text, createddate) " + 
                $"values ('{Guid.NewGuid().ToString()}', NULL, '{text}', '{DateTimeOffset.UtcNow}')";

            var res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create notifications failed");
            }            
        }
    }
}