// ComplianceService - Queries

namespace ComplianceService.Application.Queries;

using MediatR;
using ComplianceService.Domain.Entities;
using ComplianceService.Application.DTOs;

#region Framework Queries

public record GetFrameworkByIdQuery(Guid Id) : IRequest<RegulatoryFrameworkDetailDto?>;

public record GetFrameworkByCodeQuery(string Code) : IRequest<RegulatoryFrameworkDto?>;

public record GetAllFrameworksQuery(bool IncludeInactive = false) : IRequest<IEnumerable<RegulatoryFrameworkDto>>;

public record GetFrameworksByTypeQuery(RegulationType Type) : IRequest<IEnumerable<RegulatoryFrameworkDto>>;

#endregion

#region Requirement Queries

public record GetRequirementByIdQuery(Guid Id) : IRequest<ComplianceRequirementDto?>;

public record GetRequirementsByFrameworkQuery(Guid FrameworkId) : IRequest<IEnumerable<ComplianceRequirementDto>>;

public record GetRequirementsByCriticalityQuery(CriticalityLevel Criticality) : IRequest<IEnumerable<ComplianceRequirementDto>>;

public record GetUpcomingRequirementDeadlinesQuery(int DaysAhead = 30) : IRequest<IEnumerable<ComplianceRequirementDto>>;

#endregion

#region Control Queries

public record GetControlByIdQuery(Guid Id) : IRequest<ComplianceControlDto?>;

public record GetControlsByFrameworkQuery(Guid FrameworkId) : IRequest<IEnumerable<ComplianceControlDto>>;

public record GetControlsByStatusQuery(ComplianceStatus Status) : IRequest<IEnumerable<ComplianceControlDto>>;

public record GetControlsDueForTestingQuery(int DaysAhead = 30) : IRequest<IEnumerable<ComplianceControlDto>>;

public record GetControlStatisticsQuery() : IRequest<ControlStatisticsDto>;

public record GetControlTestsQuery(Guid ControlId) : IRequest<IEnumerable<ControlTestDto>>;

#endregion

#region Assessment Queries

public record GetAssessmentByIdQuery(Guid Id) : IRequest<ComplianceAssessmentDto?>;

public record GetAssessmentsByEntityQuery(string EntityType, Guid EntityId) : IRequest<IEnumerable<ComplianceAssessmentDto>>;

public record GetAssessmentsByStatusQuery(ComplianceStatus Status) : IRequest<IEnumerable<ComplianceAssessmentDto>>;

public record GetOverdueAssessmentsQuery() : IRequest<IEnumerable<ComplianceAssessmentDto>>;

public record GetAssessmentsPaginatedQuery(
    int Page = 1,
    int PageSize = 20,
    ComplianceStatus? Status = null
) : IRequest<PaginatedResult<ComplianceAssessmentDto>>;

public record GetAssessmentStatisticsQuery() : IRequest<AssessmentStatisticsDto>;

#endregion

#region Finding Queries

public record GetFindingByIdQuery(Guid Id) : IRequest<ComplianceFindingDto?>;

public record GetFindingsByAssessmentQuery(Guid AssessmentId) : IRequest<IEnumerable<ComplianceFindingDto>>;

public record GetFindingsByStatusQuery(FindingStatus Status) : IRequest<IEnumerable<ComplianceFindingDto>>;

public record GetOverdueFindingsQuery() : IRequest<IEnumerable<ComplianceFindingDto>>;

public record GetFindingsByAssignedToQuery(string UserId) : IRequest<IEnumerable<ComplianceFindingDto>>;

public record GetFindingsPaginatedQuery(
    int Page = 1,
    int PageSize = 20,
    FindingStatus? Status = null
) : IRequest<PaginatedResult<ComplianceFindingDto>>;

public record GetFindingStatisticsQuery() : IRequest<FindingStatisticsDto>;

public record GetCriticalFindingsQuery() : IRequest<IEnumerable<ComplianceFindingDto>>;

#endregion

#region Remediation Queries

public record GetRemediationByIdQuery(Guid Id) : IRequest<RemediationActionDto?>;

public record GetRemediationsByFindingQuery(Guid FindingId) : IRequest<IEnumerable<RemediationActionDto>>;

public record GetRemediationsByAssignedToQuery(string UserId) : IRequest<IEnumerable<RemediationActionDto>>;

public record GetOverdueRemediationsQuery() : IRequest<IEnumerable<RemediationActionDto>>;

#endregion

#region Report Queries

public record GetReportByIdQuery(Guid Id) : IRequest<RegulatoryReportDetailDto?>;

public record GetReportByNumberQuery(string ReportNumber) : IRequest<RegulatoryReportDto?>;

public record GetReportsByTypeQuery(RegulatoryReportType Type) : IRequest<IEnumerable<RegulatoryReportDto>>;

public record GetReportsByStatusQuery(ReportStatus Status) : IRequest<IEnumerable<RegulatoryReportDto>>;

public record GetPendingReportsQuery() : IRequest<IEnumerable<RegulatoryReportDto>>;

public record GetReportsByPeriodQuery(DateTime Start, DateTime End) : IRequest<IEnumerable<RegulatoryReportDto>>;

public record GetReportsPaginatedQuery(
    int Page = 1,
    int PageSize = 20,
    ReportStatus? Status = null
) : IRequest<PaginatedResult<RegulatoryReportDto>>;

#endregion

#region Calendar Queries

public record GetCalendarItemByIdQuery(Guid Id) : IRequest<ComplianceCalendarDto?>;

public record GetUpcomingCalendarItemsQuery(int DaysAhead = 30) : IRequest<IEnumerable<ComplianceCalendarDto>>;

public record GetCalendarItemsByAssignedToQuery(string UserId) : IRequest<IEnumerable<ComplianceCalendarDto>>;

public record GetOverdueCalendarItemsQuery() : IRequest<IEnumerable<ComplianceCalendarDto>>;

public record GetCalendarItemsByRegulationQuery(RegulationType Type) : IRequest<IEnumerable<ComplianceCalendarDto>>;

#endregion

#region Training Queries

public record GetTrainingByIdQuery(Guid Id) : IRequest<ComplianceTrainingDto?>;

public record GetAllActiveTrainingsQuery() : IRequest<IEnumerable<ComplianceTrainingDto>>;

public record GetMandatoryTrainingsQuery() : IRequest<IEnumerable<ComplianceTrainingDto>>;

public record GetTrainingsByRegulationQuery(RegulationType Type) : IRequest<IEnumerable<ComplianceTrainingDto>>;

public record GetTrainingCompletionsByUserQuery(Guid UserId) : IRequest<IEnumerable<TrainingCompletionDto>>;

public record GetTrainingCompletionsByTrainingQuery(Guid TrainingId) : IRequest<IEnumerable<TrainingCompletionDto>>;

public record GetExpiringTrainingCompletionsQuery(int DaysAhead = 30) : IRequest<IEnumerable<TrainingCompletionDto>>;

public record GetTrainingStatisticsQuery() : IRequest<TrainingStatisticsDto>;

#endregion

#region Metric Queries

public record GetMetricsByRegulationQuery(RegulationType Type) : IRequest<IEnumerable<ComplianceMetricDto>>;

public record GetMetricsByPeriodQuery(DateTime Start, DateTime End) : IRequest<IEnumerable<ComplianceMetricDto>>;

public record GetMetricTrendQuery(string MetricName, int Count = 12) : IRequest<IEnumerable<ComplianceMetricDto>>;

public record GetOutOfTargetMetricsQuery() : IRequest<IEnumerable<ComplianceMetricDto>>;

#endregion

#region Dashboard Queries

public record GetComplianceDashboardQuery() : IRequest<ComplianceDashboardDto>;

#endregion
