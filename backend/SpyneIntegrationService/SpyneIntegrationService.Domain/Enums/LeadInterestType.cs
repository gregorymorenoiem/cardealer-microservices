namespace SpyneIntegrationService.Domain.Enums;

/// <summary>
/// Type of interest captured from chat lead
/// </summary>
public enum LeadInterestType
{
    /// <summary>General information request</summary>
    GeneralInfo = 0,
    
    /// <summary>Just looking around</summary>
    JustLooking = 1,
    
    /// <summary>Comparing different options</summary>
    ComparingOptions = 2,
    
    /// <summary>Wants to schedule a test drive</summary>
    TestDrive = 3,
    
    /// <summary>Interested in financing options</summary>
    Financing = 4,
    
    /// <summary>Needs financing</summary>
    NeedsFinancing = 4,
    
    /// <summary>Has a trade-in vehicle</summary>
    TradeIn = 5,
    
    /// <summary>Interested in trade-in</summary>
    TradeInInterested = 5,
    
    /// <summary>Ready to purchase</summary>
    Purchase = 6,
    
    /// <summary>Ready to buy</summary>
    ReadyToBuy = 6,
    
    /// <summary>Wants to negotiate price</summary>
    PriceNegotiation = 7
}
