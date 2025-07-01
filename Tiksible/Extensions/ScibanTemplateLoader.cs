using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Tiksible.Extensions;

public class ScibanTemplateLoader : ITemplateLoader
{
    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        if (string.IsNullOrEmpty(templateName))
        {
            throw new ArgumentException("Template name cannot be null or empty", nameof(templateName));
        }

        if (Path.IsPathFullyQualified(templateName))
        {
            return templateName;
        }

        var currentTemplate = context.CurrentSourceFile;

        if (!string.IsNullOrEmpty(currentTemplate))
        {
            var currentDir = Path.GetDirectoryName(currentTemplate);
            return Path.GetFullPath(Path.Combine(currentDir!, templateName));
        }

        return Path.GetFullPath(templateName);
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        if (string.IsNullOrEmpty(templatePath))
        {
            throw new ArgumentException("Template path cannot be null or empty", nameof(templatePath));
        }

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at path: {templatePath}");
        }

        return File.ReadAllText(templatePath);
    }

    public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        if (string.IsNullOrEmpty(templatePath))
        {
            throw new ArgumentException("Template path cannot be null or empty", nameof(templatePath));
        }

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at path: {templatePath}");
        }

        return await File.ReadAllTextAsync(templatePath);
    }
}