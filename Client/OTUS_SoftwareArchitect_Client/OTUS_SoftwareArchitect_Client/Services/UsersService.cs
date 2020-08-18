using OTUS_SoftwareArchitect_Client.DTO.ListDtos;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class UsersService
    {
        private readonly WebApiClient _webClient;

        public UsersService()
        {
            _webClient = DependencyService.Resolve<WebApiClient>();
        }

        public List<UserRegions> AvailabelRegions { get; } = new List<UserRegions>
        {
            new UserRegions("CN", "China"),
            new UserRegions("RU", "Russia"),
            new UserRegions("EU", "Europe"),
            new UserRegions("US", "USA")
        };

        public Task<RequestResult<IEnumerable<UserModel>>> GetUsersAsync()
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetUsers());
        }

        public Task<RequestResult<UserModel>> GetUserAsync(string userId)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetUser(userId));
        }

        public Task<RequestResult<string>> LogoutAsync()
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.Logout());
        }

        public Task<RequestResult<UserModel>> UpdateUserAsync(UpdateUserDto updationDto)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.UpdateUser(updationDto));
        }
    }




    public class UserRegions
    {     
        public UserRegions(string key, string title)
        {
            Key = key;
            Title = title;
        }
        public string Title { get; }
        public string Key { get; }
    }
}
