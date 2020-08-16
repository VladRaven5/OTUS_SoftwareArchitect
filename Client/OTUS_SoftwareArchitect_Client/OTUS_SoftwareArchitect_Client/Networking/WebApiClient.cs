using Refit;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public class WebApiClient
    {
        private readonly IWebApi _webApi;

        public WebApiClient()
        {
            _webApi = DependencyService.Resolve<WebApiProvider>().Api;
                
        }


        public async Task<RequestResult<TResult>> ExecuteRequestAsync<TResult>(Func<IWebApi, Task<TResult>> request)
        {
            try
            {
                var result = await request(_webApi);
                return RequestResult<TResult>.Success(result);
            }
            catch(ApiException apie)
            {
                string message = apie.Content ?? apie.Message;
                return RequestResult<TResult>.Failure(apie.StatusCode, message);
            }
        }

        public async Task<RequestResult> ExecuteRequestAsync(Func<IWebApi, Task> request)
        {
            try
            {
                await request(_webApi);
                return RequestResult.Success();
            }
            catch (ApiException apie)
            {
                string message = apie.Content ?? apie.Message;
                return RequestResult.Failure(apie.StatusCode, message);
            }
        }
    }
}
