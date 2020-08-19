using System.ComponentModel.DataAnnotations;

namespace UsersService
{
    public class UpdateUserDto : CreateUserDto
    {
        [Required(ErrorMessage="Id must be specified")]
        public string Id { get; set; }
        [Required(ErrorMessage="Version must be specified")]
        public int Version { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}