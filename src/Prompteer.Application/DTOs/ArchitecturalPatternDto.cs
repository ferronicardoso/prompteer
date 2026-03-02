using Prompteer.Domain.Enums;

namespace Prompteer.Application.DTOs;

public class ArchitecturalPatternDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechEcosystem Ecosystem { get; set; }
    public string EcosystemDisplay { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ArchitecturalPatternFormDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechEcosystem Ecosystem { get; set; }
}
