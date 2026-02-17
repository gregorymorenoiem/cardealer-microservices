namespace UserService.Application.DTOs;

/// <summary>
/// DTO for dealer module subscription information
/// </summary>
public class DealerModuleDto
{
    public string ModuleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for module details (available modules to subscribe)
/// </summary>
public class ModuleDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for dealer onboarding process status
/// </summary>
public class DealerOnboardingDto
{
    public Guid DealerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public List<string> StepsCompleted { get; set; } = new();
    public int Progress { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
