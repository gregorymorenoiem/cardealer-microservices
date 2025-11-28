using Microsoft.Extensions.Options;
using NotificationService.Domain.Interfaces;
using NotificationService.Shared;

namespace NotificationService.Infrastructure.Templates;

public class TemplateEngine : ITemplateEngine
{
    private readonly string _templatesPath;

    public TemplateEngine(IOptions<NotificationSettings> settings)
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