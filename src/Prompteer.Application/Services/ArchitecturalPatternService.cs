using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.DTOs;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Application.Services;

public class ArchitecturalPatternService : IArchitecturalPatternService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ArchitecturalPatternService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ArchitecturalPatternDto>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _uow.Repository<ArchitecturalPattern>().Query();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Description.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ArchitecturalPatternDto>
        {
            Items = _mapper.Map<List<ArchitecturalPatternDto>>(items),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<ArchitecturalPatternDto?> GetByIdAsync(Guid id)
    {
        var e = await _uow.Repository<ArchitecturalPattern>().GetByIdAsync(id);
        return e is null ? null : _mapper.Map<ArchitecturalPatternDto>(e);
    }

    public async Task<IEnumerable<ArchitecturalPatternDto>> GetAllAsync()
    {
        var items = await _uow.Repository<ArchitecturalPattern>().Query().OrderBy(x => x.Name).ToListAsync();
        return _mapper.Map<IEnumerable<ArchitecturalPatternDto>>(items);
    }

    public async Task<ArchitecturalPatternDto> CreateAsync(ArchitecturalPatternFormDto dto)
    {
        var entity = _mapper.Map<ArchitecturalPattern>(dto);
        await _uow.Repository<ArchitecturalPattern>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ArchitecturalPatternDto>(entity);
    }

    public async Task<ArchitecturalPatternDto> UpdateAsync(ArchitecturalPatternFormDto dto)
    {
        var entity = await _uow.Repository<ArchitecturalPattern>().GetByIdAsync(dto.Id!.Value)
            ?? throw new KeyNotFoundException("Padrão não encontrado.");
        _mapper.Map(dto, entity);
        _uow.Repository<ArchitecturalPattern>().Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ArchitecturalPatternDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _uow.Repository<ArchitecturalPattern>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Padrão não encontrado.");
        _uow.Repository<ArchitecturalPattern>().SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
