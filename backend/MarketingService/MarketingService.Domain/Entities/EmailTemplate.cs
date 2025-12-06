using CarDealer.Shared.MultiTenancy;

namespace MarketingService.Domain.Entities;

public enum TemplateType
{
    Email,
    Sms,
    WhatsApp,
    Push
}

public class EmailTemplate : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public TemplateType Type { get; private set; }

    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? HtmlBody { get; private set; }

    public string? PreheaderText { get; private set; }
    public string? FromName { get; private set; }
    public string? FromEmail { get; private set; }
    public string? ReplyToEmail { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    public string Category { get; private set; } = "General";
    public string? Tags { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private EmailTemplate() { }

    public EmailTemplate(
        Guid dealerId,
        string name,
        TemplateType type,
        string subject,
        string body,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        Subject = subject;
        Body = body;
        CreatedBy = createdBy;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string subject, string body, string? htmlBody = null)
    {
        Name = name;
        Subject = subject;
        Body = body;
        HtmlBody = htmlBody;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPreheader(string? preheaderText)
    {
        PreheaderText = preheaderText;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSender(string? fromName, string? fromEmail, string? replyToEmail)
    {
        FromName = fromName;
        FromEmail = fromEmail;
        ReplyToEmail = replyToEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCategory(string category)
    {
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTags(string? tags)
    {
        Tags = tags;
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
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
