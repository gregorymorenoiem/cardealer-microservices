namespace AdminService.Domain.Enums;

/// <summary>
/// Status of a moderation item
/// </summary>
public enum ModerationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Escalated = 3,
    Skipped = 4
}

/// <summary>
/// Priority of a moderation item
/// </summary>
public enum ModerationPriority
{
    Normal = 0,
    High = 1,
    Urgent = 2
}

/// <summary>
/// Type of content being moderated
/// </summary>
public enum ModerationType
{
    Vehicle = 0,
    Dealer = 1,
    User = 2,
    Content = 3,
    Review = 4
}
