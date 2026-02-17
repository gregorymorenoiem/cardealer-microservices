// =====================================================
// VehicleRegistryService - Queries
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using MediatR;
using VehicleRegistryService.Application.DTOs;

namespace VehicleRegistryService.Application.Queries;

// ==================== Registros ====================
public record GetRegistrationByIdQuery(Guid Id) : IRequest<VehicleRegistrationDto?>;
public record GetRegistrationByPlateQuery(string PlateNumber) : IRequest<VehicleRegistrationDto?>;
public record GetRegistrationByVinQuery(string Vin) : IRequest<VehicleRegistrationDto?>;
public record GetRegistrationsByOwnerQuery(string OwnerIdentification) : IRequest<IEnumerable<VehicleRegistrationDto>>;
public record GetExpiredRegistrationsQuery() : IRequest<IEnumerable<VehicleRegistrationDto>>;

// ==================== Transferencias ====================
public record GetTransferByIdQuery(Guid Id) : IRequest<OwnershipTransferDto?>;
public record GetTransfersByVehicleQuery(Guid VehicleRegistrationId) : IRequest<IEnumerable<OwnershipTransferDto>>;
public record GetTransfersByOwnerQuery(string OwnerIdentification) : IRequest<IEnumerable<OwnershipTransferDto>>;
public record GetPendingTransfersQuery() : IRequest<IEnumerable<OwnershipTransferDto>>;

// ==================== Gravámenes ====================
public record GetLiensByVehicleQuery(Guid VehicleRegistrationId) : IRequest<IEnumerable<LienRecordDto>>;
public record GetActiveLiensByVehicleQuery(Guid VehicleRegistrationId) : IRequest<IEnumerable<LienRecordDto>>;
public record CheckVehicleHasLiensQuery(Guid VehicleRegistrationId) : IRequest<bool>;

// ==================== Validación VIN ====================
public record GetVinValidationQuery(string Vin) : IRequest<VinValidationDto?>;

// ==================== Estadísticas ====================
public record GetRegistryStatisticsQuery() : IRequest<RegistryStatisticsDto>;
