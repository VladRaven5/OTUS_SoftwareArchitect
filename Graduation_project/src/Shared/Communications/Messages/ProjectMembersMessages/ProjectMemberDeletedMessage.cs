namespace Shared
{
    public class ProjectMemberDeletedMessage : BaseMessage
    {
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public string Username { get; set; }
        public string ProjectTitle { get; set; }
    }
}