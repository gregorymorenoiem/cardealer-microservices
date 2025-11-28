using MediatR;

namespace AuthService.Domain.Events;

public class EmailConfirmedEvent : INotification
{
    public string UserId { get; }
    public string Email { get; }
    public DateTime OccurredAt { get; }

    public EmailConfirmedEvent(string userId, string email)
    {
        UserId = userId;
        Email = email;
        OccurredAt = DateTime.UtcNow;
    }
}