namespace Shared
{
    public class ProjectDeletedMessage : BaseMessage
    {
        public string ProjectId { get; set; }
        public string Title { get; set; }
    }
}