using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Infrastructure
{
    public static class AuthDataProvider
    {
        private const string _cookieStringSettingName = "auth_cookie";
        private const string _cookieDomainSettingName = "cookie_domain";
        private const string _cookiePathSettingName = "cookie_path";

        public static void SetUserAuthData(AuthInfo authInfo)
        {
            Application.Current.Properties[_cookieDomainSettingName] = authInfo.Domain;
            Application.Current.Properties[_cookieStringSettingName] = authInfo.Cookie;
            Application.Current.Properties[_cookiePathSettingName] = authInfo.Path;
        }

        public static AuthInfo GetUserAuthData()
        {
            bool isDomainSpecified = Application.Current.Properties.TryGetValue(_cookieDomainSettingName, out object domainObj);
            bool isCookieSpecified = Application.Current.Properties.TryGetValue(_cookieStringSettingName, out object cookieObj);
            bool isPathSpecified = Application.Current.Properties.TryGetValue(_cookiePathSettingName, out object pathObj);

            if (!isDomainSpecified || !isCookieSpecified || !isPathSpecified)
            {
                return null;
            }

            return new AuthInfo(cookieObj.ToString(), domainObj.ToString(), pathObj.ToString());
        }

        public static bool HasAuthData()
        {
            bool isDomainSpecified = Application.Current.Properties.ContainsKey(_cookieDomainSettingName);
            bool isCookieSpecified = Application.Current.Properties.ContainsKey(_cookieStringSettingName);
            bool isPathSpecified = Application.Current.Properties.ContainsKey(_cookiePathSettingName);

            return isDomainSpecified && isCookieSpecified && isPathSpecified;
        }

        public static void ResetAuthData()
        {
            Application.Current.Properties.Remove(_cookieStringSettingName);
            Application.Current.Properties.Remove(_cookieDomainSettingName);
            Application.Current.Properties.Remove(_cookiePathSettingName);
        }
    }
}
