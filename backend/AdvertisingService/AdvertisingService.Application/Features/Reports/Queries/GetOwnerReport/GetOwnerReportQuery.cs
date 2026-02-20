using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.Reports.Queries.GetOwnerReport;

public record GetOwnerReportQuery(
    Guid OwnerId,
    string OwnerType = "Individual",
    int DaysBack = 30
) : IRequest<OwnerReportDto?>;
