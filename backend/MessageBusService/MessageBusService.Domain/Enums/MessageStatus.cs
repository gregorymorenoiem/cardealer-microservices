namespace MessageBusService.Domain.Enums;

public enum MessageStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    DeadLettered = 4
}
