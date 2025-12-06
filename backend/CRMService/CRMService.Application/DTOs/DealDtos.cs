using CRMService.Domain.Entities;

namespace CRMService.Application.DTOs;

public record DealDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = "MXN";
    public Guid PipelineId { get; init; }
    public string? PipelineName { get; init; }
    public Guid StageId { get; init; }
    public string? StageName { get; init; }
    public string? StageColor { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Probability { get; init; }
    public DateTime? ExpectedCloseDate { get; init; }
    public DateTime? ActualCloseDate { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? ProductId { get; init; }
    public string? VIN { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static DealDto FromEntity(Deal deal) => new()
    {
        Id = deal.Id,
        Title = deal.Title,
        Description = deal.Description,
        Value = deal.Value,
        Currency = deal.Currency,
        PipelineId = deal.PipelineId,
        PipelineName = deal.Pipeline?.Name,
        StageId = deal.StageId,
        StageName = deal.Stage?.Name,
        StageColor = deal.Stage?.Color,
        Status = deal.Status.ToString(),
        Probability = deal.Probability,
        ExpectedCloseDate = deal.ExpectedCloseDate,
        ActualCloseDate = deal.ActualCloseDate,
        LeadId = deal.LeadId,
        ContactId = deal.ContactId,
        AssignedToUserId = deal.AssignedToUserId,
        ProductId = deal.ProductId,
        VIN = deal.VIN,
        Tags = deal.Tags,
        CreatedAt = deal.CreatedAt,
        UpdatedAt = deal.UpdatedAt
    };
}

public record CreateDealRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = "MXN";
    public Guid? PipelineId { get; init; }
    public Guid? StageId { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? ProductId { get; init; }
    public string? VIN { get; init; }
    public DateTime? ExpectedCloseDate { get; init; }
}

public record UpdateDealRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = "MXN";
    public Guid? StageId { get; init; }
    public int? Probability { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? ProductId { get; init; }
    public string? VIN { get; init; }
    public DateTime? ExpectedCloseDate { get; init; }
}

public record MoveDealRequest
{
    public Guid StageId { get; init; }
    public int? Order { get; init; }
}

public record CloseDealRequest
{
    public bool IsWon { get; init; }
    public string? Notes { get; init; }
    public string? LostReason { get; init; }
}
