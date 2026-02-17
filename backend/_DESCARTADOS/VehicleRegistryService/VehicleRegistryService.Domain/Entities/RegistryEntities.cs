// =====================================================
// VehicleRegistryService - Entities
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

namespace VehicleRegistryService.Domain.Entities;

/// <summary>
/// Registro de vehículo ante INTRANT
/// </summary>
public class VehicleRegistration
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty; // 17 caracteres
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string OwnerIdentification { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerAddress { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Enums.RegistrationStatus Status { get; set; }
    public Enums.VehicleType VehicleType { get; set; }
    public decimal FiscalValue { get; set; }
    public Guid? SaleListingId { get; set; } // Referencia a listado en OKLA
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<OwnershipTransfer> Transfers { get; set; } = new List<OwnershipTransfer>();
    public ICollection<LienRecord> Liens { get; set; } = new List<LienRecord>();
}

/// <summary>
/// Transferencia de propiedad vehicular
/// </summary>
public class OwnershipTransfer
{
    public Guid Id { get; set; }
    public Guid VehicleRegistrationId { get; set; }
    public string PreviousOwnerIdentification { get; set; } = string.Empty;
    public string PreviousOwnerName { get; set; } = string.Empty;
    public string NewOwnerIdentification { get; set; } = string.Empty;
    public string NewOwnerName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public DateTime TransferDate { get; set; }
    public Enums.TransferStatus Status { get; set; }
    public string? NotaryName { get; set; }
    public string? NotaryRegistrationNumber { get; set; }
    public Guid? SaleTransactionId { get; set; } // Referencia a venta en OKLA
    public DateTime CreatedAt { get; set; }
    
    public VehicleRegistration? VehicleRegistration { get; set; }
}

/// <summary>
/// Gravámenes/Prendas sobre el vehículo
/// </summary>
public class LienRecord
{
    public Guid Id { get; set; }
    public Guid VehicleRegistrationId { get; set; }
    public string LienHolderName { get; set; } = string.Empty;
    public string LienHolderRnc { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public Enums.LienStatus Status { get; set; }
    public string? ReleaseReason { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public VehicleRegistration? VehicleRegistration { get; set; }
}

/// <summary>
/// Validación de VIN en listas internacionales
/// </summary>
public class VinValidation
{
    public Guid Id { get; set; }
    public string Vin { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool IsStolen { get; set; }
    public bool IsSalvage { get; set; }
    public bool HasRecall { get; set; }
    public string? ValidationSource { get; set; }
    public string? ValidationDetails { get; set; }
    public DateTime ValidatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
