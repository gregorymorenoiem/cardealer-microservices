using MediatR;
using RegulatoryAlertService.Application.DTOs;
using RegulatoryAlertService.Domain.Entities;
using RegulatoryAlertService.Domain.Interfaces;

namespace RegulatoryAlertService.Application.Features.Alerts.Commands;

// ===== CREATE ALERT =====

public record CreateAlertCommand(CreateAlertDto Dto) : IRequest<RegulatoryAlertDto>;

public class CreateAlertHandler : IRequestHandler<CreateAlertCommand, RegulatoryAlertDto>
{
    private readonly IRegulatoryAlertRepository _repository;

    public CreateAlertHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegulatoryAlertDto> Handle(CreateAlertCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var alert = new RegulatoryAlert(
            dto.Title,
            dto.Description,
            dto.AlertType,
            dto.Priority,
            dto.RegulatoryBody,
            dto.Category,
            dto.CreatedBy);

        if (dto.EffectiveDate.HasValue)
            alert.SetEffectiveDate(dto.EffectiveDate.Value);

        if (dto.DeadlineDate.HasValue)
            alert.SetDeadline(dto.DeadlineDate.Value);

        if (!string.IsNullOrEmpty(dto.LegalReference))
            alert.SetLegalReference(dto.LegalReference, dto.OfficialDocumentUrl);

        if (!string.IsNullOrEmpty(dto.ActionRequired))
            alert.SetActionRequired(dto.ActionRequired);

        if (!string.IsNullOrEmpty(dto.Tags))
            alert.SetTags(dto.Tags);

        var created = await _repository.AddAsync(alert, ct);

        return MapToDto(created);
    }

    private static RegulatoryAlertDto MapToDto(RegulatoryAlert a) => new(
        a.Id, a.Title, a.Description, a.DetailedContent, a.AlertType, a.Priority,
        a.Status, a.RegulatoryBody, a.Category, a.EffectiveDate, a.DeadlineDate,
        a.ExpirationDate, a.LegalReference, a.OfficialDocumentUrl, a.RequiresAction,
        a.ActionRequired, a.Tags, a.CreatedAt);
}

// ===== PUBLISH ALERT =====

public record PublishAlertCommand(Guid AlertId) : IRequest<bool>;

public class PublishAlertHandler : IRequestHandler<PublishAlertCommand, bool>
{
    private readonly IRegulatoryAlertRepository _repository;

    public PublishAlertHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(PublishAlertCommand request, CancellationToken ct)
    {
        var alert = await _repository.GetByIdAsync(request.AlertId, ct)
            ?? throw new InvalidOperationException($"Alerta {request.AlertId} no encontrada");

        alert.Publish();
        await _repository.UpdateAsync(alert, ct);
        return true;
    }
}

// ===== RESOLVE ALERT =====

public record ResolveAlertCommand(Guid AlertId, string Resolution) : IRequest<bool>;

public class ResolveAlertHandler : IRequestHandler<ResolveAlertCommand, bool>
{
    private readonly IRegulatoryAlertRepository _repository;

    public ResolveAlertHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken ct)
    {
        var alert = await _repository.GetByIdAsync(request.AlertId, ct)
            ?? throw new InvalidOperationException($"Alerta {request.AlertId} no encontrada");

        alert.Resolve(request.Resolution);
        await _repository.UpdateAsync(alert, ct);
        return true;
    }
}

// ===== CREATE SUBSCRIPTION =====

public record CreateSubscriptionCommand(CreateSubscriptionDto Dto) : IRequest<AlertSubscriptionDto>;

public class CreateSubscriptionHandler : IRequestHandler<CreateSubscriptionCommand, AlertSubscriptionDto>
{
    private readonly IAlertSubscriptionRepository _repository;

    public CreateSubscriptionHandler(IAlertSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<AlertSubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var subscription = new AlertSubscription(
            dto.UserId,
            dto.Frequency,
            dto.PreferredChannel,
            dto.Email);

        if (dto.RegulatoryBody.HasValue)
            subscription.FilterByRegulatoryBody(dto.RegulatoryBody.Value);

        if (dto.Category.HasValue)
            subscription.FilterByCategory(dto.Category.Value);

        subscription.SetMinimumPriority(dto.MinimumPriority);

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
            subscription.UpdateContactInfo(dto.Email, dto.PhoneNumber);

        var created = await _repository.AddAsync(subscription, ct);

        return MapToDto(created);
    }

    private static AlertSubscriptionDto MapToDto(AlertSubscription s) => new(
        s.Id, s.UserId, s.RegulatoryBody, s.Category, s.MinimumPriority,
        s.Frequency, s.PreferredChannel, s.IsActive, s.Email, s.PhoneNumber);
}

// ===== CREATE CALENDAR ENTRY =====

public record CreateCalendarEntryCommand(CreateCalendarEntryDto Dto) : IRequest<RegulatoryCalendarEntryDto>;

public class CreateCalendarEntryHandler : IRequestHandler<CreateCalendarEntryCommand, RegulatoryCalendarEntryDto>
{
    private readonly IRegulatoryCalendarEntryRepository _repository;

    public CreateCalendarEntryHandler(IRegulatoryCalendarEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegulatoryCalendarEntryDto> Handle(CreateCalendarEntryCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var entry = new RegulatoryCalendarEntry(
            dto.Title,
            dto.Description,
            dto.RegulatoryBody,
            dto.Category,
            dto.DueDate,
            dto.IsRecurring);

        if (dto.IsRecurring && !string.IsNullOrEmpty(dto.RecurrencePattern))
            entry.SetRecurrence(dto.RecurrencePattern);

        entry.SetReminder(dto.ReminderDaysBefore);

        var created = await _repository.AddAsync(entry, ct);

        return MapToDto(created);
    }

    private static RegulatoryCalendarEntryDto MapToDto(RegulatoryCalendarEntry e) => new(
        e.Id, e.Title, e.Description, e.RegulatoryBody, e.Category, e.DueDate,
        e.IsRecurring, e.RecurrencePattern, e.LegalBasis, e.ReminderDaysBefore);
}

// ===== CREATE DEADLINE =====

public record CreateDeadlineCommand(CreateDeadlineDto Dto) : IRequest<ComplianceDeadlineDto>;

public class CreateDeadlineHandler : IRequestHandler<CreateDeadlineCommand, ComplianceDeadlineDto>
{
    private readonly IComplianceDeadlineRepository _repository;

    public CreateDeadlineHandler(IComplianceDeadlineRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceDeadlineDto> Handle(CreateDeadlineCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var deadline = new ComplianceDeadline(
            dto.UserId,
            dto.Title,
            dto.DueDate,
            dto.Priority);

        if (dto.CalendarEntryId.HasValue)
            deadline.LinkToCalendar(dto.CalendarEntryId.Value);

        if (dto.AlertId.HasValue)
            deadline.LinkToAlert(dto.AlertId.Value);

        var created = await _repository.AddAsync(deadline, ct);

        return MapToDto(created);
    }

    private static ComplianceDeadlineDto MapToDto(ComplianceDeadline d) => new(
        d.Id, d.UserId, d.Title, d.Description, d.DueDate, d.IsCompleted,
        d.CompletedAt, d.Priority, !d.IsCompleted && d.DueDate < DateTime.UtcNow);
}

// ===== COMPLETE DEADLINE =====

public record CompleteDeadlineCommand(CompleteDeadlineDto Dto) : IRequest<bool>;

public class CompleteDeadlineHandler : IRequestHandler<CompleteDeadlineCommand, bool>
{
    private readonly IComplianceDeadlineRepository _repository;

    public CompleteDeadlineHandler(IComplianceDeadlineRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteDeadlineCommand request, CancellationToken ct)
    {
        var deadline = await _repository.GetByIdAsync(request.Dto.Id, ct)
            ?? throw new InvalidOperationException($"Deadline {request.Dto.Id} no encontrado");

        deadline.Complete(request.Dto.CompletedBy, request.Dto.Notes);
        await _repository.UpdateAsync(deadline, ct);
        return true;
    }
}
