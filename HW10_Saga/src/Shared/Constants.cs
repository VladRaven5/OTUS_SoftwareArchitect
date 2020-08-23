namespace Shared
{
    public static class Constants
    {
        public static readonly string RequestIdHeaderName = "X-RequestId";
        public static readonly string UserIdHeaderName = "X-UserId";

        public static readonly int RequestIdLifetimeDays = 1;
        public static readonly int BrokerMessageLifetimeDays = 2;
    }
}