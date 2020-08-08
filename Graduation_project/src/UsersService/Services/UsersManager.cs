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
        private readonly Repository _repository;
        private readonly IConfiguration _configuration;
        private readonly RabbitMqTopicManager _rabbitMq;
        private HttpClient _httpClient;

        public UsersManager(Repository repository, IConfiguration configuration, RabbitMqTopicManager rabbitMq)
        {
            _repository = repository;
            _configuration = configuration;
            _rabbitMq = rabbitMq;
        }

        public async Task<UserModel> CreateUserAsync(string username)
        {
            bool isUsernameBusy = await _repository.IsAnyUserByPredicateAsync(usr => usr.Username == username);

            if(isUsernameBusy)
            {
                throw new EntityExistsException("This username already in use");
            }

            var createdUser = await _repository.CreateUserAsync(username);

            var message = new UserCreatedUpdatedMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = createdUser.Id,
                Username = createdUser.Username
            };

            SendMessageToBroker(message, MessageActions.Created);

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
            var updatedUser = await _repository.UpdateUserAsync(userId, username);

            var message = new UserCreatedUpdatedMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Username = username
            };

            SendMessageToBroker(message, MessageActions.Updated);

            return updatedUser;
        }

        internal async Task<(bool isSuccess, string error)> DeleteUserAsync(string userId)
        {
            var isAuthInfoDeleted = await TryDeleteUserAuthInfoAsync(userId);
            if(!isAuthInfoDeleted.isSuccess)
                return isAuthInfoDeleted;

            await _repository.DeleteUserAsync(userId);


            var message = new UserDeletedMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId
            };
            SendMessageToBroker(message, MessageActions.Deleted);

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

        private void SendMessageToBroker(BaseMessage message, string action)
        {
            try
            {
                _rabbitMq.SendMessage(Topics.Users, message, action);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Message sending error:\n{e.Message}\n{e.StackTrace}");
            }   
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