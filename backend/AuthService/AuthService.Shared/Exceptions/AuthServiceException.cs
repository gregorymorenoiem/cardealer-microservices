namespace AuthService.Shared.Exceptions;

/// <summary>
/// Base exception for all AuthService custom exceptions.
/// </summary>
public abstract class AuthServiceException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected AuthServiceException(
        string message, 
        int statusCode, 
        string errorCode,
        Exception? innerException = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
