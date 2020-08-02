using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UsersService
{
    public class UserService
    {
        private readonly Repository _repository;

        private HttpClient _httpClient;

        public UserService(Repository repository)
        {
            _repository = repository;
        }

        public async Task<UserModel> CreateUserAsync(string username)
        {
            bool isUsernameBusy = await _repository.IsAnyUserByPredicateAsync(usr => usr.Username == username);

            if(isUsernameBusy)
            {
                throw new Exception("This username already in use");
            }

            return await _repository.CreateUserAsync(username);
        }

        public Task<List<UserModel>> GetUsersAsync()
        {
            return _repository.GetUsersAsync();
        }

        internal Task<UserModel> GetUserAsync(string userId)
        {
            return _repository.GetUserAsync(userId);
        }

        internal Task<UserModel> UpdateUserAsync(string userId, string username)
        {
            return _repository.UpdateUserAsync(userId, username);
        }

        internal async Task<(bool isSuccess, string error)> DeleteUserAsync(string userId)
        {
            var isAuthInfoDeleted = await TryDeleteUserAuthInfoAsync(userId);
            if(!isAuthInfoDeleted.isSuccess)
                return isAuthInfoDeleted;

            await _repository.DeleteUserAsync(userId);

            return (true, string.Empty);
        }

        private async Task<(bool isSuccess, string error)> TryDeleteUserAuthInfoAsync(string userId)
        {
            _httpClient ??= CreateHttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"https://localhost:5001/svc/userauth/{userId}")
            };

            var response = await _httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorModel = new {Title = "", Status = default(HttpStatusCode)};
            JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), errorModel);

            return (false, errorModel.Title);            
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