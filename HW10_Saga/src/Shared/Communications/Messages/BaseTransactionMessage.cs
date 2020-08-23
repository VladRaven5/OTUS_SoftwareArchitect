namespace Shared
{
    public abstract class BaseTransactionMessage : BaseMessage
    {
        public string TransactionId { get; set; }
    }    
}