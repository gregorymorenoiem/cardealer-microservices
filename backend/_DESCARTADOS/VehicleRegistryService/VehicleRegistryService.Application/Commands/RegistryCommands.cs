// =====================================================
// VehicleRegistryService - Commands
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using MediatR;
using VehicleRegistryService.Application.DTOs;

namespace VehicleRegistryService.Application.Commands;

// ==================== Registros ====================
public record CreateRegistrationCommand(CreateRegistrationDto Data) : IRequest<VehicleRegistrationDto>;
public record UpdateRegistrationCommand(Guid Id, CreateRegistrationDto Data) : IRequest<bool>;
public record RenewRegistrationCommand(Guid Id) : IRequest<VehicleRegistrationDto>;
public record SuspendRegistrationCommand(Guid Id, string Reason) : IRequest<bool>;
public record CancelRegistrationCommand(Guid Id, string Reason) : IRequest<bool>;

// ==================== Transferencias ====================
public record CreateTransferCommand(CreateTransferDto Data) : IRequest<OwnershipTransferDto>;
public record CompleteTransferCommand(Guid TransferId, CompleteTransferDto Data) : IRequest<bool>;
public record RejectTransferCommand(Guid TransferId, string Reason) : IRequest<bool>;
public record CancelTransferCommand(Guid TransferId, string Reason) : IRequest<bool>;

// ==================== Gravámenes ====================
public record CreateLienCommand(CreateLienDto Data) : IRequest<LienRecordDto>;
public record ReleaseLienCommand(Guid LienId, ReleaseLienDto Data) : IRequest<bool>;

// ==================== Validación VIN ====================
public record ValidateVinCommand(ValidateVinDto Data) : IRequest<VinValidationDto>;
