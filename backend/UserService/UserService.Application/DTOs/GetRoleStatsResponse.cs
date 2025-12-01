using System.Collections.Generic;

namespace UserService.Application.DTOs
{
    public record GetErrorStatsResponse(
        int TotalErrors,
        int ErrorsLast24Hours,
        int ErrorsLast7Days,
        Dictionary<string, int> ErrorsByService,
        Dictionary<int, int> ErrorsByStatusCode
    );
}
