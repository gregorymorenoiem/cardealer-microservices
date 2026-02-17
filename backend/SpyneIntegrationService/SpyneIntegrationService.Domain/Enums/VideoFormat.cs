namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Video output format/orientation and quality
/// </summary>
public enum VideoFormat
{
    /// <summary>Horizontal 16:9 (YouTube, Website)</summary>
    Horizontal = 0,
    
    /// <summary>Vertical 9:16 (TikTok, Reels, Stories)</summary>
    Vertical = 1,
    
    /// <summary>Square 1:1 (Instagram Feed)</summary>
    Square = 2,
    
    /// <summary>MP4 720p HD</summary>
    Mp4_720p = 10,
    
    /// <summary>MP4 1080p Full HD</summary>
    Mp4_1080p = 11,
    
    /// <summary>MP4 4K Ultra HD</summary>
    Mp4_4K = 12,
    
    /// <summary>WebM 720p HD</summary>
    Webm_720p = 20,
    
    /// <summary>WebM 1080p Full HD</summary>
    Webm_1080p = 21
}
