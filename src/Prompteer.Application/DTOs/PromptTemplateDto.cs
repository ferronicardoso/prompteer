namespace Prompteer.Application.DTOs;

public class PromptTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CurrentVersionNumber { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PromptTemplateSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CurrentVersionNumber { get; set; }
    public bool IsPublic { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> TechnologyNames { get; set; } = new();
}

public class PromptTemplateVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string GeneratedPrompt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class DashboardStatsDto
{
    public int TotalTemplates { get; set; }
    public int TotalVersions { get; set; }
    public int TotalAgentProfiles { get; set; }
    public int TotalTechnologies { get; set; }
    public IEnumerable<PromptTemplateSummaryDto> RecentTemplates { get; set; } = [];
    public IEnumerable<TopTechnologyDto> TopTechnologies { get; set; } = [];
    public IEnumerable<TopTechnologyDto> TopPatterns { get; set; } = [];
}

public class TopTechnologyDto
{
    public string TechName { get; set; } = string.Empty;
    public int Count { get; set; }
}
