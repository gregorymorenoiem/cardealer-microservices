namespace AlertService.Domain.Entities;

public class SavedSearch
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string SearchCriteria { get; private set; } // JSON con filtros
    public bool SendEmailNotifications { get; private set; }
    public NotificationFrequency Frequency { get; private set; }
    public DateTime? LastNotificationSent { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // For EF Core
    private SavedSearch() 
    { 
        Name = string.Empty;
        SearchCriteria = string.Empty;
    }

    public SavedSearch(
        Guid userId,
        string name,
        string searchCriteria,
        bool sendEmailNotifications = true,
        NotificationFrequency frequency = NotificationFrequency.Daily)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(searchCriteria))
            throw new ArgumentException("Search criteria is required", nameof(searchCriteria));

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        SearchCriteria = searchCriteria;
        SendEmailNotifications = sendEmailNotifications;
        Frequency = frequency;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name is required", nameof(newName));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSearchCriteria(string newCriteria)
    {
        if (string.IsNullOrWhiteSpace(newCriteria))
            throw new ArgumentException("Search criteria is required", nameof(newCriteria));

        SearchCriteria = newCriteria;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotificationSettings(bool sendEmail, NotificationFrequency frequency)
    {
        SendEmailNotifications = sendEmail;
        Frequency = frequency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkNotificationSent()
    {
        LastNotificationSent = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ShouldSendNotification()
    {
        if (!IsActive || !SendEmailNotifications)
            return false;

        if (LastNotificationSent == null)
            return true;

        var timeSinceLastNotification = DateTime.UtcNow - LastNotificationSent.Value;

        return Frequency switch
        {
            NotificationFrequency.Instant => true,
            NotificationFrequency.Daily => timeSinceLastNotification.TotalHours >= 24,
            NotificationFrequency.Weekly => timeSinceLastNotification.TotalDays >= 7,
            _ => false
        };
    }
}

public enum NotificationFrequency
{
    Instant = 0,  // Inmediatamente cuando hay nuevos resultados
    Daily = 1,    // Una vez al día
    Weekly = 2    // Una vez a la semana
}
