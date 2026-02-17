using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Queries;
using DataProtectionService.Domain.Interfaces;

namespace DataProtectionService.Application.Handlers;

public class GetUserConsentsQueryHandler : IRequestHandler<GetUserConsentsQuery, List<UserConsentDto>>
{
    private readonly IUserConsentRepository _repository;

    public GetUserConsentsQueryHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserConsentDto>> Handle(GetUserConsentsQuery request, CancellationToken cancellationToken)
    {
        var consents = await _repository.GetByUserIdAsync(request.UserId, request.ActiveOnly ?? true, cancellationToken);
        
        return consents.Select(c => new UserConsentDto(
            c.Id,
            c.UserId,
            c.Type.ToString(),
            c.Version,
            c.Granted,
            c.GrantedAt,
            c.RevokedAt,
            c.RevokeReason,
            c.IsActive,
            c.CreatedAt
        )).ToList();
    }
}

public class GetConsentByIdQueryHandler : IRequestHandler<GetConsentByIdQuery, UserConsentDto?>
{
    private readonly IUserConsentRepository _repository;

    public GetConsentByIdQueryHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserConsentDto?> Handle(GetConsentByIdQuery request, CancellationToken cancellationToken)
    {
        var consent = await _repository.GetByIdAsync(request.ConsentId, cancellationToken);
        if (consent == null) return null;

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

public class CheckConsentQueryHandler : IRequestHandler<CheckConsentQuery, bool>
{
    private readonly IUserConsentRepository _repository;

    public CheckConsentQueryHandler(IUserConsentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckConsentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.HasActiveConsentAsync(request.UserId, request.Type, cancellationToken);
    }
}

public class GetCurrentPrivacyPolicyQueryHandler : IRequestHandler<GetCurrentPrivacyPolicyQuery, PrivacyPolicyDto?>
{
    private readonly IPrivacyPolicyRepository _repository;

    public GetCurrentPrivacyPolicyQueryHandler(IPrivacyPolicyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PrivacyPolicyDto?> Handle(GetCurrentPrivacyPolicyQuery request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetCurrentPolicyAsync(request.Language, cancellationToken);
        if (policy == null) return null;

        return new PrivacyPolicyDto(
            policy.Id,
            policy.Version,
            policy.DocumentType,
            policy.Content,
            policy.ChangesSummary,
            policy.Language,
            policy.EffectiveDate,
            policy.IsActive,
            policy.RequiresReAcceptance,
            policy.CreatedAt
        );
    }
}

public class GetAllPrivacyPoliciesQueryHandler : IRequestHandler<GetAllPrivacyPoliciesQuery, List<PrivacyPolicyDto>>
{
    private readonly IPrivacyPolicyRepository _repository;

    public GetAllPrivacyPoliciesQueryHandler(IPrivacyPolicyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PrivacyPolicyDto>> Handle(GetAllPrivacyPoliciesQuery request, CancellationToken cancellationToken)
    {
        var policies = await _repository.GetAllAsync(request.ActiveOnly ?? true, cancellationToken);

        return policies.Select(p => new PrivacyPolicyDto(
            p.Id,
            p.Version,
            p.DocumentType,
            p.Content,
            p.ChangesSummary,
            p.Language,
            p.EffectiveDate,
            p.IsActive,
            p.RequiresReAcceptance,
            p.CreatedAt
        )).ToList();
    }
}
