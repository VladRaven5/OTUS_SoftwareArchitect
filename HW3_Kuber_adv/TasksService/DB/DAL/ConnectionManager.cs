using System.Diagnostics;
using System.Data;

namespace TasksService
{
    public class ConnectionManager
    {
        public static string ConnectionString;


        public IDbConnection GetConnection()
        {
            var connection = new Npgsql.NpgsqlConnection(ConnectionString);
            connection.Open();

            return connection;
        }
    }
}