using System;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public static class RequestIdProvider
    {
        public static string GetRequestId() => Guid.NewGuid().ToString();
    }
}
