// ContractService - Queries (Consistent with Handlers)

namespace ContractService.Application.Queries;

using MediatR;
using ContractService.Domain.Entities;
using ContractService.Application.DTOs;

#region Template Queries

public record GetTemplateByIdQuery(Guid Id) : IRequest<ContractTemplateDto?>;

public record GetTemplateByCodeQuery(string Code) : IRequest<ContractTemplateDto?>;

public record GetActiveTemplatesQuery() : IRequest<List<ContractTemplateDto>>;

public record GetTemplatesByTypeQuery(ContractType Type) : IRequest<List<ContractTemplateDto>>;

#endregion

#region Contract Queries

public record GetContractByIdQuery(Guid Id) : IRequest<ContractDto?>;

public record GetContractByNumberQuery(string ContractNumber) : IRequest<ContractDto?>;

public record GetUserContractsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<ContractSummaryDto>>;

public record GetSubjectContractsQuery(string SubjectType, Guid SubjectId) : IRequest<List<ContractSummaryDto>>;

public record GetContractsByStatusQuery(ContractStatus Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ContractSummaryDto>>;

public record GetExpiringContractsQuery(int DaysUntilExpiration = 30) : IRequest<List<ContractSummaryDto>>;

#endregion

#region Party Queries

public record GetContractPartiesQuery(Guid ContractId) : IRequest<List<ContractPartyDto>>;

#endregion

#region Signature Queries

public record GetContractSignaturesQuery(Guid ContractId) : IRequest<List<ContractSignatureDto>>;

public record GetSignatureByIdQuery(Guid Id) : IRequest<ContractSignatureDto?>;

public record GetPendingSignaturesQuery(Guid UserId) : IRequest<List<ContractSignatureDto>>;

#endregion

#region Clause Queries

public record GetContractClausesQuery(Guid ContractId) : IRequest<List<ContractClauseDto>>;

#endregion

#region Version Queries

public record GetContractVersionsQuery(Guid ContractId) : IRequest<List<ContractVersionDto>>;

#endregion

#region Document Queries

public record GetContractDocumentsQuery(Guid ContractId) : IRequest<List<ContractDocumentDto>>;

#endregion

#region Audit Queries

public record GetContractAuditLogQuery(Guid ContractId) : IRequest<List<ContractAuditLogDto>>;

public record GetUserContractActivityQuery(string UserId, DateTime From, DateTime To) : IRequest<List<ContractAuditLogDto>>;

#endregion

#region Certification Authority Queries

public record GetCertificationAuthoritiesQuery() : IRequest<List<CertificationAuthorityDto>>;

public record GetActiveCertificationAuthoritiesQuery() : IRequest<List<CertificationAuthorityDto>>;

#endregion
