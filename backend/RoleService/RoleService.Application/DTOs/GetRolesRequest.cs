using System;

namespace RoleService.Application.DTOs
{
    public record GetErrorsRequest(
        string? ServiceName,
        DateTime? From,
        DateTime? To,
        int Page,
        int PageSize
    );
}
