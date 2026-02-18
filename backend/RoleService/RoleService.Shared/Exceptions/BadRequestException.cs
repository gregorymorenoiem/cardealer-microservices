namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción para solicitudes mal formadas o inválidas (400)
    /// </summary>
    public class BadRequestException : AppException
    {
        public BadRequestException(string message, string? errorCode = null)
            : base(message, 400, errorCode) { }
    }
}
