namespace NotificationService.Domain.Interfaces;

public interface ITemplateEngine
{
    Task<string> RenderTemplateAsync(string templateName, Dictionary<string, object> parameters);
    void ClearCache();
    bool ValidateTemplate(string templateContent, out List<string> errors);
    List<string> ExtractPlaceholders(string templateContent);
}