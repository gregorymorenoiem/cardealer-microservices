// =====================================================
// VehicleRegistryService - Interfaces
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using VehicleRegistryService.Domain.Entities;
using VehicleRegistryService.Domain.Enums;

namespace VehicleRegistryService.Domain.Interfaces;

public interface IVehicleRegistrationRepository
{
    Task<VehicleRegistration?> GetByIdAsync(Guid id);
    Task<VehicleRegistration?> GetByPlateNumberAsync(string plateNumber);
    Task<VehicleRegistration?> GetByVinAsync(string vin);
    Task<IEnumerable<VehicleRegistration>> GetByOwnerIdentificationAsync(string identification);
    Task<IEnumerable<VehicleRegistration>> GetExpiredRegistrationsAsync();
    Task<VehicleRegistration> AddAsync(VehicleRegistration registration);
    Task UpdateAsync(VehicleRegistration registration);
    Task<int> GetCountAsync();
}

public interface IOwnershipTransferRepository
{
    Task<OwnershipTransfer?> GetByIdAsync(Guid id);
    Task<IEnumerable<OwnershipTransfer>> GetByVehicleIdAsync(Guid vehicleRegistrationId);
    Task<IEnumerable<OwnershipTransfer>> GetByOwnerIdentificationAsync(string identification);
    Task<IEnumerable<OwnershipTransfer>> GetPendingTransfersAsync();
    Task<OwnershipTransfer> AddAsync(OwnershipTransfer transfer);
    Task UpdateAsync(OwnershipTransfer transfer);
}

public interface ILienRecordRepository
{
    Task<LienRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<LienRecord>> GetByVehicleIdAsync(Guid vehicleRegistrationId);
    Task<IEnumerable<LienRecord>> GetActiveByVehicleIdAsync(Guid vehicleRegistrationId);
    Task<LienRecord> AddAsync(LienRecord lien);
    Task UpdateAsync(LienRecord lien);
}

public interface IVinValidationRepository
{
    Task<VinValidation?> GetByVinAsync(string vin);
    Task<VinValidation?> GetLatestByVinAsync(string vin);
    Task<VinValidation> AddAsync(VinValidation validation);
}
