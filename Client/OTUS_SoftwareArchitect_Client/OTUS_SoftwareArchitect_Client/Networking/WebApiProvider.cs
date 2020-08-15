using System;
using System.Net.Http;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public class WebApiProvider
    {
        private const string _baseUrl = "http://192.168.232.129:5007";

        private static IWebApi _webApi = Refit.RestService.For<IWebApi>(
            new HttpClient(new HttpAuthHandler())
            {
                BaseAddress = new Uri(_baseUrl)
            });

        public IWebApi Api => _webApi;
    }
}
