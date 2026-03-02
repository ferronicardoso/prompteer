using Prompteer.Domain.Common;

namespace Prompteer.Domain.Entities;

public class BacklogTool : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DefaultInstructions { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
}
