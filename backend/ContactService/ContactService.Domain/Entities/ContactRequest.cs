using System;
using System.ComponentModel.DataAnnotations;

namespace ContactService.Domain.Entities
{
    using CarDealer.Shared.MultiTenancy;
    public class ContactRequest : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Multi-tenant
        
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
        [StringLength(100)]
        public string BuyerName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string BuyerEmail { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? BuyerPhone { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Responded, Closed
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }  // ✅ AUDIT FIX: Track status changes
        public DateTime? RespondedAt { get; set; }
        
        // Navigation property
        public List<ContactMessage> Messages { get; set; } = new();
        
        public ContactRequest()
        {
            Id = Guid.NewGuid();
        }
        
        public ContactRequest(Guid vehicleId, Guid buyerId, Guid sellerId, string subject, 
                            string buyerName, string buyerEmail, string message) : this()
        {
            VehicleId = vehicleId;
            BuyerId = buyerId;
            SellerId = sellerId;
            Subject = subject;
            BuyerName = buyerName;
            BuyerEmail = buyerEmail;
            Message = message;
        }
    }
}
