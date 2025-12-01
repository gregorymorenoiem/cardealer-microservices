using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleService.Domain.Interfaces
{
    public interface IErrorReporter
    {
        Task<Guid> ReportErrorAsync(ErrorReport request);
    }

    public class ErrorReport
    {
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
    }
}
