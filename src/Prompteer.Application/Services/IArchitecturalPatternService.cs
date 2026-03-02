using Prompteer.Application.DTOs;

namespace Prompteer.Application.Services;

public interface IArchitecturalPatternService
{
    Task<PagedResult<ArchitecturalPatternDto>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<ArchitecturalPatternDto?> GetByIdAsync(Guid id);
    Task<ArchitecturalPatternDto> CreateAsync(ArchitecturalPatternFormDto dto);
    Task<ArchitecturalPatternDto> UpdateAsync(ArchitecturalPatternFormDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<ArchitecturalPatternDto>> GetAllAsync();
}
