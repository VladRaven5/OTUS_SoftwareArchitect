namespace Shared
{
    public class UserCreatedUpdatedMessage : BaseMessage
    {
        public string UserId { get; set; }
        public string Username { get; set; }
    }
}