namespace SealHackathon.API.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; init; }

        protected AppException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
