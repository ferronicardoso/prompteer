using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.DTOs;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Application.Services;

public class TechnologyService : ITechnologyService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TechnologyService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<TechnologyDto>> GetPagedAsync(int page, int pageSize, string? search = null, TechCategory? category = null, TechEcosystem? ecosystem = null)
    {
        var query = _uow.Repository<Technology>().Query();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));
        if (category.HasValue)
            query = query.Where(x => x.Category == category.Value);
        if (ecosystem.HasValue)
            query = query.Where(x => x.Ecosystem == ecosystem.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Category).ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<TechnologyDto>
        {
            Items = _mapper.Map<List<TechnologyDto>>(items),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<TechnologyDto?> GetByIdAsync(Guid id)
    {
        var entity = await _uow.Repository<Technology>().GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TechnologyDto>(entity);
    }

    public async Task<IEnumerable<TechnologySelectDto>> GetAllForSelectAsync()
    {
        var items = await _uow.Repository<Technology>().Query()
            .OrderBy(x => x.Category).ThenBy(x => x.Name)
            .ToListAsync();
        return _mapper.Map<IEnumerable<TechnologySelectDto>>(items);
    }

    public async Task<TechnologyDto> CreateAsync(TechnologyFormDto dto)
    {
        var entity = _mapper.Map<Technology>(dto);
        await _uow.Repository<Technology>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<TechnologyDto>(entity);
    }

    public async Task<TechnologyDto> UpdateAsync(TechnologyFormDto dto)
    {
        var entity = await _uow.Repository<Technology>().GetByIdAsync(dto.Id!.Value)
            ?? throw new KeyNotFoundException("Tecnologia não encontrada.");
        _mapper.Map(dto, entity);
        _uow.Repository<Technology>().Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<TechnologyDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _uow.Repository<Technology>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Tecnologia não encontrada.");
        if (entity.IsSystemDefault)
            throw new InvalidOperationException("Tecnologias padrão do sistema não podem ser excluídas.");
        _uow.Repository<Technology>().SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
