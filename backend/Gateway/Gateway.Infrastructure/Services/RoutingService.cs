using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gateway.Infrastructure.Services;

public class RoutingService : IRoutingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RoutingService> _logger;
    private JObject? _ocelotConfig;

    public RoutingService(IConfiguration configuration, ILogger<RoutingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        LoadOcelotConfiguration();
    }

    private void LoadOcelotConfiguration()
    {
        try
        {
            var ocelotFile = _configuration["ASPNETCORE_ENVIRONMENT"] == "Development"
                ? "ocelot.dev.json"
                : "ocelot.prod.json";

            var ocelotPath = Path.Combine(Directory.GetCurrentDirectory(), ocelotFile);

            if (File.Exists(ocelotPath))
            {
                var json = File.ReadAllText(ocelotPath);
                _ocelotConfig = JObject.Parse(json);
            }
            else
            {
                _logger.LogWarning("Ocelot configuration file not found: {OcelotFile}", ocelotFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Ocelot configuration");
        }
    }

    public Task<bool> RouteExists(string upstreamPath)
    {
        try
        {
            if (_ocelotConfig == null)
                return Task.FromResult(false);

            var routes = _ocelotConfig["Routes"] as JArray;
            if (routes == null)
                return Task.FromResult(false);

            // Check if any route matches the upstream path
            var routeExists = routes.Any(route =>
            {
                var upstreamTemplate = route["UpstreamPathTemplate"]?.ToString();
                return upstreamTemplate != null && MatchesTemplate(upstreamPath, upstreamTemplate);
            });

            return Task.FromResult(routeExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if route exists for {UpstreamPath}", upstreamPath);
            return Task.FromResult(false);
        }
    }

    public Task<string> ResolveDownstreamPath(string upstreamPath)
    {
        try
        {
            if (_ocelotConfig == null)
                return Task.FromResult(string.Empty);

            var routes = _ocelotConfig["Routes"] as JArray;
            if (routes == null)
                return Task.FromResult(string.Empty);

            // Find matching route
            var matchingRoute = routes.FirstOrDefault(route =>
            {
                var upstreamTemplate = route["UpstreamPathTemplate"]?.ToString();
                return upstreamTemplate != null && MatchesTemplate(upstreamPath, upstreamTemplate);
            });

            if (matchingRoute != null)
            {
                var downstreamTemplate = matchingRoute["DownstreamPathTemplate"]?.ToString();
                var downstreamScheme = matchingRoute["DownstreamScheme"]?.ToString() ?? "http";
                var hostAndPorts = matchingRoute["DownstreamHostAndPorts"] as JArray;

                if (downstreamTemplate != null && hostAndPorts?.Any() == true)
                {
                    var firstHost = hostAndPorts[0];
                    var host = firstHost["Host"]?.ToString();
                    var port = firstHost["Port"]?.ToString();

                    // Substitute path parameters if needed
                    var resolvedPath = SubstitutePathParameters(upstreamPath,
                        matchingRoute["UpstreamPathTemplate"]?.ToString() ?? string.Empty,
                        downstreamTemplate);

                    return Task.FromResult($"{downstreamScheme}://{host}:{port}{resolvedPath}");
                }
            }

            return Task.FromResult(string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving downstream path for {UpstreamPath}", upstreamPath);
            return Task.FromResult(string.Empty);
        }
    }

    /// <summary>
    /// Check if an actual path matches a template with path parameters
    /// Example: "/api/errors/123" matches "/api/errors/{id}"
    /// </summary>
    private bool MatchesTemplate(string actualPath, string template)
    {
        // Simple template matching - split by '/' and compare segments
        var actualSegments = actualPath.TrimStart('/').Split('/');
        var templateSegments = template.TrimStart('/').Split('/');

        if (actualSegments.Length != templateSegments.Length)
            return false;

        for (int i = 0; i < actualSegments.Length; i++)
        {
            // Template parameters are in curly braces {id}, {*catchAll}, etc.
            if (templateSegments[i].StartsWith("{") && templateSegments[i].EndsWith("}"))
                continue; // Parameter matches any value

            if (!actualSegments[i].Equals(templateSegments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Substitute path parameters from upstream path to downstream path
    /// Example: upstream="/api/errors/123", upstreamTemplate="/api/errors/{id}", downstreamTemplate="/errors/{id}"
    /// Result: "/errors/123"
    /// </summary>
    private string SubstitutePathParameters(string actualPath, string upstreamTemplate, string downstreamTemplate)
    {
        var actualSegments = actualPath.TrimStart('/').Split('/');
        var upstreamSegments = upstreamTemplate.TrimStart('/').Split('/');

        // Extract parameters from actual path
        var parameters = new Dictionary<string, string>();
        for (int i = 0; i < upstreamSegments.Length && i < actualSegments.Length; i++)
        {
            if (upstreamSegments[i].StartsWith("{") && upstreamSegments[i].EndsWith("}"))
            {
                var paramName = upstreamSegments[i].Trim('{', '}');
                parameters[paramName] = actualSegments[i];
            }
        }

        // Substitute parameters in downstream template
        var result = downstreamTemplate;
        foreach (var kvp in parameters)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        return result;
    }
}
