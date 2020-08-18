using System.Collections.Generic;

namespace Shared
{
    public static class UsersRegions
    {
        public static readonly string USA = "US";
        public static readonly string Europe = "EU";
        public static readonly string Russia = "RU";
        public static readonly string China = "CN";

        public static readonly List<string> AvailableRegions = new List<string>
        {
            USA, Europe, Russia, China
        };

        public static bool HasRegion(string currentRegion)
        {
            return AvailableRegions.Contains(currentRegion);
        }
    }
}