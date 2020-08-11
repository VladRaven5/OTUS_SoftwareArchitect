using System.ComponentModel.DataAnnotations;

namespace WorkingHoursService
{
    public class CreateWorkingHoursRecordDto
    {
        [Required(ErrorMessage="TaskId cannot be empty")]
        public string TaskId { get; set; }

        [Required(ErrorMessage="Hours value must be positive")]
        public double Hours { get; set; }

        [Required(ErrorMessage="Description is required")]
        public string Description { get; set; }
    }
}