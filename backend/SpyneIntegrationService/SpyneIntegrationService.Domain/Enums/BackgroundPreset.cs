namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Available background presets for image transformation
/// </summary>
public enum BackgroundPreset
{
    /// <summary>Indoor showroom background</summary>
    Showroom = 0,
    
    /// <summary>Outdoor scenic background</summary>
    Outdoor = 1,
    
    /// <summary>Professional studio background</summary>
    Studio = 2,
    
    /// <summary>White/transparent background</summary>
    White = 3,
    
    /// <summary>Urban city background</summary>
    Urban = 4,
    
    /// <summary>Luxury dealership background</summary>
    Luxury = 5,
    
    /// <summary>Custom branded background</summary>
    Custom = 6,
    
    /// <summary>Transparent background</summary>
    Transparent = 7
}
