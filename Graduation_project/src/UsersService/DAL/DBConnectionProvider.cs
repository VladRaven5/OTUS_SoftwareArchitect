using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Shared;

namespace UsersService
{
    public class DBConnectionProvider : IDisposable
    {
        private const string _databaseName = "users";
        private readonly Dictionary<string, IDocumentStore> _stores;
        public DBConnectionProvider(IConfiguration configuration)
        {
            string USconnectionString = configuration.GetConnectionString(UsersRegions.USA);
            string EUconnectionString = configuration.GetConnectionString(UsersRegions.Europe);
            string RUconnectionString = configuration.GetConnectionString(UsersRegions.Russia);
            string CNconnectionString = configuration.GetConnectionString(UsersRegions.China);
            CurrentRegion = configuration.GetConnectionString("LocalRegion").ToUpperInvariant();

            if(!UsersRegions.HasRegion(CurrentRegion))
            {
                throw new NotFoundException($"Region {CurrentRegion} not found");
            }

            _stores = new Dictionary<string, IDocumentStore>
            {
                { UsersRegions.USA, CreateStore(USconnectionString) },
                { UsersRegions.Europe, CreateStore(EUconnectionString) },
                { UsersRegions.Russia, CreateStore(RUconnectionString) },
                { UsersRegions.China, CreateStore(CNconnectionString) },
            };
        }

        public string CurrentRegion { get; }

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

        public IAsyncDocumentSession GetConnection(string regionKey)
        {
            regionKey = regionKey.ToUpperInvariant();
            if(!UsersRegions.HasRegion(regionKey))
            {
                throw new NotFoundException($"Region {regionKey} not found on open connection");
            }

            return _stores[regionKey].OpenAsyncSession();
        }

        public void Dispose()
        {
            foreach(var store in _stores)
            {
                store.Value.Dispose();
            }
        }
    }
}