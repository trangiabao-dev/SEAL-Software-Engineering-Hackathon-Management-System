namespace SealHackathon.API.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(int resourceName, object id)
            : base(404, $"{resourceName} with id '{id}' was not found.")
        { }

        public NotFoundException(string message) : base(404, message)
        { }
    }
}
