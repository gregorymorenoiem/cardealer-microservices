using CarDealer.Shared.MultiTenancy;
using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Notification template with optional multi-tenant support.
/// When DealerId is null, the template is global/system-wide and available to all dealers.
/// When DealerId has a value, the template belongs to a specific dealer.
/// </summary>
public class NotificationTemplate : IOptionalTenantEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// Optional dealer ID for tenant-specific templates.
    /// Null = system/global template, Value = dealer-specific template.
    /// </summary>
    public Guid? DealerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }

    // ✅ New: Version tracking
    public int Version { get; set; } = 1;
    public Guid? PreviousVersionId { get; set; }

    // ✅ New: Enhanced metadata
    public string? Tags { get; set; } // Comma-separated tags
    public string? ValidationRules { get; set; } // JSON: validation rules for variables
    public string? PreviewData { get; set; } // JSON: sample data for preview
    public string CreatedBy { get; set; } = "System";
    public string? UpdatedBy { get; set; }

    // Constructor
    public NotificationTemplate()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Variables = new Dictionary<string, string>();
        Version = 1;
        CreatedBy = "System";
    }

    // Factory method
    public static NotificationTemplate Create(string name, string subject, string body,
        NotificationType type, string? description = null, string? category = null, string? createdBy = null, Guid? dealerId = null)
    {
        return new NotificationTemplate
        {
            Name = name,
            Subject = subject,
            Body = body,
            Type = type,
            Description = description,
            Category = category,
            CreatedBy = createdBy ?? "System",
            DealerId = dealerId
        };
    }

    // Business methods
    public void Update(string subject, string body, string? description = null, string? updatedBy = null)
    {
        Subject = subject;
        Body = body;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    // ✅ New: Create a new version
    public NotificationTemplate CreateNewVersion(string? updatedBy = null)
    {
        var newVersion = new NotificationTemplate
        {
            Name = Name,
            Subject = Subject,
            Body = Body,
            Type = Type,
            Description = Description,
            Category = Category,
            Variables = Variables != null ? new Dictionary<string, string>(Variables) : null,
            Tags = Tags,
            ValidationRules = ValidationRules,
            PreviewData = PreviewData,
            IsActive = true,
            Version = Version + 1,
            PreviousVersionId = Id,
            CreatedBy = updatedBy ?? UpdatedBy ?? CreatedBy
        };

        return newVersion;
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

    public void AddVariable(string key, string defaultValue = "")
    {
        Variables ??= new Dictionary<string, string>();
        Variables[key] = defaultValue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveVariable(string key)
    {
        Variables?.Remove(key);
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ New: Tag management
    public void AddTag(string tag)
    {
        var tags = GetTagsList();
        if (!tags.Contains(tag))
        {
            tags.Add(tag);
            Tags = string.Join(",", tags);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        var tags = GetTagsList();
        if (tags.Remove(tag))
        {
            Tags = string.Join(",", tags);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public List<string> GetTagsList()
    {
        return string.IsNullOrWhiteSpace(Tags)
            ? new List<string>()
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
    }

    // ✅ New: Validation
    public bool ValidateVariables(Dictionary<string, object> parameters)
    {
        if (Variables == null || !Variables.Any())
            return true;

        foreach (var variable in Variables.Keys)
        {
            if (!parameters.ContainsKey(variable))
                return false;
        }

        return true;
    }

    public string RenderBody(Dictionary<string, object> parameters)
    {
        var renderedBody = Body;

        if (Variables != null && parameters != null)
        {
            foreach (var variable in Variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                var value = parameters.ContainsKey(variable.Key) ?
                    parameters[variable.Key]?.ToString() : variable.Value;
                renderedBody = renderedBody.Replace(placeholder, value ?? string.Empty);
            }
        }

        return renderedBody;
    }

    public string RenderSubject(Dictionary<string, object> parameters)
    {
        var renderedSubject = Subject;

        if (Variables != null && parameters != null)
        {
            foreach (var variable in Variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                var value = parameters.ContainsKey(variable.Key) ?
                    parameters[variable.Key]?.ToString() : variable.Value;
                renderedSubject = renderedSubject.Replace(placeholder, value ?? string.Empty);
            }
        }

        return renderedSubject;
    }
}