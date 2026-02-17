using CarDealer.Shared.MultiTenancy;

namespace InvoicingService.Domain.Entities;

public class CfdiConfiguration : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    // Issuer info (Emisor)
    public string Rfc { get; private set; } = string.Empty;
    public string BusinessName { get; private set; } = string.Empty;
    public string TaxRegime { get; private set; } = string.Empty; // RegimenFiscal
    public string FiscalAddress { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;

    // Certificate info
    public string? CertificateNumber { get; private set; }
    public byte[]? Certificate { get; private set; }
    public byte[]? PrivateKey { get; private set; }
    public string? PrivateKeyPassword { get; private set; }
    public DateTime? CertificateValidFrom { get; private set; }
    public DateTime? CertificateValidTo { get; private set; }

    // PAC (Proveedor Autorizado de Certificación) config
    public string? PacProvider { get; private set; } // e.g., "Finkok", "SolucionFactible"
    public string? PacUsername { get; private set; }
    public string? PacPassword { get; private set; }
    public bool IsProduction { get; private set; }

    // Series configuration
    public string DefaultSeries { get; private set; } = "A";
    public int CurrentFolio { get; private set; } = 1;

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CfdiConfiguration() { }

    public CfdiConfiguration(
        Guid dealerId,
        string rfc,
        string businessName,
        string taxRegime,
        string fiscalAddress,
        string postalCode)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Rfc = rfc;
        BusinessName = businessName;
        TaxRegime = taxRegime;
        FiscalAddress = fiscalAddress;
        PostalCode = postalCode;
        IsActive = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateIssuerInfo(string rfc, string businessName, string taxRegime, string fiscalAddress, string postalCode)
    {
        Rfc = rfc;
        BusinessName = businessName;
        TaxRegime = taxRegime;
        FiscalAddress = fiscalAddress;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCertificate(string certificateNumber, byte[] certificate, byte[] privateKey, string password, DateTime validFrom, DateTime validTo)
    {
        CertificateNumber = certificateNumber;
        Certificate = certificate;
        PrivateKey = privateKey;
        PrivateKeyPassword = password;
        CertificateValidFrom = validFrom;
        CertificateValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfigurePac(string provider, string username, string password, bool isProduction)
    {
        PacProvider = provider;
        PacUsername = username;
        PacPassword = password;
        IsProduction = isProduction;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSeries(string series, int startFolio = 1)
    {
        DefaultSeries = series;
        CurrentFolio = startFolio;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetNextFolio()
    {
        return CurrentFolio++;
    }

    public void Activate()
    {
        if (string.IsNullOrEmpty(CertificateNumber))
            throw new InvalidOperationException("Cannot activate CFDI without certificate");

        if (string.IsNullOrEmpty(PacProvider))
            throw new InvalidOperationException("Cannot activate CFDI without PAC configuration");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCertificateValid()
    {
        if (!CertificateValidFrom.HasValue || !CertificateValidTo.HasValue)
            return false;

        var now = DateTime.UtcNow;
        return now >= CertificateValidFrom.Value && now <= CertificateValidTo.Value;
    }
}
