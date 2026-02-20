using System;
using System.Collections.Generic;

namespace ContactService.Domain.Entities
{
    using CarDealer.Shared.MultiTenancy;
    public class ContactRequest : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Multi-tenant
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid? VehicleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public Guid? ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ContactMessage> Messages { get; set; } = new List<ContactMessage>();
    }
}
