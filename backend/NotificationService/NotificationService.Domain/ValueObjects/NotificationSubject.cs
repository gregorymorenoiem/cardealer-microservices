namespace NotificationService.Domain.ValueObjects;

public record NotificationSubject
{
    public string Value { get; }

    public NotificationSubject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Subject cannot be empty");

        if (value.Length > 200)
            throw new ArgumentException("Subject too long");

        Value = value.Trim();
    }

    public static implicit operator string(NotificationSubject subject) => subject.Value;
    public static implicit operator NotificationSubject(string subject) => new(subject);

    public override string ToString() => Value;

    public string Truncate(int maxLength)
    {
        return Value.Length <= maxLength ? Value : Value[..maxLength] + "...";
    }
}