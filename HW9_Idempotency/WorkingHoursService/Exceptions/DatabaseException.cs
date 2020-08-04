using System;

namespace WorkingHoursService
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {            
        }
    }
}