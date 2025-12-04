using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AuthService.Infrastructure.Services;

/// <summary>
/// Implementation of IRequestContext that extracts information from the current HTTP context.
/// </summary>
public class HttpRequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "unknown";

            // Check for forwarded headers first (when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs, take the first one (original client)
                var ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            // Check for X-Real-IP header (nginx)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            // Fallback to direct connection IP
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                // Handle IPv4-mapped IPv6 addresses
                if (remoteIp.IsIPv4MappedToIPv6)
                    return remoteIp.MapToIPv4().ToString();
                return remoteIp.ToString();
            }

            return "unknown";
        }
    }

    public string? UserAgent
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["User-Agent"].FirstOrDefault();
        }
    }

    public string? UserId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context?.User.FindFirst("sub")?.Value;
        }
    }

    public string? CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? context?.TraceIdentifier;
        }
    }
}
