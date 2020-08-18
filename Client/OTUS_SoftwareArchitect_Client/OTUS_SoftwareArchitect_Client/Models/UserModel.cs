using OTUS_SoftwareArchitect_Client.Models.BaseModels;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class UserModel : BaseModel
    {
        public string Username { get; set; }
        public string Region { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int Version { get; set; }
    }
}
