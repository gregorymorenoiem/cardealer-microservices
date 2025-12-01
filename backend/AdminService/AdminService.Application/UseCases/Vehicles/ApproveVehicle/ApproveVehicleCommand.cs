using MediatR;
using System;

namespace AdminService.Application.UseCases.Vehicles.ApproveVehicle
{
    public record ApproveVehicleCommand(
        Guid VehicleId,
        string ApprovedBy,
        string Reason,
        string OwnerEmail,
        string VehicleTitle
    ) : IRequest<bool>;
}
