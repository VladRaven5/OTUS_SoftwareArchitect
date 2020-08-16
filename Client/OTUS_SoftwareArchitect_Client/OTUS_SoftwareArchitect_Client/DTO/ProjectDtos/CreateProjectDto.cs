using System;

namespace OTUS_SoftwareArchitect_Client.DTO.ProjectDtos
{
    public class CreateProjectDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
