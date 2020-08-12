using System.ComponentModel.DataAnnotations;

namespace ListsService
{
    public class CreateListDto
    {
        [Required(ErrorMessage="Title must be specified!")]
        public string Title { get; set; }
        [Required(ErrorMessage="ProjectId must be specified!")]
        public string ProjectId { get; set; }
    }
}