using LeadScoringService.Application.DTOs;
using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using MediatR;

namespace LeadScoringService.Application.Features.Leads.Commands;

/// <summary>
/// Command para crear o actualizar un lead
/// </summary>
public record CreateOrUpdateLeadCommand(CreateLeadDto Dto) : IRequest<LeadDto>;

public class CreateOrUpdateLeadCommandHandler : IRequestHandler<CreateOrUpdateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadScoringEngine _scoringEngine;

    public CreateOrUpdateLeadCommandHandler(
        ILeadRepository leadRepository,
        ILeadScoringEngine scoringEngine)
    {
        _leadRepository = leadRepository;
        _scoringEngine = scoringEngine;
    }

    public async Task<LeadDto> Handle(CreateOrUpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Verificar si ya existe un lead para este usuario/vehículo
        var existingLead = await _leadRepository.GetByUserAndVehicleAsync(
            dto.UserId, 
            dto.VehicleId, 
            cancellationToken);

        Lead lead;
        
        if (existingLead != null)
        {
            // Actualizar lead existente
            existingLead.LastInteractionAt = DateTime.UtcNow;
            existingLead.UpdatedAt = DateTime.UtcNow;
            lead = await _leadRepository.UpdateAsync(existingLead, cancellationToken);
        }
        else
        {
            // Crear nuevo lead
            lead = new Lead
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                UserEmail = dto.UserEmail,
                UserFullName = dto.UserFullName,
                UserPhone = dto.UserPhone,
                VehicleId = dto.VehicleId,
                VehicleTitle = dto.VehicleTitle,
                VehiclePrice = dto.VehiclePrice,
                DealerId = dto.DealerId,
                DealerName = dto.DealerName,
                Score = 0,
                Temperature = LeadTemperature.Cold,
                ConversionProbability = 0,
                Status = LeadStatus.New,
                Source = Enum.Parse<LeadSource>(dto.Source),
                FirstInteractionAt = DateTime.UtcNow,
                LastInteractionAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            lead = await _leadRepository.CreateAsync(lead, cancellationToken);
        }

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
