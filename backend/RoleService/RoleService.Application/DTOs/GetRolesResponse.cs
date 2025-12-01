using System.Collections.Generic;

namespace RoleService.Application.DTOs
{
    public record GetErrorsResponse(
        List<ErrorItemDto> Errors,
        int TotalCount,
        int Page,
        int PageSize
    );
}
