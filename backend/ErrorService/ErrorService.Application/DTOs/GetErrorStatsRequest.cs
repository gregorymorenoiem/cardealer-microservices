using System;

namespace ErrorService.Application.DTOs
{
    public record GetErrorStatsRequest(DateTime? From, DateTime? To);
}