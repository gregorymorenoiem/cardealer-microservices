namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción para conflictos (409)
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message, string? errorCode = null)
            : base(message, 409, errorCode) { }
    }
}
