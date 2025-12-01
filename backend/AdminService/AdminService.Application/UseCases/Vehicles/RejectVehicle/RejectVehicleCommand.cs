using MediatR;
using System;

namespace AdminService.Application.UseCases.Vehicles.RejectVehicle
{
    public record RejectVehicleCommand(
        Guid VehicleId,
        string RejectedBy,
        string Reason,
        string OwnerEmail,
        string VehicleTitle
    ) : IRequest<bool>;
}
