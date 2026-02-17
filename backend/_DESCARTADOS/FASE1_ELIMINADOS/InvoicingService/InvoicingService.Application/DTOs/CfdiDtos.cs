using InvoicingService.Domain.Entities;

namespace InvoicingService.Application.DTOs;

public record CfdiConfigurationDto(
    Guid Id,
    Guid DealerId,
    string Rfc,
    string BusinessName,
    string TaxRegime,
    string FiscalAddress,
    string PostalCode,
    string? CertificateNumber,
    DateTime? CertificateValidFrom,
    DateTime? CertificateValidTo,
    string? PacProvider,
    bool IsProduction,
    string DefaultSeries,
    int CurrentFolio,
    bool IsActive,
    bool IsCertificateValid,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static CfdiConfigurationDto FromEntity(CfdiConfiguration entity) => new(
        entity.Id,
        entity.DealerId,
        entity.Rfc,
        entity.BusinessName,
        entity.TaxRegime,
        entity.FiscalAddress,
        entity.PostalCode,
        entity.CertificateNumber,
        entity.CertificateValidFrom,
        entity.CertificateValidTo,
        entity.PacProvider,
        entity.IsProduction,
        entity.DefaultSeries,
        entity.CurrentFolio,
        entity.IsActive,
        entity.IsCertificateValid(),
        entity.CreatedAt,
        entity.UpdatedAt);
}

public record CreateCfdiConfigurationRequest(
    string Rfc,
    string BusinessName,
    string TaxRegime,
    string FiscalAddress,
    string PostalCode,
    string DefaultSeries);

public record UpdateCfdiConfigurationRequest(
    string Rfc,
    string BusinessName,
    string TaxRegime,
    string FiscalAddress,
    string PostalCode);

public record UploadCertificateRequest(
    string CertificateNumber,
    string CertificateBase64,
    string PrivateKeyBase64,
    string Password,
    DateTime ValidFrom,
    DateTime ValidTo);

public record ConfigurePacRequest(
    string Provider,
    string Username,
    string Password,
    bool IsProduction);

public record SetSeriesRequest(
    string Series,
    int StartFolio);
