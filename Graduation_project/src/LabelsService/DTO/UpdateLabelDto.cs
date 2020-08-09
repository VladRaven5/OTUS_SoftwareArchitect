using System.ComponentModel.DataAnnotations;

namespace LabelsService
{
    public class UpdateLabelDto : CreateLabelDto
    {
        [Required(ErrorMessage = "Id must be specified!")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Version must be specified!")]
        public int Version { get; set; }

    }
}