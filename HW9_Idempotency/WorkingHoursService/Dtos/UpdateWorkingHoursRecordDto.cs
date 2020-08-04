using System.ComponentModel.DataAnnotations;

namespace WorkingHoursService
{
    public class UpdateWorkingHoursRecordDto : CreateWorkingHoursRecordDto
    {
        [Required(ErrorMessage="Id must be specified")]
        public string Id { get; set; }

        [Required(ErrorMessage="Version must be specified")]
        public int Version { get; set; }
    }
}