using FluentValidation;
using UserService.Application.DTOs;
using System.Text.RegularExpressions;

namespace UserService.Application.UseCases.LogError
{
    /// <summary>
    /// Validador robusto para LogErrorRequest con reglas de seguridad y sanitización
    /// </summary>
    public class LogErrorCommandValidator : AbstractValidator<LogErrorRequest>
    {
        private static readonly Regex ServiceNameRegex = new(@"^[a-zA-Z0-9\-_.]+$", RegexOptions.Compiled);
        private static readonly Regex HttpMethodRegex = new(@"^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EndpointRegex = new(@"^[a-zA-Z0-9\-_/:.?&=]+$", RegexOptions.Compiled);

        public LogErrorCommandValidator()
        {
            // ServiceName - REQUERIDO y con formato estricto
            RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("ServiceName es requerido")
                .MaximumLength(100).WithMessage("ServiceName no puede exceder 100 caracteres")
                .Matches(ServiceNameRegex).WithMessage("ServiceName solo puede contener letras, números, guiones, puntos y guiones bajos")
                .Must(NotContainSqlInjection).WithMessage("ServiceName contiene caracteres potencialmente peligrosos");

            // ExceptionType - REQUERIDO
            RuleFor(x => x.ExceptionType)
                .NotEmpty().WithMessage("ExceptionType es requerido")
                .MaximumLength(200).WithMessage("ExceptionType no puede exceder 200 caracteres")
                .Must(NotContainSqlInjection).WithMessage("ExceptionType contiene caracteres potencialmente peligrosos");

            // Message - REQUERIDO con límite de tamaño
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message es requerido")
                .MaximumLength(5000).WithMessage("Message no puede exceder 5000 caracteres (demasiado largo)")
                .Must(NotContainXssPatterns).WithMessage("Message contiene patrones potencialmente peligrosos");

            // StackTrace - OPCIONAL pero con límite
            RuleFor(x => x.StackTrace)
                .MaximumLength(50000).WithMessage("StackTrace no puede exceder 50000 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.StackTrace));

            // Endpoint - OPCIONAL con formato validado
            RuleFor(x => x.Endpoint)
                .MaximumLength(500).WithMessage("Endpoint no puede exceder 500 caracteres")
                .Matches(EndpointRegex).WithMessage("Endpoint contiene caracteres inválidos")
                .When(x => !string.IsNullOrWhiteSpace(x.Endpoint));

            // HttpMethod - OPCIONAL pero con valores permitidos
            RuleFor(x => x.HttpMethod)
                .MaximumLength(10).WithMessage("HttpMethod no puede exceder 10 caracteres")
                .Matches(HttpMethodRegex).WithMessage("HttpMethod debe ser un método HTTP válido (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE)")
                .When(x => !string.IsNullOrWhiteSpace(x.HttpMethod));

            // UserId - OPCIONAL con límite
            RuleFor(x => x.UserId)
                .MaximumLength(100).WithMessage("UserId no puede exceder 100 caracteres")
                .Must(NotContainSqlInjection).WithMessage("UserId contiene caracteres potencialmente peligrosos")
                .When(x => !string.IsNullOrWhiteSpace(x.UserId));

            // StatusCode - OPCIONAL pero con rango válido
            RuleFor(x => x.StatusCode)
                .InclusiveBetween(100, 599).WithMessage("StatusCode debe estar entre 100 y 599")
                .When(x => x.StatusCode.HasValue);

            // Metadata - OPCIONAL pero con límite de tamaño
            RuleFor(x => x.Metadata)
                .Must(metadata => metadata == null || metadata.Count <= 50)
                .WithMessage("Metadata no puede contener más de 50 entradas")
                .When(x => x.Metadata != null);

            // Validación de tamaño total de Metadata
            RuleFor(x => x.Metadata)
                .Must(BeValidMetadataSize)
                .WithMessage("El tamaño total de Metadata no puede exceder 10KB")
                .When(x => x.Metadata != null && x.Metadata.Any());
        }

        /// <summary>
        /// Detecta patrones comunes de SQL Injection
        /// </summary>
        private bool NotContainSqlInjection(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var dangerousPatterns = new[]
            {
                "';--",
                "' OR '1'='1",
                "' OR 1=1--",
                "EXEC(",
                "EXECUTE(",
                "DROP TABLE",
                "DROP DATABASE",
                "INSERT INTO",
                "DELETE FROM",
                "UPDATE ",
                "UNION SELECT"
            };

            return !dangerousPatterns.Any(pattern =>
                value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Detecta patrones comunes de XSS
        /// </summary>
        private bool NotContainXssPatterns(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var xssPatterns = new[]
            {
                "<script",
                "javascript:",
                "onerror=",
                "onload=",
                "onclick=",
                "eval(",
                "alert(",
                "<iframe"
            };

            return !xssPatterns.Any(pattern =>
                value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Valida que el tamaño total de Metadata no exceda 10KB
        /// </summary>
        private bool BeValidMetadataSize(Dictionary<string, object>? metadata)
        {
            if (metadata == null || !metadata.Any())
                return true;

            var totalSize = metadata.Sum(kvp =>
            {
                var keySize = kvp.Key?.Length ?? 0;
                var valueSize = kvp.Value?.ToString()?.Length ?? 0;
                return keySize + valueSize;
            });

            return totalSize <= 10240; // 10KB
        }
    }
}
