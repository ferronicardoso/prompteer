using Prompteer.Domain.Common;
using Prompteer.Domain.Enums;

namespace Prompteer.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TechCategory Category { get; set; }
    public TechEcosystem Ecosystem { get; set; }
    public string? Version { get; set; }
    public string? ShortDescription { get; set; }
    public bool IsSystemDefault { get; set; }

    // Navigation
    public ICollection<PromptVersionTechnology> PromptVersionTechnologies { get; set; } = new List<PromptVersionTechnology>();
}
