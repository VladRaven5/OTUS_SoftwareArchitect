namespace OTUS_SoftwareArchitect_Client.Infrastructure
{
    public class AuthInfo
    {
        public AuthInfo(string cookie, string domain, string path)
        {
            Cookie = cookie;
            Domain = domain;
            Path = path;
        }

        public string Cookie { get; }
        public string Domain { get; }
        public string Path { get; }
    }
}
