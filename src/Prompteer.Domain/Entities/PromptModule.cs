using Prompteer.Domain.Common;

namespace Prompteer.Domain.Entities;

public class PromptModule : BaseEntity
{
    public Guid PromptTemplateVersionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    // Navigation
    public PromptTemplateVersion Version { get; set; } = null!;
    public ICollection<PromptModuleItem> Items { get; set; } = new List<PromptModuleItem>();
}
