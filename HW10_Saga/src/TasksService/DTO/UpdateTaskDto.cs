using System.ComponentModel.DataAnnotations;

namespace TasksService
{
    public class UpdateTaskDto : CreateTaskDto
    {
        [Required(ErrorMessage="Id must be specified")]
        public string Id { get; set; }
        [Required(ErrorMessage="Version must be specified")]
        public int Version { get; set; }
    }
}