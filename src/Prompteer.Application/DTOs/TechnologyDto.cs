using Prompteer.Domain.Enums;

namespace Prompteer.Application.DTOs;

public class TechnologyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TechCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;
    public TechEcosystem Ecosystem { get; set; }
    public string EcosystemDisplay { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? ShortDescription { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TechnologyFormDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TechCategory Category { get; set; }
    public TechEcosystem Ecosystem { get; set; }
    public string? Version { get; set; }
    public string? ShortDescription { get; set; }
}

public class TechnologySelectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Label => Version is null ? Name : $"{Name} {Version}";
}
