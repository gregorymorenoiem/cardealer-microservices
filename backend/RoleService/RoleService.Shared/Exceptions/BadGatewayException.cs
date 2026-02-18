namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción para errores de gateway (502)
    /// </summary>
    public class BadGatewayException : AppException
    {
        public BadGatewayException(string message, string? errorCode = null)
            : base(message, 502, errorCode) { }
    }
}
