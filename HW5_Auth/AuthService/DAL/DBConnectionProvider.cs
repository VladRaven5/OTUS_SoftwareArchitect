using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace AuthService
{
    public class DBConnectionProvider : IDisposable
    {
        private const string _databaseName = "users_auth_info";

        private readonly IDocumentStore _store;
        public DBConnectionProvider(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            _store = CreateStore(connectionString);
        }

        private IDocumentStore CreateStore(string connectionString)
        {
            var store = new DocumentStore
            {
                Urls = new[] { connectionString },
                Database = _databaseName
            };
            store.Initialize();

            var availableDatabases = store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 100));
            if(!availableDatabases.Contains(_databaseName))
            {
                Console.WriteLine("Database created");
                CreateDatabase(store, _databaseName);
            }

            return store;
        }


        private void CreateDatabase(IDocumentStore store, string databaseName)
        {
            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(databaseName)));
        }

        public IAsyncDocumentSession GetConnection()
        {
            return _store.OpenAsyncSession();
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}