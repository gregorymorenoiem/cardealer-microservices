using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Queries;

/// <summary>
/// Query para obtener estadísticas de leads de un dealer
/// </summary>
public record GetLeadStatisticsQuery(Guid DealerId) : IRequest<LeadStatisticsDto>;

public class GetLeadStatisticsQueryHandler : IRequestHandler<GetLeadStatisticsQuery, LeadStatisticsDto>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadStatisticsQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<LeadStatisticsDto> Handle(GetLeadStatisticsQuery request, CancellationToken cancellationToken)
    {
        var allLeads = await _leadRepository.GetLeadsByDealerAsync(request.DealerId, cancellationToken);
        
        var totalLeads = allLeads.Count;
        var hotLeads = allLeads.Count(l => l.Temperature == Domain.Entities.LeadTemperature.Hot);
        var warmLeads = allLeads.Count(l => l.Temperature == Domain.Entities.LeadTemperature.Warm);
        var coldLeads = allLeads.Count(l => l.Temperature == Domain.Entities.LeadTemperature.Cold);
        
        var newLeads = allLeads.Count(l => l.Status == Domain.Entities.LeadStatus.New);
        var contactedLeads = allLeads.Count(l => l.Status == Domain.Entities.LeadStatus.Contacted);
        var convertedLeads = allLeads.Count(l => l.Status == Domain.Entities.LeadStatus.Converted);
        
        var averageScore = totalLeads > 0 
            ? await _leadRepository.GetAverageScoreByDealerAsync(request.DealerId, cancellationToken)
            : 0;
        
        var conversionRate = totalLeads > 0
            ? await _leadRepository.GetConversionRateByDealerAsync(request.DealerId, cancellationToken)
            : 0;

        // Calcular tendencias de los últimos 30 días
        var trends = CalculateTrends(allLeads);

        return new LeadStatisticsDto(
            request.DealerId,
            totalLeads,
            hotLeads,
            warmLeads,
            coldLeads,
            newLeads,
            contactedLeads,
            convertedLeads,
            averageScore,
            conversionRate,
            trends
        );
    }

    private static List<LeadTrendDto> CalculateTrends(List<Domain.Entities.Lead> leads)
    {
        var last30Days = Enumerable.Range(0, 30)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .OrderBy(d => d);

        var trends = new List<LeadTrendDto>();

        foreach (var date in last30Days)
        {
            var leadsOnDate = leads.Where(l => 
                l.FirstInteractionAt.Date <= date && 
                l.UpdatedAt.Date >= date).ToList();

            var avgScore = leadsOnDate.Any() 
                ? (decimal)leadsOnDate.Average(l => l.Score)
                : 0;

            trends.Add(new LeadTrendDto(date, avgScore, leadsOnDate.Count));
        }

        return trends;
    }
}
