using System;

namespace RoleService.Application.DTOs
{
    public record GetErrorStatsRequest(DateTime? From, DateTime? To);
}
