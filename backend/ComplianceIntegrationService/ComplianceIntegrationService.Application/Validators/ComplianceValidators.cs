// =====================================================
// C12: ComplianceIntegrationService - Validadores
// FluentValidation para DTOs de entrada
// =====================================================

using ComplianceIntegrationService.Application.DTOs;
using ComplianceIntegrationService.Domain.Enums;
using FluentValidation;

namespace ComplianceIntegrationService.Application.Validators;

/// <summary>
/// Validador para creación de configuración de integración
/// </summary>
public class CreateIntegrationConfigValidator : AbstractValidator<CreateIntegrationConfigDto>
{
    public CreateIntegrationConfigValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la integración es requerido.")
            .MaximumLength(200).WithMessage("El nombre no debe exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no debe exceder 1000 caracteres.");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("El ente regulador especificado no es válido.");

        RuleFor(x => x.IntegrationType)
            .IsInEnum().WithMessage("El tipo de integración especificado no es válido.");

        RuleFor(x => x.EndpointUrl)
            .MaximumLength(500).WithMessage("La URL del endpoint no debe exceder 500 caracteres.")
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.EndpointUrl))
            .WithMessage("La URL del endpoint no es válida.");

        RuleFor(x => x.SandboxUrl)
            .MaximumLength(500).WithMessage("La URL de sandbox no debe exceder 500 caracteres.")
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.SandboxUrl))
            .WithMessage("La URL de sandbox no es válida.");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).When(x => x.Port.HasValue)
            .WithMessage("El puerto debe estar entre 1 y 65535.");

        RuleFor(x => x.SyncFrequency)
            .IsInEnum().WithMessage("La frecuencia de sincronización no es válida.");

        RuleFor(x => x.ScheduledTime)
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").When(x => !string.IsNullOrEmpty(x.ScheduledTime))
            .WithMessage("La hora programada debe estar en formato HH:mm.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(5, 300).WithMessage("El timeout debe estar entre 5 y 300 segundos.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10).WithMessage("El número máximo de reintentos debe estar entre 0 y 10.");

        RuleFor(x => x.RetryIntervalSeconds)
            .InclusiveBetween(10, 3600).WithMessage("El intervalo de reintentos debe estar entre 10 y 3600 segundos.");

        RuleFor(x => x.ProtocolVersion)
            .MaximumLength(50).WithMessage("La versión del protocolo no debe exceder 50 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Las notas no deben exceder 2000 caracteres.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validador para actualización de configuración de integración
/// </summary>
public class UpdateIntegrationConfigValidator : AbstractValidator<UpdateIntegrationConfigDto>
{
    public UpdateIntegrationConfigValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la integración es requerido.")
            .MaximumLength(200).WithMessage("El nombre no debe exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no debe exceder 1000 caracteres.");

        RuleFor(x => x.EndpointUrl)
            .MaximumLength(500).WithMessage("La URL del endpoint no debe exceder 500 caracteres.");

        RuleFor(x => x.SandboxUrl)
            .MaximumLength(500).WithMessage("La URL de sandbox no debe exceder 500 caracteres.");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).When(x => x.Port.HasValue)
            .WithMessage("El puerto debe estar entre 1 y 65535.");

        RuleFor(x => x.SyncFrequency)
            .IsInEnum().WithMessage("La frecuencia de sincronización no es válida.");

        RuleFor(x => x.ScheduledTime)
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").When(x => !string.IsNullOrEmpty(x.ScheduledTime))
            .WithMessage("La hora programada debe estar en formato HH:mm.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(5, 300).WithMessage("El timeout debe estar entre 5 y 300 segundos.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10).WithMessage("El número máximo de reintentos debe estar entre 0 y 10.");

        RuleFor(x => x.RetryIntervalSeconds)
            .InclusiveBetween(10, 3600).WithMessage("El intervalo de reintentos debe estar entre 10 y 3600 segundos.");
    }
}

/// <summary>
/// Validador para creación de credencial
/// </summary>
public class CreateCredentialValidator : AbstractValidator<CreateCredentialDto>
{
    public CreateCredentialValidator()
    {
        RuleFor(x => x.IntegrationConfigId)
            .NotEmpty().WithMessage("El ID de la integración es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la credencial es requerido.")
            .MaximumLength(100).WithMessage("El nombre no debe exceder 100 caracteres.");

        RuleFor(x => x.CredentialType)
            .IsInEnum().WithMessage("El tipo de credencial no es válido.");

        RuleFor(x => x.Username)
            .MaximumLength(200).WithMessage("El usuario no debe exceder 200 caracteres.");

        RuleFor(x => x.Password)
            .MaximumLength(500).WithMessage("La contraseña no debe exceder 500 caracteres.");

        RuleFor(x => x.ApiKey)
            .MaximumLength(1000).WithMessage("La API Key no debe exceder 1000 caracteres.");

        RuleFor(x => x.Environment)
            .NotEmpty().WithMessage("El ambiente es requerido.")
            .Must(x => x == "Production" || x == "Sandbox" || x == "Development")
            .WithMessage("El ambiente debe ser 'Production', 'Sandbox' o 'Development'.");

        // Validaciones condicionales según tipo
        When(x => x.CredentialType == CredentialType.BasicAuth, () =>
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido para autenticación básica.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida para autenticación básica.");
        });

        When(x => x.CredentialType == CredentialType.ApiKey, () =>
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("La API Key es requerida.");
        });

        When(x => x.CredentialType == CredentialType.Certificate, () =>
        {
            RuleFor(x => x.CertificateBase64)
                .NotEmpty().WithMessage("El certificado es requerido.");
        });

        When(x => x.CredentialType == CredentialType.BearerToken, () =>
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("El token Bearer es requerido.");
        });
    }
}

/// <summary>
/// Validador para creación de transmisión
/// </summary>
public class CreateTransmissionValidator : AbstractValidator<CreateTransmissionDto>
{
    public CreateTransmissionValidator()
    {
        RuleFor(x => x.IntegrationConfigId)
            .NotEmpty().WithMessage("El ID de la integración es requerido.");

        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("El tipo de reporte no es válido.");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("La dirección de transmisión no es válida.");

        RuleFor(x => x.PeriodStart)
            .LessThanOrEqualTo(x => x.PeriodEnd).When(x => x.PeriodStart.HasValue && x.PeriodEnd.HasValue)
            .WithMessage("La fecha de inicio debe ser menor o igual a la fecha de fin.");

        RuleFor(x => x.FileName)
            .MaximumLength(255).WithMessage("El nombre del archivo no debe exceder 255 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no deben exceder 1000 caracteres.");
    }
}

/// <summary>
/// Validador para actualización de estado de transmisión
/// </summary>
public class UpdateTransmissionStatusValidator : AbstractValidator<UpdateTransmissionStatusDto>
{
    public UpdateTransmissionStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("El estado de transmisión no es válido.");

        RuleFor(x => x.ConfirmationNumber)
            .MaximumLength(100).WithMessage("El número de confirmación no debe exceder 100 caracteres.");

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000).WithMessage("El mensaje de error no debe exceder 1000 caracteres.");

        RuleFor(x => x.RecordCount)
            .GreaterThanOrEqualTo(0).WithMessage("El conteo de registros no puede ser negativo.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThanOrEqualTo(0).When(x => x.FileSizeBytes.HasValue)
            .WithMessage("El tamaño del archivo no puede ser negativo.");
    }
}

/// <summary>
/// Validador para creación de mapeo de campos
/// </summary>
public class CreateFieldMappingValidator : AbstractValidator<CreateFieldMappingDto>
{
    public CreateFieldMappingValidator()
    {
        RuleFor(x => x.IntegrationConfigId)
            .NotEmpty().WithMessage("El ID de la integración es requerido.");

        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("El tipo de reporte no es válido.");

        RuleFor(x => x.SourceField)
            .NotEmpty().WithMessage("El campo origen es requerido.")
            .MaximumLength(100).WithMessage("El campo origen no debe exceder 100 caracteres.");

        RuleFor(x => x.TargetField)
            .NotEmpty().WithMessage("El campo destino es requerido.")
            .MaximumLength(100).WithMessage("El campo destino no debe exceder 100 caracteres.");

        RuleFor(x => x.SourceDataType)
            .MaximumLength(50).WithMessage("El tipo de dato origen no debe exceder 50 caracteres.");

        RuleFor(x => x.TargetDataType)
            .MaximumLength(50).WithMessage("El tipo de dato destino no debe exceder 50 caracteres.");

        RuleFor(x => x.Transformation)
            .MaximumLength(200).WithMessage("La transformación no debe exceder 200 caracteres.");

        RuleFor(x => x.DefaultValue)
            .MaximumLength(200).WithMessage("El valor por defecto no debe exceder 200 caracteres.");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0).When(x => x.MaxLength.HasValue)
            .WithMessage("La longitud máxima debe ser mayor a 0.");

        RuleFor(x => x.ValidationPattern)
            .MaximumLength(500).WithMessage("El patrón de validación no debe exceder 500 caracteres.");

        RuleFor(x => x.FieldOrder)
            .GreaterThanOrEqualTo(0).WithMessage("El orden del campo no puede ser negativo.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no debe exceder 500 caracteres.");
    }
}

/// <summary>
/// Validador para creación de webhook
/// </summary>
public class CreateWebhookValidator : AbstractValidator<CreateWebhookDto>
{
    public CreateWebhookValidator()
    {
        RuleFor(x => x.IntegrationConfigId)
            .NotEmpty().WithMessage("El ID de la integración es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del webhook es requerido.")
            .MaximumLength(100).WithMessage("El nombre no debe exceder 100 caracteres.");

        RuleFor(x => x.WebhookUrl)
            .NotEmpty().WithMessage("La URL del webhook es requerida.")
            .MaximumLength(500).WithMessage("La URL no debe exceder 500 caracteres.")
            .Must(BeValidUrl).WithMessage("La URL del webhook no es válida.");

        RuleFor(x => x.SubscribedEvents)
            .MaximumLength(1000).WithMessage("Los eventos suscritos no deben exceder 1000 caracteres.");
    }

    private bool BeValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
