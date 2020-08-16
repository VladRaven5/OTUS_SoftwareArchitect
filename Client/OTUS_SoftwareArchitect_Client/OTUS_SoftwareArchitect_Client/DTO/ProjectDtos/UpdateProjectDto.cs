namespace OTUS_SoftwareArchitect_Client.DTO.ProjectDtos
{
    public class UpdateProjectDto : CreateProjectDto
    {
        public string Id { get; set; }
        public int Version { get; set; }
    }
}
