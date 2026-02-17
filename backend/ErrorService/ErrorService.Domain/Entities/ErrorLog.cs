using System;
using System.Collections.Generic;

namespace ErrorService.Domain.Entities
{
    public class ErrorLog
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = null!;
        public string ExceptionType { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? StackTrace { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? UserId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // ✅ AUDIT FIX: Audit timestamp (when the log record was created in DB)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}