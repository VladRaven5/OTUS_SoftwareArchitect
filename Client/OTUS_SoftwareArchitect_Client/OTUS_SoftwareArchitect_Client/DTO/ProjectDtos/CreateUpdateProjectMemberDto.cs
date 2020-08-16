using OTUS_SoftwareArchitect_Client.Models.ProjectModels;

namespace OTUS_SoftwareArchitect_Client.DTO.ProjectDtos
{
    public class CreateUpdateProjectMemberDto : DeleteProjectMemberDto
    {
        public int Role { get; set; }
    }
}
