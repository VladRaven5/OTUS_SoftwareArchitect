using System;

namespace Shared
{
    public class VersionsNotMatchException : Exception
    {
        public VersionsNotMatchException()
            : base("Your version doesn't match, update your model from server")
        {          
        }
    }
}