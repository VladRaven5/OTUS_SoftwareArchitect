using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared;

namespace AuthService
{
    public class AuthenticationService
    {
        private readonly Repository _repository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher _passwordHasher;

        private HttpClient _httpClient;

        public AuthenticationService(Repository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher("54321");
        }

        public async Task<UserAuthInfo> LoginAsync(string login, string password)
        {
            string passwordHash = _passwordHasher.HashPassword(password);

            return (await _repository
                .FilterUsersByPredicateAsync(user => user.Login == login && user.PasswordHash == passwordHash))
                .SingleOrDefault();
        }

        internal Task DeleteUserAsync(string userId)
        {
            return _repository.DeleteUserInfoAsync(userId);
        }

        public async Task<UserAuthInfo> RegisterAsync(string username, string login, string password)
        {
            if(! (await CheckIsLoginAvailable(login)))
            {
                throw new ProhibitedException($"Login {login} already in use");
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

        private async Task<UserCreationResult> CheckUsernameAndCreateAsync(string username)
        {
            _httpClient ??= CreateHttpClient();

            string usersServiceUrl = _configuration["ServicesUris:UsersService"];

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{usersServiceUrl}/svc/users?username={username}")
            };

            var response = await _httpClient.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                var errorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(await response.Content.ReadAsStringAsync());
                return UserCreationResult.Failure(errorDto.Title);
            }
                

            var userModel = JsonConvert.DeserializeObject<UserModel>(await response.Content.ReadAsStringAsync());
            
            return UserCreationResult.Success(userModel.Id);
        }

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler()
            { 
                ServerCertificateCustomValidationCallback 
                    = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return new HttpClient(handler);
        }
    }
}