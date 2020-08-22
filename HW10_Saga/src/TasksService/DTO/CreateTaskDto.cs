using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TasksService
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage="Task must have title")]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage="List id must be specified")]
        public string ListId { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public IEnumerable<string> MembersIds { get; set; }
        public IEnumerable<string> LabelsIds { get; set; }
    }
}