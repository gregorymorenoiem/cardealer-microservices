// =====================================================
// VehicleRegistryService - Repositories
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using Microsoft.EntityFrameworkCore;
using VehicleRegistryService.Domain.Entities;
using VehicleRegistryService.Domain.Interfaces;
using VehicleRegistryService.Domain.Enums;
using VehicleRegistryService.Infrastructure.Persistence;

namespace VehicleRegistryService.Infrastructure.Repositories;

public class VehicleRegistrationRepository : IVehicleRegistrationRepository
{
    private readonly RegistryDbContext _context;

    public VehicleRegistrationRepository(RegistryDbContext context) => _context = context;

    public async Task<VehicleRegistration?> GetByIdAsync(Guid id)
        => await _context.Registrations.Include(r => r.Transfers).Include(r => r.Liens).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<VehicleRegistration?> GetByPlateNumberAsync(string plateNumber)
        => await _context.Registrations.Include(r => r.Transfers).FirstOrDefaultAsync(r => r.PlateNumber == plateNumber);

    public async Task<VehicleRegistration?> GetByVinAsync(string vin)
        => await _context.Registrations.Include(r => r.Transfers).FirstOrDefaultAsync(r => r.Vin == vin);

    public async Task<IEnumerable<VehicleRegistration>> GetByOwnerIdentificationAsync(string identification)
        => await _context.Registrations.Where(r => r.OwnerIdentification == identification).ToListAsync();

    public async Task<IEnumerable<VehicleRegistration>> GetExpiredRegistrationsAsync()
        => await _context.Registrations.Where(r => r.ExpirationDate < DateTime.UtcNow).ToListAsync();

    public async Task<VehicleRegistration> AddAsync(VehicleRegistration registration)
    {
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        return registration;
    }

    public async Task UpdateAsync(VehicleRegistration registration)
    {
        _context.Entry(registration).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Registrations.CountAsync();
}

public class OwnershipTransferRepository : IOwnershipTransferRepository
{
    private readonly RegistryDbContext _context;

    public OwnershipTransferRepository(RegistryDbContext context) => _context = context;

    public async Task<OwnershipTransfer?> GetByIdAsync(Guid id)
        => await _context.Transfers.FindAsync(id);

    public async Task<IEnumerable<OwnershipTransfer>> GetByVehicleIdAsync(Guid vehicleRegistrationId)
        => await _context.Transfers.Where(t => t.VehicleRegistrationId == vehicleRegistrationId).ToListAsync();

    public async Task<IEnumerable<OwnershipTransfer>> GetByOwnerIdentificationAsync(string identification)
        => await _context.Transfers.Where(t => t.PreviousOwnerIdentification == identification || t.NewOwnerIdentification == identification).ToListAsync();

    public async Task<IEnumerable<OwnershipTransfer>> GetPendingTransfersAsync()
        => await _context.Transfers.Where(t => t.Status == TransferStatus.Pending || t.Status == TransferStatus.InProcess).ToListAsync();

    public async Task<OwnershipTransfer> AddAsync(OwnershipTransfer transfer)
    {
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task UpdateAsync(OwnershipTransfer transfer)
    {
        _context.Entry(transfer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}

public class LienRecordRepository : ILienRecordRepository
{
    private readonly RegistryDbContext _context;

    public LienRecordRepository(RegistryDbContext context) => _context = context;

    public async Task<LienRecord?> GetByIdAsync(Guid id)
        => await _context.Liens.FindAsync(id);

    public async Task<IEnumerable<LienRecord>> GetByVehicleIdAsync(Guid vehicleRegistrationId)
        => await _context.Liens.Where(l => l.VehicleRegistrationId == vehicleRegistrationId).ToListAsync();

    public async Task<IEnumerable<LienRecord>> GetActiveByVehicleIdAsync(Guid vehicleRegistrationId)
        => await _context.Liens.Where(l => l.VehicleRegistrationId == vehicleRegistrationId && l.Status == LienStatus.Active).ToListAsync();

    public async Task<LienRecord> AddAsync(LienRecord lien)
    {
        _context.Liens.Add(lien);
        await _context.SaveChangesAsync();
        return lien;
    }

    public async Task UpdateAsync(LienRecord lien)
    {
        _context.Entry(lien).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}

public class VinValidationRepository : IVinValidationRepository
{
    private readonly RegistryDbContext _context;

    public VinValidationRepository(RegistryDbContext context) => _context = context;

    public async Task<VinValidation?> GetByVinAsync(string vin)
        => await _context.VinValidations.FirstOrDefaultAsync(v => v.Vin == vin);

    public async Task<VinValidation?> GetLatestByVinAsync(string vin)
        => await _context.VinValidations.Where(v => v.Vin == vin).OrderByDescending(v => v.ValidatedAt).FirstOrDefaultAsync();

    public async Task<VinValidation> AddAsync(VinValidation validation)
    {
        _context.VinValidations.Add(validation);
        await _context.SaveChangesAsync();
        return validation;
    }
}
