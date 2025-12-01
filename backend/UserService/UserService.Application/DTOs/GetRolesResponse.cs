using System.Collections.Generic;

namespace UserService.Application.DTOs
{
    public record GetErrorsResponse(
        List<ErrorItemDto> Errors,
        int TotalCount,
        int Page,
        int PageSize
    );
}
