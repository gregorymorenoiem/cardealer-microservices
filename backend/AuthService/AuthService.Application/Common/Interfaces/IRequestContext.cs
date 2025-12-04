namespace AuthService.Application.Common.Interfaces;

/// <summary>
/// Abstraction for accessing HTTP request context information.
/// This allows the Application layer to access request-specific data
/// without depending on ASP.NET Core infrastructure.
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// Gets the IP address of the client making the request.
    /// Returns "unknown" if the IP cannot be determined.
    /// </summary>
    string IpAddress { get; }

    /// <summary>
    /// Gets the User-Agent header from the request.
    /// Returns null if not available.
    /// </summary>
    string? UserAgent { get; }

    /// <summary>
    /// Gets the current authenticated user's ID.
    /// Returns null if no user is authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the correlation ID for the current request.
    /// </summary>
    string? CorrelationId { get; }
}
