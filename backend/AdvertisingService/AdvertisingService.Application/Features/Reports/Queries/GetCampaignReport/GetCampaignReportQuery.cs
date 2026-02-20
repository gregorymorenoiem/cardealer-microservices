using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.Reports.Queries.GetCampaignReport;

public record GetCampaignReportQuery(Guid CampaignId, int DaysBack = 30) : IRequest<CampaignReportDto?>;
