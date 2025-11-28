using Microsoft.Extensions.Options;
using NotificationService.Domain.Interfaces; // ✅ CORRECTO - Domain
using NotificationService.Shared;

namespace NotificationService.Infrastructure.Services;

public class TemplateService : ITemplateEngine // ✅ Implementa interfaz de Domain
{
    private readonly string _templatesPath;

    public TemplateService(IOptions<NotificationSettings> settings)
    {
        _templatesPath = settings.Value.TemplatesPath ?? "Templates";
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, object> parameters)
    {
        var templatePath = Path.Combine(_templatesPath, templateName);
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template {templateName} not found at {templatePath}");

        var templateContent = await File.ReadAllTextAsync(templatePath);

        foreach (var param in parameters)
        {
            templateContent = templateContent.Replace($"{{{{ {param.Key} }}}}", param.Value?.ToString() ?? string.Empty);
        }

        return templateContent;
    }
}