using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Shared
{
    public abstract class BaseDapperRepository : IDisposable
    {
        protected abstract string _tableName { get; }
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

        protected Task DeleteModelAsync(string modelId)
        {
            string deleteQuery = $"delete from {_tableName} where id = '{modelId}';";
            return _connection.ExecuteAsync(deleteQuery);    
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}