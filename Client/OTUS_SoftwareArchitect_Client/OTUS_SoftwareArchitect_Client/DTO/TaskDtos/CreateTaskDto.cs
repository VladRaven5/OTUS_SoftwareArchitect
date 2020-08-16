using System;
using System.Collections.Generic;

namespace OTUS_SoftwareArchitect_Client.DTO.TaskDtos
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ListId { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public IEnumerable<string> MembersIds { get; set; }
        public IEnumerable<string> LabelsIds { get; set; }
    }
}
