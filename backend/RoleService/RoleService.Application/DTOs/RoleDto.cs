using System;
using System.Collections.Generic;

namespace RoleService.Application.DTOs
{
    public record ErrorDto(
        Guid Id,
        string ServiceName,
        string ExceptionType,
        string Message,
        string? StackTrace,
        DateTime OccurredAt,
        string? Endpoint,
        string? HttpMethod,
        int? StatusCode,
        string? UserId,
        Dictionary<string, object> Metadata
    );
}
