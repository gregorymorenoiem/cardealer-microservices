using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Commands;

public record CreateConsentCommand(
    Guid UserId,
    string Type,
    string Version,
    string DocumentHash,
    bool Granted,
    string? IpAddress,
    string? UserAgent,
    string? CollectionMethod
) : IRequest<UserConsentDto>;

public record RevokeConsentCommand(
    Guid ConsentId,
    Guid UserId,
    string Reason,
    string? IpAddress
) : IRequest<UserConsentDto>;

public record BulkRevokeConsentsCommand(
    Guid UserId,
    List<string> ConsentTypes,
    string Reason,
    string? IpAddress
) : IRequest<List<UserConsentDto>>;

public record AcceptPrivacyPolicyCommand(
    Guid UserId,
    Guid PolicyId,
    string IpAddress,
    string? UserAgent
) : IRequest<UserConsentDto>;
