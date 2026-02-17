using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using System.Security.Claims;

namespace NotificationService.Api.Controllers;

/// <summary>
/// Controller for managing user notifications (in-app notifications shown in header)
/// </summary>
[ApiController]
[Authorize]
[Route("api/notifications")]
public class UserNotificationsController : ControllerBase
{
    private readonly ILogger<UserNotificationsController> _logger;
    
    // In-memory storage for demo - in production use database
    private static readonly List<UserNotification> _notifications = InitializeMockNotifications();

    public UserNotificationsController(ILogger<UserNotificationsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get paginated notifications for the current user
    /// </summary>
    [HttpGet]
    public ActionResult<UserNotificationsListResponse> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "User ID not found in token" });
        
        _logger.LogInformation("Getting notifications for user {UserId}, page {Page}, pageSize {PageSize}", 
            userId, page, pageSize);

        var query = _notifications
            .Where(n => n.UserId.ToString() == userId || n.UserId == Guid.Empty) // Include global notifications
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(n => n.CreatedAt);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt);
        }

        var total = query.Count();
        var unreadCount = _notifications.Count(n => 
            (n.UserId.ToString() == userId || n.UserId == Guid.Empty) && !n.IsRead);

        var notifications = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return Ok(new UserNotificationsListResponse(
            Notifications: notifications,
            Total: total,
            UnreadCount: unreadCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: (int)Math.Ceiling((double)total / pageSize)
        ));
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    [HttpGet("unread/count")]
    public ActionResult<UnreadCountResponse> GetUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "User ID not found in token" });
        
        var count = _notifications.Count(n => 
            (n.UserId.ToString() == userId || n.UserId == Guid.Empty) && !n.IsRead);

        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPatch("{notificationId}/read")]
    public ActionResult MarkAsRead(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id.ToString() == notificationId);
        
        if (notification == null)
        {
            return NotFound(new { Message = "Notification not found" });
        }

        notification.MarkAsRead();
        _logger.LogInformation("Marked notification {NotificationId} as read", notificationId);

        return Ok(new { Message = "Notification marked as read" });
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPatch("read-all")]
    public ActionResult MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "User ID not found in token" });
        
        var userNotifications = _notifications
            .Where(n => (n.UserId.ToString() == userId || n.UserId == Guid.Empty) && !n.IsRead);

        foreach (var notification in userNotifications)
        {
            notification.MarkAsRead();
        }

        _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);

        return Ok(new { Message = "All notifications marked as read" });
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{notificationId}")]
    public ActionResult DeleteNotification(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id.ToString() == notificationId);
        
        if (notification == null)
        {
            return NotFound(new { Message = "Notification not found" });
        }

        _notifications.Remove(notification);
        _logger.LogInformation("Deleted notification {NotificationId}", notificationId);

        return Ok(new { Message = "Notification deleted" });
    }

    /// <summary>
    /// Delete all read notifications
    /// </summary>
    [HttpDelete("read")]
    public ActionResult DeleteAllRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "User ID not found in token" });
        
        var readNotifications = _notifications
            .Where(n => (n.UserId.ToString() == userId || n.UserId == Guid.Empty) && n.IsRead)
            .ToList();

        foreach (var notification in readNotifications)
        {
            _notifications.Remove(notification);
        }

        _logger.LogInformation("Deleted {Count} read notifications for user {UserId}", 
            readNotifications.Count, userId);

        return Ok(new { Message = $"Deleted {readNotifications.Count} read notifications" });
    }

    /// <summary>
    /// Create a new notification (system/admin use)
    /// </summary>
    [HttpPost]
    public ActionResult<UserNotificationDto> CreateNotification(
        [FromBody] CreateUserNotificationRequest request)
    {
        var notification = UserNotification.Create(
            userId: Guid.TryParse(request.UserId, out var uid) ? uid : Guid.Empty,
            type: request.Type,
            title: request.Title,
            message: request.Message,
            icon: request.Icon,
            link: request.Link,
            dealerId: Guid.TryParse(request.DealerId, out var did) ? did : null,
            expiresAt: DateTime.TryParse(request.ExpiresAt, out var exp) ? exp : null
        );

        _notifications.Insert(0, notification);
        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", 
            notification.Id, request.UserId);

        return CreatedAtAction(nameof(GetNotifications), MapToDto(notification));
    }

    /// <summary>
    /// Send bulk notifications to multiple users
    /// </summary>
    [HttpPost("bulk")]
    public ActionResult BulkCreateNotifications([FromBody] BulkNotificationRequest request)
    {
        var created = 0;
        foreach (var userId in request.UserIds)
        {
            var notification = UserNotification.Create(
                userId: Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
                type: request.Type,
                title: request.Title,
                message: request.Message,
                icon: request.Icon,
                link: request.Link
            );
            _notifications.Insert(0, notification);
            created++;
        }

        _logger.LogInformation("Created {Count} bulk notifications", created);
        return Ok(new { Message = $"Created {created} notifications" });
    }

    #region Helper Methods

    private static UserNotificationDto MapToDto(UserNotification n)
    {
        return new UserNotificationDto(
            Id: n.Id.ToString(),
            UserId: n.UserId.ToString(),
            Type: n.Type,
            Title: n.Title,
            Message: n.Message,
            Icon: n.Icon,
            Link: n.Link,
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt.ToString("o"),
            ReadAt: n.ReadAt?.ToString("o"),
            ExpiresAt: n.ExpiresAt?.ToString("o")
        );
    }

    private static List<UserNotification> InitializeMockNotifications()
    {
        return new List<UserNotification>
        {
            UserNotification.Create(
                userId: Guid.Empty, // Global notification
                type: "message",
                title: "Nuevo mensaje recibido",
                message: "Juan Pérez respondió sobre el Toyota Corolla 2024",
                icon: "💬",
                link: "/messages"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "favorite",
                title: "¡Tu vehículo fue guardado!",
                message: "Tu BMW Serie 3 fue añadido a favoritos por 3 usuarios",
                icon: "❤️"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "approval",
                title: "Publicación aprobada",
                message: "Tu Honda Accord 2023 ya está visible en el marketplace",
                icon: "✅",
                link: "/dealer/listings"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "system",
                title: "¡Alerta de precio!",
                message: "Un vehículo en tus búsquedas guardadas bajó $50,000 de precio",
                icon: "💰",
                link: "/browse"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "sale",
                title: "¡Vehículo vendido!",
                message: "¡Felicidades! Tu Hyundai Tucson ha sido marcado como vendido.",
                icon: "🎉"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "vehicle",
                title: "Nuevo lead recibido",
                message: "María García está interesada en tu Mercedes-Benz C300",
                icon: "🚗",
                link: "/dealer/leads"
            ),
            UserNotification.Create(
                userId: Guid.Empty,
                type: "system",
                title: "Actualización del sistema",
                message: "Hemos añadido nuevas funciones de análisis a tu dashboard",
                icon: "📢"
            )
        };
    }

    #endregion
}
