// =====================================================
// VehicleRegistryService - Handlers
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using MediatR;
using VehicleRegistryService.Application.Commands;
using VehicleRegistryService.Application.Queries;
using VehicleRegistryService.Application.DTOs;
using VehicleRegistryService.Domain.Entities;
using VehicleRegistryService.Domain.Interfaces;
using VehicleRegistryService.Domain.Enums;

namespace VehicleRegistryService.Application.Handlers;

// ==================== Registration Handlers ====================

public class CreateRegistrationHandler : IRequestHandler<CreateRegistrationCommand, VehicleRegistrationDto>
{
    private readonly IVehicleRegistrationRepository _repository;

    public CreateRegistrationHandler(IVehicleRegistrationRepository repository) => _repository = repository;

    public async Task<VehicleRegistrationDto> Handle(CreateRegistrationCommand request, CancellationToken ct)
    {
        var registration = new VehicleRegistration
        {
            Id = Guid.NewGuid(),
            PlateNumber = request.Data.PlateNumber.ToUpperInvariant(),
            Vin = request.Data.Vin.ToUpperInvariant(),
            Brand = request.Data.Brand,
            Model = request.Data.Model,
            Year = request.Data.Year,
            Color = request.Data.Color,
            EngineNumber = request.Data.EngineNumber,
            ChassisNumber = request.Data.ChassisNumber,
            OwnerIdentification = request.Data.OwnerIdentification,
            OwnerName = request.Data.OwnerName,
            OwnerAddress = request.Data.OwnerAddress,
            RegistrationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1), // Registro anual
            Status = RegistrationStatus.Active,
            VehicleType = request.Data.VehicleType,
            FiscalValue = request.Data.FiscalValue,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(registration);

        return new VehicleRegistrationDto(
            registration.Id, registration.PlateNumber, registration.Vin, registration.Brand,
            registration.Model, registration.Year, registration.Color, registration.OwnerIdentification,
            registration.OwnerName, registration.RegistrationDate, registration.ExpirationDate,
            registration.Status, registration.VehicleType, false, 0
        );
    }
}

public class GetRegistrationByPlateHandler : IRequestHandler<GetRegistrationByPlateQuery, VehicleRegistrationDto?>
{
    private readonly IVehicleRegistrationRepository _regRepo;
    private readonly ILienRecordRepository _lienRepo;

    public GetRegistrationByPlateHandler(IVehicleRegistrationRepository regRepo, ILienRecordRepository lienRepo)
    {
        _regRepo = regRepo;
        _lienRepo = lienRepo;
    }

    public async Task<VehicleRegistrationDto?> Handle(GetRegistrationByPlateQuery request, CancellationToken ct)
    {
        var reg = await _regRepo.GetByPlateNumberAsync(request.PlateNumber.ToUpperInvariant());
        if (reg == null) return null;

        var liens = await _lienRepo.GetActiveByVehicleIdAsync(reg.Id);

        return new VehicleRegistrationDto(
            reg.Id, reg.PlateNumber, reg.Vin, reg.Brand, reg.Model, reg.Year, reg.Color,
            reg.OwnerIdentification, reg.OwnerName, reg.RegistrationDate, reg.ExpirationDate,
            reg.Status, reg.VehicleType, liens.Any(), reg.Transfers?.Count ?? 0
        );
    }
}

public class GetRegistrationByVinHandler : IRequestHandler<GetRegistrationByVinQuery, VehicleRegistrationDto?>
{
    private readonly IVehicleRegistrationRepository _regRepo;
    private readonly ILienRecordRepository _lienRepo;

    public GetRegistrationByVinHandler(IVehicleRegistrationRepository regRepo, ILienRecordRepository lienRepo)
    {
        _regRepo = regRepo;
        _lienRepo = lienRepo;
    }

    public async Task<VehicleRegistrationDto?> Handle(GetRegistrationByVinQuery request, CancellationToken ct)
    {
        var reg = await _regRepo.GetByVinAsync(request.Vin.ToUpperInvariant());
        if (reg == null) return null;

        var liens = await _lienRepo.GetActiveByVehicleIdAsync(reg.Id);

        return new VehicleRegistrationDto(
            reg.Id, reg.PlateNumber, reg.Vin, reg.Brand, reg.Model, reg.Year, reg.Color,
            reg.OwnerIdentification, reg.OwnerName, reg.RegistrationDate, reg.ExpirationDate,
            reg.Status, reg.VehicleType, liens.Any(), reg.Transfers?.Count ?? 0
        );
    }
}

public class RenewRegistrationHandler : IRequestHandler<RenewRegistrationCommand, VehicleRegistrationDto>
{
    private readonly IVehicleRegistrationRepository _repository;

    public RenewRegistrationHandler(IVehicleRegistrationRepository repository) => _repository = repository;

    public async Task<VehicleRegistrationDto> Handle(RenewRegistrationCommand request, CancellationToken ct)
    {
        var reg = await _repository.GetByIdAsync(request.Id);
        if (reg == null) throw new KeyNotFoundException("Registro no encontrado");

        reg.ExpirationDate = DateTime.UtcNow.AddYears(1);
        reg.Status = RegistrationStatus.Active;
        reg.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(reg);

        return new VehicleRegistrationDto(
            reg.Id, reg.PlateNumber, reg.Vin, reg.Brand, reg.Model, reg.Year, reg.Color,
            reg.OwnerIdentification, reg.OwnerName, reg.RegistrationDate, reg.ExpirationDate,
            reg.Status, reg.VehicleType, false, reg.Transfers?.Count ?? 0
        );
    }
}

// ==================== Transfer Handlers ====================

public class CreateTransferHandler : IRequestHandler<CreateTransferCommand, OwnershipTransferDto>
{
    private readonly IOwnershipTransferRepository _transferRepo;
    private readonly IVehicleRegistrationRepository _regRepo;
    private readonly ILienRecordRepository _lienRepo;

    public CreateTransferHandler(
        IOwnershipTransferRepository transferRepo,
        IVehicleRegistrationRepository regRepo,
        ILienRecordRepository lienRepo)
    {
        _transferRepo = transferRepo;
        _regRepo = regRepo;
        _lienRepo = lienRepo;
    }

    public async Task<OwnershipTransferDto> Handle(CreateTransferCommand request, CancellationToken ct)
    {
        var reg = await _regRepo.GetByIdAsync(request.Data.VehicleRegistrationId);
        if (reg == null) throw new KeyNotFoundException("Registro vehicular no encontrado");

        // Verificar que no hay gravámenes activos
        var liens = await _lienRepo.GetActiveByVehicleIdAsync(reg.Id);
        if (liens.Any())
            throw new InvalidOperationException("No se puede transferir un vehículo con gravámenes activos");

        var transfer = new OwnershipTransfer
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = reg.Id,
            PreviousOwnerIdentification = reg.OwnerIdentification,
            PreviousOwnerName = reg.OwnerName,
            NewOwnerIdentification = request.Data.NewOwnerIdentification,
            NewOwnerName = request.Data.NewOwnerName,
            SalePrice = request.Data.SalePrice,
            TransferDate = DateTime.UtcNow,
            Status = TransferStatus.Pending,
            NotaryName = request.Data.NotaryName,
            NotaryRegistrationNumber = request.Data.NotaryRegistrationNumber,
            SaleTransactionId = request.Data.SaleTransactionId,
            CreatedAt = DateTime.UtcNow
        };

        await _transferRepo.AddAsync(transfer);

        return new OwnershipTransferDto(
            transfer.Id, transfer.VehicleRegistrationId, reg.PlateNumber,
            transfer.PreviousOwnerName, transfer.NewOwnerName, transfer.SalePrice,
            transfer.TransferDate, transfer.Status
        );
    }
}

public class CompleteTransferHandler : IRequestHandler<CompleteTransferCommand, bool>
{
    private readonly IOwnershipTransferRepository _transferRepo;
    private readonly IVehicleRegistrationRepository _regRepo;

    public CompleteTransferHandler(IOwnershipTransferRepository transferRepo, IVehicleRegistrationRepository regRepo)
    {
        _transferRepo = transferRepo;
        _regRepo = regRepo;
    }

    public async Task<bool> Handle(CompleteTransferCommand request, CancellationToken ct)
    {
        var transfer = await _transferRepo.GetByIdAsync(request.TransferId);
        if (transfer == null) return false;

        var reg = await _regRepo.GetByIdAsync(transfer.VehicleRegistrationId);
        if (reg == null) return false;

        // Actualizar propietario en registro
        reg.OwnerIdentification = transfer.NewOwnerIdentification;
        reg.OwnerName = transfer.NewOwnerName;
        reg.UpdatedAt = DateTime.UtcNow;
        await _regRepo.UpdateAsync(reg);

        // Completar transferencia
        transfer.Status = TransferStatus.Completed;
        transfer.NotaryName = request.Data.NotaryName;
        transfer.NotaryRegistrationNumber = request.Data.NotaryRegistrationNumber;
        await _transferRepo.UpdateAsync(transfer);

        return true;
    }
}

// ==================== VIN Validation Handler ====================

public class ValidateVinHandler : IRequestHandler<ValidateVinCommand, VinValidationDto>
{
    private readonly IVinValidationRepository _repository;

    public ValidateVinHandler(IVinValidationRepository repository) => _repository = repository;

    public async Task<VinValidationDto> Handle(ValidateVinCommand request, CancellationToken ct)
    {
        var vin = request.Data.Vin.ToUpperInvariant();
        
        // Validación básica de formato VIN
        var isValid = vin.Length == 17 && vin.All(c => char.IsLetterOrDigit(c));

        var validation = new VinValidation
        {
            Id = Guid.NewGuid(),
            Vin = vin,
            IsValid = isValid,
            IsStolen = false, // En producción, consultar base de datos internacional
            IsSalvage = false,
            HasRecall = false,
            ValidationSource = "OKLA Internal Validation",
            ValidatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(validation);

        return new VinValidationDto(
            validation.Vin, validation.IsValid, validation.IsStolen,
            validation.IsSalvage, validation.HasRecall, validation.ValidationSource,
            validation.ValidatedAt
        );
    }
}

// ==================== Statistics Handler ====================

public class GetRegistryStatisticsHandler : IRequestHandler<GetRegistryStatisticsQuery, RegistryStatisticsDto>
{
    private readonly IVehicleRegistrationRepository _regRepo;
    private readonly IOwnershipTransferRepository _transferRepo;

    public GetRegistryStatisticsHandler(IVehicleRegistrationRepository regRepo, IOwnershipTransferRepository transferRepo)
    {
        _regRepo = regRepo;
        _transferRepo = transferRepo;
    }

    public async Task<RegistryStatisticsDto> Handle(GetRegistryStatisticsQuery request, CancellationToken ct)
    {
        var total = await _regRepo.GetCountAsync();
        var expired = await _regRepo.GetExpiredRegistrationsAsync();
        var pendingTransfers = await _transferRepo.GetPendingTransfersAsync();

        return new RegistryStatisticsDto(
            TotalRegistrations: total,
            ActiveRegistrations: total - expired.Count(),
            ExpiredRegistrations: expired.Count(),
            PendingTransfers: pendingTransfers.Count(),
            ActiveLiens: 0 // Simplificado
        );
    }
}
