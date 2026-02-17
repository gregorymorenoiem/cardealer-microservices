using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace DataProtectionService.Application.Handlers;

public class CreateConsentCommandHandler : IRequestHandler<CreateConsentCommand, UserConsentDto>
{
    private readonly IUserConsentRepository _repository;

    public CreateConsentCommandHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserConsentDto> Handle(CreateConsentCommand request, CancellationToken cancellationToken)
    {
        // Revoke any existing consent of the same type for this user
        var existingConsent = await _repository.GetActiveConsentAsync(request.UserId, request.Type, cancellationToken);
        if (existingConsent != null)
        {
            existingConsent.RevokedAt = DateTime.UtcNow;
            existingConsent.RevokeReason = "Superseded by new consent";
            await _repository.UpdateAsync(existingConsent, cancellationToken);
        }

        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = Enum.Parse<ConsentType>(request.Type),
            Version = request.Version,
            DocumentHash = request.DocumentHash,
            Granted = request.Granted,
            GrantedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress ?? string.Empty,
            UserAgent = request.UserAgent ?? string.Empty,
            CollectionMethod = request.CollectionMethod ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(consent, cancellationToken);

        return MapToDto(consent);
    }

    private static UserConsentDto MapToDto(UserConsent consent) => new(
        consent.Id,
        consent.UserId,
        consent.Type.ToString(),
        consent.Version,
        consent.Granted,
        consent.GrantedAt,
        consent.RevokedAt,
        consent.RevokeReason,
        consent.IsActive,
        consent.CreatedAt
    );
}

public class RevokeConsentCommandHandler : IRequestHandler<RevokeConsentCommand, UserConsentDto>
{
    private readonly IUserConsentRepository _repository;

    public RevokeConsentCommandHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserConsentDto> Handle(RevokeConsentCommand request, CancellationToken cancellationToken)
    {
        var consent = await _repository.GetByIdAsync(request.ConsentId, cancellationToken)
            ?? throw new InvalidOperationException($"Consent {request.ConsentId} not found");

        if (consent.UserId != request.UserId)
            throw new UnauthorizedAccessException("User does not own this consent");

        consent.RevokedAt = DateTime.UtcNow;
        consent.RevokeReason = request.Reason;
        consent.IpAddress = request.IpAddress ?? consent.IpAddress;

        await _repository.UpdateAsync(consent, cancellationToken);

        return new UserConsentDto(
            consent.Id,
            consent.UserId,
            consent.Type.ToString(),
            consent.Version,
            consent.Granted,
            consent.GrantedAt,
            consent.RevokedAt,
            consent.RevokeReason,
            consent.IsActive,
            consent.CreatedAt
        );
    }
}

public class BulkRevokeConsentsCommandHandler : IRequestHandler<BulkRevokeConsentsCommand, List<UserConsentDto>>
{
    private readonly IUserConsentRepository _repository;

    public BulkRevokeConsentsCommandHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserConsentDto>> Handle(BulkRevokeConsentsCommand request, CancellationToken cancellationToken)
    {
        var results = new List<UserConsentDto>();

        foreach (var consentType in request.ConsentTypes)
        {
            var consent = await _repository.GetActiveConsentAsync(request.UserId, consentType, cancellationToken);
            if (consent != null)
            {
                consent.RevokedAt = DateTime.UtcNow;
                consent.RevokeReason = request.Reason;
                consent.IpAddress = request.IpAddress ?? consent.IpAddress;
                await _repository.UpdateAsync(consent, cancellationToken);

                results.Add(new UserConsentDto(
                    consent.Id,
                    consent.UserId,
                    consent.Type.ToString(),
                    consent.Version,
                    consent.Granted,
                    consent.GrantedAt,
                    consent.RevokedAt,
                    consent.RevokeReason,
                    consent.IsActive,
                    consent.CreatedAt
                ));
            }
        }

        return results;
    }
}

public class AcceptPrivacyPolicyCommandHandler : IRequestHandler<AcceptPrivacyPolicyCommand, UserConsentDto>
{
    private readonly IUserConsentRepository _consentRepository;
    private readonly IPrivacyPolicyRepository _policyRepository;

    public AcceptPrivacyPolicyCommandHandler(
        IUserConsentRepository consentRepository,
        IPrivacyPolicyRepository policyRepository)
    {
        _consentRepository = consentRepository;
        _policyRepository = policyRepository;
    }

    public async Task<UserConsentDto> Handle(AcceptPrivacyPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new InvalidOperationException($"Policy {request.PolicyId} not found");

        // Create document hash
        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(policy.Content)));

        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = ConsentType.PrivacyPolicy,
            Version = policy.Version,
            DocumentHash = hash,
            Granted = true,
            GrantedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent ?? string.Empty,
            CollectionMethod = "WebForm",
            CreatedAt = DateTime.UtcNow
        };

        await _consentRepository.AddAsync(consent, cancellationToken);

        return new UserConsentDto(
            consent.Id,
            consent.UserId,
            consent.Type.ToString(),
            consent.Version,
            consent.Granted,
            consent.GrantedAt,
            consent.RevokedAt,
            consent.RevokeReason,
            consent.IsActive,
            consent.CreatedAt
        );
    }
}
