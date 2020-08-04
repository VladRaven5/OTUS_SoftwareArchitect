using System.Data;
using Microsoft.Extensions.Configuration;

namespace WorkingHoursService
{
    public class DbConnectionManager
    {
        private readonly string _connectionString;

        public DbConnectionManager(IConfiguration configuration)
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