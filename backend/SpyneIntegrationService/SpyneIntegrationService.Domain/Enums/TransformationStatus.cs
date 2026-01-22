namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Status of a Spyne transformation job
/// </summary>
public enum TransformationStatus
{
    /// <summary>Waiting to be processed</summary>
    Pending = 0,
    
    /// <summary>Currently being processed by Spyne</summary>
    Processing = 1,
    
    /// <summary>Successfully completed</summary>
    Completed = 2,
    
    /// <summary>Failed to process</summary>
    Failed = 3,
    
    /// <summary>Cancelled by user</summary>
    Cancelled = 4
}
