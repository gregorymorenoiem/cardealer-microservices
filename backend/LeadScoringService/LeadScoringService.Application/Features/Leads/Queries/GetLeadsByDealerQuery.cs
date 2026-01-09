using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Queries;

/// <summary>
/// Query para obtener leads de un dealer con paginación y filtros
/// </summary>
public record GetLeadsByDealerQuery(
    Guid DealerId,
    int Page = 1,
    int PageSize = 20,
    string? Temperature = null,
    string? Status = null,
    string? SearchTerm = null
) : IRequest<PagedLeadsResponse>;

public class GetLeadsByDealerQueryHandler : IRequestHandler<GetLeadsByDealerQuery, PagedLeadsResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadActionRepository _actionRepository;

    public GetLeadsByDealerQueryHandler(
        ILeadRepository leadRepository,
        ILeadActionRepository actionRepository)
    {
        _leadRepository = leadRepository;
        _actionRepository = actionRepository;
    }

    public async Task<PagedLeadsResponse> Handle(GetLeadsByDealerQuery request, CancellationToken cancellationToken)
    {
        LeadTemperature? temperature = null;
        if (!string.IsNullOrEmpty(request.Temperature))
        {
            temperature = Enum.Parse<LeadTemperature>(request.Temperature);
        }

        LeadStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status))
        {
            status = Enum.Parse<LeadStatus>(request.Status);
        }

        var (leads, totalCount) = await _leadRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.DealerId,
            temperature,
            status,
            request.SearchTerm,
            cancellationToken);

        var leadDtos = new List<LeadDto>();
        foreach (var lead in leads)
        {
            var recentActions = await _actionRepository.GetRecentActionsByLeadAsync(lead.Id, 5, cancellationToken);
            leadDtos.Add(MapToDto(lead, recentActions));
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedLeadsResponse(
            leadDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }

    private static LeadDto MapToDto(Lead lead, List<LeadAction> recentActions)
    {
        var actionDtos = recentActions.Select(a => new LeadActionDto(
            a.Id,
            a.ActionType.ToString(),
            a.Description,
            a.ScoreImpact,
            a.OccurredAt
        )).ToList();

        return new LeadDto(
            lead.Id,
            lead.UserId,
            lead.UserEmail,
            lead.UserFullName,
            lead.UserPhone,
            lead.VehicleId,
            lead.VehicleTitle,
            lead.VehiclePrice,
            lead.DealerId,
            lead.DealerName,
            lead.Score,
            lead.Temperature.ToString(),
            lead.ConversionProbability,
            lead.EngagementScore,
            lead.RecencyScore,
            lead.IntentScore,
            lead.ViewCount,
            lead.ContactCount,
            lead.FavoriteCount,
            lead.ShareCount,
            lead.ComparisonCount,
            lead.HasScheduledTestDrive,
            lead.HasRequestedFinancing,
            lead.TotalTimeSpentSeconds,
            lead.Status.ToString(),
            lead.Source.ToString(),
            lead.FirstInteractionAt,
            lead.LastInteractionAt,
            lead.LastContactedAt,
            lead.ConvertedAt,
            lead.DealerNotes,
            actionDtos
        );
    }
}
