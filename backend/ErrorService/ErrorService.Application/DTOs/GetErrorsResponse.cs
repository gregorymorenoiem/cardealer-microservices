using System.Collections.Generic;

namespace ErrorService.Application.DTOs
{
    public record GetErrorsResponse(
        List<ErrorItemDto> Errors,
        int TotalCount,
        int Page,
        int PageSize
    );
}