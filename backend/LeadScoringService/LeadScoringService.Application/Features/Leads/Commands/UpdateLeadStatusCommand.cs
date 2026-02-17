using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Commands;

/// <summary>
/// Command para actualizar el estado de un lead
/// </summary>
public record UpdateLeadStatusCommand(Guid LeadId, UpdateLeadStatusDto Dto) : IRequest<LeadDto>;

public class UpdateLeadStatusCommandHandler : IRequestHandler<UpdateLeadStatusCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;

    public UpdateLeadStatusCommandHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<LeadDto> Handle(UpdateLeadStatusCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken)
            ?? throw new InvalidOperationException($"Lead {request.LeadId} not found");

        lead.Status = Enum.Parse<LeadStatus>(request.Dto.Status);
        lead.DealerNotes = request.Dto.DealerNotes ?? lead.DealerNotes;
        lead.UpdatedAt = DateTime.UtcNow;

        if (lead.Status == LeadStatus.Contacted && !lead.LastContactedAt.HasValue)
        {
            lead.LastContactedAt = DateTime.UtcNow;
        }

        if (lead.Status == LeadStatus.Converted && !lead.ConvertedAt.HasValue)
        {
            lead.ConvertedAt = DateTime.UtcNow;
        }

        lead = await _leadRepository.UpdateAsync(lead, cancellationToken);

        return MapToDto(lead);
    }

    private static LeadDto MapToDto(Lead lead)
    {
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
            new List<LeadActionDto>()
        );
    }
}
