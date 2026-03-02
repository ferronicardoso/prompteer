using Prompteer.Domain.Common;

namespace Prompteer.Domain.Entities;

public class PromptModuleItem : BaseEntity
{
    public Guid PromptModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    // Navigation
    public PromptModule Module { get; set; } = null!;
}
