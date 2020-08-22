using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shared;

namespace TasksService
{
    public class UsersRepository : BaseDapperRepository
    {
        protected override string _tableName => "users";       

        public UsersRepository(PostgresConnectionManager connectionManager)
            : base(connectionManager.GetConnection())
        {
        }


        public  Task<IEnumerable<UserModel>> GetUsersAsync()
        {
            return GetModelsAsync<UserModel>();
        }

        public  Task<IEnumerable<UserModel>> GetUsersAsync(IEnumerable<string> usersIds)
        {
            return GetModelsWithCheckIsAllAsync<UserModel>(usersIds);       
        }

        public Task<UserModel> GetUserAsync(string userId)
        {
            return GetModelByIdAsync<UserModel>(userId);
        }

        public async Task CreateOrUpdateUserAsync(UserModel user)
        {
            var existingUser = await GetModelByIdAsync<UserModel>(user.Id);

            if(existingUser == null)
            {
                await CreateUserAsync(user);
            }
            else
            {
                await UpdateUserAsync(user);
            }
        }
        
        private async Task CreateUserAsync(UserModel newUser)
        {
            string insertQuery = $"insert into {_tableName} (id, username) "
                + $"values('{newUser.Id}', '{newUser.Username}');";

            int res = await _connection.ExecuteAsync(insertQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Create user failed");
            }
        }

        private async Task UpdateUserAsync(UserModel user)
        {
            string updateQuery = $"update {_tableName} set username = '{user.Username}' where id = '{user.Id}';";

            int res = await _connection.ExecuteAsync(updateQuery);

            if(res <= 0)
            {
                throw new DatabaseException("Update user failed");
            }
        }

        public Task DeleteUserAsync(string userId)
        {
            return DeleteModelAsync(userId);
        }        
    }
}