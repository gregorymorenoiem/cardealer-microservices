using System;

namespace ContactService.Domain.Entities
{
    using CarDealer.Shared.MultiTenancy;
    public class ContactRequest : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Multi-tenant
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
