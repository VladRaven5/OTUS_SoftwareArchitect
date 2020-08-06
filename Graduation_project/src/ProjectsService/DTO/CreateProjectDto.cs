using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectsService
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage="Title can't be empty")]
        public string Title { get; set; }    
        public string Description { get; set; }        
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}