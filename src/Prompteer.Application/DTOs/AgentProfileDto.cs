using Prompteer.Domain.Enums;

namespace Prompteer.Application.DTOs;

public class AgentProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string KnowledgeDomain { get; set; } = string.Empty;
    public ToneType Tone { get; set; }
    public string ToneDisplay { get; set; } = string.Empty;
    public string DefaultConstraints { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AgentProfileFormDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string KnowledgeDomain { get; set; } = string.Empty;
    public ToneType Tone { get; set; }
    public string DefaultConstraints { get; set; } = string.Empty;
}
