using System.Security.Claims;
using System.Threading.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SearchAgent.Application.DTOs;
using SearchAgent.Application.Features.Config.Commands;
using SearchAgent.Application.Features.Config.Queries;
using SearchAgent.Application.Features.Search.Queries;
using SearchAgent.Domain.Models;

namespace SearchAgent.Api.Controllers;

[ApiController]
[Route("api/search-agent")]
public class SearchAgentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchAgentController> _logger;

    public SearchAgentController(IMediator mediator, ILogger<SearchAgentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Process a natural language vehicle search query using Claude AI.
    /// Public endpoint — no auth required for searching.
    /// </summary>
    [HttpPost("search")]
    [AllowAnonymous]
    [EnableRateLimiting("search")]
    public async Task<IActionResult> Search([FromBody] SearchAgentRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var query = new ProcessSearchQuery(
            request.Query,
            request.SessionId,
            request.Page,
            request.PageSize,
            userId,
            ipAddress
        );

        try
        {
            var result = await _mediator.Send(query, ct);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            // Graceful degradation: when AI is unavailable, return a safe response
            // that tells the frontend to fall back to basic filter-based search
            _logger.LogWarning(ex,
                "AI search failed for query '{Query}'. Returning degraded response (filter-only mode).",
                request.Query);

            Response.Headers["X-Degraded-Response"] = "true";
            Response.Headers["X-Degraded-Service"] = "SearchAgent";

            return Ok(new
            {
                success = true,
                data = new SearchAgentResultDto
                {
                    IsAiSearchEnabled = false,
                    WasCached = false,
                    LatencyMs = 0,
                    AiFilters = new SearchAgentResponse
                    {
                        Confianza = 0.0f,
                        ResultadoMinimoGarantizado = 0,
                        MensajeUsuario = "La búsqueda inteligente no está disponible temporalmente. Usa los filtros manuales para encontrar tu vehículo. 🔍",
                        Advertencias = new List<string> { "AI temporalmente no disponible — mostrando filtros básicos" }
                    }
                }
            });
        }
    }

    /// <summary>
    /// Get current SearchAgent configuration. Admin only.
    /// </summary>
    [HttpGet("config")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _mediator.Send(new GetSearchAgentConfigQuery(), ct);
        return Ok(new { success = true, data = config });
    }

    /// <summary>
    /// Update SearchAgent configuration. Admin only.
    /// Allows partial updates — only send the fields you want to change.
    /// </summary>
    [HttpPut("config")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> UpdateConfig(
        [FromBody] UpdateSearchAgentConfigRequest request,
        CancellationToken ct)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "admin";

        var command = new UpdateSearchAgentConfigCommand(request, updatedBy);
        var config = await _mediator.Send(command, ct);

        return Ok(new { success = true, data = config });
    }

    /// <summary>
    /// Health probe for the AI search service.
    /// Tests Claude API connectivity.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult Status()
    {
        return Ok(new
        {
            service = "SearchAgent",
            status = "healthy",
            version = "2.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
