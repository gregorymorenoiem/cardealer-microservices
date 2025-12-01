using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record CreateSecretCommand(
    string Key,
    string PlainValue,
    string Environment,
    string CreatedBy,
    string? Description = null,
    string? TenantId = null,
    DateTime? ExpiresAt = null
) : IRequest<EncryptedSecret>;
