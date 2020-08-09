using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Shared
{
    public abstract class BaseDapperRepository : IDisposable
    {
        protected abstract string _tableName { get; }
        protected readonly string _outboxTableName = "outbox_messages";
        protected readonly IDbConnection _connection;

        protected BaseDapperRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        protected Task<TModel> GetModelByIdAsync<TModel>(string modelId)
        {
            string query = $"select * from {_tableName} where id = '{modelId}' limit 1;";
            return _connection.QueryFirstOrDefaultAsync<TModel>(query);
        }

        protected Task DeleteModelAsync(string modelId, OutboxMessageModel message = null)
        {
            string deleteQuery = $"delete from {_tableName} where id = '{modelId}';";

            if(message != null)
            {
                string insertMessageQuery = TakeInsertMessageQuery(message);
                deleteQuery += insertMessageQuery;
            }

            return _connection.ExecuteAsync(deleteQuery);    
        }

        protected string TakeInsertMessageQuery(OutboxMessageModel message)
        {
            return $" insert into {_outboxTableName} (topic, message, action) " +
                $"values('{message.Topic}', '{message.Message}', '{message.Action}'); ";
        }

        public async Task<OutboxMessageModel> PopOutboxMessageAsync()
        {
            string query = $" select * from {_outboxTableName} order by id limit 1; ";
            var message = await _connection.QueryFirstOrDefaultAsync<OutboxMessageModel>(query);
            if(message != null)
            {
                string deleteQuery = $" delete from {_outboxTableName} where id = '{message.Id}'; ";
                await _connection.ExecuteAsync(deleteQuery); 
            }
            
            return message;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}