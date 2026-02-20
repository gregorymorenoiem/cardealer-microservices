using System;
using System.Collections.Generic;

namespace ContactService.Domain.Entities
{
    using CarDealer.Shared.MultiTenancy;
    public class ContactRequest : ITenantEntity
    {
        public ContactRequest() { }

        public ContactRequest(Guid? vehicleId, Guid buyerId, Guid sellerId, string subject, string buyerName, string buyerEmail, string message)
        {
            Id = Guid.NewGuid();
            VehicleId = vehicleId;
            BuyerId = buyerId;
            SellerId = sellerId;
            Subject = subject;
            BuyerName = buyerName;
            BuyerEmail = buyerEmail;
            Message = message;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Multi-tenant
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid? VehicleId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public Guid? ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        // Navigation
        public ICollection<ContactMessage> Messages { get; set; } = new List<ContactMessage>();
    }
}

