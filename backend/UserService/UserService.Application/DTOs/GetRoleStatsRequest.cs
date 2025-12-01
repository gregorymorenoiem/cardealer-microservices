using System;

namespace UserService.Application.DTOs
{
    public record GetErrorStatsRequest(DateTime? From, DateTime? To);
}
