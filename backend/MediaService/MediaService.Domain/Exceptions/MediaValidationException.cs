namespace MediaService.Domain.Exceptions;

/// <summary>
/// Exception thrown when media validation fails
/// </summary>
public class MediaValidationException : Exception
{
    public string? Field { get; }
    public string? ValidationError { get; }

    public MediaValidationException(string message)
        : base(message)
    {
    }

    public MediaValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MediaValidationException(string field, string validationError)
        : base($"Validation failed for field '{field}': {validationError}")
    {
        Field = field;
        ValidationError = validationError;
    }

    public MediaValidationException(string field, string validationError, Exception innerException)
        : base($"Validation failed for field '{field}': {validationError}", innerException)
    {
        Field = field;
        ValidationError = validationError;
    }
}