// ComplianceService - Additional Handlers for Controller Compatibility

namespace ComplianceService.Application.Handlers;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Application.Commands;
using ComplianceService.Application.DTOs;

#region Calendar Event Handlers (Controller Compatibility)

public class CreateCalendarEventHandler : IRequestHandler<CreateCalendarEventCommand, Guid>
{
    private readonly IComplianceCalendarRepository _repository;

    public CreateCalendarEventHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCalendarEventCommand request, CancellationToken ct)
    {
        var item = new ComplianceCalendar
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            RegulationType = request.RegulationType,
            DueDate = request.DueDate,
            ReminderDaysBefore = request.ReminderDaysBefore,
            IsRecurring = request.IsRecurring,
            RecurrencePattern = request.RecurrencePattern,
            AssignedTo = request.AssignedTo,
            Status = TaskStatus.Pending,
            NotificationSent = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(item, ct);
        return item.Id;
    }
}

public class UpdateCalendarEventHandler : IRequestHandler<UpdateCalendarEventCommand, bool>
{
    private readonly IComplianceCalendarRepository _repository;

    public UpdateCalendarEventHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCalendarEventCommand request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(request.Id, ct);
        if (item == null) return false;

        item.Title = request.Title;
        item.Description = request.Description;
        item.DueDate = request.DueDate;
        item.Status = request.Status;
        item.AssignedTo = request.AssignedTo;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(item, ct);
        return true;
    }
}

public class CompleteCalendarEventHandler : IRequestHandler<CompleteCalendarEventCommand, bool>
{
    private readonly IComplianceCalendarRepository _repository;

    public CompleteCalendarEventHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteCalendarEventCommand request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(request.Id, ct);
        if (item == null) return false;

        item.Status = TaskStatus.Completed;
        item.CompletedAt = DateTime.UtcNow;
        item.CompletedBy = request.CompletedBy;
        item.CompletionNotes = request.Notes;

        await _repository.UpdateAsync(item, ct);

        // If recurring, create next occurrence
        if (item.IsRecurring && item.RecurrencePattern.HasValue)
        {
            var nextDueDate = item.RecurrencePattern.Value switch
            {
                EvaluationFrequency.Daily => item.DueDate.AddDays(1),
                EvaluationFrequency.Weekly => item.DueDate.AddDays(7),
                EvaluationFrequency.Monthly => item.DueDate.AddMonths(1),
                EvaluationFrequency.Quarterly => item.DueDate.AddMonths(3),
                EvaluationFrequency.SemiAnnual => item.DueDate.AddMonths(6),
                EvaluationFrequency.Annual => item.DueDate.AddYears(1),
                _ => item.DueDate.AddMonths(1)
            };

            var newItem = new ComplianceCalendar
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                Description = item.Description,
                RegulationType = item.RegulationType,
                DueDate = nextDueDate,
                ReminderDaysBefore = item.ReminderDaysBefore,
                IsRecurring = true,
                RecurrencePattern = item.RecurrencePattern,
                AssignedTo = item.AssignedTo,
                Status = TaskStatus.Pending,
                NotificationSent = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CompletedBy
            };

            await _repository.AddAsync(newItem, ct);
        }

        return true;
    }
}

public class DeleteCalendarEventHandler : IRequestHandler<DeleteCalendarEventCommand, bool>
{
    private readonly IComplianceCalendarRepository _repository;

    public DeleteCalendarEventHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCalendarEventCommand request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(request.Id, ct);
        if (item == null) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}

#endregion

#region Metric Handler (Controller Compatibility)

public class RecordMetricHandler : IRequestHandler<RecordMetricCommand, Guid>
{
    private readonly IComplianceMetricRepository _repository;

    public RecordMetricHandler(IComplianceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(RecordMetricCommand request, CancellationToken ct)
    {
        var metric = new ComplianceMetric
        {
            Id = Guid.NewGuid(),
            RegulationType = request.RegulationType,
            MetricName = request.MetricName,
            Description = request.Description,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Value = request.Value,
            Unit = request.Unit,
            Target = request.Target,
            Threshold = request.Threshold,
            IsWithinTarget = request.Target.HasValue && request.Value >= request.Target.Value,
            CalculationMethod = request.CalculationMethod,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = request.CalculatedBy
        };

        await _repository.AddAsync(metric, ct);
        return metric.Id;
    }
}

#endregion

#region Control Test Handler (Controller Compatibility)

public class RecordControlTestHandler : IRequestHandler<RecordControlTestCommand, Guid>
{
    private readonly IControlTestRepository _testRepository;
    private readonly IComplianceControlRepository _controlRepository;

    public RecordControlTestHandler(
        IControlTestRepository testRepository,
        IComplianceControlRepository controlRepository)
    {
        _testRepository = testRepository;
        _controlRepository = controlRepository;
    }

    public async Task<Guid> Handle(RecordControlTestCommand request, CancellationToken ct)
    {
        var control = await _controlRepository.GetByIdAsync(request.ControlId, ct);
        if (control == null)
            throw new InvalidOperationException($"Control con ID {request.ControlId} no encontrado");

        var test = new ControlTest
        {
            Id = Guid.NewGuid(),
            ControlId = request.ControlId,
            TestedAt = DateTime.UtcNow,
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

        // Update control status based on test result
        control.LastTestedAt = DateTime.UtcNow;
        control.LastTestedBy = request.TestedBy;
        control.EffectivenessScore = request.EffectivenessScore ?? 0;
        control.UpdatedAt = DateTime.UtcNow;
        control.UpdatedBy = request.TestedBy;
        await _controlRepository.UpdateAsync(control, ct);

        return test.Id;
    }
}

#endregion

#region Training Update Handler

public class UpdateTrainingHandler : IRequestHandler<UpdateTrainingCommand, bool>
{
    private readonly IComplianceTrainingRepository _repository;

    public UpdateTrainingHandler(IComplianceTrainingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateTrainingCommand request, CancellationToken ct)
    {
        var training = await _repository.GetByIdAsync(request.Id, ct);
        if (training == null) return false;

        training.Title = request.Title;
        training.Description = request.Description;
        training.IsMandatory = request.IsMandatory;
        training.DurationMinutes = request.DurationMinutes;
        training.ContentUrl = request.ContentUrl;
        training.ExamUrl = request.ExamUrl;
        training.PassingScore = request.PassingScore;
        training.ValidUntil = request.ValidUntil;
        training.IsActive = request.IsActive;
        training.UpdatedAt = DateTime.UtcNow;
        training.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(training, ct);
        return true;
    }
}

#endregion

// CompleteRemediationHandler is defined in ComplianceHandlers.cs

#region Finding Close Handler

public class CloseFindingHandler : IRequestHandler<CloseFindingCommand, bool>
{
    private readonly IComplianceFindingRepository _repository;

    public CloseFindingHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CloseFindingCommand request, CancellationToken ct)
    {
        var finding = await _repository.GetByIdAsync(request.Id, ct);
        if (finding == null) return false;

        finding.Status = FindingStatus.Closed;
        finding.Resolution = request.Resolution;
        finding.ResolvedAt = DateTime.UtcNow;
        finding.ResolvedBy = request.ClosedBy;
        finding.UpdatedAt = DateTime.UtcNow;
        finding.UpdatedBy = request.ClosedBy;

        await _repository.UpdateAsync(finding, ct);
        return true;
    }
}

public record CloseFindingCommand(Guid Id, string Resolution, string ClosedBy) : IRequest<bool>;

#endregion

#region Assessment Complete Handler

public class CompleteAssessmentHandler : IRequestHandler<CompleteAssessmentCommand, bool>
{
    private readonly IComplianceAssessmentRepository _repository;

    public CompleteAssessmentHandler(IComplianceAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteAssessmentCommand request, CancellationToken ct)
    {
        var assessment = await _repository.GetByIdAsync(request.Id, ct);
        if (assessment == null) return false;

        assessment.Score = (int)request.FinalScore;
        assessment.Status = request.FinalScore >= 80 ? ComplianceStatus.Compliant :
                           request.FinalScore >= 60 ? ComplianceStatus.PartiallyCompliant :
                           ComplianceStatus.NonCompliant;
        assessment.Observations = request.Notes;
        assessment.UpdatedAt = DateTime.UtcNow;
        assessment.UpdatedBy = request.CompletedBy;

        await _repository.UpdateAsync(assessment, ct);
        return true;
    }
}

public record CompleteAssessmentCommand(Guid Id, decimal FinalScore, string CompletedBy, string? Notes) : IRequest<bool>;

#endregion

#region Report Accept/Reject Handlers

public class AcceptReportHandler : IRequestHandler<AcceptReportCommand, bool>
{
    private readonly IRegulatoryReportRepository _repository;

    public AcceptReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(AcceptReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return false;

        report.Status = ReportStatus.Accepted;
        report.RegulatoryResponse = request.RegulatoryResponse;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = request.AcceptedBy;

        await _repository.UpdateAsync(report, ct);
        return true;
    }
}

public record AcceptReportCommand(Guid Id, string AcceptedBy, string? RegulatoryResponse) : IRequest<bool>;

public class RejectReportHandler : IRequestHandler<RejectReportCommand, bool>
{
    private readonly IRegulatoryReportRepository _repository;

    public RejectReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(RejectReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return false;

        report.Status = ReportStatus.Rejected;
        report.RejectionReason = request.RejectionReason;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = request.RejectedBy;

        await _repository.UpdateAsync(report, ct);
        return true;
    }
}

public record RejectReportCommand(Guid Id, string RejectedBy, string RejectionReason) : IRequest<bool>;

#endregion
