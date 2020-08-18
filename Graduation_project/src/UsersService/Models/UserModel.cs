using Shared;

namespace UsersService
{
    public class UserModel : BaseVersionedModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }     
        public string Region { get; set; }
    }
}