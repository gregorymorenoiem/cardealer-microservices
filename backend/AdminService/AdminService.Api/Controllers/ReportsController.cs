using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AdminService.Application.UseCases.Reports.ResolveReport;

namespace AdminService.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Resolve a user report
        /// </summary>
        [HttpPost("{reportId}/resolve")]
        public async Task<IActionResult> ResolveReport(
            Guid reportId,
            [FromBody] ResolveReportRequest request)
        {
            try
            {
                var command = new ResolveReportCommand(
                    reportId,
                    request.ResolvedBy,
                    request.Resolution,
                    request.ReporterEmail,
                    request.ReportSubject
                );

                var result = await _mediator.Send(command);

                return Ok(new { Success = result, Message = "Report resolved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving report {ReportId}", reportId);
                return StatusCode(500, new { Error = "Failed to resolve report" });
            }
        }
    }

    public record ResolveReportRequest(
        string ResolvedBy,
        string Resolution,
        string ReporterEmail,
        string ReportSubject
    );
}
