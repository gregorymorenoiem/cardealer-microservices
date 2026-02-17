using System.ComponentModel.DataAnnotations;

namespace ContactService.Domain.Entities;

public class InquiryMessage
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid InquiryId { get; set; }
    
    [Required]
    public Guid SenderId { get; set; }
    
    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    public bool IsFromBuyer { get; set; }
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Inquiry? Inquiry { get; set; }
    
    public InquiryMessage()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public InquiryMessage(Guid inquiryId, Guid senderId, string message, bool isFromBuyer) : this()
    {
        InquiryId = inquiryId;
        SenderId = senderId;
        Message = message;
        IsFromBuyer = isFromBuyer;
    }
}