using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiDocsService.Api.Controllers;

/// <summary>
/// Controller for managing API versions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VersionController : ControllerBase
{
    private readonly IVersionService _versionService;
    private readonly ILogger<VersionController> _logger;

    public VersionController(IVersionService versionService, ILogger<VersionController> logger)
    {
        _versionService = versionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all versions of a specific service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("services/{serviceName}")]
    [ProducesResponseType(typeof(VersionedServiceInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VersionedServiceInfo>> GetServiceVersions(
        string serviceName,
        CancellationToken cancellationToken)
    {
        var versionInfo = await _versionService.GetServiceVersionsAsync(serviceName, cancellationToken);

        if (versionInfo == null)
            return NotFound(new { message = $"Service '{serviceName}' not found" });

        return Ok(versionInfo);
    }

    /// <summary>
    /// Get all versioned services
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(typeof(List<VersionedServiceInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VersionedServiceInfo>>> GetAllVersionedServices(
        CancellationToken cancellationToken)
    {
        var services = await _versionService.GetAllVersionedServicesAsync(cancellationToken);
        return Ok(services);
    }

    /// <summary>
    /// Compare two versions of a service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="fromVersion">Source version</param>
    /// <param name="toVersion">Target version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("compare/{serviceName}")]
    [ProducesResponseType(typeof(VersionComparison), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VersionComparison>> CompareVersions(
        string serviceName,
        [FromQuery] string fromVersion,
        [FromQuery] string toVersion,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fromVersion) || string.IsNullOrWhiteSpace(toVersion))
            return BadRequest(new { message = "Both fromVersion and toVersion are required" });

        var comparison = await _versionService.CompareVersionsAsync(serviceName, fromVersion, toVersion, cancellationToken);

        if (comparison == null)
            return NotFound(new { message = $"Could not compare versions for service '{serviceName}'" });

        return Ok(comparison);
    }

    /// <summary>
    /// Get all deprecated APIs across services
    /// </summary>
    [HttpGet("deprecated")]
    [ProducesResponseType(typeof(List<ApiVersion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApiVersion>>> GetDeprecatedApis(CancellationToken cancellationToken)
    {
        var deprecated = await _versionService.GetDeprecatedApisAsync(cancellationToken);
        return Ok(deprecated);
    }

    /// <summary>
    /// Check if a specific version is deprecated
    /// </summary>
    [HttpGet("deprecated/{serviceName}/{version}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsVersionDeprecated(
        string serviceName,
        string version,
        CancellationToken cancellationToken)
    {
        var isDeprecated = await _versionService.IsVersionDeprecatedAsync(serviceName, version, cancellationToken);
        return Ok(new { serviceName, version, isDeprecated });
    }

    /// <summary>
    /// Get migration path between versions
    /// </summary>
    [HttpGet("migration/{serviceName}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetMigrationPath(
        string serviceName,
        [FromQuery] string fromVersion,
        [FromQuery] string toVersion,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fromVersion) || string.IsNullOrWhiteSpace(toVersion))
            return BadRequest(new { message = "Both fromVersion and toVersion are required" });

        var path = await _versionService.GetMigrationPathAsync(serviceName, fromVersion, toVersion, cancellationToken);
        return Ok(new { serviceName, fromVersion, toVersion, migrationPath = path });
    }
}
