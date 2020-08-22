using System;

namespace Shared
{
    public class AlreadyHandledException : Exception
    {
        public AlreadyHandledException() : base("Request already handled")
        {            
        }
    }
}