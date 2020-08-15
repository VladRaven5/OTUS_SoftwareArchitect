using System;
using System.Net.Http;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public class WebApiProvider
    {
        //auth 5007
        //tasks 5014

        private const string _baseUrl = "http://192.168.232.129:5014";

        private static IWebApi _webApi = Refit.RestService.For<IWebApi>(
            new HttpClient(new HttpAuthHandler())
            {
                BaseAddress = new Uri(_baseUrl)
            });

        public IWebApi Api => _webApi;
    }
}
