using System;

namespace AuthService
{
    public class UserAuthInfo
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string SessionId { get; private set; }
        public DateTimeOffset? SessionExpiredAt { get; private set; }

        internal void SetSession(string sessionId)
        {
            SessionId = sessionId;
            SessionExpiredAt = DateTimeOffset.UtcNow.AddDays(1);
        }
    }
}