using System.Net;

namespace AuthService
{
    public class ErrorResponseDto
    {
        public string Title { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}