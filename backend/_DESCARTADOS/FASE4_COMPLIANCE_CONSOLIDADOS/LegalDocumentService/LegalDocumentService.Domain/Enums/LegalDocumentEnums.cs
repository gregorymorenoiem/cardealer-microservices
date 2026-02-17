namespace LegalDocumentService.Domain.Enums;

/// <summary>
/// Types of legal documents
/// Ley 126-02 E-Commerce, Ley 172-13 Protección de Datos, Pro-Consumidor
/// </summary>
public enum LegalDocumentType
{
    // === CONTRATOS ===
    TermsOfService = 1,
    PrivacyPolicy = 2,
    CookiePolicy = 3,
    SaleContract = 4,
    PurchaseContract = 5,
    LeaseContract = 6,
    ServiceAgreement = 7,
    NDAgreement = 8,
    
    // === POLÍTICAS LEGALES ===
    RefundPolicy = 10,
    ReturnPolicy = 11,
    CancellationPolicy = 12,
    ShippingPolicy = 13,
    WarrantyPolicy = 14,
    DisputePolicy = 15,
    
    // === AVISOS LEGALES (Ley 126-02) ===
    LegalDisclaimer = 20,
    IntellectualPropertyNotice = 21,
    DMCANotice = 22,
    
    // === CONSENTIMIENTOS (Ley 172-13) ===
    DataProcessingConsent = 30,
    MarketingConsent = 31,
    ThirdPartyDataSharingConsent = 32,
    InternationalTransferConsent = 33,
    
    // === DGII / FISCALES ===
    NCFNotice = 40,
    TaxComplianceNotice = 41,
    
    // === PRO-CONSUMIDOR ===
    ConsumerRightsNotice = 50,
    ComplaintProcedure = 51,
    
    // === KYC/AML (Ley 155-17) ===
    AMLPolicy = 60,
    KYCDisclosure = 61,
    PEPDisclosure = 62,
    
    // === OTROS ===
    Other = 99
}

/// <summary>
/// Status of legal document
/// </summary>
public enum LegalDocumentStatus
{
    Draft = 1,
    PendingReview = 2,
    PendingLegalApproval = 3,
    Approved = 4,
    Published = 5,
    Archived = 6,
    Superseded = 7,
    Rejected = 8
}

/// <summary>
/// Legal jurisdiction
/// </summary>
public enum Jurisdiction
{
    DominicanRepublic = 1,
    USA = 2,
    EU = 3,
    International = 4,
    Caribbean = 5
}

/// <summary>
/// Language of the document
/// </summary>
public enum DocumentLanguage
{
    Spanish = 1,
    English = 2,
    French = 3,
    Portuguese = 4
}

/// <summary>
/// User acceptance status
/// </summary>
public enum AcceptanceStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Expired = 4,
    Revoked = 5
}

/// <summary>
/// Acceptance method
/// </summary>
public enum AcceptanceMethod
{
    ClickWrap = 1,          // User clicks "I accept"
    BrowseWrap = 2,         // Implied by using the site
    ScrollWrap = 3,         // Must scroll to bottom
    SignWrap = 4,           // Digital signature required
    Verbal = 5,             // Phone/recording acceptance
    Written = 6             // Physical signature
}

/// <summary>
/// Template variable types
/// </summary>
public enum TemplateVariableType
{
    Text = 1,
    Date = 2,
    Number = 3,
    Currency = 4,
    Boolean = 5,
    List = 6,
    Entity = 7
}
