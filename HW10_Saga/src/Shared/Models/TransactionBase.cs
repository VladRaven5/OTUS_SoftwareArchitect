namespace Shared
{
    public class TransactionBase : BaseDatedModel
    {
        /// <summary>
        /// Id of transaction-related object
        /// </summary>
        /// <value></value>
        public string ObjectId { get; set; }
        /// <summary>
        /// Constant-string of transaction type
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
        /// <summary>
        /// Serialized technical data of this transaction
        /// </summary>
        /// <value></value>
        public string Data { get; set; }
        /// <summary>
        /// Some custom message for user
        /// </summary>
        /// <value></value>
        public string Message { get; set; }
        public TransactionState State { get; set; }
    }

    public enum TransactionState
    {
        Pending = 0,
        Processing = 1,
        Success = 2,
        Denied = 3
    }    
}