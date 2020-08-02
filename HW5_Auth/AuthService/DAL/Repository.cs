using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AuthService
{
    public class Repository : IDisposable
    {
        private readonly IAsyncDocumentSession _connection;

        public Repository(DBConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.GetConnection();
        }

        public Task<UserAuthInfo> GetUserAuthInfoAsync(string userId)
        {
            return _connection.LoadAsync<UserAuthInfo>(userId);
        }

        public async Task<UserAuthInfo> CreateUserInfo(string userId, string login, string password)
        {
            var userInfo = new UserAuthInfo
            {
                Id = userId,
                Login = login,
                PasswordHash = password,
            };

            await _connection.StoreAsync(userInfo);
            await _connection.SaveChangesAsync();

            return userInfo;
        }

        public async Task<IEnumerable<UserAuthInfo>> FilterUsersByPredicateAsync(Expression<Func<UserAuthInfo, bool>> predicate)
        {
            return await Queryable.Where(
                    _connection.Query<UserAuthInfo>(),
                    predicate)
                .ToListAsync();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}