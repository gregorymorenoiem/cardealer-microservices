namespace ErrorService.Shared.Exceptions
{
    public class LengthRequiredException : AppException
    {
        public LengthRequiredException(string message) : base(message, 411) { }
    }
}