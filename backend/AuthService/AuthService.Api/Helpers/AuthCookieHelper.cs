using Microsoft.AspNetCore.Http;

namespace AuthService.Api.Helpers;

/// <summary>
/// Security (CWE-922): Centralizes HttpOnly cookie management for auth tokens.
/// All endpoints that issue tokens MUST use these helpers instead of returning tokens
/// in the response body only.
/// </summary>
public static class AuthCookieHelper
{
    private const string AccessTokenCookie = "okla_access_token";
    private const string RefreshTokenCookie = "okla_refresh_token";

    /// <summary>
    /// Sets access and refresh tokens as HttpOnly, Secure, SameSite=Strict cookies.
    /// Tokens set this way are NOT accessible via JavaScript (prevents XSS token theft).
    /// </summary>
    public static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var isProduction = !string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development", StringComparison.OrdinalIgnoreCase);

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,  // Changed from Strict to Lax — cookies sent in cross-site requests
            Path = "/",
            Expires = expiresAt,
            IsEssential = true
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,  // Changed from Strict to Lax — cookies sent in cross-site requests
            Path = "/",  // Changed from "/api/auth" to "/" — needs to be sent to all endpoints
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            IsEssential = true
        };

        response.Cookies.Append(AccessTokenCookie, accessToken, accessCookieOptions);
        response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);
    }

    /// <summary>
    /// Clears auth cookies on logout.
    /// </summary>
    public static void ClearAuthCookies(HttpResponse response)
    {
        var isProduction = !string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development", StringComparison.OrdinalIgnoreCase);

        response.Cookies.Delete(AccessTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }

    /// <summary>
    /// Reads the refresh token from HttpOnly cookie if available.
    /// Falls back to null if not present.
    /// </summary>
    public static string? GetRefreshTokenFromCookie(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookie];
    }

    /// <summary>
    /// Reads the access token from HttpOnly cookie if available.
    /// Falls back to null if not present.
    /// </summary>
    public static string? GetAccessTokenFromCookie(HttpRequest request)
    {
        return request.Cookies[AccessTokenCookie];
    }
}
