namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Lead generado cuando un comprador contacta al vendedor de un vehículo.
/// Almacena la conversación comprador ↔ vendedor.
/// </summary>
public class Lead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; } // Owner of the vehicle
    public Guid? DealerId { get; set; }

    // Buyer info
    public Guid? BuyerId { get; set; } // If authenticated
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerPhone { get; set; }

    // Initial message
    public string Message { get; set; } = string.Empty;

    // Vehicle snapshot (denormalized for quick display)
    public string VehicleTitle { get; set; } = string.Empty;
    public decimal? VehiclePrice { get; set; }
    public string? VehicleImageUrl { get; set; }

    // Status
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadSource Source { get; set; } = LeadSource.ContactForm;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ContactedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Audit
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation
    public Vehicle? Vehicle { get; set; }
    public ICollection<LeadMessage> Messages { get; set; } = new List<LeadMessage>();
}

/// <summary>
/// Mensaje individual en la conversación de un lead.
/// </summary>
public class LeadMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LeadId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public MessageSenderRole SenderRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public Lead? Lead { get; set; }
}

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Negotiating = 2,
    Closed = 3,
    Lost = 4,
    Spam = 5
}

public enum LeadSource
{
    ContactForm = 0,
    WhatsApp = 1,
    Phone = 2,
    Email = 3,
    Chat = 4,
    External = 5
}

public enum MessageSenderRole
{
    Buyer = 0,
    Seller = 1,
    System = 2
}
