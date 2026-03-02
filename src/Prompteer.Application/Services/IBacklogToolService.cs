using Prompteer.Application.DTOs;

namespace Prompteer.Application.Services;

public interface IBacklogToolService
{
    Task<PagedResult<BacklogToolDto>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<BacklogToolDto?> GetByIdAsync(Guid id);
    Task<BacklogToolDto> CreateAsync(BacklogToolFormDto dto);
    Task<BacklogToolDto> UpdateAsync(BacklogToolFormDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<BacklogToolDto>> GetAllAsync();
}
