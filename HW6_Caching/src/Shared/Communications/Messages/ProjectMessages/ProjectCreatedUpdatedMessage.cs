namespace Shared
{
    public class ProjectCreatedUpdatedMessage : BaseMessage
    {
        public string ProjectId { get; set; }
        public string Title { get; set; }        
        public string OldTitle { get; set; } //updated only
    }
}