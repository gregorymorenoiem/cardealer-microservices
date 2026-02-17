// ComplianceService - Repository Interfaces

namespace ComplianceService.Domain.Interfaces;

using ComplianceService.Domain.Entities;

#region Framework & Requirements

public interface IRegulatoryFrameworkRepository
{
    Task<RegulatoryFramework?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RegulatoryFramework?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryFramework>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryFramework>> GetByTypeAsync(RegulationType type, CancellationToken ct = default);
    Task<RegulatoryFramework?> GetWithRequirementsAsync(Guid id, CancellationToken ct = default);
    Task<RegulatoryFramework?> GetWithControlsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RegulatoryFramework framework, CancellationToken ct = default);
    Task UpdateAsync(RegulatoryFramework framework, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}

public interface IComplianceRequirementRepository
{
    Task<ComplianceRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceRequirement>> GetByFrameworkIdAsync(Guid frameworkId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceRequirement>> GetByCriticalityAsync(CriticalityLevel criticality, CancellationToken ct = default);
    Task<IEnumerable<ComplianceRequirement>> GetUpcomingDeadlinesAsync(int daysAhead, CancellationToken ct = default);
    Task AddAsync(ComplianceRequirement requirement, CancellationToken ct = default);
    Task UpdateAsync(ComplianceRequirement requirement, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}

#endregion

#region Controls

public interface IComplianceControlRepository
{
    Task<ComplianceControl?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceControl>> GetByFrameworkIdAsync(Guid frameworkId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceControl>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceControl>> GetByStatusAsync(ComplianceStatus status, CancellationToken ct = default);
    Task<IEnumerable<ComplianceControl>> GetDueForTestingAsync(DateTime beforeDate, CancellationToken ct = default);
    Task AddAsync(ComplianceControl control, CancellationToken ct = default);
    Task UpdateAsync(ComplianceControl control, CancellationToken ct = default);
    Task<ControlStatistics> GetStatisticsAsync(CancellationToken ct = default);
}

public class ControlStatistics
{
    public int TotalControls { get; set; }
    public int CompliantControls { get; set; }
    public int NonCompliantControls { get; set; }
    public int PendingTestControls { get; set; }
    public double OverallEffectiveness { get; set; }
}

public interface IControlTestRepository
{
    Task<ControlTest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ControlTest>> GetByControlIdAsync(Guid controlId, CancellationToken ct = default);
    Task<ControlTest?> GetLatestByControlIdAsync(Guid controlId, CancellationToken ct = default);
    Task AddAsync(ControlTest test, CancellationToken ct = default);
}

#endregion

#region Assessments & Findings

public interface IComplianceAssessmentRepository
{
    Task<ComplianceAssessment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceAssessment>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceAssessment>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceAssessment>> GetByStatusAsync(ComplianceStatus status, CancellationToken ct = default);
    Task<IEnumerable<ComplianceAssessment>> GetOverdueAsync(CancellationToken ct = default);
    Task<(IEnumerable<ComplianceAssessment> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, ComplianceStatus? status = null, CancellationToken ct = default);
    Task AddAsync(ComplianceAssessment assessment, CancellationToken ct = default);
    Task UpdateAsync(ComplianceAssessment assessment, CancellationToken ct = default);
    Task<AssessmentStatistics> GetStatisticsAsync(CancellationToken ct = default);
}

public class AssessmentStatistics
{
    public int TotalAssessments { get; set; }
    public int CompliantCount { get; set; }
    public int NonCompliantCount { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public double ComplianceRate { get; set; }
}

public interface IComplianceFindingRepository
{
    Task<ComplianceFinding?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceFinding>> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceFinding>> GetByStatusAsync(FindingStatus status, CancellationToken ct = default);
    Task<IEnumerable<ComplianceFinding>> GetByTypeAsync(FindingType type, CancellationToken ct = default);
    Task<IEnumerable<ComplianceFinding>> GetOverdueAsync(CancellationToken ct = default);
    Task<IEnumerable<ComplianceFinding>> GetByAssignedToAsync(string userId, CancellationToken ct = default);
    Task<(IEnumerable<ComplianceFinding> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, FindingStatus? status = null, CancellationToken ct = default);
    Task AddAsync(ComplianceFinding finding, CancellationToken ct = default);
    Task UpdateAsync(ComplianceFinding finding, CancellationToken ct = default);
    Task<FindingStatistics> GetStatisticsAsync(CancellationToken ct = default);
}

public class FindingStatistics
{
    public int TotalFindings { get; set; }
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int OverdueCount { get; set; }
    public int CriticalCount { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionDays { get; set; }
}

public interface IRemediationActionRepository
{
    Task<RemediationAction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RemediationAction>> GetByFindingIdAsync(Guid findingId, CancellationToken ct = default);
    Task<IEnumerable<RemediationAction>> GetByAssignedToAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<RemediationAction>> GetOverdueAsync(CancellationToken ct = default);
    Task AddAsync(RemediationAction action, CancellationToken ct = default);
    Task UpdateAsync(RemediationAction action, CancellationToken ct = default);
}

#endregion

#region Reports

public interface IRegulatoryReportRepository
{
    Task<RegulatoryReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RegulatoryReport?> GetByReportNumberAsync(string reportNumber, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryReport>> GetByTypeAsync(RegulatoryReportType type, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryReport>> GetByStatusAsync(ReportStatus status, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryReport>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IEnumerable<RegulatoryReport>> GetPendingSubmissionAsync(CancellationToken ct = default);
    Task<(IEnumerable<RegulatoryReport> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, ReportStatus? status = null, CancellationToken ct = default);
    Task AddAsync(RegulatoryReport report, CancellationToken ct = default);
    Task UpdateAsync(RegulatoryReport report, CancellationToken ct = default);
    Task<string> GenerateReportNumberAsync(RegulatoryReportType type, CancellationToken ct = default);
}

#endregion

#region Calendar & Training

public interface IComplianceCalendarRepository
{
    Task<ComplianceCalendar?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceCalendar>> GetUpcomingAsync(int daysAhead, CancellationToken ct = default);
    Task<IEnumerable<ComplianceCalendar>> GetByAssignedToAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<ComplianceCalendar>> GetOverdueAsync(CancellationToken ct = default);
    Task<IEnumerable<ComplianceCalendar>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default);
    Task AddAsync(ComplianceCalendar item, CancellationToken ct = default);
    Task UpdateAsync(ComplianceCalendar item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IComplianceTrainingRepository
{
    Task<ComplianceTraining?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceTraining>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<ComplianceTraining>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default);
    Task<IEnumerable<ComplianceTraining>> GetMandatoryAsync(CancellationToken ct = default);
    Task AddAsync(ComplianceTraining training, CancellationToken ct = default);
    Task UpdateAsync(ComplianceTraining training, CancellationToken ct = default);
}

public interface ITrainingCompletionRepository
{
    Task<TrainingCompletion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TrainingCompletion>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<TrainingCompletion>> GetByTrainingIdAsync(Guid trainingId, CancellationToken ct = default);
    Task<TrainingCompletion?> GetByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken ct = default);
    Task<IEnumerable<TrainingCompletion>> GetExpiringAsync(int daysAhead, CancellationToken ct = default);
    Task AddAsync(TrainingCompletion completion, CancellationToken ct = default);
    Task UpdateAsync(TrainingCompletion completion, CancellationToken ct = default);
    Task<TrainingStatistics> GetStatisticsAsync(CancellationToken ct = default);
}

public class TrainingStatistics
{
    public int TotalTrainings { get; set; }
    public int TotalCompletions { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public double PassRate { get; set; }
}

#endregion

#region Metrics

public interface IComplianceMetricRepository
{
    Task<ComplianceMetric?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ComplianceMetric>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default);
    Task<IEnumerable<ComplianceMetric>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IEnumerable<ComplianceMetric>> GetLatestByMetricNameAsync(string metricName, int count = 12, CancellationToken ct = default);
    Task<IEnumerable<ComplianceMetric>> GetOutOfTargetAsync(CancellationToken ct = default);
    Task AddAsync(ComplianceMetric metric, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ComplianceMetric> metrics, CancellationToken ct = default);
}

#endregion
