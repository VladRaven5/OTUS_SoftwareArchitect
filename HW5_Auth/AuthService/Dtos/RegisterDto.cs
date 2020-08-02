using System.ComponentModel.DataAnnotations;

namespace AuthService
{
    public class RegisterDto : LoginDto
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string Username { get; set; }
    }
}