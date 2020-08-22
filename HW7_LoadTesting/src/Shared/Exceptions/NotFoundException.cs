using System;

namespace Shared
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {            
        }
    }
}