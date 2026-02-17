// =====================================================
// VehicleRegistryService - DTOs
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using VehicleRegistryService.Domain.Enums;

namespace VehicleRegistryService.Application.DTOs;

// ==================== Registro ====================
public record VehicleRegistrationDto(
    Guid Id,
    string PlateNumber,
    string Vin,
    string Brand,
    string Model,
    int Year,
    string Color,
    string OwnerIdentification,
    string OwnerName,
    DateTime RegistrationDate,
    DateTime ExpirationDate,
    RegistrationStatus Status,
    VehicleType VehicleType,
    bool HasActiveLiens,
    int TransferCount
);

public record CreateRegistrationDto(
    string PlateNumber,
    string Vin,
    string Brand,
    string Model,
    int Year,
    string Color,
    string EngineNumber,
    string ChassisNumber,
    string OwnerIdentification,
    string OwnerName,
    string? OwnerAddress,
    VehicleType VehicleType,
    decimal FiscalValue
);

// ==================== Transferencias ====================
public record OwnershipTransferDto(
    Guid Id,
    Guid VehicleRegistrationId,
    string PlateNumber,
    string PreviousOwnerName,
    string NewOwnerName,
    decimal SalePrice,
    DateTime TransferDate,
    TransferStatus Status
);

public record CreateTransferDto(
    Guid VehicleRegistrationId,
    string NewOwnerIdentification,
    string NewOwnerName,
    decimal SalePrice,
    string? NotaryName,
    string? NotaryRegistrationNumber,
    Guid? SaleTransactionId
);

public record CompleteTransferDto(
    string NotaryName,
    string NotaryRegistrationNumber
);

// ==================== Gravámenes ====================
public record LienRecordDto(
    Guid Id,
    Guid VehicleRegistrationId,
    string LienHolderName,
    string LienHolderRnc,
    decimal Amount,
    DateTime RecordedAt,
    DateTime? ReleasedAt,
    LienStatus Status
);

public record CreateLienDto(
    Guid VehicleRegistrationId,
    string LienHolderName,
    string LienHolderRnc,
    decimal Amount
);

public record ReleaseLienDto(
    string ReleaseReason
);

// ==================== Validación VIN ====================
public record VinValidationDto(
    string Vin,
    bool IsValid,
    bool IsStolen,
    bool IsSalvage,
    bool HasRecall,
    string? ValidationSource,
    DateTime ValidatedAt
);

public record ValidateVinDto(
    string Vin
);

// ==================== Estadísticas ====================
public record RegistryStatisticsDto(
    int TotalRegistrations,
    int ActiveRegistrations,
    int ExpiredRegistrations,
    int PendingTransfers,
    int ActiveLiens
);
