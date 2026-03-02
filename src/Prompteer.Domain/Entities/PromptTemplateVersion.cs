namespace Prompteer.Domain.Entities;

/// <summary>
/// Versão imutável de um template — não possui soft delete nem UpdatedAt.
/// </summary>
public class PromptTemplateVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PromptTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string GeneratedPrompt { get; set; } = string.Empty;
    public string WizardDataJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public PromptTemplate Template { get; set; } = null!;
    public ICollection<PromptVersionTechnology> Technologies { get; set; } = new List<PromptVersionTechnology>();
    public ICollection<PromptVersionPattern> Patterns { get; set; } = new List<PromptVersionPattern>();
    public ICollection<PromptModule> Modules { get; set; } = new List<PromptModule>();
}
