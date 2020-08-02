using System;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService
{
    public class AuthenticationService
    {
        private readonly Repository _repository;
        private readonly PasswordHasher _passwordHasher;

        public AuthenticationService(Repository repository)
        {
            _repository = repository;
            _passwordHasher = new PasswordHasher("54321");
        }

        public async Task<UserAuthInfo> LoginAsync(string login, string password)
        {
            string passwordHash = _passwordHasher.HashPassword(password);

            return (await _repository
                .FilterUsersByPredicateAsync(user => user.Login == login && user.PasswordHash == passwordHash))
                .SingleOrDefault();
        }

        public async Task<UserAuthInfo> RegisterAsync(string username, string login, string password)
        {
            if(! (await CheckIsLoginAvailable(login)))
            {
                throw new Exception($"Login {login} already in use");
            }

            UserCreationResult userCreationResult = await CheckUsernameAndCreateAsync(username);

            if(!userCreationResult.IsSuccess)
            {
                throw new Exception(userCreationResult.Error);
            }

            string userId = userCreationResult.UserId;
            string hashedPassword = _passwordHasher.HashPassword(password);

            return await _repository.CreateUserInfo(userId, login, hashedPassword);      
        }

        private async Task<bool> CheckIsLoginAvailable(string login)
        {
            return !(await _repository
                .FilterUsersByPredicateAsync((user) => user.Login == login))
                .Any();
        }

        private Task<UserCreationResult> CheckUsernameAndCreateAsync(string username)
        {
            return Task<UserCreationResult>.FromResult(
                new UserCreationResult
                {
                    IsSuccess = true,
                    UserId = Guid.NewGuid().ToString()
                });
        }
    }
}