namespace RoleService.Shared.Exceptions
{
    public class BadGatewayException : AppException
    {
        public BadGatewayException(string message) : base(message, 502) { }
    }
}
