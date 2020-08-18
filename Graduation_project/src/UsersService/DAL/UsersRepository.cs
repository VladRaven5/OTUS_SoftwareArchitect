using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Shared;

namespace UsersService
{

    public class UsersRepository : IDisposable, IOutboxRepository, IUsersRepository
    {
        private readonly IAsyncDocumentSession _connection;

        public UsersRepository(IAsyncDocumentSession connection)
        {
            _connection = connection;
        }

        public Task<List<UserModel>> GetUsersAsync()
        {
            return _connection.Query<UserModel>().ToListAsync();
        }

        public async Task<UserModel> GetUserAsync(string userId)
        {
            var user = await GetUserAsyncInternal(userId);

            if (user == null)
                throw new NotFoundException($"User with id {userId} not found");

            return user;
        }

        public Task<UserModel> CreateUserSilentAsync(UserModel user)
        {
            return CreateUserAsync(user, null);
        }

        public async Task<UserModel> CreateUserAsync(UserModel user, OutboxMessageModel message)
        {
            await _connection.StoreAsync(user);
            if(message != null)
            {
                await _connection.StoreAsync(message.ToRavendDb());
            }
            
            await _connection.SaveChangesAsync();

            return user;
        }

        public async Task<UserModel> UpdateUserAsync(UserModel updatingUser, OutboxMessageModel message)
        {
            var currentUser = await GetUserAsync(updatingUser.Id);

            currentUser.Username = updatingUser.Username;
            currentUser.Region = updatingUser.Region;
            currentUser.PhoneNumber = updatingUser.PhoneNumber;
            currentUser.Email = updatingUser.Email;

            await _connection.StoreAsync(message.ToRavendDb());
            await _connection.SaveChangesAsync();

            return currentUser;
        }

        public Task DeleteUserSilentAsync(string userId)
        {
            return DeleteUserAsync(userId, null);
        }

        public async Task DeleteUserAsync(string userId, OutboxMessageModel message)
        {
            var user = await GetUserAsyncInternal(userId);
            if (user == null)
                return;

            _connection.Delete(user);            
            if(message != null)
            {
                await _connection.StoreAsync(message.ToRavendDb());
            }
            
            await _connection.SaveChangesAsync();
        }

        public Task<bool> IsAnyUserByPredicateAsync(Expression<Func<UserModel, bool>> predicate)
        {
            return GetPredicatedQuery<UserModel>(predicate).AnyAsync();
        }

        private Task<UserModel> GetUserAsyncInternal(string userId)
        {
            return _connection.LoadAsync<UserModel>(userId);
        }

        private IQueryable<T> GetPredicatedQuery<T>(Expression<Func<T, bool>> predicate)
        {
            return _connection.Query<T>().Where(predicate);
        }

        #region Outbox

        public async Task<OutboxMessageModel> PopOutboxMessageAsync()
        {
            var rdbMessage = await _connection.Query<RavendbOutboxMessageModel>()
                .Where(m => !m.IsInProcess)
                .OrderBy(m => m.Index)
                .FirstOrDefaultAsync();

            if (rdbMessage == null)
                return null;

            rdbMessage.IsInProcess = true;
            await _connection.SaveChangesAsync();
            var outbox = rdbMessage.ToBasicOutbox();

            return outbox;
        }

        public async Task<OutboxMessageModel> GetOutboxMessageAsync(int messageId)
        {
            var rdbMessage = await _connection.Query<RavendbOutboxMessageModel>()
                .Where(m => m.OutboxMessageId == messageId)
                .FirstOrDefaultAsync();

            if (rdbMessage == null)
                return null;

            var outbox = rdbMessage.ToBasicOutbox();

            return outbox;
        }

        public async Task ReturnOutboxMessageToPendingAsync(int messageId)
        {
            var rdbMessage = await _connection.Query<RavendbOutboxMessageModel>()
                .Where(m => m.OutboxMessageId == messageId)
                .FirstOrDefaultAsync();

            if (rdbMessage == null)
                return;

            rdbMessage.IsInProcess = false;
            await _connection.SaveChangesAsync();
        }

        public async Task DeleteOutboxMessageAsync(int messageId)
        {
            var rdbMessage = await _connection.Query<RavendbOutboxMessageModel>()
                .Where(m => m.OutboxMessageId == messageId)
                .FirstOrDefaultAsync();

            if (rdbMessage == null)
                return;

            _connection.Delete(rdbMessage);
            await _connection.SaveChangesAsync();
        }

        #endregion Outbox

        #region Sharding

        public Task<UserShardRecord> GetUserShardAsync(string userId)
        {
            return _connection.Query<UserShardRecord>()
                .Where(m => m.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateUserShardRecordAsync(UserShardRecord record)
        {
            await _connection.StoreAsync(record);
            await _connection.SaveChangesAsync();
        }

        public Task UpdateUserShardRecordAsync(UserShardRecord record)
        {
            return _connection.SaveChangesAsync();
        }

        public async Task DeleteUserShardRecordAsync(string userId)
        {
            var record = await GetUserShardAsync(userId);
            if(record == null)
            {
                return;
            }

            _connection.Delete(record);
            await _connection.SaveChangesAsync();
        }     

        #endregion Sharding


        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}