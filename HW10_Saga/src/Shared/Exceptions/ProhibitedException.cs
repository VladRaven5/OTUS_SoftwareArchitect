using System;

namespace Shared
{
    public class ProhibitedException : Exception
    {
        public ProhibitedException(string message) : base(message)
        {            
        }
    }
}