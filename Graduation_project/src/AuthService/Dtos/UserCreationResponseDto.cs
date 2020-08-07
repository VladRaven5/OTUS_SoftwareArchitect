namespace AuthService
{
    public class UserCreationResult
    {
        public static UserCreationResult Success(string userId)
        {
            return new UserCreationResult
            {
                IsSuccess = true,
                UserId = userId
            };
        }

        public static UserCreationResult Failure(string error)
        {
            return new UserCreationResult
            {
                IsSuccess = false,
                Error = error
            };
        }        
        
        protected UserCreationResult()
        {}

        public bool IsSuccess { get; private set; }
        //if success
        public string UserId { get; private set; }
        //if not very
        public string Error { get; private set; }
    }
}