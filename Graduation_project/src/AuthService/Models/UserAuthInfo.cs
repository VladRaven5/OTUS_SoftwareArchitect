using Shared;

namespace AuthService
{
    public class UserAuthInfo : BaseModel
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
    }
}