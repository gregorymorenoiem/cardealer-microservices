using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Queries;

/// <summary>
/// Query para obtener un lead por ID con todo su historial
/// </summary>
public record GetLeadByIdQuery(Guid LeadId) : IRequest<LeadDto>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadActionRepository _actionRepository;

    public GetLeadByIdQueryHandler(
        ILeadRepository leadRepository,
        ILeadActionRepository actionRepository)
    {
        _leadRepository = leadRepository;
        _actionRepository = actionRepository;
    }

    public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken)
            ?? throw new InvalidOperationException($"Lead {request.LeadId} not found");

        var allActions = await _actionRepository.GetByLeadIdAsync(lead.Id, cancellationToken);
        
        return MapToDto(lead, allActions);
    }

    private static LeadDto MapToDto(Lead lead, List<LeadAction> actions)
    {
        var actionDtos = actions
            .OrderByDescending(a => a.OccurredAt)
            .Select(a => new LeadActionDto(
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
