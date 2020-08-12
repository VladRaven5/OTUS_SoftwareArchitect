using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace UsersService
{
    public class UsersManager
    {
        private readonly UsersRepository _repository;
        private readonly IConfiguration _configuration;
        private HttpClient _httpClient;

        public UsersManager(UsersRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<UserModel> CreateUserAsync(string username)
        {
            bool isUsernameBusy = await _repository.IsAnyUserByPredicateAsync(usr => usr.Username == username);

            if(isUsernameBusy)
            {
                throw new EntityExistsException("This username already in use");
            }

            var user = new UserModel
            {
                Username = username
            }; 
            user.Init();

            var message = OutboxMessageModel.Create(
                new UserCreatedUpdatedMessage
                {
                    UserId = user.Id,
                    Username = user.Username
                }, Topics.Users, MessageActions.Created
            );

            var createdUser = await _repository.CreateUserAsync(user, message);

            return createdUser;
        }

        public Task<List<UserModel>> GetUsersAsync()
        {
            return _repository.GetUsersAsync();
        }

        internal Task<UserModel> GetUserAsync(string userId)
        {
            return _repository.GetUserAsync(userId);
        }

        internal async Task<UserModel> UpdateUserAsync(string userId, string username)
        {   
            var message = OutboxMessageModel.Create(
                new UserCreatedUpdatedMessage
                {
                    UserId = userId,
                    Username = username
                }, Topics.Users, MessageActions.Updated
            );

            var updatedUser = await _repository.UpdateUserAsync(userId, username, message);

            return updatedUser;
        }

        internal async Task<(bool isSuccess, string error)> DeleteUserAsync(string userId)
        {
            var isAuthInfoDeleted = await TryDeleteUserAuthInfoAsync(userId);
            if(!isAuthInfoDeleted.isSuccess)
                return isAuthInfoDeleted;            

            var message = OutboxMessageModel.Create(
                new UserDeletedMessage
                {
                    UserId = userId
                }, Topics.Users, MessageActions.Deleted
            );

            await _repository.DeleteUserAsync(userId, message);

            return (true, string.Empty);
        }

        private async Task<(bool isSuccess, string error)> TryDeleteUserAuthInfoAsync(string userId)
        {
            _httpClient ??= CreateHttpClient();

            string authServiceUrl = _configuration["ServicesUris:AuthService"];

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{authServiceUrl}/svc/userauth/{userId}")
            };

            var response = await _httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorModel = new {Title = "", Status = default(HttpStatusCode)};
            JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), errorModel);

            return (false, $"Error during connection with Auth service:{errorModel.Title}");            
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