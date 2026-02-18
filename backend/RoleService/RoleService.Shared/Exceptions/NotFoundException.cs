namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción para recursos no encontrados (404)
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message, string? errorCode = null)
            : base(message, 404, errorCode) { }
    }
}
