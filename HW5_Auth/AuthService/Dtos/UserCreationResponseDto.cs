namespace AuthService
{
    public class UserCreationResult
    {
        public bool IsSuccess { get; set; }
        //if success
        public string UserId { get; set; }
        //if not very
        public string Error { get; set; }
    }
}