using MediatR;
using System;

namespace AdminService.Application.UseCases.Reports.ResolveReport
{
    public record ResolveReportCommand(
        Guid ReportId,
        string ResolvedBy,
        string Resolution,
        string ReporterEmail,
        string ReportSubject
    ) : IRequest<bool>;
}
