// ComplianceService - Report and Calendar Handlers

namespace ComplianceService.Application.Handlers;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Application.Commands;

#region Report Handlers

public class CreateReportHandler : IRequestHandler<CreateReportCommand, CreateReportResponse>
{
    private readonly IRegulatoryReportRepository _repository;

    public CreateReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateReportResponse> Handle(CreateReportCommand request, CancellationToken ct)
    {
        var reportNumber = await _repository.GenerateReportNumberAsync(request.ReportType, ct);
        
        var report = new RegulatoryReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = reportNumber,
            Type = request.ReportType,
            RegulationType = request.RegulationType,
            Title = request.Title,
            Description = request.Description,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            RegulatoryBody = request.RegulatoryBody,
            SubmissionDeadline = request.SubmissionDeadline,
            Content = request.Content,
            Status = ReportStatus.Draft,
            PreparedBy = request.CreatedBy,
            PreparedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(report, ct);
        return new CreateReportResponse(report.Id, reportNumber);
    }
}

public class UpdateReportHandler : IRequestHandler<UpdateReportCommand, bool>
{
    private readonly IRegulatoryReportRepository _repository;

    public UpdateReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return false;

        if (request.Title != null) report.Title = request.Title;
        if (request.Description != null) report.Description = request.Description;
        if (request.Content != null) report.Content = request.Content;
        if (request.Attachments != null) report.Attachments = request.Attachments;
        if (request.ReviewComments != null) report.ReviewComments = request.ReviewComments;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = request.UpdatedBy;

        await _repository.UpdateAsync(report, ct);
        return true;
    }
}

public class ApproveReportHandler : IRequestHandler<ApproveReportCommand, bool>
{
    private readonly IRegulatoryReportRepository _repository;

    public ApproveReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ApproveReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return false;

        report.Status = ReportStatus.Approved;
        report.ApprovedBy = request.ApprovedBy;
        report.ApprovedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = request.ApprovedBy;

        await _repository.UpdateAsync(report, ct);
        return true;
    }
}

public class SubmitReportHandler : IRequestHandler<SubmitReportCommand, bool>
{
    private readonly IRegulatoryReportRepository _repository;

    public SubmitReportHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(SubmitReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return false;

        // Validate report is approved before submission
        if (report.Status != ReportStatus.Approved)
        {
            return false;
        }

        report.Status = ReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;
        report.SubmittedBy = request.SubmittedBy;
        report.SubmissionReference = request.SubmissionReference;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = request.SubmittedBy;

        await _repository.UpdateAsync(report, ct);
        return true;
    }
}

#endregion

#region Calendar Handlers

public class CreateCalendarItemHandler : IRequestHandler<CreateCalendarItemCommand, Guid>
{
    private readonly IComplianceCalendarRepository _repository;

    public CreateCalendarItemHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCalendarItemCommand request, CancellationToken ct)
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

public class CompleteCalendarItemHandler : IRequestHandler<CompleteCalendarItemCommand, bool>
{
    private readonly IComplianceCalendarRepository _repository;

    public CompleteCalendarItemHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteCalendarItemCommand request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(request.Id, ct);
        if (item == null) return false;

        item.Status = TaskStatus.Completed;
        item.CompletedAt = DateTime.UtcNow;
        item.CompletedBy = request.CompletedBy;

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

public class DeleteCalendarItemHandler : IRequestHandler<DeleteCalendarItemCommand, bool>
{
    private readonly IComplianceCalendarRepository _repository;

    public DeleteCalendarItemHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCalendarItemCommand request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(request.Id, ct);
        if (item == null) return false;

        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}

#endregion

#region Training Handlers

public class CreateTrainingHandler : IRequestHandler<CreateTrainingCommand, Guid>
{
    private readonly IComplianceTrainingRepository _repository;

    public CreateTrainingHandler(IComplianceTrainingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTrainingCommand request, CancellationToken ct)
    {
        var training = new ComplianceTraining
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            RegulationType = request.RegulationType,
            TargetRoles = request.TargetRoles,
            IsMandatory = request.IsMandatory,
            DurationMinutes = request.DurationMinutes,
            ContentUrl = request.ContentUrl,
            ExamUrl = request.ExamUrl,
            PassingScore = request.PassingScore,
            ValidUntil = request.ValidUntil,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(training, ct);
        return training.Id;
    }
}

public class RecordTrainingCompletionHandler : IRequestHandler<RecordTrainingCompletionCommand, Guid>
{
    private readonly ITrainingCompletionRepository _completionRepository;
    private readonly IComplianceTrainingRepository _trainingRepository;

    public RecordTrainingCompletionHandler(
        ITrainingCompletionRepository completionRepository,
        IComplianceTrainingRepository trainingRepository)
    {
        _completionRepository = completionRepository;
        _trainingRepository = trainingRepository;
    }

    public async Task<Guid> Handle(RecordTrainingCompletionCommand request, CancellationToken ct)
    {
        var training = await _trainingRepository.GetByIdAsync(request.TrainingId, ct);
        
        var completion = new TrainingCompletion
        {
            Id = Guid.NewGuid(),
            TrainingId = request.TrainingId,
            UserId = request.UserId,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Score = request.Score,
            IsPassed = request.IsPassed,
            CertificateUrl = request.CertificateUrl,
            ExpiresAt = training?.ValidUntil
        };

        await _completionRepository.AddAsync(completion, ct);
        return completion.Id;
    }
}

#endregion

#region Metric Handlers

public class CreateMetricHandler : IRequestHandler<CreateMetricCommand, Guid>
{
    private readonly IComplianceMetricRepository _repository;

    public CreateMetricHandler(IComplianceMetricRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateMetricCommand request, CancellationToken ct)
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
