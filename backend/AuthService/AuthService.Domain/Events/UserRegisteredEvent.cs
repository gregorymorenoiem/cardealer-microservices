using MediatR;

namespace AuthService.Domain.Events;

public class UserRegisteredEvent : INotification
{
    public string UserId { get; }
    public string Email { get; }
    public string UserName { get; }
    public DateTime OccurredAt { get; }

    public UserRegisteredEvent(string userId, string email, string userName)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        OccurredAt = DateTime.UtcNow;
    }
}
