using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CarDealer.Shared.Middleware;

/// <summary>
/// Middleware that adds standard security headers to all HTTP responses.
/// Implements OWASP security headers best practices:
/// - X-Content-Type-Options: Prevents MIME-type sniffing
/// - X-Frame-Options: Prevents clickjacking
/// - X-XSS-Protection: Legacy XSS filter (for older browsers)
/// - Strict-Transport-Security: Enforces HTTPS (HSTS)
/// - Content-Security-Policy: Controls resource loading
/// - Referrer-Policy: Controls referrer information
/// - Permissions-Policy: Controls browser feature access
/// - Cache-Control: Prevents caching of sensitive responses
/// 
/// References:
/// - OWASP Secure Headers Project: https://owasp.org/www-project-secure-headers/
/// - NIST SP 800-53 SC-8: Transmission Confidentiality and Integrity
/// - ISO 27001 A.14.1.2: Securing application services
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions? options = null)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-type sniffing (OWASP)
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking (OWASP)
        headers["X-Frame-Options"] = _options.XFrameOptions;

        // Legacy XSS protection for older browsers
        headers["X-XSS-Protection"] = "1; mode=block";

        // Enforce HTTPS via HSTS (only in production)
        if (_options.EnableHsts)
        {
            headers["Strict-Transport-Security"] = $"max-age={_options.HstsMaxAgeSeconds}; includeSubDomains; preload";
        }

        // Content Security Policy — restrict resource origins
        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
        }

        // CSP Report-Only — monitor violations without blocking (useful in dev/staging)
        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicyReportOnly))
        {
            headers["Content-Security-Policy-Report-Only"] = _options.ContentSecurityPolicyReportOnly;
        }

        // Referrer Policy — control referrer information sent with requests
        headers["Referrer-Policy"] = _options.ReferrerPolicy;

        // Permissions Policy — restrict browser features
        headers["Permissions-Policy"] = _options.PermissionsPolicy;

        // Prevent caching of authenticated responses
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";
        }

        // Remove server identification header
        headers.Remove("Server");
        headers.Remove("X-Powered-By");

        await _next(context);
    }
}

/// <summary>
/// Configuration options for SecurityHeadersMiddleware
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// X-Frame-Options value. Default: DENY (prevents all framing)
    /// Use "SAMEORIGIN" if the service needs to be framed by same-origin pages
    /// </summary>
    public string XFrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Whether to enable HSTS header. Should be true in production.
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// HSTS max-age in seconds. Default: 1 year (31536000)
    /// </summary>
    public int HstsMaxAgeSeconds { get; set; } = 31536000;

    /// <summary>
    /// Content-Security-Policy header value.
    /// Default is restrictive — API services don't serve HTML so "default-src 'none'" is appropriate.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'none'; frame-ancestors 'none'";

    /// <summary>
    /// Content-Security-Policy-Report-Only header value.
    /// When set, violations are reported but not blocked (useful for dev/staging).
    /// Set this to test a stricter CSP before enforcing it.
    /// </summary>
    public string? ContentSecurityPolicyReportOnly { get; set; }

    /// <summary>
    /// Referrer-Policy value. Default: strict-origin-when-cross-origin
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Permissions-Policy header value. Default: restrictive (disable unnecessary browser features)
    /// </summary>
    public string PermissionsPolicy { get; set; } = "camera=(), microphone=(), geolocation=(), payment=()";
}

/// <summary>
/// Extension methods for adding SecurityHeadersMiddleware to the pipeline
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security headers to all HTTP responses following OWASP guidelines.
    /// Should be added early in the middleware pipeline (before routing).
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, Action<SecurityHeadersOptions>? configure = null)
    {
        var options = new SecurityHeadersOptions();
        configure?.Invoke(options);
        return app.UseMiddleware<SecurityHeadersMiddleware>(options);
    }

    /// <summary>
    /// Adds security headers optimized for API services (no HTML rendering).
    /// Uses strict CSP and DENY framing.
    /// </summary>
    public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder app, bool isProduction = true)
    {
        return app.UseSecurityHeaders(options =>
        {
            options.EnableHsts = isProduction;
            options.XFrameOptions = "DENY";
            options.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            options.ReferrerPolicy = "no-referrer";
            options.PermissionsPolicy = "camera=(), microphone=(), geolocation=(), payment=()";
        });
    }
}
