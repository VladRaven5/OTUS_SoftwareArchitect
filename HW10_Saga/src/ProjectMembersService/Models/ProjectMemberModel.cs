using Shared;

namespace ProjectMembersService
{
    public class ProjectMemberModel
    {
        public string ProjectId { get; set;}
        public string UserId { get; set; }
        public ProjectMemberRole Role { get; set; }
    }
}