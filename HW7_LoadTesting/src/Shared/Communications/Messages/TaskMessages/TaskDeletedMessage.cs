namespace Shared
{
    public class TaskDeletedMessage : BaseMessage
    {
        public string TaskId { get; set; }
        public string ProjectId { get; set; }
        public string Title { get; set; }
    }
}