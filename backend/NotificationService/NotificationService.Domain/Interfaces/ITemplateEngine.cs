namespace NotificationService.Domain.Interfaces;

public interface ITemplateEngine
{
    Task<string> RenderTemplateAsync(string templateName, Dictionary<string, object> parameters);
}