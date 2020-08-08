using System.ComponentModel.DataAnnotations;
using Shared;

namespace ProjectMembersService
{
    public class ProjectMemberDto : ProjectUserPairDto
    {
        [Required(ErrorMessage="User role must be specified")]
        public ProjectMemberRole Role { get; set; }
    }
}