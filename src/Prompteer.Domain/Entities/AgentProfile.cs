using Prompteer.Domain.Common;
using Prompteer.Domain.Enums;

namespace Prompteer.Domain.Entities;

public class AgentProfile : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string KnowledgeDomain { get; set; } = string.Empty;
    public ToneType Tone { get; set; }
    public string DefaultConstraints { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
}
