using System;

namespace Shared
{
    public class AlreadyInTransactionException : Exception
    {
        public AlreadyInTransactionException() : base("This item is already in transaction")
        {            
        }
    }    
}