using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AdminService.Application.UseCases.Vehicles.ApproveVehicle;
using AdminService.Application.UseCases.Vehicles.RejectVehicle;

namespace AdminService.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IMediator mediator, ILogger<VehiclesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Approve a vehicle for publication
        /// </summary>
        [HttpPost("{vehicleId}/approve")]
        public async Task<IActionResult> ApproveVehicle(
            Guid vehicleId,
            [FromBody] ApproveVehicleRequest request)
        {
            try
            {
                var command = new ApproveVehicleCommand(
                    vehicleId,
                    request.ApprovedBy,
                    request.Reason,
                    request.OwnerEmail,
                    request.VehicleTitle
                );

                var result = await _mediator.Send(command);

                return Ok(new { Success = result, Message = "Vehicle approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving vehicle {VehicleId}", vehicleId);
                return StatusCode(500, new { Error = "Failed to approve vehicle" });
            }
        }

        /// <summary>
        /// Reject a vehicle submission
        /// </summary>
        [HttpPost("{vehicleId}/reject")]
        public async Task<IActionResult> RejectVehicle(
            Guid vehicleId,
            [FromBody] RejectVehicleRequest request)
        {
            try
            {
                var command = new RejectVehicleCommand(
                    vehicleId,
                    request.RejectedBy,
                    request.Reason,
                    request.OwnerEmail,
                    request.VehicleTitle
                );

                var result = await _mediator.Send(command);

                return Ok(new { Success = result, Message = "Vehicle rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting vehicle {VehicleId}", vehicleId);
                return StatusCode(500, new { Error = "Failed to reject vehicle" });
            }
        }
    }

    public record ApproveVehicleRequest(
        string ApprovedBy,
        string Reason,
        string OwnerEmail,
        string VehicleTitle
    );

    public record RejectVehicleRequest(
        string RejectedBy,
        string Reason,
        string OwnerEmail,
        string VehicleTitle
    );
}
