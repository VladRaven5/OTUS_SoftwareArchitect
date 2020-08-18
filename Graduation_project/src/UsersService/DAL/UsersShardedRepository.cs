using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Shared;

namespace UsersService
{
    public class UsersShardedRepository: IDisposable, IOutboxRepository, IUsersRepository
    {
        private readonly Dictionary<string, UsersRepository> _repositories = new Dictionary<string, UsersRepository>();
        private readonly UsersRepository _localRepository;
        private readonly IMapper _mapper;

        public UsersShardedRepository(DBConnectionProvider connectionsProvider, IMapper mapper)
        {
            InitializeRepositories(connectionsProvider);
            _localRepository = _repositories[connectionsProvider.CurrentRegion];
            this._mapper = mapper;
        }   
        private void InitializeRepositories(DBConnectionProvider connectionsProvider)
        {
            foreach(var region in UsersRegions.AvailableRegions)
            {
                _repositories.Add(region, new UsersRepository(connectionsProvider.GetConnection(region)));
            }
        }

        public async Task<UserModel> CreateUserAsync(UserModel user, OutboxMessageModel message)
        {
            string userRegion = user.Region.ToUpperInvariant();
            EnsureUserRegionExists(userRegion);
            
            try
            {
                var createdUser = await _repositories[userRegion].CreateUserAsync(user, message);
                await SetUserShardAsync(createdUser.Id, userRegion);
                return createdUser;
            }
            catch(Exception)
            {
                await DeleteUserShardRecordAsync(user.Id);
                throw;
            }
        }

        public async Task<UserModel> GetUserAsync(string userId)
        {
            var userShardingRecord = await _localRepository.GetUserShardAsync(userId);
            if(userShardingRecord == null)
            {
                return null;
            }
            
            return await _repositories[userShardingRecord.ShardKey].GetUserAsync(userId);
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            var userQueryingTasks = new List<Task<List<UserModel>>>();
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                userQueryingTasks.Add(_repositories[shardKey].GetUsersAsync());
            }

            await Task.WhenAll(userQueryingTasks);

            List<UserModel> result = new List<UserModel>();
            foreach(var queryingTask in userQueryingTasks)
            {
                queryingTask.CheckAndThrowIfFaulted();
                result.AddRange(queryingTask.Result);
            }

            return result;            
        }        

        public async Task<bool> IsAnyUserByPredicateAsync(Expression<Func<UserModel, bool>> predicate)
        {
            var userQueryingTasks = new List<Task<bool>>();
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                userQueryingTasks.Add(_repositories[shardKey].IsAnyUserByPredicateAsync(predicate));
            }

            await Task.WhenAll(userQueryingTasks);

            bool result = false;
            foreach(var queryingTask in userQueryingTasks)
            {
                queryingTask.CheckAndThrowIfFaulted();
                result = result || queryingTask.Result;
            }

            return result;
        }

        public async Task<UserModel> UpdateUserAsync(UserModel updatingUser, OutboxMessageModel message)
        {
            string userId = updatingUser.Id;            
            var currentUser = await GetUserAsync(userId);
            if(currentUser == null)
            {
                throw new NotFoundException($"User with id {userId} not found");
            }            

            if(currentUser.Region == updatingUser.Region)
            {
                return await _repositories[updatingUser.Region]
                    .UpdateUserAsync(updatingUser, message);
            }

            string newShardKey = updatingUser.Region.ToUpper();
            string oldShardKey = currentUser.Region.ToUpper();

            EnsureUserRegionExists(newShardKey);
            EnsureUserRegionExists(oldShardKey);            

            var oldShardRepository = _repositories[oldShardKey];
            var newShardRepository = _repositories[newShardKey];

            var newModel = new UserModel();
            _mapper.Map<UserModel, UserModel>(updatingUser, newModel);
            newModel.CreatedDate = currentUser.CreatedDate;

            try
            {
                var replacedUserModel = await newShardRepository.CreateUserAsync(newModel, message);
                await SetUserShardAsync(userId, newShardKey);
                await oldShardRepository.DeleteUserSilentAsync(userId);

                return replacedUserModel;
            }
            catch(Exception)
            {
                //rollback changes
                await newShardRepository.DeleteOutboxMessageAsync(message.Id);
                await newShardRepository.DeleteUserSilentAsync(userId);                
                await SetUserShardAsync(userId, oldShardKey);
                var oldUser = await oldShardRepository.GetUserAsync(userId);
                if(oldUser == null)
                {
                    await oldShardRepository.CreateUserSilentAsync(currentUser);
                }                
                throw;
            }            
        }

        public async Task DeleteUserAsync(string userId, OutboxMessageModel message)
        {
            var userShardingRecord = await _localRepository.GetUserShardAsync(userId);
            if(userShardingRecord == null)
            {
                return;
            }
            
            await _repositories[userShardingRecord.ShardKey].DeleteUserAsync(userId, message);
            await DeleteUserShardRecordAsync(userId);          
        }

        private void EnsureUserRegionExists(string userRegion)
        {
            if(!UsersRegions.HasRegion(userRegion))
                throw new NotFoundException($"User region {userRegion} not found!");
        }
        

        #region Outboxing

        public async Task<OutboxMessageModel> PopOutboxMessageAsync()
        {
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                var outboxMessage = await _repositories[shardKey].PopOutboxMessageAsync();
                if(outboxMessage != null)
                {
                    return outboxMessage;
                }
            }

            return null;
        }

        public async Task ReturnOutboxMessageToPendingAsync(int messageId)
        {
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                var repository = _repositories[shardKey];
                var outboxMessage = await repository.GetOutboxMessageAsync(messageId);
                if(outboxMessage != null)
                {
                    await repository.ReturnOutboxMessageToPendingAsync(messageId);
                    return;
                }
            }

            throw new NotFoundException($"Outbox message with id {messageId} not found (on return to box)");
        }

        public async Task DeleteOutboxMessageAsync(int messageId)
        {
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                var repository = _repositories[shardKey];
                var outboxMessage = await repository.GetOutboxMessageAsync(messageId);
                if(outboxMessage != null)
                {
                    await repository.DeleteOutboxMessageAsync(messageId);
                    return;
                }
            }

            throw new NotFoundException($"Outbox message with id {messageId} not found (on delete)");
        }        

        #endregion Outboxing

        #region Sharding logic
        private async Task SetUserShardAsync(string userId, string userShardKey)
        {
            var userShardsTasks = new List<Task>();
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {
                userShardsTasks.Add(SetUserShardAsync(userId, userShardKey, shardKey));
            }

            await Task.WhenAll(userShardsTasks);

            foreach(var shardTask in userShardsTasks)
            {
                shardTask.CheckAndThrowIfFaulted();
            }
        }

        private async Task SetUserShardAsync(string userId, string userShardKey, string receiverShardKey)
        {
            var repository =_repositories[receiverShardKey];
            var shardRecord = await repository.GetUserShardAsync(userId);
            if(shardRecord == null)
            {
                await repository.CreateUserShardRecordAsync(
                    new UserShardRecord 
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        ShardKey = userShardKey
                    });
            }
            else
            {
                if(shardRecord.ShardKey == userShardKey)
                {
                    return;
                }

                shardRecord.ShardKey = userShardKey;
                await repository.UpdateUserShardRecordAsync(shardRecord);
            }
        }

        private async Task DeleteUserShardRecordAsync(string userId)
        {
            var userShardsTasks = new List<Task>();
            foreach(var shardKey in UsersRegions.AvailableRegions)
            {

                userShardsTasks.Add(_repositories[shardKey].DeleteUserShardRecordAsync(userId));
            }

            await Task.WhenAll(userShardsTasks);

            foreach(var shardTask in userShardsTasks)
            {
                shardTask.CheckAndThrowIfFaulted();
            }
        }

        #endregion Sharding logic


        public void Dispose()
        {
            foreach(var repository in _repositories.Values)
            {
                repository.Dispose();
            }
        }        
    }
}