using System.ComponentModel.DataAnnotations;

namespace TasksService
{
    public class MoveTaskDto
    {
        [Required(ErrorMessage="Moving task id must be specified")]
        public string TaskId { get; set; }
        [Required(ErrorMessage="Target project id must be specified")]
        public string TargetProjectId { get; set; }
        [Required(ErrorMessage="Target list title must be specified")]
        public string TargetListTitle { get; set; }
    }
}