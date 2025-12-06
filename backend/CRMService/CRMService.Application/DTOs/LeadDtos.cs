using CRMService.Domain.Entities;

namespace CRMService.Application.DTOs;

public record LeadDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Score { get; init; }
    public decimal? EstimatedValue { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? InterestedProductId { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ConvertedAt { get; init; }

    public static LeadDto FromEntity(Lead lead) => new()
    {
        Id = lead.Id,
        FirstName = lead.FirstName,
        LastName = lead.LastName,
        FullName = lead.FullName,
        Email = lead.Email,
        Phone = lead.Phone,
        Company = lead.Company,
        JobTitle = lead.JobTitle,
        Source = lead.Source.ToString(),
        Status = lead.Status.ToString(),
        Score = lead.Score,
        EstimatedValue = lead.EstimatedValue,
        AssignedToUserId = lead.AssignedToUserId,
        InterestedProductId = lead.InterestedProductId,
        Tags = lead.Tags,
        Notes = lead.Notes,
        CreatedAt = lead.CreatedAt,
        UpdatedAt = lead.UpdatedAt,
        ConvertedAt = lead.ConvertedAt
    };
}

public record CreateLeadRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string Source { get; init; } = "Website";
    public Guid? AssignedToUserId { get; init; }
    public Guid? InterestedProductId { get; init; }
    public decimal? EstimatedValue { get; init; }
}

public record UpdateLeadRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? Status { get; init; }
    public int? Score { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public decimal? EstimatedValue { get; init; }
}

public record ConvertLeadRequest
{
    public string DealTitle { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public Guid? PipelineId { get; init; }
    public Guid? StageId { get; init; }
}
