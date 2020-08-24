namespace Shared
{
    public class MoveTaskHandleHoursMessage : BaseTransactionMessage
    {
        public string TaskId { get; set; }
        public string ProjectId { get; set; } 
    }    
}