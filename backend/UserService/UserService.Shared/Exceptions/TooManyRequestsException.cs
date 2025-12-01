namespace UserService.Shared.Exceptions
{
    public class TooManyRequestsException : AppException
    {
        public TooManyRequestsException(string message) : base(message, 429) { }
    }
}
