using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace UsersService
{
    public class Repository : IDisposable
    {
        private readonly IAsyncDocumentSession _connection;

        public Repository(DBConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.GetConnection();
        }

        public Task<List<UserModel>> GetUsersAsync()
        {
            return _connection.Query<UserModel>().ToListAsync();
        }

        public async Task<UserModel> GetUserAsync(string userId)
        {
            var user = await _connection.LoadAsync<UserModel>(userId);
            if(user == null)
                throw new Exception($"User with id {userId} not found");

            return user;
        }

        public async Task<UserModel> CreateUserAsync(string username)
        {
            var user = new UserModel
            {
                Username = username,
                Id = Guid.NewGuid().ToString()
            }; 

            await _connection.StoreAsync(user);
            await _connection.SaveChangesAsync();

            return user;
        }

        public async Task<UserModel> UpdateUserAsync(string userId, string newUsername)
        {
            var currentUser = await GetUserAsync(userId);

            currentUser.Username = newUsername;

            await _connection.SaveChangesAsync();

            return currentUser;
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            _connection.Delete(user);
            await _connection.SaveChangesAsync();
        }


        public Task<List<UserModel>> FilterUsersByPredicateAsync(Expression<Func<UserModel, bool>> predicate)
        {
            return GetPredicatedQuery<UserModel>(predicate).ToListAsync();
        }

        public Task<bool> IsAnyUserByPredicateAsync(Expression<Func<UserModel, bool>> predicate)
        {
            return GetPredicatedQuery<UserModel>(predicate).AnyAsync();
        }

        private IQueryable<T> GetPredicatedQuery<T>(Expression<Func<T, bool>> predicate)
        {
            return _connection.Query<T>().Where(predicate);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}