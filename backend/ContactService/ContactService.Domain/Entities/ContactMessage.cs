using System;

namespace ContactService.Domain.Entities
{
    using CarDealer.Shared.MultiTenancy;
    public class ContactMessage : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Multi-tenant
        public Guid ContactRequestId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
