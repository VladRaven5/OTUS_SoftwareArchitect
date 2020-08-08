using System.Collections.Generic;
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
            var endpointsSection = configuration.GetSection("Endpoints");
            foreach(var endpointSection in endpointsSection.GetChildren())
            {
                string name = endpointSection.GetValue<string>("Name");
                string url = endpointSection.GetValue<string>("Url");
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