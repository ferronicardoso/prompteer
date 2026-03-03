using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Application.Wizard;
using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
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

    public async Task<PagedResult<PromptTemplateSummaryDto>> GetPagedAsync(int page, int pageSize, string? search = null, Guid? currentUserId = null, UserRole? currentUserRole = null)
    {
        var query = _db.PromptTemplates.AsQueryable();

        // Data isolation by role
        if (currentUserRole == UserRole.Viewer)
            query = query.Where(x => x.IsPublic);
        else if (currentUserRole == UserRole.Editor && currentUserId.HasValue)
            query = query.Where(x => x.IsPublic || x.CreatedByUserId == currentUserId);
        // Admin (or null) sees all

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
                IsPublic = t.IsPublic, CreatedByUserId = t.CreatedByUserId,
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
            IsPublic = t.IsPublic,
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
        WizardSessionData data, string generatedPrompt, bool isPublic = false, Guid? createdByUserId = null)
    {
        PromptTemplate template;
        if (templateId.HasValue)
        {
            template = await _db.PromptTemplates.FindAsync(templateId.Value)
                ?? throw new KeyNotFoundException("Template não encontrado.");
            template.Name = name;
            template.Description = description;
            template.IsPublic = isPublic;
            template.CurrentVersionNumber++;
        }
        else
        {
            template = new PromptTemplate
            {
                Name = name, Description = description,
                IsPublic = isPublic, CurrentVersionNumber = 1,
                CreatedByUserId = createdByUserId
            };
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
            IsPublic = template.IsPublic,
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

    public async Task SetVisibilityAsync(Guid id, bool isPublic)
    {
        var template = await _db.PromptTemplates.FindAsync(id)
            ?? throw new KeyNotFoundException("Template não encontrado.");
        template.IsPublic = isPublic;
        await _db.SaveChangesAsync();
    }

    // ── Export ────────────────────────────────────────────────────────────────
    public async Task<TemplateExportDto> ExportAsync(IEnumerable<Guid>? templateIds, string exportedBy)
    {
        var query = _db.PromptTemplates
            .Include(t => t.Versions)
                .ThenInclude(v => v.Technologies).ThenInclude(vt => vt.Technology)
            .Include(t => t.Versions)
                .ThenInclude(v => v.Patterns).ThenInclude(vp => vp.Pattern)
            .AsQueryable();

        if (templateIds is not null)
            query = query.Where(t => templateIds.Contains(t.Id));

        var templates = await query.OrderBy(t => t.Name).ToListAsync();

        // Pre-load agent profiles and backlog tools for name resolution
        var agentProfileById = (await _db.AgentProfiles.ToListAsync())
            .ToDictionary(a => a.Id, a => a.Name);
        var backlogToolById = (await _db.BacklogTools.ToListAsync())
            .ToDictionary(b => b.Id, b => b.Name);

        var package = new TemplateExportDto
        {
            ExportedAt    = DateTime.UtcNow,
            ExportedBy    = exportedBy,
            SchemaVersion = "1.0"
        };

        foreach (var t in templates)
        {
            var latest = t.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
            if (latest is null) continue;

            // Resolve agent profile and backlog tool names from WizardDataJson
            string? agentProfileName = null;
            string? backlogToolName  = null;
            if (!string.IsNullOrEmpty(latest.WizardDataJson))
            {
                var wd = JsonSerializer.Deserialize<Application.Wizard.WizardSessionData>(
                    latest.WizardDataJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (wd?.AgentProfileId.HasValue == true)
                    agentProfileById.TryGetValue(wd.AgentProfileId.Value, out agentProfileName);
                if (wd?.BacklogToolId.HasValue == true)
                    backlogToolById.TryGetValue(wd.BacklogToolId.Value, out backlogToolName);
            }

            package.Templates.Add(new TemplateExportItemDto
            {
                Name              = t.Name,
                Description       = t.Description,
                VersionNumber     = latest.VersionNumber,
                CreatedAt         = t.CreatedAt,
                GeneratedPrompt   = latest.GeneratedPrompt,
                WizardDataJson    = latest.WizardDataJson,
                TechnologyNames   = latest.Technologies.Select(vt => vt.Technology?.Name ?? "").Where(n => n != "").ToList(),
                PatternNames      = latest.Patterns.Select(vp => vp.Pattern?.Name ?? "").Where(n => n != "").ToList(),
                AgentProfileName  = agentProfileName,
                BacklogToolName   = backlogToolName
            });
        }

        return package;
    }

    // ── Import ────────────────────────────────────────────────────────────────

    public async Task<ImportResultDto> ImportAsync(TemplateExportDto package, bool skipDuplicates = true)
    {
        var result = new ImportResultDto();

        // Pre-load existing names for deduplication
        var existingNames = await _db.PromptTemplates
            .Select(t => t.Name.ToLower()).ToHashSetAsync();

        // Pre-load technologies and patterns indexed by name (case-insensitive)
        var allTechs = await _db.Technologies.ToListAsync();
        var allPatterns = await _db.ArchitecturalPatterns.ToListAsync();

        var techByName    = allTechs.GroupBy(t => t.Name.ToLower()).ToDictionary(g => g.Key, g => g.First());
        var patternByName = allPatterns.GroupBy(p => p.Name.ToLower()).ToDictionary(g => g.Key, g => g.First());

        // Pre-load agent profiles and backlog tools indexed by name (case-insensitive)
        var agentProfileByName = (await _db.AgentProfiles.ToListAsync())
            .GroupBy(a => a.Name.ToLower()).ToDictionary(g => g.Key, g => g.First());
        var backlogToolByName = (await _db.BacklogTools.ToListAsync())
            .GroupBy(b => b.Name.ToLower()).ToDictionary(g => g.Key, g => g.First());

        foreach (var item in package.Templates)
        {
            try
            {
                if (skipDuplicates && existingNames.Contains(item.Name.ToLower()))
                {
                    result.Skipped++;
                    continue;
                }

                // Deserialize wizard data
                WizardSessionData? wizardData = null;
                if (!string.IsNullOrEmpty(item.WizardDataJson))
                {
                    wizardData = JsonSerializer.Deserialize<WizardSessionData>(
                        item.WizardDataJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                wizardData ??= new WizardSessionData();
                wizardData.EditingTemplateId = null; // always create new

                // Resolve/create technologies
                var resolvedTechIds = new List<Guid>();
                foreach (var techName in item.TechnologyNames)
                {
                    var key = techName.ToLower();
                    if (!techByName.TryGetValue(key, out var tech))
                    {
                        tech = new Technology
                        {
                            Name      = techName,
                            Category  = Domain.Enums.TechCategory.Other,
                            Ecosystem = Domain.Enums.TechEcosystem.Agnostic
                        };
                        _db.Technologies.Add(tech);
                        await _db.SaveChangesAsync();
                        techByName[key] = tech;
                        allTechs.Add(tech);
                    }
                    resolvedTechIds.Add(tech.Id);
                }

                // Resolve/create architectural patterns
                var resolvedPatternIds = new List<Guid>();
                foreach (var patternName in item.PatternNames)
                {
                    var key = patternName.ToLower();
                    if (!patternByName.TryGetValue(key, out var pattern))
                    {
                        pattern = new ArchitecturalPattern
                        {
                            Name        = patternName,
                            Description = string.Empty,
                            Ecosystem   = Domain.Enums.TechEcosystem.Agnostic
                        };
                        _db.ArchitecturalPatterns.Add(pattern);
                        await _db.SaveChangesAsync();
                        patternByName[key] = pattern;
                    }
                    resolvedPatternIds.Add(pattern.Id);
                }

                // Override wizard IDs with resolved ones
                wizardData.TechnologyIds           = resolvedTechIds;
                wizardData.ArchitecturalPatternIds = resolvedPatternIds;

                // Resolve agent profile by name (when exported with AgentProfileName)
                if (!string.IsNullOrEmpty(item.AgentProfileName))
                {
                    wizardData.AgentProfileId = agentProfileByName.TryGetValue(
                        item.AgentProfileName.ToLower(), out var agentProfile)
                        ? agentProfile.Id
                        : null;
                }

                // Resolve backlog tool by name (when exported with BacklogToolName)
                if (!string.IsNullOrEmpty(item.BacklogToolName))
                {
                    wizardData.BacklogToolId = backlogToolByName.TryGetValue(
                        item.BacklogToolName.ToLower(), out var backlogTool)
                        ? backlogTool.Id
                        : null;
                }

                var dto = await SaveFromWizardAsync(
                    null,
                    item.Name,
                    item.Description,
                    wizardData,
                    item.GeneratedPrompt);

                existingNames.Add(item.Name.ToLower());
                result.Imported++;
                result.ImportedNames.Add(dto.Name);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"'{item.Name}': {ex.Message}");
            }
        }

        return result;
    }
}
