namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Type of Spyne transformation
/// </summary>
public enum TransformationType
{
    /// <summary>Single image transformation</summary>
    Image = 0,
    
    /// <summary>Background transformation</summary>
    Background = 1,
    
    /// <summary>360 degree spin generation</summary>
    Spin = 2,
    
    /// <summary>360 degree spin generation (alias)</summary>
    Spin360 = 2,
    
    /// <summary>Video tour generation</summary>
    Video = 3,
    
    /// <summary>AI Chat session</summary>
    Chat = 4,
    
    /// <summary>Background removal</summary>
    BackgroundRemoval = 5,
    
    /// <summary>Background replacement</summary>
    BackgroundReplacement = 6,
    
    /// <summary>Image enhancement</summary>
    Enhancement = 7,
    
    /// <summary>License plate masking</summary>
    PlateMasking = 8,
    
    /// <summary>Shadow addition</summary>
    ShadowAdd = 9,
    
    /// <summary>Color correction</summary>
    ColorCorrection = 10
}
