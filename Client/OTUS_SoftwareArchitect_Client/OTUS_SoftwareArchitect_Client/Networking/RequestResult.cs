using System.Net;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public class RequestResult<TResult> : RequestResult
    {
        public static RequestResult<TResult> Success(TResult result)
        {
            return new RequestResult<TResult>(result);
        }

        public static RequestResult<TResult> Failure(HttpStatusCode statusCode, string error)
        {
            return new RequestResult<TResult>(statusCode, error);
        }

        private RequestResult(TResult result) : base()
        {
            Result = result;
        }

        private RequestResult(HttpStatusCode statusCode, string error) : base(statusCode, error)
        {
        }

        public TResult Result { get; }
    }

    public class RequestResult
    {
        public static RequestResult Success()
        {
            return new RequestResult();
        }

        public static RequestResult Failure(HttpStatusCode statusCode, string error)
        {
            return new RequestResult(statusCode, error);
        }

        protected RequestResult()
        {
            StatusCode = HttpStatusCode.OK;
            IsSuccess = true;
        }

        protected RequestResult(HttpStatusCode statusCode, string error)
        {
            StatusCode = statusCode;
            Error = error;
        }

        public HttpStatusCode StatusCode { get; }
        public string Error { get; }
        public bool IsSuccess { get; }

        public string GetFullMessage() => $"(Error {StatusCode}) {Error}";
    }
}
