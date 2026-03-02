using Prompteer.Application.Services;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.DTOs;
using Prompteer.Application.Wizard;
using Prompteer.Domain.Entities;
using Prompteer.Infrastructure.Data;
using System.Text.Json;

namespace Prompteer.Infrastructure.Services;

public class PromptTemplateService : IPromptTemplateService
{
    private readonly AppDbContext _db;

    public PromptTemplateService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PromptTemplateSummaryDto>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _db.PromptTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var result = new List<PromptTemplateSummaryDto>();
        foreach (var t in items)
        {
            var dto = new PromptTemplateSummaryDto
            {
                Id = t.Id, Name = t.Name, Description = t.Description,
                CurrentVersionNumber = t.CurrentVersionNumber,
                CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
            };

            // Load tech names from latest version
            var latestVersion = await _db.PromptTemplateVersions
                .Where(v => v.PromptTemplateId == t.Id && v.VersionNumber == t.CurrentVersionNumber)
                .Include(v => v.Technologies).ThenInclude(vt => vt.Technology)
                .FirstOrDefaultAsync();

            if (latestVersion is not null)
                dto.TechnologyNames = latestVersion.Technologies.Select(vt => vt.Technology.Name).ToList();

            result.Add(dto);
        }

        return new PagedResult<PromptTemplateSummaryDto>
        {
            Items = result, TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PromptTemplateDto?> GetByIdAsync(Guid id)
    {
        var t = await _db.PromptTemplates.FindAsync(id);
        if (t is null) return null;
        return new PromptTemplateDto
        {
            Id = t.Id, Name = t.Name, Description = t.Description,
            CurrentVersionNumber = t.CurrentVersionNumber,
            CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
        };
    }

    public async Task<IEnumerable<PromptTemplateVersionDto>> GetVersionsAsync(Guid templateId)
    {
        return await _db.PromptTemplateVersions
            .Where(v => v.PromptTemplateId == templateId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new PromptTemplateVersionDto
            {
                Id = v.Id, VersionNumber = v.VersionNumber,
                GeneratedPrompt = v.GeneratedPrompt, CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PromptTemplateVersionDto?> GetVersionAsync(Guid templateId, int versionNumber)
    {
        var v = await _db.PromptTemplateVersions
            .FirstOrDefaultAsync(v => v.PromptTemplateId == templateId && v.VersionNumber == versionNumber);
        if (v is null) return null;
        return new PromptTemplateVersionDto
        {
            Id = v.Id, VersionNumber = v.VersionNumber,
            GeneratedPrompt = v.GeneratedPrompt, CreatedAt = v.CreatedAt
        };
    }

    public async Task<PromptTemplateDto> SaveFromWizardAsync(
        Guid? templateId, string name, string? description,
        WizardSessionData data, string generatedPrompt)
    {
        PromptTemplate template;
        if (templateId.HasValue)
        {
            template = await _db.PromptTemplates.FindAsync(templateId.Value)
                ?? throw new KeyNotFoundException("Template não encontrado.");
            template.Name = name;
            template.Description = description;
            template.CurrentVersionNumber++;
        }
        else
        {
            template = new PromptTemplate { Name = name, Description = description, CurrentVersionNumber = 1 };
            _db.PromptTemplates.Add(template);
            await _db.SaveChangesAsync();
        }

        var version = new PromptTemplateVersion
        {
            PromptTemplateId = template.Id,
            VersionNumber = template.CurrentVersionNumber,
            GeneratedPrompt = generatedPrompt,
            WizardDataJson = JsonSerializer.Serialize(data)
        };
        _db.PromptTemplateVersions.Add(version);
        await _db.SaveChangesAsync();

        // Save technology and pattern links
        foreach (var techId in data.TechnologyIds)
            _db.PromptVersionTechnologies.Add(new PromptVersionTechnology { PromptTemplateVersionId = version.Id, TechnologyId = techId });
        foreach (var patternId in data.ArchitecturalPatternIds)
            _db.PromptVersionPatterns.Add(new PromptVersionPattern { PromptTemplateVersionId = version.Id, ArchitecturalPatternId = patternId });

        // Save modules
        for (int i = 0; i < data.Modules.Count; i++)
        {
            var mod = new PromptModule
            {
                PromptTemplateVersionId = version.Id,
                Name = data.Modules[i].Name,
                DisplayOrder = i
            };
            _db.PromptModules.Add(mod);
            await _db.SaveChangesAsync();

            for (int j = 0; j < data.Modules[i].SubItems.Count; j++)
            {
                _db.PromptModuleItems.Add(new PromptModuleItem
                {
                    PromptModuleId = mod.Id,
                    Name = data.Modules[i].SubItems[j],
                    DisplayOrder = j
                });
            }
        }

        await _db.SaveChangesAsync();
        return new PromptTemplateDto
        {
            Id = template.Id, Name = template.Name,
            Description = template.Description,
            CurrentVersionNumber = template.CurrentVersionNumber,
            CreatedAt = template.CreatedAt, UpdatedAt = template.UpdatedAt
        };
    }

    public async Task<PromptTemplateDto> CloneAsync(Guid templateId, string newName)
    {
        var original = await _db.PromptTemplates
            .Include(t => t.Versions)
            .FirstOrDefaultAsync(t => t.Id == templateId)
            ?? throw new KeyNotFoundException("Template não encontrado.");

        var latestVersion = original.Versions.OrderByDescending(v => v.VersionNumber).First();
        var data = JsonSerializer.Deserialize<WizardSessionData>(latestVersion.WizardDataJson) ?? new WizardSessionData();

        return await SaveFromWizardAsync(null, newName, original.Description, data, latestVersion.GeneratedPrompt);
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await _db.PromptTemplates.FindAsync(id)
            ?? throw new KeyNotFoundException("Template não encontrado.");
        template.IsDeleted = true;
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<PromptTemplateSummaryDto>> GetRecentAsync(int count = 5)
    {
        return await _db.PromptTemplates
            .OrderByDescending(t => t.UpdatedAt)
            .Take(count)
            .Select(t => new PromptTemplateSummaryDto
            {
                Id = t.Id, Name = t.Name, Description = t.Description,
                CurrentVersionNumber = t.CurrentVersionNumber,
                CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<(string TechName, int Count)>> GetTopTechnologiesAsync(int count = 5)
    {
        // Materializa os counts por TechnologyId primeiro (GroupBy traduz sem Include)
        var counts = await _db.PromptVersionTechnologies
            .GroupBy(vt => vt.TechnologyId)
            .Select(g => new { TechnologyId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToListAsync();

        if (!counts.Any()) return [];

        var ids = counts.Select(x => x.TechnologyId).ToList();
        var names = await _db.Technologies
            .Where(t => ids.Contains(t.Id))
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        return counts.Select(c => (
            names.FirstOrDefault(t => t.Id == c.TechnologyId)?.Name ?? "Desconhecido",
            c.Count
        ));
    }

    public async Task<int> GetTotalCountAsync()
        => await _db.PromptTemplates.CountAsync();

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var total = await GetTotalCountAsync();
        var recent = await GetRecentAsync(5);
        var topTechs = await GetTopTechnologiesAsync(5);

        var totalVersions = await _db.PromptTemplateVersions.CountAsync();
        var totalAgentProfiles = await _db.AgentProfiles.CountAsync();
        var totalTechnologies = await _db.Technologies.CountAsync();

        // Top architectural patterns
        var patternCounts = await _db.PromptVersionPatterns
            .GroupBy(p => p.ArchitecturalPatternId)
            .Select(g => new { PatternId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var patternIds = patternCounts.Select(x => x.PatternId).ToList();
        var patternNames = await _db.ArchitecturalPatterns
            .Where(p => patternIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        var topPatterns = patternCounts
            .Select(x => new TopTechnologyDto
            {
                TechName = patternNames.FirstOrDefault(p => p.Id == x.PatternId)?.Name ?? "Desconhecido",
                Count = x.Count
            })
            .ToList();

        return new DashboardStatsDto
        {
            TotalTemplates = total,
            TotalVersions = totalVersions,
            TotalAgentProfiles = totalAgentProfiles,
            TotalTechnologies = totalTechnologies,
            RecentTemplates = recent,
            TopTechnologies = topTechs.Select(t => new TopTechnologyDto
            {
                TechName = t.TechName,
                Count = t.Count
            }),
            TopPatterns = topPatterns
        };
    }

    public async Task<WizardSessionData?> GetLatestWizardDataAsync(Guid templateId)
    {
        var latest = await _db.PromptTemplateVersions
            .Where(v => v.PromptTemplateId == templateId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => v.WizardDataJson)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(latest)) return null;

        return JsonSerializer.Deserialize<WizardSessionData>(latest,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
