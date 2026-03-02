using Prompteer.Application.DTOs;

namespace Prompteer.Application.Services;

public interface IAgentProfileService
{
    Task<PagedResult<AgentProfileDto>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<AgentProfileDto?> GetByIdAsync(Guid id);
    Task<AgentProfileDto> CreateAsync(AgentProfileFormDto dto);
    Task<AgentProfileDto> UpdateAsync(AgentProfileFormDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<AgentProfileDto>> GetAllAsync();
}
