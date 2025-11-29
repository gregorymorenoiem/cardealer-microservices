namespace CarDealer.Contracts.Enums;

/// <summary>
/// Enumeration of all microservices in the CarDealer system.
/// Used for logging, monitoring, and event routing.
/// </summary>
public enum ServiceNames
{
    Gateway,
    AuthService,
    VehicleService,
    MediaService,
    NotificationService,
    ErrorService,
    AuditService,
    AdminService,
    ContactService
}
