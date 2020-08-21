using System;

namespace Shared
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {            
        }
    }
}