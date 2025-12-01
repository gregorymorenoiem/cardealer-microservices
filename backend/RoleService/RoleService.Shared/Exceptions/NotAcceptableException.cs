namespace RoleService.Shared.Exceptions
{
    public class NotAcceptableException : AppException
    {
        public NotAcceptableException(string message) : base(message, 406) { }
    }
}
