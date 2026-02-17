using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Commands;

/// <summary>
/// Command para registrar una acción de un lead y recalcular su score
/// </summary>
public record RecordLeadActionCommand(RecordLeadActionDto Dto) : IRequest<LeadDto>;

public class RecordLeadActionCommandHandler : IRequestHandler<RecordLeadActionCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadActionRepository _actionRepository;
    private readonly ILeadScoringEngine _scoringEngine;

    public RecordLeadActionCommandHandler(
        ILeadRepository leadRepository,
        ILeadActionRepository actionRepository,
        ILeadScoringEngine scoringEngine)
    {
        _leadRepository = leadRepository;
        _actionRepository = actionRepository;
        _scoringEngine = scoringEngine;
    }

    public async Task<LeadDto> Handle(RecordLeadActionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Obtener lead
        var lead = await _leadRepository.GetByIdAsync(dto.LeadId, cancellationToken)
            ?? throw new InvalidOperationException($"Lead {dto.LeadId} not found");

        // Parse action type
        var actionType = Enum.Parse<LeadActionType>(dto.ActionType);
        
        // Calcular impacto en score basado en tipo de acción
        var scoreImpact = GetScoreImpact(actionType);
        
        // Crear acción
        var action = new LeadAction
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActionType = actionType,
            Description = GetActionDescription(actionType),
            Metadata = dto.Metadata,
            ScoreImpact = scoreImpact,
            OccurredAt = DateTime.UtcNow,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent
        };

        await _actionRepository.CreateAsync(action, cancellationToken);

        // Actualizar contadores en el lead
        UpdateLeadCounters(lead, actionType);
        
        // Actualizar última interacción
        lead.LastInteractionAt = DateTime.UtcNow;

        // Recalcular score
        var previousScore = lead.Score;
        var previousTemperature = lead.Temperature;
        
        lead.EngagementScore = _scoringEngine.CalculateEngagementScore(lead);
        lead.RecencyScore = _scoringEngine.CalculateRecencyScore(lead);
        lead.IntentScore = _scoringEngine.CalculateIntentScore(lead);
        lead.Score = lead.EngagementScore + lead.RecencyScore + lead.IntentScore;
        lead.Temperature = _scoringEngine.DetermineTemperature(lead.Score);
        lead.ConversionProbability = _scoringEngine.CalculateConversionProbability(lead);
        lead.UpdatedAt = DateTime.UtcNow;

        // Guardar cambios
        lead = await _leadRepository.UpdateAsync(lead, cancellationToken);

        // Guardar historial si hubo cambio significativo
        if (Math.Abs(lead.Score - previousScore) >= 5 || lead.Temperature != previousTemperature)
        {
            var history = new LeadScoreHistory
            {
                Id = Guid.NewGuid(),
                LeadId = lead.Id,
                PreviousScore = previousScore,
                NewScore = lead.Score,
                ScoreDelta = lead.Score - previousScore,
                PreviousTemperature = previousTemperature,
                NewTemperature = lead.Temperature,
                Reason = $"Action: {actionType}",
                TriggeringActionId = action.Id,
                ChangedAt = DateTime.UtcNow
            };
            
            lead.ScoreHistory.Add(history);
        }

        return MapToDto(lead, action);
    }

    private static int GetScoreImpact(LeadActionType actionType)
    {
        return actionType switch
        {
            LeadActionType.ViewListing => 2,
            LeadActionType.ViewImages => 1,
            LeadActionType.ViewSellerProfile => 3,
            LeadActionType.ClickPhone => 5,
            LeadActionType.ClickEmail => 5,
            LeadActionType.ClickWhatsApp => 7,
            LeadActionType.SendMessage => 10,
            LeadActionType.AddToFavorites => 8,
            LeadActionType.RemoveFromFavorites => -5,
            LeadActionType.ShareListing => 6,
            LeadActionType.AddToComparison => 10,
            LeadActionType.ScheduleTestDrive => 20,
            LeadActionType.RequestFinancing => 25,
            LeadActionType.MakeOffer => 30,
            LeadActionType.PurchaseCompleted => 100,
            LeadActionType.ReportListing => -10,
            LeadActionType.BlockSeller => -50,
            _ => 0
        };
    }

    private static string GetActionDescription(LeadActionType actionType)
    {
        return actionType switch
        {
            LeadActionType.ViewListing => "Viewed listing",
            LeadActionType.ViewImages => "Viewed image gallery",
            LeadActionType.ViewSellerProfile => "Viewed seller profile",
            LeadActionType.ClickPhone => "Clicked phone number",
            LeadActionType.ClickEmail => "Clicked email address",
            LeadActionType.ClickWhatsApp => "Clicked WhatsApp button",
            LeadActionType.SendMessage => "Sent message to seller",
            LeadActionType.AddToFavorites => "Added to favorites",
            LeadActionType.RemoveFromFavorites => "Removed from favorites",
            LeadActionType.ShareListing => "Shared listing",
            LeadActionType.AddToComparison => "Added to comparison",
            LeadActionType.ScheduleTestDrive => "Scheduled test drive",
            LeadActionType.RequestFinancing => "Requested financing",
            LeadActionType.MakeOffer => "Made an offer",
            LeadActionType.PurchaseCompleted => "Purchase completed",
            LeadActionType.ReportListing => "Reported listing",
            LeadActionType.BlockSeller => "Blocked seller",
            _ => "Unknown action"
        };
    }

    private static void UpdateLeadCounters(Lead lead, LeadActionType actionType)
    {
        switch (actionType)
        {
            case LeadActionType.ViewListing:
            case LeadActionType.ViewImages:
            case LeadActionType.ViewSellerProfile:
                lead.ViewCount++;
                break;
            
            case LeadActionType.ClickPhone:
            case LeadActionType.ClickEmail:
            case LeadActionType.ClickWhatsApp:
            case LeadActionType.SendMessage:
                lead.ContactCount++;
                break;
            
            case LeadActionType.AddToFavorites:
                lead.FavoriteCount++;
                break;
            
            case LeadActionType.RemoveFromFavorites:
                lead.FavoriteCount = Math.Max(0, lead.FavoriteCount - 1);
                break;
            
            case LeadActionType.ShareListing:
                lead.ShareCount++;
                break;
            
            case LeadActionType.AddToComparison:
                lead.ComparisonCount++;
                break;
            
            case LeadActionType.ScheduleTestDrive:
                lead.HasScheduledTestDrive = true;
                break;
            
            case LeadActionType.RequestFinancing:
                lead.HasRequestedFinancing = true;
                break;
            
            case LeadActionType.PurchaseCompleted:
                lead.Status = LeadStatus.Converted;
                lead.ConvertedAt = DateTime.UtcNow;
                break;
        }
    }

    private static LeadDto MapToDto(Lead lead, LeadAction? latestAction = null)
    {
        var actions = latestAction != null 
            ? new List<LeadActionDto> 
            { 
                new LeadActionDto(
                    latestAction.Id,
                    latestAction.ActionType.ToString(),
                    latestAction.Description,
                    latestAction.ScoreImpact,
                    latestAction.OccurredAt
                )
            }
            : new List<LeadActionDto>();

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
            actions
        );
    }
}
