using MediatR;
using KYCService.Domain.Entities;
using KYCService.Domain.Interfaces;
using KYCService.Application.Queries;
using KYCService.Application.DTOs;

namespace KYCService.Application.Handlers;

/// <summary>
/// Handler para obtener perfil KYC por ID
/// </summary>
public class GetKYCProfileByIdHandler : IRequestHandler<GetKYCProfileByIdQuery, KYCProfileDto?>
{
    private readonly IKYCProfileRepository _repository;
    private readonly IKYCDocumentRepository _documentRepository;
    private readonly IKYCVerificationRepository _verificationRepository;

    public GetKYCProfileByIdHandler(
        IKYCProfileRepository repository,
        IKYCDocumentRepository documentRepository,
        IKYCVerificationRepository verificationRepository)
    {
        _repository = repository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
    }

    public async Task<KYCProfileDto?> Handle(GetKYCProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile == null) return null;

        var documents = await _documentRepository.GetByProfileIdAsync(request.Id, cancellationToken);
        var verifications = await _verificationRepository.GetByProfileIdAsync(request.Id, cancellationToken);

        return MapToDto(profile, documents, verifications);
    }

    private static KYCProfileDto MapToDto(KYCProfile p, List<KYCDocument> docs, List<KYCVerification> vers) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        RiskFactors = p.RiskFactors,
        FullName = p.FullName,
        MiddleName = p.MiddleName,
        LastName = p.LastName,
        DateOfBirth = p.DateOfBirth,
        PlaceOfBirth = p.PlaceOfBirth,
        Nationality = p.Nationality,
        PrimaryDocumentType = p.PrimaryDocumentType,
        PrimaryDocumentNumber = p.PrimaryDocumentNumber,
        PrimaryDocumentExpiry = p.PrimaryDocumentExpiry,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        City = p.City,
        Province = p.Province,
        Country = p.Country,
        IsPEP = p.IsPEP,
        PEPPosition = p.PEPPosition,
        BusinessName = p.BusinessName,
        RNC = p.RNC,
        IdentityVerifiedAt = p.IdentityVerifiedAt,
        AddressVerifiedAt = p.AddressVerifiedAt,
        CreatedAt = p.CreatedAt,
        ApprovedAt = p.ApprovedAt,
        ExpiresAt = p.ExpiresAt,
        NextReviewAt = p.NextReviewAt,
        Documents = docs.Select(d => new KYCDocumentDto
        {
            Id = d.Id,
            KYCProfileId = d.KYCProfileId,
            Type = d.Type,
            DocumentName = d.DocumentName,
            FileName = d.FileName,
            FileUrl = d.FileUrl,
            FileType = d.FileType,
            FileSize = d.FileSize,
            Status = d.Status,
            RejectionReason = d.RejectionReason,
            UploadedAt = d.UploadedAt,
            VerifiedAt = d.VerifiedAt
        }).ToList(),
        Verifications = vers.Select(v => new KYCVerificationDto
        {
            Id = v.Id,
            KYCProfileId = v.KYCProfileId,
            VerificationType = v.VerificationType,
            Provider = v.Provider,
            Passed = v.Passed,
            FailureReason = v.FailureReason,
            ConfidenceScore = v.ConfidenceScore,
            VerifiedAt = v.VerifiedAt,
            ExpiresAt = v.ExpiresAt
        }).ToList()
    };
}

/// <summary>
/// Handler para obtener perfil KYC por User ID
/// </summary>
public class GetKYCProfileByUserIdHandler : IRequestHandler<GetKYCProfileByUserIdQuery, KYCProfileDto?>
{
    private readonly IKYCProfileRepository _repository;

    public GetKYCProfileByUserIdHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto?> Handle(GetKYCProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null) return null;

        return MapToDto(profile);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        FullName = p.FullName,
        IsPEP = p.IsPEP,
        CreatedAt = p.CreatedAt,
        ApprovedAt = p.ApprovedAt,
        ExpiresAt = p.ExpiresAt
    };
}

/// <summary>
/// Handler para obtener perfil KYC por número de documento
/// </summary>
public class GetKYCProfileByDocumentHandler : IRequestHandler<GetKYCProfileByDocumentQuery, KYCProfileDto?>
{
    private readonly IKYCProfileRepository _repository;

    public GetKYCProfileByDocumentHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCProfileDto?> Handle(GetKYCProfileByDocumentQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByDocumentNumberAsync(request.DocumentNumber, cancellationToken);
        if (profile == null) return null;

        return MapToDto(profile);
    }

    private static KYCProfileDto MapToDto(KYCProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        EntityType = p.EntityType,
        Status = p.Status,
        RiskLevel = p.RiskLevel,
        RiskScore = p.RiskScore,
        FullName = p.FullName,
        PrimaryDocumentType = p.PrimaryDocumentType,
        PrimaryDocumentNumber = p.PrimaryDocumentNumber,
        IsPEP = p.IsPEP,
        CreatedAt = p.CreatedAt
    };
}

/// <summary>
/// Handler para listar perfiles KYC
/// </summary>
public class GetKYCProfilesHandler : IRequestHandler<GetKYCProfilesQuery, PaginatedResult<KYCProfileSummaryDto>>
{
    private readonly IKYCProfileRepository _repository;
    private readonly IKYCDocumentRepository _documentRepository;

    public GetKYCProfilesHandler(IKYCProfileRepository repository, IKYCDocumentRepository documentRepository)
    {
        _repository = repository;
        _documentRepository = documentRepository;
    }

    public async Task<PaginatedResult<KYCProfileSummaryDto>> Handle(GetKYCProfilesQuery request, CancellationToken cancellationToken)
    {
        List<KYCProfile> profiles;
        int totalCount;

        if (request.Status.HasValue)
        {
            profiles = await _repository.GetByStatusAsync(request.Status.Value, request.Page, request.PageSize, cancellationToken);
            totalCount = await _repository.GetCountByStatusAsync(request.Status.Value, cancellationToken);
        }
        else if (request.RiskLevel.HasValue)
        {
            profiles = await _repository.GetByRiskLevelAsync(request.RiskLevel.Value, request.Page, request.PageSize, cancellationToken);
            totalCount = profiles.Count; // Simplificado
        }
        else if (request.IsPEP == true)
        {
            profiles = await _repository.GetPEPProfilesAsync(request.Page, request.PageSize, cancellationToken);
            totalCount = profiles.Count;
        }
        else
        {
            profiles = await _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var stats = await _repository.GetStatisticsAsync(cancellationToken);
            totalCount = stats.TotalProfiles;
        }

        var summaries = new List<KYCProfileSummaryDto>();
        foreach (var p in profiles)
        {
            var docs = await _documentRepository.GetByProfileIdAsync(p.Id, cancellationToken);
            summaries.Add(new KYCProfileSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.FullName,
                EntityType = p.EntityType,
                Status = p.Status,
                RiskLevel = p.RiskLevel,
                IsPEP = p.IsPEP,
                CreatedAt = p.CreatedAt,
                ExpiresAt = p.ExpiresAt,
                DocumentsCount = docs.Count,
                HasPendingDocuments = docs.Any(d => d.Status == KYCDocumentStatus.Pending)
            });
        }

        return new PaginatedResult<KYCProfileSummaryDto>
        {
            Items = summaries,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Handler para obtener perfiles pendientes
/// </summary>
public class GetPendingKYCProfilesHandler : IRequestHandler<GetPendingKYCProfilesQuery, PaginatedResult<KYCProfileSummaryDto>>
{
    private readonly IKYCProfileRepository _repository;

    public GetPendingKYCProfilesHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<KYCProfileSummaryDto>> Handle(GetPendingKYCProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetPendingReviewAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await _repository.GetCountByStatusAsync(KYCStatus.UnderReview, cancellationToken);

        return new PaginatedResult<KYCProfileSummaryDto>
        {
            Items = profiles.Select(p => new KYCProfileSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.FullName,
                EntityType = p.EntityType,
                Status = p.Status,
                RiskLevel = p.RiskLevel,
                IsPEP = p.IsPEP,
                CreatedAt = p.CreatedAt,
                ExpiresAt = p.ExpiresAt
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Handler para obtener perfiles próximos a expirar
/// </summary>
public class GetExpiringKYCProfilesHandler : IRequestHandler<GetExpiringKYCProfilesQuery, PaginatedResult<KYCProfileSummaryDto>>
{
    private readonly IKYCProfileRepository _repository;

    public GetExpiringKYCProfilesHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<KYCProfileSummaryDto>> Handle(GetExpiringKYCProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetExpiringAsync(request.DaysUntilExpiry, request.Page, request.PageSize, cancellationToken);

        return new PaginatedResult<KYCProfileSummaryDto>
        {
            Items = profiles.Select(p => new KYCProfileSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = p.FullName,
                EntityType = p.EntityType,
                Status = p.Status,
                RiskLevel = p.RiskLevel,
                IsPEP = p.IsPEP,
                CreatedAt = p.CreatedAt,
                ExpiresAt = p.ExpiresAt
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = profiles.Count
        };
    }
}

/// <summary>
/// Handler para obtener documentos de un perfil
/// </summary>
public class GetKYCDocumentsHandler : IRequestHandler<GetKYCDocumentsQuery, List<KYCDocumentDto>>
{
    private readonly IKYCDocumentRepository _repository;

    public GetKYCDocumentsHandler(IKYCDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<KYCDocumentDto>> Handle(GetKYCDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetByProfileIdAsync(request.KYCProfileId, cancellationToken);

        return documents.Select(d => new KYCDocumentDto
        {
            Id = d.Id,
            KYCProfileId = d.KYCProfileId,
            Type = d.Type,
            DocumentName = d.DocumentName,
            FileName = d.FileName,
            FileUrl = d.FileUrl,
            FileType = d.FileType,
            FileSize = d.FileSize,
            Status = d.Status,
            RejectionReason = d.RejectionReason,
            UploadedAt = d.UploadedAt,
            VerifiedAt = d.VerifiedAt
        }).ToList();
    }
}

/// <summary>
/// Handler para obtener verificaciones de un perfil
/// </summary>
public class GetKYCVerificationsHandler : IRequestHandler<GetKYCVerificationsQuery, List<KYCVerificationDto>>
{
    private readonly IKYCVerificationRepository _repository;

    public GetKYCVerificationsHandler(IKYCVerificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<KYCVerificationDto>> Handle(GetKYCVerificationsQuery request, CancellationToken cancellationToken)
    {
        var verifications = await _repository.GetByProfileIdAsync(request.KYCProfileId, cancellationToken);

        return verifications.Select(v => new KYCVerificationDto
        {
            Id = v.Id,
            KYCProfileId = v.KYCProfileId,
            VerificationType = v.VerificationType,
            Provider = v.Provider,
            Passed = v.Passed,
            FailureReason = v.FailureReason,
            ConfidenceScore = v.ConfidenceScore,
            VerifiedAt = v.VerifiedAt,
            ExpiresAt = v.ExpiresAt
        }).ToList();
    }
}

/// <summary>
/// Handler para obtener historial de evaluaciones de riesgo
/// </summary>
public class GetKYCRiskAssessmentsHandler : IRequestHandler<GetKYCRiskAssessmentsQuery, List<KYCRiskAssessmentDto>>
{
    private readonly IKYCRiskAssessmentRepository _repository;

    public GetKYCRiskAssessmentsHandler(IKYCRiskAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<KYCRiskAssessmentDto>> Handle(GetKYCRiskAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var assessments = await _repository.GetByProfileIdAsync(request.KYCProfileId, cancellationToken);

        return assessments.Select(a => new KYCRiskAssessmentDto
        {
            Id = a.Id,
            KYCProfileId = a.KYCProfileId,
            PreviousLevel = a.PreviousLevel,
            NewLevel = a.NewLevel,
            PreviousScore = a.PreviousScore,
            NewScore = a.NewScore,
            Reason = a.Reason,
            Factors = a.Factors,
            RecommendedActions = a.RecommendedActions,
            AssessedByName = a.AssessedByName,
            AssessedAt = a.AssessedAt
        }).ToList();
    }
}

/// <summary>
/// Handler para obtener estadísticas KYC
/// </summary>
public class GetKYCStatisticsHandler : IRequestHandler<GetKYCStatisticsQuery, KYCStatisticsDto>
{
    private readonly IKYCProfileRepository _repository;

    public GetKYCStatisticsHandler(IKYCProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<KYCStatisticsDto> Handle(GetKYCStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatisticsAsync(cancellationToken);

        return new KYCStatisticsDto
        {
            TotalProfiles = stats.TotalProfiles,
            PendingProfiles = stats.PendingProfiles,
            InProgressProfiles = stats.InProgressProfiles,
            ApprovedProfiles = stats.ApprovedProfiles,
            RejectedProfiles = stats.RejectedProfiles,
            ExpiredProfiles = stats.ExpiredProfiles,
            HighRiskProfiles = stats.HighRiskProfiles,
            PEPProfiles = stats.PEPProfiles,
            ExpiringIn30Days = stats.ExpiringIn30Days,
            ApprovalRate = stats.TotalProfiles > 0 ? (double)stats.ApprovedProfiles / stats.TotalProfiles * 100 : 0,
            HighRiskPercentage = stats.TotalProfiles > 0 ? (double)stats.HighRiskProfiles / stats.TotalProfiles * 100 : 0
        };
    }
}

/// <summary>
/// Handler para obtener STR por ID
/// </summary>
public class GetSTRByIdHandler : IRequestHandler<GetSTRByIdQuery, SuspiciousTransactionReportDto?>
{
    private readonly ISuspiciousTransactionReportRepository _repository;

    public GetSTRByIdHandler(ISuspiciousTransactionReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<SuspiciousTransactionReportDto?> Handle(GetSTRByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (report == null) return null;

        return MapToDto(report);
    }

    private static SuspiciousTransactionReportDto MapToDto(SuspiciousTransactionReport r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        KYCProfileId = r.KYCProfileId,
        ReportNumber = r.ReportNumber,
        SuspiciousActivityType = r.SuspiciousActivityType,
        Description = r.Description,
        Amount = r.Amount,
        Currency = r.Currency,
        RedFlags = r.RedFlags,
        Status = r.Status,
        DetectedAt = r.DetectedAt,
        ReportingDeadline = r.ReportingDeadline,
        UAFReportNumber = r.UAFReportNumber,
        SentToUAFAt = r.SentToUAFAt,
        CreatedByName = r.CreatedByName,
        CreatedAt = r.CreatedAt
    };
}

/// <summary>
/// Handler para obtener STR por número
/// </summary>
public class GetSTRByNumberHandler : IRequestHandler<GetSTRByNumberQuery, SuspiciousTransactionReportDto?>
{
    private readonly ISuspiciousTransactionReportRepository _repository;

    public GetSTRByNumberHandler(ISuspiciousTransactionReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<SuspiciousTransactionReportDto?> Handle(GetSTRByNumberQuery request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByReportNumberAsync(request.ReportNumber, cancellationToken);
        if (report == null) return null;

        return new SuspiciousTransactionReportDto
        {
            Id = report.Id,
            UserId = report.UserId,
            KYCProfileId = report.KYCProfileId,
            ReportNumber = report.ReportNumber,
            SuspiciousActivityType = report.SuspiciousActivityType,
            Description = report.Description,
            Amount = report.Amount,
            Currency = report.Currency,
            RedFlags = report.RedFlags,
            Status = report.Status,
            DetectedAt = report.DetectedAt,
            ReportingDeadline = report.ReportingDeadline,
            UAFReportNumber = report.UAFReportNumber,
            SentToUAFAt = report.SentToUAFAt,
            CreatedByName = report.CreatedByName,
            CreatedAt = report.CreatedAt
        };
    }
}

/// <summary>
/// Handler para listar STRs
/// </summary>
public class GetSTRsHandler : IRequestHandler<GetSTRsQuery, PaginatedResult<SuspiciousTransactionReportDto>>
{
    private readonly ISuspiciousTransactionReportRepository _repository;

    public GetSTRsHandler(ISuspiciousTransactionReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<SuspiciousTransactionReportDto>> Handle(GetSTRsQuery request, CancellationToken cancellationToken)
    {
        List<SuspiciousTransactionReport> reports;

        if (request.IsOverdue == true)
        {
            reports = await _repository.GetOverdueAsync(cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            reports = await _repository.GetByStatusAsync(request.Status.Value, request.Page, request.PageSize, cancellationToken);
        }
        else
        {
            reports = await _repository.GetByStatusAsync(STRStatus.Draft, request.Page, request.PageSize, cancellationToken);
            // Agregar otros estados también
            var pending = await _repository.GetByStatusAsync(STRStatus.PendingReview, request.Page, request.PageSize, cancellationToken);
            reports.AddRange(pending);
        }

        return new PaginatedResult<SuspiciousTransactionReportDto>
        {
            Items = reports.Select(r => new SuspiciousTransactionReportDto
            {
                Id = r.Id,
                UserId = r.UserId,
                KYCProfileId = r.KYCProfileId,
                ReportNumber = r.ReportNumber,
                SuspiciousActivityType = r.SuspiciousActivityType,
                Description = r.Description,
                Amount = r.Amount,
                Currency = r.Currency,
                RedFlags = r.RedFlags,
                Status = r.Status,
                DetectedAt = r.DetectedAt,
                ReportingDeadline = r.ReportingDeadline,
                UAFReportNumber = r.UAFReportNumber,
                SentToUAFAt = r.SentToUAFAt,
                CreatedByName = r.CreatedByName,
                CreatedAt = r.CreatedAt
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = reports.Count
        };
    }
}

/// <summary>
/// Handler para obtener estadísticas de STR
/// </summary>
public class GetSTRStatisticsHandler : IRequestHandler<GetSTRStatisticsQuery, STRStatisticsDto>
{
    private readonly ISuspiciousTransactionReportRepository _repository;

    public GetSTRStatisticsHandler(ISuspiciousTransactionReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<STRStatisticsDto> Handle(GetSTRStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatisticsAsync(cancellationToken);

        return new STRStatisticsDto
        {
            TotalReports = stats.TotalReports,
            DraftReports = stats.DraftReports,
            PendingReviewReports = stats.PendingReviewReports,
            ApprovedReports = stats.ApprovedReports,
            SentToUAFReports = stats.SentToUAFReports,
            OverdueReports = stats.OverdueReports,
            TotalAmountReported = stats.TotalAmountReported
        };
    }
}

/// <summary>
/// Handler para buscar en watchlist
/// </summary>
public class SearchWatchlistHandler : IRequestHandler<SearchWatchlistQuery, List<WatchlistEntryDto>>
{
    private readonly IWatchlistRepository _repository;

    public SearchWatchlistHandler(IWatchlistRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WatchlistEntryDto>> Handle(SearchWatchlistQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.SearchAsync(request.SearchTerm, request.ListType, cancellationToken);

        return entries.Select(e => new WatchlistEntryDto
        {
            Id = e.Id,
            ListType = e.ListType,
            Source = e.Source,
            FullName = e.FullName,
            Aliases = e.Aliases,
            DocumentNumber = e.DocumentNumber,
            DateOfBirth = e.DateOfBirth,
            Nationality = e.Nationality,
            Details = e.Details,
            ListedDate = e.ListedDate,
            IsActive = e.IsActive
        }).ToList();
    }
}

/// <summary>
/// Handler para obtener entrada de watchlist por ID
/// </summary>
public class GetWatchlistEntryByIdHandler : IRequestHandler<GetWatchlistEntryByIdQuery, WatchlistEntryDto?>
{
    private readonly IWatchlistRepository _repository;

    public GetWatchlistEntryByIdHandler(IWatchlistRepository repository)
    {
        _repository = repository;
    }

    public async Task<WatchlistEntryDto?> Handle(GetWatchlistEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry == null) return null;

        return new WatchlistEntryDto
        {
            Id = entry.Id,
            ListType = entry.ListType,
            Source = entry.Source,
            FullName = entry.FullName,
            Aliases = entry.Aliases,
            DocumentNumber = entry.DocumentNumber,
            DateOfBirth = entry.DateOfBirth,
            Nationality = entry.Nationality,
            Details = entry.Details,
            ListedDate = entry.ListedDate,
            IsActive = entry.IsActive
        };
    }
}
