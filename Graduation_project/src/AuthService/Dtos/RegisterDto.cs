using System.ComponentModel.DataAnnotations;

namespace AuthService
{
    public class RegisterDto : LoginDto
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Region must be specified")]
        public string Region { get; set; }
    }
}