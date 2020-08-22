namespace Shared
{
    public class ProjectMemberCreatedUpdatedMessage : ProjectMemberDeletedMessage
    {        
        public ProjectMemberRole Role { get; set; }
    }
}