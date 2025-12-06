using CRMService.Domain.Entities;

namespace CRMService.Application.DTOs;

public record ActivityDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? DealId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? Outcome { get; init; }
    public string Priority { get; init; } = "Normal";
    public bool IsOverdue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static ActivityDto FromEntity(Activity activity) => new()
    {
        Id = activity.Id,
        Type = activity.Type.ToString(),
        Subject = activity.Subject,
        Description = activity.Description,
        IsCompleted = activity.IsCompleted,
        DueDate = activity.DueDate,
        CompletedAt = activity.CompletedAt,
        DurationMinutes = activity.DurationMinutes,
        LeadId = activity.LeadId,
        DealId = activity.DealId,
        ContactId = activity.ContactId,
        CreatedByUserId = activity.CreatedByUserId,
        AssignedToUserId = activity.AssignedToUserId,
        Outcome = activity.Outcome,
        Priority = activity.Priority.ToString(),
        IsOverdue = activity.IsOverdue,
        CreatedAt = activity.CreatedAt,
        UpdatedAt = activity.UpdatedAt
    };
}

public record CreateActivityRequest
{
    public string Type { get; init; } = "Task";
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? DealId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string Priority { get; init; } = "Normal";
}

public record UpdateActivityRequest
{
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string Priority { get; init; } = "Normal";
}

public record CompleteActivityRequest
{
    public string? Outcome { get; init; }
    public int? DurationMinutes { get; init; }
}
