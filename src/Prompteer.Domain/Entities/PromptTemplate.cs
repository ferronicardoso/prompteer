using Prompteer.Domain.Common;

namespace Prompteer.Domain.Entities;

public class PromptTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CurrentVersionNumber { get; set; }

    // Navigation
    public ICollection<PromptTemplateVersion> Versions { get; set; } = new List<PromptTemplateVersion>();
}
