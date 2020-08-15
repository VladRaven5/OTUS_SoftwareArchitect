using OTUS_SoftwareArchitect_Client.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public class HttpAuthHandler : HttpClientHandler
    {
        private const string _setCookieHeaderName = "Set-Cookie";
        private const string _authHeaderName = "UserAuthCookie";        

        public HttpAuthHandler()
        {
            var authInfo = AuthDataProvider.GetUserAuthData();
            if (authInfo == null)
                return;

            CookieContainer.Add(new System.Net.Cookie(_authHeaderName,
                authInfo.Cookie,
                authInfo.Path,
                authInfo.Domain));

            Debug.WriteLine($"Set auth cookie: {authInfo.Cookie}");
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddHost(request);


            var res = await base.SendAsync(request, cancellationToken);
            GetHeader(res);
            return res;
        }

        private void AddHost(HttpRequestMessage request)
        {
            string currentHost = request.Headers.Host;

            if(currentHost != "arch.homework")
            {
                request.Headers.Host = "arch.homework";
            }
        }

        private void GetHeader(HttpResponseMessage res)
        {
            if (res.Headers.TryGetValues(_setCookieHeaderName, out IEnumerable<string> values))
            {
                string value = values.First();

                string[] splittedValue = value.Split(';')[0].Split('=');

                string key = splittedValue[0];
                string val = splittedValue[1];
                Debug.WriteLine($"Hedaer: {key} = {val}");
                if (key == _authHeaderName)
                {
                    string dom = res.RequestMessage.RequestUri.Host;
                    Debug.WriteLine($"Auth hedaer for {dom} found: {val}");

                    string path = "/";

                    AuthDataProvider.SetUserAuthData(new AuthInfo(val, dom, path));                    

                    CookieContainer.Add(new System.Net.Cookie(_authHeaderName, val, path, dom));
                }                
            }
        }
    }
}
