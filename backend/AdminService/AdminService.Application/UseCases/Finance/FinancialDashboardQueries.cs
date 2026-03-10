using MediatR;

namespace AdminService.Application.UseCases.Finance;

/// <summary>
/// Query to get the full financial dashboard for the admin internal view.
/// Aggregates expenses, revenue, margin, and runway projection.
/// </summary>
public record GetFinancialDashboardQuery(
    /// <summary>Period in YYYY-MM format. Defaults to current month.</summary>
    string? Period = null
) : IRequest<FinancialDashboardDto>;
