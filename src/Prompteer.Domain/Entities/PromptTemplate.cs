using Prompteer.Domain.Common;

namespace Prompteer.Domain.Entities;

public class PromptTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CurrentVersionNumber { get; set; }
    public bool IsPublic { get; set; } = true;

    // Navigation
    public ICollection<PromptTemplateVersion> Versions { get; set; } = new List<PromptTemplateVersion>();
}
