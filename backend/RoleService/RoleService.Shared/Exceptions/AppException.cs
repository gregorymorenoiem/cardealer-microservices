using System;

namespace RoleService.Shared.Exceptions
{
    /// <summary>
    /// Excepción base para la aplicación.
    /// Incluye código de estado HTTP y código de error opcional.
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Código de estado HTTP asociado a la excepción
        /// </summary>
        public int StatusCode { get; }
        
        /// <summary>
        /// Código de error técnico (ej: "ROLE_NOT_FOUND")
        /// </summary>
        public string? ErrorCode { get; }

        public AppException(string message, int statusCode = 400, string? errorCode = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
