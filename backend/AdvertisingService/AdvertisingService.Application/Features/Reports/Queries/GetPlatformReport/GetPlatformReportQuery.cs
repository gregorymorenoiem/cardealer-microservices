using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.Reports.Queries.GetPlatformReport;

public record GetPlatformReportQuery(int DaysBack = 30) : IRequest<PlatformReportDto>;
