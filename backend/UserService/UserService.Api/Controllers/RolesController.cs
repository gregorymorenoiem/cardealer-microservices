using UserService.Application.DTOs;
using UserService.Application.UseCases.GetError;
using UserService.Application.UseCases.GetErrors;
using UserService.Application.UseCases.GetErrorStats;
using UserService.Application.UseCases.GetServiceNames;
using UserService.Application.UseCases.LogError;
using UserService.Shared;
using UserService.Shared.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace UserService.Api.Controllers
{
    [Authorize(Policy = "UserServiceAccess")]
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ErrorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registra un nuevo error en el sistema
        /// </summary>
        [HttpPost]
        [RateLimit(maxRequests: 200, windowSeconds: 60)]
        public async Task<ActionResult<ApiResponse<LogErrorResponse>>> LogError([FromBody] LogErrorRequest request)
        {
            var command = new LogErrorCommand(request);
            var result = await _mediator.Send(command);
            return ApiResponse<LogErrorResponse>.Ok(result);
        }

        /// <summary>
        /// Obtiene una lista de errores con filtros opcionales
        /// </summary>
        [HttpGet]
        [RateLimit(maxRequests: 150, windowSeconds: 60)]
        public async Task<ActionResult<ApiResponse<GetErrorsResponse>>> GetErrors(
            [FromQuery] string? serviceName = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var request = new GetErrorsRequest(serviceName, from, to, page, pageSize);
            var query = new GetErrorsQuery(request);
            var result = await _mediator.Send(query);
            return ApiResponse<GetErrorsResponse>.Ok(result);
        }

        /// <summary>
        /// Obtiene los detalles de un error específico
        /// </summary>
        [HttpGet("{id:guid}")]
        [RateLimit(maxRequests: 200, windowSeconds: 60)]
        public async Task<ActionResult<ApiResponse<GetErrorResponse>>> GetError(Guid id)
        {
            var request = new GetErrorRequest(id);
            var query = new GetErrorQuery(request);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Error not found"));

            return ApiResponse<GetErrorResponse>.Ok(result);
        }

        /// <summary>
        /// Obtiene estadísticas de errores en un rango de fechas
        /// </summary>
        [HttpGet("stats")]
        [RateLimit(maxRequests: 100, windowSeconds: 60)]
        public async Task<ActionResult<ApiResponse<GetErrorStatsResponse>>> GetStats(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var request = new GetErrorStatsRequest(from, to);
            var query = new GetErrorStatsQuery(request);
            var result = await _mediator.Send(query);
            return ApiResponse<GetErrorStatsResponse>.Ok(result);
        }

        /// <summary>
        /// Obtiene la lista de servicios que han registrado errores
        /// </summary>
        [HttpGet("services")]
        [RateLimit(maxRequests: 150, windowSeconds: 60)]
        public async Task<ActionResult<ApiResponse<GetServiceNamesResponse>>> GetServiceNames()
        {
            var request = new GetServiceNamesRequest();
            var query = new GetServiceNamesQuery(request);
            var result = await _mediator.Send(query);
            return ApiResponse<GetServiceNamesResponse>.Ok(result);
        }
    }
}
