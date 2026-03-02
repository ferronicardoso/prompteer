namespace Prompteer.Domain.Entities;

public class PromptVersionTechnology
{
    public Guid PromptTemplateVersionId { get; set; }
    public Guid TechnologyId { get; set; }

    // Navigation
    public PromptTemplateVersion Version { get; set; } = null!;
    public Technology Technology { get; set; } = null!;
}
