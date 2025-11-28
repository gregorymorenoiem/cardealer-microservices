using AuditService.Application.DTOs;
using AuditService.Shared;
using MediatR;

namespace AuditService.Application.Features.Audit.Queries.GetAuditLogById;

public class GetAuditLogByIdQuery : IRequest<ApiResponse<AuditLogDto>>
{
    public string Id { get; set; } = string.Empty;

    public GetAuditLogByIdQuery(string id)
    {
        Id = id;
    }

    public GetAuditLogByIdQuery() { }
}