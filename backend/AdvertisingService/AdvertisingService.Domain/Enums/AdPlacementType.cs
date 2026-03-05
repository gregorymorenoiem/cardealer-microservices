namespace AdvertisingService.Domain.Enums;

/// <summary>
/// Tipos de placement publicitario OKLA v2
/// </summary>
public enum AdPlacementType
{
    /// <summary>Listing Destacado — Badge dorado, prioridad en resultados ($0.50/día)</summary>
    FeaturedSpot = 0,
    
    /// <summary>Posición Top 3 en búsquedas ($1.50/día)</summary>
    PremiumSpot = 1,
    
    /// <summary>Oferta del Día — Homepage + email blast ($15/día)</summary>
    DealOfTheDay = 2,
    
    /// <summary>Banner Homepage 728x90 — Max 3 simultáneos ($120/mes)</summary>
    HomepageBanner = 3,
    
    /// <summary>Dealer Showcase — Directorio destacado ($50/mes)</summary>
    DealerShowcase = 4,
    
    /// <summary>Pack Alertas Email — Alertas automáticas por modelo ($35/mes)</summary>
    EmailAlerts = 5,
    
    /// <summary>Paquete Visibilidad Total — Bundle completo ($175/mes)</summary>
    VisibilityBundle = 6
}
