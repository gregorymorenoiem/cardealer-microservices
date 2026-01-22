namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Status of a chat session with Vini AI
/// </summary>
public enum ChatSessionStatus
{
    /// <summary>Session is active and accepting messages</summary>
    Active = 0,
    
    /// <summary>Session ended by user or timeout</summary>
    Ended = 1,
    
    /// <summary>Session closed</summary>
    Closed = 1,
    
    /// <summary>Session expired due to inactivity</summary>
    Expired = 2,
    
    /// <summary>Lead was captured from session</summary>
    LeadCaptured = 3
}
