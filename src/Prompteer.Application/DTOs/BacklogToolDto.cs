namespace Prompteer.Application.DTOs;

public class BacklogToolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefaultInstructions { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BacklogToolFormDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefaultInstructions { get; set; } = string.Empty;
}
