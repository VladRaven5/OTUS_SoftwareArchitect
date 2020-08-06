using System.Data;
using Microsoft.Extensions.Configuration;

namespace Shared
{
    public class PostgresConnectionManager
    {
        private readonly string _connectionString;

        public PostgresConnectionManager(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection GetConnection()
        {
            var connection = new Npgsql.NpgsqlConnection(_connectionString);
            connection.Open();

            return connection;
        }
    }
}