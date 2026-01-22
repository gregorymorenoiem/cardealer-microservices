// ComplianceService - Query Handlers

namespace ComplianceService.Application.Handlers;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Application.DTOs;
using ComplianceService.Application.Queries;

#region Framework Query Handlers

public class GetFrameworkByIdHandler : IRequestHandler<GetFrameworkByIdQuery, RegulatoryFrameworkDetailDto?>
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public GetFrameworkByIdHandler(IRegulatoryFrameworkRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegulatoryFrameworkDetailDto?> Handle(GetFrameworkByIdQuery request, CancellationToken ct)
    {
        var framework = await _repository.GetWithRequirementsAsync(request.Id, ct);
        if (framework == null) return null;

        return new RegulatoryFrameworkDetailDto(
            framework.Id,
            framework.Code,
            framework.Name,
            framework.Description,
            framework.Type,
            framework.LegalReference,
            framework.RegulatoryBody,
            framework.EffectiveDate,
            framework.ExpirationDate,
            framework.IsActive,
            framework.Version,
            framework.Notes,
            framework.CreatedAt,
            framework.Requirements.Select(r => new ComplianceRequirementDto(
                r.Id, r.FrameworkId, r.Code, r.Title, r.Description, r.Criticality,
                r.ArticleReference, r.DeadlineDays, r.EvaluationFrequency,
                r.RequiresEvidence, r.RequiresApproval, r.IsActive
            )).ToList(),
            framework.Controls.Select(c => new ComplianceControlDto(
                c.Id, c.FrameworkId, c.RequirementId, c.Code, c.Name, c.Description,
                c.Type, c.ResponsibleRole, c.TestingFrequency, c.LastTestedAt,
                c.NextTestDate, c.Status, c.EffectivenessScore, c.IsActive
            )).ToList()
        );
    }
}

public class GetAllFrameworksHandler : IRequestHandler<GetAllFrameworksQuery, IEnumerable<RegulatoryFrameworkDto>>
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public GetAllFrameworksHandler(IRegulatoryFrameworkRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RegulatoryFrameworkDto>> Handle(GetAllFrameworksQuery request, CancellationToken ct)
    {
        var frameworks = await _repository.GetAllAsync(request.IncludeInactive, ct);
        
        return frameworks.Select(f => new RegulatoryFrameworkDto(
            f.Id, f.Code, f.Name, f.Description, f.Type, f.LegalReference,
            f.RegulatoryBody, f.EffectiveDate, f.ExpirationDate, f.IsActive,
            f.Version, f.CreatedAt, f.Requirements.Count, f.Controls.Count
        ));
    }
}

public class GetFrameworksByTypeHandler : IRequestHandler<GetFrameworksByTypeQuery, IEnumerable<RegulatoryFrameworkDto>>
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public GetFrameworksByTypeHandler(IRegulatoryFrameworkRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RegulatoryFrameworkDto>> Handle(GetFrameworksByTypeQuery request, CancellationToken ct)
    {
        var frameworks = await _repository.GetByTypeAsync(request.Type, ct);
        
        return frameworks.Select(f => new RegulatoryFrameworkDto(
            f.Id, f.Code, f.Name, f.Description, f.Type, f.LegalReference,
            f.RegulatoryBody, f.EffectiveDate, f.ExpirationDate, f.IsActive,
            f.Version, f.CreatedAt, f.Requirements.Count, f.Controls.Count
        ));
    }
}

#endregion

#region Assessment Query Handlers

public class GetAssessmentByIdHandler : IRequestHandler<GetAssessmentByIdQuery, ComplianceAssessmentDto?>
{
    private readonly IComplianceAssessmentRepository _repository;

    public GetAssessmentByIdHandler(IComplianceAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceAssessmentDto?> Handle(GetAssessmentByIdQuery request, CancellationToken ct)
    {
        var assessment = await _repository.GetByIdAsync(request.Id, ct);
        if (assessment == null) return null;

        return new ComplianceAssessmentDto(
            assessment.Id, assessment.EntityType, assessment.EntityId,
            assessment.RequirementId, assessment.Requirement?.Title,
            assessment.Status, assessment.AssessmentDate, assessment.AssessedBy,
            assessment.Score, assessment.Observations, assessment.NextAssessmentDate,
            assessment.DeadlineDate, assessment.Findings.Count
        );
    }
}

public class GetAssessmentsPaginatedHandler : IRequestHandler<GetAssessmentsPaginatedQuery, PaginatedResult<ComplianceAssessmentDto>>
{
    private readonly IComplianceAssessmentRepository _repository;

    public GetAssessmentsPaginatedHandler(IComplianceAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ComplianceAssessmentDto>> Handle(GetAssessmentsPaginatedQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.Page, request.PageSize, request.Status, ct);
        
        var dtos = items.Select(a => new ComplianceAssessmentDto(
            a.Id, a.EntityType, a.EntityId, a.RequirementId, a.Requirement?.Title,
            a.Status, a.AssessmentDate, a.AssessedBy, a.Score, a.Observations,
            a.NextAssessmentDate, a.DeadlineDate, a.Findings.Count
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        
        return new PaginatedResult<ComplianceAssessmentDto>(dtos, totalCount, request.Page, request.PageSize, totalPages);
    }
}

public class GetAssessmentStatisticsHandler : IRequestHandler<GetAssessmentStatisticsQuery, AssessmentStatisticsDto>
{
    private readonly IComplianceAssessmentRepository _repository;

    public GetAssessmentStatisticsHandler(IComplianceAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssessmentStatisticsDto> Handle(GetAssessmentStatisticsQuery request, CancellationToken ct)
    {
        var stats = await _repository.GetStatisticsAsync(ct);
        
        return new AssessmentStatisticsDto(
            stats.TotalAssessments,
            stats.CompliantCount,
            stats.NonCompliantCount,
            stats.PendingCount,
            stats.OverdueCount,
            stats.ComplianceRate
        );
    }
}

#endregion

#region Finding Query Handlers

public class GetFindingByIdHandler : IRequestHandler<GetFindingByIdQuery, ComplianceFindingDto?>
{
    private readonly IComplianceFindingRepository _repository;

    public GetFindingByIdHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceFindingDto?> Handle(GetFindingByIdQuery request, CancellationToken ct)
    {
        var finding = await _repository.GetByIdAsync(request.Id, ct);
        if (finding == null) return null;

        return new ComplianceFindingDto(
            finding.Id, finding.AssessmentId, finding.Title, finding.Description,
            finding.Type, finding.Status, finding.Criticality, finding.RootCause,
            finding.Impact, finding.Recommendation, finding.AssignedTo, finding.DueDate,
            finding.ResolvedAt, finding.Resolution, finding.CreatedAt,
            finding.RemediationActions.Count
        );
    }
}

public class GetFindingsPaginatedHandler : IRequestHandler<GetFindingsPaginatedQuery, PaginatedResult<ComplianceFindingDto>>
{
    private readonly IComplianceFindingRepository _repository;

    public GetFindingsPaginatedHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ComplianceFindingDto>> Handle(GetFindingsPaginatedQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.Page, request.PageSize, request.Status, ct);
        
        var dtos = items.Select(f => new ComplianceFindingDto(
            f.Id, f.AssessmentId, f.Title, f.Description, f.Type, f.Status,
            f.Criticality, f.RootCause, f.Impact, f.Recommendation, f.AssignedTo,
            f.DueDate, f.ResolvedAt, f.Resolution, f.CreatedAt, f.RemediationActions.Count
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        
        return new PaginatedResult<ComplianceFindingDto>(dtos, totalCount, request.Page, request.PageSize, totalPages);
    }
}

public class GetFindingStatisticsHandler : IRequestHandler<GetFindingStatisticsQuery, FindingStatisticsDto>
{
    private readonly IComplianceFindingRepository _repository;

    public GetFindingStatisticsHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<FindingStatisticsDto> Handle(GetFindingStatisticsQuery request, CancellationToken ct)
    {
        var stats = await _repository.GetStatisticsAsync(ct);
        
        return new FindingStatisticsDto(
            stats.TotalFindings,
            stats.OpenCount,
            stats.InProgressCount,
            stats.ResolvedCount,
            stats.OverdueCount,
            stats.CriticalCount,
            stats.ResolutionRate,
            stats.AverageResolutionDays
        );
    }
}

public class GetCriticalFindingsHandler : IRequestHandler<GetCriticalFindingsQuery, IEnumerable<ComplianceFindingDto>>
{
    private readonly IComplianceFindingRepository _repository;

    public GetCriticalFindingsHandler(IComplianceFindingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ComplianceFindingDto>> Handle(GetCriticalFindingsQuery request, CancellationToken ct)
    {
        var findings = await _repository.GetByTypeAsync(FindingType.CriticalNonConformity, ct);
        
        return findings.Select(f => new ComplianceFindingDto(
            f.Id, f.AssessmentId, f.Title, f.Description, f.Type, f.Status,
            f.Criticality, f.RootCause, f.Impact, f.Recommendation, f.AssignedTo,
            f.DueDate, f.ResolvedAt, f.Resolution, f.CreatedAt, f.RemediationActions.Count
        ));
    }
}

#endregion

#region Control Query Handlers

public class GetControlStatisticsHandler : IRequestHandler<GetControlStatisticsQuery, ControlStatisticsDto>
{
    private readonly IComplianceControlRepository _repository;

    public GetControlStatisticsHandler(IComplianceControlRepository repository)
    {
        _repository = repository;
    }

    public async Task<ControlStatisticsDto> Handle(GetControlStatisticsQuery request, CancellationToken ct)
    {
        var stats = await _repository.GetStatisticsAsync(ct);
        
        return new ControlStatisticsDto(
            stats.TotalControls,
            stats.CompliantControls,
            stats.NonCompliantControls,
            stats.PendingTestControls,
            stats.OverallEffectiveness
        );
    }
}

#endregion

#region Report Query Handlers

public class GetReportByIdHandler : IRequestHandler<GetReportByIdQuery, RegulatoryReportDetailDto?>
{
    private readonly IRegulatoryReportRepository _repository;

    public GetReportByIdHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegulatoryReportDetailDto?> Handle(GetReportByIdQuery request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report == null) return null;

        return new RegulatoryReportDetailDto(
            report.Id, report.ReportNumber, report.Type, report.RegulationType,
            report.Title, report.Description, report.PeriodStart, report.PeriodEnd,
            report.Status, report.RegulatoryBody, report.SubmissionDeadline,
            report.SubmittedAt, report.SubmittedBy, report.SubmissionReference,
            report.Content, report.Attachments, report.ReviewComments,
            report.RegulatoryResponse, report.PreparedBy, report.PreparedAt,
            report.ReviewedBy, report.ReviewedAt, report.ApprovedBy,
            report.ApprovedAt, report.CreatedAt
        );
    }
}

public class GetPendingReportsHandler : IRequestHandler<GetPendingReportsQuery, IEnumerable<RegulatoryReportDto>>
{
    private readonly IRegulatoryReportRepository _repository;

    public GetPendingReportsHandler(IRegulatoryReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RegulatoryReportDto>> Handle(GetPendingReportsQuery request, CancellationToken ct)
    {
        var reports = await _repository.GetPendingSubmissionAsync(ct);
        
        return reports.Select(r => new RegulatoryReportDto(
            r.Id, r.ReportNumber, r.Type, r.RegulationType, r.Title,
            r.PeriodStart, r.PeriodEnd, r.Status, r.RegulatoryBody,
            r.SubmissionDeadline, r.SubmittedAt, r.SubmissionReference, r.CreatedAt
        ));
    }
}

#endregion

#region Training Query Handlers

public class GetTrainingStatisticsHandler : IRequestHandler<GetTrainingStatisticsQuery, TrainingStatisticsDto>
{
    private readonly ITrainingCompletionRepository _repository;

    public GetTrainingStatisticsHandler(ITrainingCompletionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrainingStatisticsDto> Handle(GetTrainingStatisticsQuery request, CancellationToken ct)
    {
        var stats = await _repository.GetStatisticsAsync(ct);
        
        return new TrainingStatisticsDto(
            stats.TotalTrainings,
            stats.TotalCompletions,
            stats.PassedCount,
            stats.FailedCount,
            stats.ExpiringSoonCount,
            stats.PassRate
        );
    }
}

#endregion

#region Calendar Query Handlers

public class GetUpcomingCalendarItemsHandler : IRequestHandler<GetUpcomingCalendarItemsQuery, IEnumerable<ComplianceCalendarDto>>
{
    private readonly IComplianceCalendarRepository _repository;

    public GetUpcomingCalendarItemsHandler(IComplianceCalendarRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ComplianceCalendarDto>> Handle(GetUpcomingCalendarItemsQuery request, CancellationToken ct)
    {
        var items = await _repository.GetUpcomingAsync(request.DaysAhead, ct);
        
        return items.Select(i => new ComplianceCalendarDto(
            i.Id, i.Title, i.Description, i.RegulationType, i.DueDate,
            i.ReminderDaysBefore, i.IsRecurring, i.RecurrencePattern,
            i.AssignedTo, i.Status, i.CompletedAt, i.NotificationSent
        ));
    }
}

#endregion

#region Dashboard Query Handler

public class GetComplianceDashboardHandler : IRequestHandler<GetComplianceDashboardQuery, ComplianceDashboardDto>
{
    private readonly IComplianceAssessmentRepository _assessmentRepository;
    private readonly IComplianceFindingRepository _findingRepository;
    private readonly IComplianceControlRepository _controlRepository;
    private readonly ITrainingCompletionRepository _trainingRepository;
    private readonly IComplianceCalendarRepository _calendarRepository;
    private readonly IRegulatoryReportRepository _reportRepository;
    private readonly IComplianceMetricRepository _metricRepository;

    public GetComplianceDashboardHandler(
        IComplianceAssessmentRepository assessmentRepository,
        IComplianceFindingRepository findingRepository,
        IComplianceControlRepository controlRepository,
        ITrainingCompletionRepository trainingRepository,
        IComplianceCalendarRepository calendarRepository,
        IRegulatoryReportRepository reportRepository,
        IComplianceMetricRepository metricRepository)
    {
        _assessmentRepository = assessmentRepository;
        _findingRepository = findingRepository;
        _controlRepository = controlRepository;
        _trainingRepository = trainingRepository;
        _calendarRepository = calendarRepository;
        _reportRepository = reportRepository;
        _metricRepository = metricRepository;
    }

    public async Task<ComplianceDashboardDto> Handle(GetComplianceDashboardQuery request, CancellationToken ct)
    {
        var assessmentStats = await _assessmentRepository.GetStatisticsAsync(ct);
        var findingStats = await _findingRepository.GetStatisticsAsync(ct);
        var controlStats = await _controlRepository.GetStatisticsAsync(ct);
        var trainingStats = await _trainingRepository.GetStatisticsAsync(ct);

        var upcomingDeadlines = await _calendarRepository.GetUpcomingAsync(30, ct);
        var criticalFindings = await _findingRepository.GetByTypeAsync(FindingType.CriticalNonConformity, ct);
        var pendingReports = await _reportRepository.GetPendingSubmissionAsync(ct);
        var outOfTargetMetrics = await _metricRepository.GetOutOfTargetAsync(ct);

        return new ComplianceDashboardDto(
            new AssessmentStatisticsDto(
                assessmentStats.TotalAssessments, assessmentStats.CompliantCount,
                assessmentStats.NonCompliantCount, assessmentStats.PendingCount,
                assessmentStats.OverdueCount, assessmentStats.ComplianceRate
            ),
            new FindingStatisticsDto(
                findingStats.TotalFindings, findingStats.OpenCount, findingStats.InProgressCount,
                findingStats.ResolvedCount, findingStats.OverdueCount, findingStats.CriticalCount,
                findingStats.ResolutionRate, findingStats.AverageResolutionDays
            ),
            new ControlStatisticsDto(
                controlStats.TotalControls, controlStats.CompliantControls,
                controlStats.NonCompliantControls, controlStats.PendingTestControls,
                controlStats.OverallEffectiveness
            ),
            new TrainingStatisticsDto(
                trainingStats.TotalTrainings, trainingStats.TotalCompletions,
                trainingStats.PassedCount, trainingStats.FailedCount,
                trainingStats.ExpiringSoonCount, trainingStats.PassRate
            ),
            upcomingDeadlines.Select(i => new ComplianceCalendarDto(
                i.Id, i.Title, i.Description, i.RegulationType, i.DueDate,
                i.ReminderDaysBefore, i.IsRecurring, i.RecurrencePattern,
                i.AssignedTo, i.Status, i.CompletedAt, i.NotificationSent
            )).ToList(),
            criticalFindings.Where(f => f.Status != FindingStatus.Closed).Take(10)
                .Select(f => new ComplianceFindingDto(
                    f.Id, f.AssessmentId, f.Title, f.Description, f.Type, f.Status,
                    f.Criticality, f.RootCause, f.Impact, f.Recommendation, f.AssignedTo,
                    f.DueDate, f.ResolvedAt, f.Resolution, f.CreatedAt, f.RemediationActions.Count
                )).ToList(),
            pendingReports.Take(10).Select(r => new RegulatoryReportDto(
                r.Id, r.ReportNumber, r.Type, r.RegulationType, r.Title,
                r.PeriodStart, r.PeriodEnd, r.Status, r.RegulatoryBody,
                r.SubmissionDeadline, r.SubmittedAt, r.SubmissionReference, r.CreatedAt
            )).ToList(),
            outOfTargetMetrics.Select(m => new ComplianceMetricDto(
                m.Id, m.RegulationType, m.MetricName, m.Description,
                m.PeriodStart, m.PeriodEnd, m.Value, m.Unit, m.Target,
                m.Threshold, m.IsWithinTarget, m.CalculatedAt
            )).ToList()
        );
    }
}

#endregion
