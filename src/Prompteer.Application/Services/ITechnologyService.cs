using Prompteer.Application.DTOs;
using Prompteer.Domain.Enums;

namespace Prompteer.Application.Services;

public interface ITechnologyService
{
    Task<PagedResult<TechnologyDto>> GetPagedAsync(int page, int pageSize, string? search = null, TechCategory? category = null, TechEcosystem? ecosystem = null);
    Task<TechnologyDto?> GetByIdAsync(Guid id);
    Task<TechnologyDto> CreateAsync(TechnologyFormDto dto);
    Task<TechnologyDto> UpdateAsync(TechnologyFormDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<TechnologySelectDto>> GetAllForSelectAsync();
    Task<TechnologyDto> CloneAsync(Guid id);
}
