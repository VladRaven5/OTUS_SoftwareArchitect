namespace Shared
{
    public class RollbackTransactionMessage : BaseTransactionMessage
    {
        public string Reason { get; set; }
    }    
}