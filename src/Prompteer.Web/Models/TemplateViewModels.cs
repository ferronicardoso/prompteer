using Prompteer.Application.DTOs;

namespace Prompteer.Web.Models;

public class TemplateDetailsViewModel
{
    public PromptTemplateDto Template { get; set; } = null!;
    public IEnumerable<PromptTemplateVersionDto> Versions { get; set; } = [];
    public string? LatestPrompt { get; set; }
}

public class VersionsViewModel
{
    public PromptTemplateDto Template { get; set; } = null!;
    public IEnumerable<PromptTemplateVersionDto> Versions { get; set; } = [];
}

public class VersionDetailViewModel
{
    public PromptTemplateDto Template { get; set; } = null!;
    public PromptTemplateVersionDto Version { get; set; } = null!;
}

public class CompareViewModel
{
    public PromptTemplateDto Template { get; set; } = null!;
    public PromptTemplateVersionDto Version1 { get; set; } = null!;
    public PromptTemplateVersionDto Version2 { get; set; } = null!;
}
