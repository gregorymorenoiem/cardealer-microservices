using System.Collections.Generic;

namespace RoleService.Application.DTOs
{
    public record GetErrorStatsResponse(
        int TotalErrors,
        int ErrorsLast24Hours,
        int ErrorsLast7Days,
        Dictionary<string, int> ErrorsByService,
        Dictionary<int, int> ErrorsByStatusCode
    );
}
