using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
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

    // Constructor
    public NotificationTemplate()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Variables = new Dictionary<string, string>();
    }

    // Factory method
    public static NotificationTemplate Create(string name, string subject, string body, 
        NotificationType type, string? description = null, string? category = null)
    {
        return new NotificationTemplate
        {
            Name = name,
            Subject = subject,
            Body = body,
            Type = type,
            Description = description,
            Category = category
        };
    }

    // Business methods
    public void Update(string subject, string body, string? description = null)
    {
        Subject = subject;
        Body = body;
        Description = description;
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

    public void AddVariable(string key, string defaultValue = "")
    {
        Variables ??= new Dictionary<string, string>();
        Variables[key] = defaultValue;
    }

    public void RemoveVariable(string key)
    {
        Variables?.Remove(key);
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