using CRMService.Domain.Entities;

namespace CRMService.Application.DTOs;

public record PipelineDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public List<StageDto> Stages { get; init; } = new();
    public DateTime CreatedAt { get; init; }

    public static PipelineDto FromEntity(Pipeline pipeline) => new()
    {
        Id = pipeline.Id,
        Name = pipeline.Name,
        Description = pipeline.Description,
        IsDefault = pipeline.IsDefault,
        IsActive = pipeline.IsActive,
        Stages = pipeline.Stages.OrderBy(s => s.Order).Select(StageDto.FromEntity).ToList(),
        CreatedAt = pipeline.CreatedAt
    };
}

public record StageDto
{
    public Guid Id { get; init; }
    public Guid PipelineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public int DefaultProbability { get; init; }
    public bool IsWonStage { get; init; }
    public bool IsLostStage { get; init; }
    public int DealsCount { get; init; }
    public decimal TotalValue { get; init; }

    public static StageDto FromEntity(Stage stage) => new()
    {
        Id = stage.Id,
        PipelineId = stage.PipelineId,
        Name = stage.Name,
        Order = stage.Order,
        Color = stage.Color,
        DefaultProbability = stage.DefaultProbability,
        IsWonStage = stage.IsWonStage,
        IsLostStage = stage.IsLostStage,
        DealsCount = stage.Deals.Count,
        TotalValue = stage.Deals.Sum(d => d.Value)
    };
}

public record CreatePipelineRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public List<CreateStageRequest>? Stages { get; init; }
}

public record UpdatePipelineRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
}

public record CreateStageRequest
{
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public int? DefaultProbability { get; init; }
}

public record UpdateStageRequest
{
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public int? DefaultProbability { get; init; }
}
