namespace AuditService.Shared.AuditMessages;

public interface IAuditEventProducer
{
    Task PublishAuditEventAsync(AuditEvent auditEvent);
}