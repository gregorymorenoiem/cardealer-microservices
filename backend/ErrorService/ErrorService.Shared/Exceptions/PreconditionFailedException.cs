namespace ErrorService.Shared.Exceptions
{
    public class PreconditionFailedException : AppException
    {
        public PreconditionFailedException(string message) : base(message, 412) { }
    }
}