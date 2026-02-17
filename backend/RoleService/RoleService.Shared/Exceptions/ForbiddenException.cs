namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción para acceso prohibido (403)
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message, string? errorCode = null) 
            : base(message, 403, errorCode) { }
    }
}
