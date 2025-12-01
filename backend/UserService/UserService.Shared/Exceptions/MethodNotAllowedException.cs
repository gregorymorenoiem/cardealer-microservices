namespace UserService.Shared.Exceptions
{
    public class MethodNotAllowedException : AppException
    {
        public MethodNotAllowedException(string message) : base(message, 405) { }
    }
}
