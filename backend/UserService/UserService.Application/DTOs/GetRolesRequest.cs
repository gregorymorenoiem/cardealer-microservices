using System;

namespace UserService.Application.DTOs
{
    public record GetErrorsRequest(
        string? ServiceName,
        DateTime? From,
        DateTime? To,
        int Page,
        int PageSize
    );
}
