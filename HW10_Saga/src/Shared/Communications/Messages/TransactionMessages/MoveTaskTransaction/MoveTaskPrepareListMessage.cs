namespace Shared
{
    public class MoveTaskPrepareListMessage : BaseTransactionMessage
    {
        public string ProjectId { get; set; }
        public string ListTitle { get; set; }
    }
}