namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Role of the chat message sender
/// </summary>
public enum ChatMessageRole
{
    /// <summary>Message from the user/visitor</summary>
    User = 0,
    
    /// <summary>Message from Vini AI assistant</summary>
    Assistant = 1,
    
    /// <summary>System message</summary>
    System = 2
}
