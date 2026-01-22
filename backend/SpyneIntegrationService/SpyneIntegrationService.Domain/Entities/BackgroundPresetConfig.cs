using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a background preset configuration
/// </summary>
public class BackgroundPresetConfig
{
    public Guid Id { get; set; }
    
    /// <summary>Preset type</summary>
    public BackgroundPreset Preset { get; set; }
    
    /// <summary>Display name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Display name alias</summary>
    public string DisplayName 
    { 
        get => Name; 
        set => Name = value; 
    }
    
    /// <summary>Description of the background</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Spyne's background ID</summary>
    public string SpyneBackgroundId { get; set; } = string.Empty;
    
    /// <summary>Preview/thumbnail URL</summary>
    public string PreviewUrl { get; set; } = string.Empty;
    
    /// <summary>Thumbnail URL alias</summary>
    public string ThumbnailUrl 
    { 
        get => PreviewUrl; 
        set => PreviewUrl = value; 
    }
    
    /// <summary>Category for grouping</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>Whether this preset is available</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Display order</summary>
    public int SortOrder { get; set; }
    
    /// <summary>Credits cost for this preset</summary>
    public decimal CreditsCost { get; set; }
    
    /// <summary>Created timestamp</summary>
    public DateTime CreatedAt { get; set; }

    public BackgroundPresetConfig()
    {
        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}
