namespace Prompteer.Domain.Entities;

public class PromptVersionPattern
{
    public Guid PromptTemplateVersionId { get; set; }
    public Guid ArchitecturalPatternId { get; set; }

    // Navigation
    public PromptTemplateVersion Version { get; set; } = null!;
    public ArchitecturalPattern Pattern { get; set; } = null!;
}
