using System.ComponentModel.DataAnnotations;

namespace ContactService.Domain.Entities;

public class Inquiry
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid VehicleId { get; set; }
    
    [Required]
    public Guid BuyerId { get; set; }
    
    [Required]
    public Guid SellerId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? BuyerPhone { get; set; }
    
    [EmailAddress]
    public string? BuyerEmail { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, Responded, Closed
    
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    // Navigation
    public List<InquiryMessage> Messages { get; set; } = new();
    
    public Inquiry()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public Inquiry(Guid vehicleId, Guid buyerId, Guid sellerId, string subject, string message) : this()
    {
        VehicleId = vehicleId;
        BuyerId = buyerId;
        SellerId = sellerId;
        Subject = subject;
        Message = message;
        
        // Create initial message
        Messages.Add(new InquiryMessage(Id, buyerId, message, true));
    }
}