using OTUS_SoftwareArchitect_Client.DTO;
using OTUS_SoftwareArchitect_Client.Infrastructure;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class AuthService
    {
        private readonly WebApiClient _webClient;

        public AuthService()
        {
            _webClient = DependencyService.Resolve<WebApiClient>();
        }

        public async Task<RequestResult<string>> LoginAsync(string login, string password)
        {
            var loginDto = new LoginDto { Login = login, Password = password };
            var result = await _webClient.ExecuteRequestAsync(webApi => webApi.Login(loginDto));

            if(result.IsSuccess)
            {
                AuthDataProvider.SetUserId(result.Result);
            }

            return result;
        }

        public async Task Logout()
        {
            AuthDataProvider.ResetAuthData();
            var result = await _webClient.ExecuteRequestAsync(webApi => webApi.Logout());
        }

        public Task<RequestResult<string>> RegisterAsync(string username, string login, string password)
        {
            var registerDto = new RegisterDto
            {
                Username = username,
                Login = login,
                Password = password
            };

            return _webClient.ExecuteRequestAsync(webApi => webApi.Register(registerDto));
        }
    }
}
