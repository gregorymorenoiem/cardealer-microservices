// ComplianceService - Repositories

namespace ComplianceService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Infrastructure.Persistence;

#region Framework Repository

public class RegulatoryFrameworkRepository : IRegulatoryFrameworkRepository
{
    private readonly ComplianceDbContext _context;

    public RegulatoryFrameworkRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<RegulatoryFramework?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks.FindAsync(new object[] { id }, ct);
    }

    public async Task<RegulatoryFramework?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks
            .FirstOrDefaultAsync(f => f.Code == code, ct);
    }

    public async Task<IEnumerable<RegulatoryFramework>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.RegulatoryFrameworks
            .Include(f => f.Requirements)
            .Include(f => f.Controls)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(f => f.IsActive);

        return await query.OrderBy(f => f.Name).ToListAsync(ct);
    }

    public async Task<IEnumerable<RegulatoryFramework>> GetByTypeAsync(RegulationType type, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks
            .Include(f => f.Requirements)
            .Include(f => f.Controls)
            .Where(f => f.Type == type && f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);
    }

    public async Task<RegulatoryFramework?> GetWithRequirementsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks
            .Include(f => f.Requirements)
            .Include(f => f.Controls)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<RegulatoryFramework?> GetWithControlsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks
            .Include(f => f.Controls)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task AddAsync(RegulatoryFramework framework, CancellationToken ct = default)
    {
        await _context.RegulatoryFrameworks.AddAsync(framework, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegulatoryFramework framework, CancellationToken ct = default)
    {
        _context.RegulatoryFrameworks.Update(framework);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks.AnyAsync(f => f.Id == id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        return await _context.RegulatoryFrameworks.AnyAsync(f => f.Code == code, ct);
    }
}

#endregion

#region Requirement Repository

public class ComplianceRequirementRepository : IComplianceRequirementRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceRequirementRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceRequirements
            .Include(r => r.Framework)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IEnumerable<ComplianceRequirement>> GetByFrameworkIdAsync(Guid frameworkId, CancellationToken ct = default)
    {
        return await _context.ComplianceRequirements
            .Where(r => r.FrameworkId == frameworkId && r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceRequirement>> GetByCriticalityAsync(CriticalityLevel criticality, CancellationToken ct = default)
    {
        return await _context.ComplianceRequirements
            .Where(r => r.Criticality == criticality && r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceRequirement>> GetUpcomingDeadlinesAsync(int daysAhead, CancellationToken ct = default)
    {
        // This would typically be based on assessments, but for simplicity we return all active
        return await _context.ComplianceRequirements
            .Where(r => r.IsActive)
            .OrderBy(r => r.DeadlineDays)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceRequirement requirement, CancellationToken ct = default)
    {
        await _context.ComplianceRequirements.AddAsync(requirement, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceRequirement requirement, CancellationToken ct = default)
    {
        _context.ComplianceRequirements.Update(requirement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceRequirements.CountAsync(r => r.IsActive, ct);
    }
}

#endregion

#region Control Repository

public class ComplianceControlRepository : IComplianceControlRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceControlRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceControl?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceControls
            .Include(c => c.Tests)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<ComplianceControl>> GetByFrameworkIdAsync(Guid frameworkId, CancellationToken ct = default)
    {
        return await _context.ComplianceControls
            .Where(c => c.FrameworkId == frameworkId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceControl>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default)
    {
        return await _context.ComplianceControls
            .Where(c => c.RequirementId == requirementId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceControl>> GetByStatusAsync(ComplianceStatus status, CancellationToken ct = default)
    {
        return await _context.ComplianceControls
            .Where(c => c.Status == status && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceControl>> GetDueForTestingAsync(DateTime beforeDate, CancellationToken ct = default)
    {
        return await _context.ComplianceControls
            .Where(c => c.IsActive && (c.NextTestDate == null || c.NextTestDate <= beforeDate))
            .OrderBy(c => c.NextTestDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceControl control, CancellationToken ct = default)
    {
        await _context.ComplianceControls.AddAsync(control, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceControl control, CancellationToken ct = default)
    {
        _context.ComplianceControls.Update(control);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ControlStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var controls = await _context.ComplianceControls.Where(c => c.IsActive).ToListAsync(ct);
        
        return new ControlStatistics
        {
            TotalControls = controls.Count,
            CompliantControls = controls.Count(c => c.Status == ComplianceStatus.Compliant),
            NonCompliantControls = controls.Count(c => c.Status == ComplianceStatus.NonCompliant),
            PendingTestControls = controls.Count(c => c.Status == ComplianceStatus.NotEvaluated || c.NextTestDate <= DateTime.UtcNow),
            OverallEffectiveness = controls.Any() ? controls.Average(c => c.EffectivenessScore) : 0
        };
    }
}

#endregion

#region Control Test Repository

public class ControlTestRepository : IControlTestRepository
{
    private readonly ComplianceDbContext _context;

    public ControlTestRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ControlTest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ControlTests.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<ControlTest>> GetByControlIdAsync(Guid controlId, CancellationToken ct = default)
    {
        return await _context.ControlTests
            .Where(t => t.ControlId == controlId)
            .OrderByDescending(t => t.TestDate)
            .ToListAsync(ct);
    }

    public async Task<ControlTest?> GetLatestByControlIdAsync(Guid controlId, CancellationToken ct = default)
    {
        return await _context.ControlTests
            .Where(t => t.ControlId == controlId)
            .OrderByDescending(t => t.TestDate)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(ControlTest test, CancellationToken ct = default)
    {
        await _context.ControlTests.AddAsync(test, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Assessment Repository

public class ComplianceAssessmentRepository : IComplianceAssessmentRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceAssessmentRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceAssessment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceAssessments
            .Include(a => a.Requirement)
            .Include(a => a.Findings)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IEnumerable<ComplianceAssessment>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _context.ComplianceAssessments
            .Include(a => a.Requirement)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceAssessment>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default)
    {
        return await _context.ComplianceAssessments
            .Where(a => a.RequirementId == requirementId)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceAssessment>> GetByStatusAsync(ComplianceStatus status, CancellationToken ct = default)
    {
        return await _context.ComplianceAssessments
            .Include(a => a.Requirement)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceAssessment>> GetOverdueAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceAssessments
            .Include(a => a.Requirement)
            .Where(a => a.DeadlineDate != null && a.DeadlineDate < DateTime.UtcNow &&
                       a.Status != ComplianceStatus.Compliant)
            .OrderBy(a => a.DeadlineDate)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<ComplianceAssessment> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, ComplianceStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.ComplianceAssessments
            .Include(a => a.Requirement)
            .Include(a => a.Findings)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AssessmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(ComplianceAssessment assessment, CancellationToken ct = default)
    {
        await _context.ComplianceAssessments.AddAsync(assessment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceAssessment assessment, CancellationToken ct = default)
    {
        _context.ComplianceAssessments.Update(assessment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<AssessmentStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var assessments = await _context.ComplianceAssessments.ToListAsync(ct);
        var total = assessments.Count;
        
        return new AssessmentStatistics
        {
            TotalAssessments = total,
            CompliantCount = assessments.Count(a => a.Status == ComplianceStatus.Compliant),
            NonCompliantCount = assessments.Count(a => a.Status == ComplianceStatus.NonCompliant),
            PendingCount = assessments.Count(a => a.Status == ComplianceStatus.Pending || a.Status == ComplianceStatus.InProgress),
            OverdueCount = assessments.Count(a => a.DeadlineDate != null && a.DeadlineDate < DateTime.UtcNow && a.Status != ComplianceStatus.Compliant),
            ComplianceRate = total > 0 
                ? (double)assessments.Count(a => a.Status == ComplianceStatus.Compliant) / total * 100 
                : 0
        };
    }
}

#endregion
