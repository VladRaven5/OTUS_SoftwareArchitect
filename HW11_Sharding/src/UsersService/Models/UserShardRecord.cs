using Shared;

namespace UsersService
{
    public class UserShardRecord  : BaseModel
    {
        public string UserId { get; set; }
        public string ShardKey { get; set; } //UsersRegions const
    }
}