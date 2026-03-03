using Prompteer.Application.DTOs;
using Prompteer.Application.Wizard;
using Prompteer.Domain.Enums;

namespace Prompteer.Application.Services;

public interface IPromptTemplateService
{
    Task<PagedResult<PromptTemplateSummaryDto>> GetPagedAsync(int page, int pageSize, string? search = null, Guid? currentUserId = null, UserRole? currentUserRole = null);
    Task<PromptTemplateDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PromptTemplateVersionDto>> GetVersionsAsync(Guid templateId);
    Task<PromptTemplateVersionDto?> GetVersionAsync(Guid templateId, int versionNumber);
    Task<PromptTemplateDto> SaveFromWizardAsync(Guid? templateId, string name, string? description, WizardSessionData data, string generatedPrompt, bool isPublic = false, Guid? createdByUserId = null);
    Task SetVisibilityAsync(Guid id, bool isPublic);
    Task<PromptTemplateDto> CloneAsync(Guid templateId, string newName);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<PromptTemplateSummaryDto>> GetRecentAsync(int count = 5);
    Task<IEnumerable<(string TechName, int Count)>> GetTopTechnologiesAsync(int count = 5);
    Task<int> GetTotalCountAsync();
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<WizardSessionData?> GetLatestWizardDataAsync(Guid templateId);
    Task<TemplateExportDto> ExportAsync(IEnumerable<Guid>? templateIds, string exportedBy);
    Task<ImportResultDto> ImportAsync(TemplateExportDto package, bool skipDuplicates = true);
}
