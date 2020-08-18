using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shared;

namespace UsersService
{
    public interface IUsersRepository
    {
        Task<UserModel> CreateUserAsync(UserModel user, OutboxMessageModel message);
        Task DeleteUserAsync(string userId, OutboxMessageModel message);
        Task<UserModel> GetUserAsync(string userId);
        Task<List<UserModel>> GetUsersAsync();
        Task<bool> IsAnyUserByPredicateAsync(Expression<Func<UserModel, bool>> predicate);
        Task<UserModel> UpdateUserAsync(UserModel updatingUser, OutboxMessageModel message);
    }
}