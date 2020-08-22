using System.ComponentModel.DataAnnotations;

namespace ListsService
{
    public class UpdateListDto
    {
        [Required(ErrorMessage="Id must be specified!")]
        public string Id { get; set; }
        [Required(ErrorMessage="Title must be specified!")]
        public string Title { get; set; }
        [Required(ErrorMessage="Version must be specified!")]
        public int Version { get; set; }
    }
}