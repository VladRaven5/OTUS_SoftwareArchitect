using System.ComponentModel.DataAnnotations;

namespace LabelsService
{
    public class CreateLabelDto
    {
        public string Title { get; set; }
        [Required(ErrorMessage="Color must be specified")]
        [MaxLength(6, ErrorMessage = "Color must be 6-digits hex")]
        [MinLength(6, ErrorMessage = "Color must be 6-digits hex")]
        public string Color { get; set; }
    }
}