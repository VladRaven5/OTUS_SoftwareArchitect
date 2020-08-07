using System;

namespace WorkingHoursService
{
    public class AlreadyHandledException : Exception
    {
        public AlreadyHandledException() : base("Request already handled")
        {            
        }
    }
}