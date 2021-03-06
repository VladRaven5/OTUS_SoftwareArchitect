using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Shared
{

    public abstract class BaseDapperRepository : IDisposable, IOutboxRepository
    {
        protected abstract string _tableName { get; }
        protected readonly string _outboxTableName = "outbox_messages";
        protected readonly IDbConnection _connection;

        protected BaseDapperRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        protected Task<IEnumerable<TModel>> GetModelsAsync<TModel>()
        {
            string query = $"select * from {_tableName};";
            return _connection.QueryAsync<TModel>(query);
        }

        protected async Task<IEnumerable<TModel>> GetModelsWithCheckIsAllAsync<TModel>(IEnumerable<string> ids) where TModel : BaseModel
        {
            if(ids == null || !ids.Any())
            {
                return new List<TModel>();
            }

            string query = $"select * from {_tableName} where id in ({string.Join(", ", ids.Select(id => $"'{id}'"))});";
            var results = (await _connection.QueryAsync<TModel>(query)).ToList();

            var notFoundUsersIds = ids.Except(results.Select(r => r.Id)).ToList();

            if(notFoundUsersIds.Any())
            {
                throw new NotFoundException($"Some {typeof(TModel).Name} not found. Ids: {string.Join(",", notFoundUsersIds)}");
            }

            int requestedCount = ids.Count();
            int resultCount = results.Count;

            if(requestedCount != resultCount)
            {
                throw new NotFoundException($"Some {typeof(TModel).Name} not found. Expected count {requestedCount}, but found {resultCount}");
            }
            
            return results;
        }

        protected Task<TModel> GetModelByIdAsync<TModel>(string modelId)
        {
            string query = $"select * from {_tableName} where id = '{modelId}' limit 1;";
            return _connection.QueryFirstOrDefaultAsync<TModel>(query);
        }

        protected Task DeleteModelAsync(string modelId, OutboxMessageModel message = null)
        {
            string deleteQuery = $"delete from {_tableName} where id = '{modelId}';";

            if (message != null)
            {
                string insertMessageQuery = ConstructInsertMessageQuery(message);
                deleteQuery += insertMessageQuery;
            }

            return _connection.ExecuteAsync(deleteQuery);
        }

        protected string ConstructInsertMessageQuery(OutboxMessageModel message)
        {
            return $" insert into {_outboxTableName} (topic, message, action) " +
                $"values('{message.Topic}', '{message.Message}', '{message.Action}'); ";
        }        

        protected string GetQueryNullableEscapedValue<T>(Nullable<T> nullableValue) where T : struct
        {
            if (nullableValue.HasValue)
            {
                return $"'{nullableValue.Value}'";
            }
            return "NULL";
        }

        protected string GetQueryNullableEscapedValue(object nullableValue)
        {
            if (nullableValue != null)
            {
                return $"'{nullableValue}'";
            }
            return "NULL";
        }


        #region Outbox

        public async Task<OutboxMessageModel> PopOutboxMessageAsync()
        {
            string query = $" select * from {_outboxTableName} where not isinprocess order by id limit 1; ";
            var message = await _connection.QueryFirstOrDefaultAsync<OutboxMessageModel>(query);
            if (message != null)
            {
                await SetOuboxMessageStateAsync(message.Id, true);
            }

            return message;
        }

        public Task ReturnOutboxMessageToPendingAsync(int messageId)
        {
            return SetOuboxMessageStateAsync(messageId, false);
        }        

        public Task DeleteOutboxMessageAsync(int messageId)
        {
            string deleteQuery = $" delete from {_outboxTableName} where id = '{messageId}'; ";
            return _connection.ExecuteAsync(deleteQuery);
        }

        private Task SetOuboxMessageStateAsync(int messageId, bool isInProcess)
        {
            string updateQuery = $" update {_outboxTableName} set isinprocess = {isInProcess} where id = {messageId}; ";
            return _connection.ExecuteAsync(updateQuery);
        }

        #endregion Outbox

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}