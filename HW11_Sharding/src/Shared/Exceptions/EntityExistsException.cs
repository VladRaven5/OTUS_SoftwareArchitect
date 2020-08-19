using System;

namespace Shared
{
    public class EntityExistsException : Exception
    {
        public EntityExistsException(string message) : base(message)
        {            
        }
    }
}