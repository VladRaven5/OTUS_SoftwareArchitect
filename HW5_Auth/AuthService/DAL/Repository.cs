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

            UpdateUserSessionInternal(userInfo);

            await _connection.StoreAsync(userInfo);
            await _connection.SaveChangesAsync();

            return userInfo;
        }

        public async Task<string> UpdateUserSessionAsync(string userId)
        {
            var userInfo = await GetUserAuthInfoAsync(userId);
            if(userInfo == null)
                throw new Exception($"User with id={userId} not found");

            var sessionId = UpdateUserSessionInternal(userInfo);

            await _connection.SaveChangesAsync();

            return sessionId;
        }

        public async Task<IEnumerable<UserAuthInfo>> FilterUsersByPredicateAsync(Expression<Func<UserAuthInfo, bool>> predicate)
        {
            return await Queryable.Where(
                    _connection.Query<UserAuthInfo>(),
                    predicate)
                .ToListAsync();
        }

        private string UpdateUserSessionInternal(UserAuthInfo userInfo)
        {
            string sessionId = Guid.NewGuid().ToString();
            userInfo.SetSession(sessionId);

            return sessionId;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}