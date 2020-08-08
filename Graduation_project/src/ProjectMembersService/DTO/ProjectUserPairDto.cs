using System.ComponentModel.DataAnnotations;

namespace ProjectMembersService
{
    public class ProjectUserPairDto
    {
        [Required(ErrorMessage="ProjectId must be specified")]
        public string ProjectId { get; set; }
        
        [Required(ErrorMessage="UserId must be specified")]
        public string UserId { get; set; }
    }
}