using System.ComponentModel.DataAnnotations;

namespace UsersService
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Region must be specified")]
        public string Region { get; set; }
    }
}