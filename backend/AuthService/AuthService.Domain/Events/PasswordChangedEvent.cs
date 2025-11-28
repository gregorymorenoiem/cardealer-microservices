using MediatR;

namespace AuthService.Domain.Events;

public class PasswordChangedEvent : INotification
{
    public string UserId { get; }
    public DateTime OccurredAt { get; }

    public PasswordChangedEvent(string userId)
    {
        UserId = userId;
        OccurredAt = DateTime.UtcNow;
    }
}
