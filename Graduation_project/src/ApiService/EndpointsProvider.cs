using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ApiService
{
    public class EndpointsProvider
    {
        public static EndpointsProvider Instance = new EndpointsProvider();

        public Endpoint[] Endpoints { get; private set; }

        public void SetEndpoints(IConfiguration configuration)
        {            

            var endpoints = new List<Endpoint>();
            var endpointsCollectionString = configuration.GetValue<string>("Endpoints");

            var endpointsStrings = endpointsCollectionString.Split(",").ToList();

            foreach(var endpointRecordString in endpointsStrings)
            {
                var keyvalues = endpointRecordString.Split('@').ToArray();
                string name = keyvalues[0].Trim();
                string url = keyvalues[1].Trim();
                endpoints.Add(new Endpoint(name, url));
            }

            Endpoints = endpoints.ToArray();
        }
    }

    public class Endpoint
    {
        public string Url { get; }
        public string Name { get; }

        public Endpoint(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}