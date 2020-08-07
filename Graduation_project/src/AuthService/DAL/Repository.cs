using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Shared;

namespace AuthService
{
    public class Repository : IDisposable
    {
        private readonly IAsyncDocumentSession _connection;

        public Repository(RavenDBConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.GetConnection();
        }

        public async Task<UserAuthInfo> GetUserAuthInfoAsync(string userId)
        {
            var userInfo = await GetUserAuthInfoAsyncInternal(userId);
            if(userInfo == null)
            {
                throw new NotFoundException($"UserInfo for user id {userId} not found");
            }

            return userInfo;
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

        public async Task DeleteUserInfoAsync(string userId)
        {
            var user = await GetUserAuthInfoAsyncInternal(userId);
            if(user == null)
                return;
                
            _connection.Delete(user);
            await _connection.SaveChangesAsync();
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

        private Task<UserAuthInfo> GetUserAuthInfoAsyncInternal(string userId)
        {
            return _connection.LoadAsync<UserAuthInfo>(userId);
        }
    }
}