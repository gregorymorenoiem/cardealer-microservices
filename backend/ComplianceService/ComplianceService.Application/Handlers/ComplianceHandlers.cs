// ComplianceService - Command Handlers

namespace ComplianceService.Application.Handlers;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Application.Commands;
using ComplianceService.Application.DTOs;

#region Framework Handlers

public class CreateFrameworkHandler : IRequestHandler<CreateFrameworkCommand, Guid>
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public CreateFrameworkHandler(IRegulatoryFrameworkRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateFrameworkCommand request, CancellationToken ct)
    {
        var framework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            LegalReference = request.LegalReference,
            RegulatoryBody = request.RegulatoryBody,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            Version = request.Version,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(framework, ct);
        return framework.Id;
    }
}

public class UpdateFrameworkHandler : IRequestHandler<UpdateFrameworkCommand, bool>
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public UpdateFrameworkHandler(IRegulatoryFrameworkRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateFrameworkCommand request, CancellationToken ct)
    {
        var framework = await _repository.GetByIdAsync(request.Id, ct);
        if (framework == null) return false;

        framework.Name = request.Name;
        framework.Description = request.Description;
        framework.LegalReference = request.LegalReference;
        framework.RegulatoryBody = request.RegulatoryBody;
        framework.ExpirationDate = request.ExpirationDate;
        framework.IsActive = request.IsActive;
        framework.Version = request.Version;
        framework.Notes = request.Notes;
        framework.UpdatedAt = DateTime.UtcNow;
        framework.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(framework, ct);
        return true;
    }
}

#endregion

#region Requirement Handlers

public class CreateRequirementHandler : IRequestHandler<CreateRequirementCommand, Guid>
{
    private readonly IComplianceRequirementRepository _repository;

    public CreateRequirementHandler(IComplianceRequirementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateRequirementCommand request, CancellationToken ct)
    {
        var requirement = new ComplianceRequirement
        {
            Id = Guid.NewGuid(),
            FrameworkId = request.FrameworkId,
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            Criticality = request.Criticality,
            ArticleReference = request.ArticleReference,
            DeadlineDays = request.DeadlineDays,
            EvaluationFrequency = request.EvaluationFrequency,
            RequiresEvidence = request.RequiresEvidence,
            RequiresApproval = request.RequiresApproval,
            EvidenceRequirements = request.EvidenceRequirements,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(requirement, ct);
        return requirement.Id;
    }
}

#endregion

#region Control Handlers

public class CreateControlHandler : IRequestHandler<CreateControlCommand, Guid>
{
    private readonly IComplianceControlRepository _repository;

    public CreateControlHandler(IComplianceControlRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateControlCommand request, CancellationToken ct)
    {
        var control = new ComplianceControl
        {
            Id = Guid.NewGuid(),
            FrameworkId = request.FrameworkId,
            RequirementId = request.RequirementId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            ImplementationDetails = request.ImplementationDetails,
            ResponsibleRole = request.ResponsibleRole,
            TestingFrequency = request.TestingFrequency,
            Status = ComplianceStatus.NotEvaluated,
            EffectivenessScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(control, ct);
        return control.Id;
    }
}

public class CreateControlTestHandler : IRequestHandler<CreateControlTestCommand, Guid>
{
    private readonly IControlTestRepository _testRepository;
    private readonly IComplianceControlRepository _controlRepository;

    public CreateControlTestHandler(
        IControlTestRepository testRepository,
        IComplianceControlRepository controlRepository)
    {
        _testRepository = testRepository;
        _controlRepository = controlRepository;
    }

    public async Task<Guid> Handle(CreateControlTestCommand request, CancellationToken ct)
    {
        var test = new ControlTest
        {
            Id = Guid.NewGuid(),
            ControlId = request.ControlId,
            TestDate = DateTime.UtcNow,
            TestedBy = request.TestedBy,
            TestProcedure = request.TestProcedure,
            TestResults = request.TestResults,
            IsPassed = request.IsPassed,
            EffectivenessScore = request.EffectivenessScore,
            Findings = request.Findings,
            Recommendations = request.Recommendations,
            EvidenceDocuments = request.EvidenceDocuments ?? new List<string>()
        };

        await _testRepository.AddAsync(test, ct);

        // Update control status
        var control = await _controlRepository.GetByIdAsync(request.ControlId, ct);
        if (control != null)
        {
            control.LastTestedAt = DateTime.UtcNow;
            control.Status = request.IsPassed ? ComplianceStatus.Compliant : ComplianceStatus.NonCompliant;
            if (request.EffectivenessScore.HasValue)
            {
                control.EffectivenessScore = request.EffectivenessScore.Value;
            }
            
            // Calculate next test date based on frequency
            control.NextTestDate = control.TestingFrequency switch
            {
                EvaluationFrequency.Daily => DateTime.UtcNow.AddDays(1),
                EvaluationFrequency.Weekly => DateTime.UtcNow.AddDays(7),
                EvaluationFrequency.Monthly => DateTime.UtcNow.AddMonths(1),
                EvaluationFrequency.Quarterly => DateTime.UtcNow.AddMonths(3),
                EvaluationFrequency.SemiAnnual => DateTime.UtcNow.AddMonths(6),
                EvaluationFrequency.Annual => DateTime.UtcNow.AddYears(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };

            await _controlRepository.UpdateAsync(control, ct);
        }

        return test.Id;
    }
}

#endregion

#region Assessment Handlers

public class CreateAssessmentHandler : IRequestHandler<CreateAssessmentCommand, Guid>
{
    private readonly IComplianceAssessmentRepository _repository;
    private readonly IComplianceRequirementRepository _requirementRepository;

    public CreateAssessmentHandler(
        IComplianceAssessmentRepository repository,
        IComplianceRequirementRepository requirementRepository)
    {
        _repository = repository;
        _requirementRepository = requirementRepository;
    }

    public async Task<Guid> Handle(CreateAssessmentCommand request, CancellationToken ct)
    {
        var requirement = await _requirementRepository.GetByIdAsync(request.RequirementId, ct);
        
        var assessment = new ComplianceAssessment
        {
            Id = Guid.NewGuid(),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            RequirementId = request.RequirementId,
            Status = request.Status,
            AssessmentDate = DateTime.UtcNow,
            AssessedBy = request.AssessedBy,
            Score = request.Score,
            Observations = request.Observations,
            EvidenceProvided = request.EvidenceProvided,
            NextAssessmentDate = request.NextAssessmentDate,
            DeadlineDate = requirement != null 
                ? DateTime.UtcNow.AddDays(requirement.DeadlineDays) 
                : null,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(assessment, ct);
        return assessment.Id;
    }
}

public class UpdateAssessmentHandler : IRequestHandler<UpdateAssessmentCommand, bool>
{
    private readonly IComplianceAssessmentRepository _repository;

    public UpdateAssessmentHandler(IComplianceAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateAssessmentCommand request, CancellationToken ct)
    {
        var assessment = await _repository.GetByIdAsync(request.Id, ct);
        if (assessment == null) return false;

        assessment.Status = request.Status;
        assessment.Score = request.Score;
        assessment.Observations = request.Observations;
        assessment.EvidenceProvided = request.EvidenceProvided;
        assessment.NextAssessmentDate = request.NextAssessmentDate;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(assessment, ct);
        return true;
    }
}

#endregion

#region Finding Handlers

public class CreateFindingHandler : IRequestHandler<CreateFindingCommand, Guid>
{
    private readonly IComplianceFindingRepository _repository;

    public CreateFindingHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateFindingCommand request, CancellationToken ct)
    {
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            AssessmentId = request.AssessmentId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Criticality = request.Criticality,
            Status = FindingStatus.Open,
            RootCause = request.RootCause,
            Impact = request.Impact,
            Recommendation = request.Recommendation,
            AssignedTo = request.AssignedTo,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(finding, ct);
        return finding.Id;
    }
}

public class UpdateFindingHandler : IRequestHandler<UpdateFindingCommand, bool>
{
    private readonly IComplianceFindingRepository _repository;

    public UpdateFindingHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateFindingCommand request, CancellationToken ct)
    {
        var finding = await _repository.GetByIdAsync(request.Id, ct);
        if (finding == null) return false;

        finding.Status = request.Status;
        finding.Resolution = request.Resolution;
        finding.AssignedTo = request.AssignedTo;
        finding.DueDate = request.DueDate;
        finding.UpdatedAt = DateTime.UtcNow;
        finding.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(finding, ct);
        return true;
    }
}

public class ResolveFindingHandler : IRequestHandler<ResolveFindingCommand, bool>
{
    private readonly IComplianceFindingRepository _repository;

    public ResolveFindingHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ResolveFindingCommand request, CancellationToken ct)
    {
        var finding = await _repository.GetByIdAsync(request.Id, ct);
        if (finding == null) return false;

        finding.Status = FindingStatus.Resolved;
        finding.Resolution = request.Resolution;
        finding.ResolvedAt = DateTime.UtcNow;
        finding.ResolvedBy = request.ResolvedBy;
        finding.UpdatedAt = DateTime.UtcNow;
        finding.UpdatedBy = request.ResolvedBy;

        await _repository.UpdateAsync(finding, ct);
        return true;
    }
}

#endregion

#region Remediation Handlers

public class CreateRemediationHandler : IRequestHandler<CreateRemediationCommand, Guid>
{
    private readonly IRemediationActionRepository _repository;

    public CreateRemediationHandler(IRemediationActionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateRemediationCommand request, CancellationToken ct)
    {
        var action = new RemediationAction
        {
            Id = Guid.NewGuid(),
            FindingId = request.FindingId,
            Title = request.Title,
            Description = request.Description,
            AssignedTo = request.AssignedTo,
            DueDate = request.DueDate,
            Priority = request.Priority,
            RequiresVerification = request.RequiresVerification,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(action, ct);
        return action.Id;
    }
}

public class CompleteRemediationHandler : IRequestHandler<CompleteRemediationCommand, bool>
{
    private readonly IRemediationActionRepository _repository;

    public CompleteRemediationHandler(IRemediationActionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteRemediationCommand request, CancellationToken ct)
    {
        var action = await _repository.GetByIdAsync(request.Id, ct);
        if (action == null) return false;

        action.Status = action.RequiresVerification ? TaskStatus.Completed : TaskStatus.Completed;
        action.CompletionNotes = request.CompletionNotes;
        action.CompletedAt = DateTime.UtcNow;
        action.CompletedBy = request.CompletedBy;

        await _repository.UpdateAsync(action, ct);
        return true;
    }
}

public class VerifyRemediationHandler : IRequestHandler<VerifyRemediationCommand, bool>
{
    private readonly IRemediationActionRepository _repository;

    public VerifyRemediationHandler(IRemediationActionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(VerifyRemediationCommand request, CancellationToken ct)
    {
        var action = await _repository.GetByIdAsync(request.Id, ct);
        if (action == null) return false;

        action.VerifiedAt = DateTime.UtcNow;
        action.VerifiedBy = request.VerifiedBy;

        await _repository.UpdateAsync(action, ct);
        return true;
    }
}

#endregion
