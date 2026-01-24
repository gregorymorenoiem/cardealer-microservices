using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

/// <summary>
/// Module (addon/feature) available for dealers
/// </summary>
public class Module
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public List<string>? Features { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dealer subscription to a module
/// </summary>
public class DealerModule
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid ModuleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public Module Module { get; set; } = null!;
}

/// <summary>
/// Dealer onboarding process tracking
/// </summary>
public class DealerOnboardingProcess
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public OnboardingStatus Status { get; set; } = OnboardingStatus.InProgress;
    public DealerOnboardingStep CurrentStep { get; set; } = DealerOnboardingStep.BasicInfo;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string StepsCompleted { get; set; } = "[]"; // JSON array
    public string StepsSkipped { get; set; } = "[]"; // JSON array
}

/// <summary>
/// Onboarding status
/// </summary>
public enum OnboardingStatus
{
    InProgress,
    Completed,
    Abandoned
}

/// <summary>
/// Dealer onboarding steps
/// </summary>
public enum DealerOnboardingStep
{
    BasicInfo,
    Documents,
    BillingInfo,
    Payment,
    Inventory,
    Features,
    Completed
}
