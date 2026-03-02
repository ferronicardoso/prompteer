using Prompteer.Domain.Common;
using Prompteer.Domain.Enums;

namespace Prompteer.Domain.Entities;

public class ArchitecturalPattern : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechEcosystem Ecosystem { get; set; }
    public bool IsSystemDefault { get; set; }

    // Navigation
    public ICollection<PromptVersionPattern> PromptVersionPatterns { get; set; } = new List<PromptVersionPattern>();
}
