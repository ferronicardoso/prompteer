using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.DTOs;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Application.Services;

public class BacklogToolService : IBacklogToolService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BacklogToolService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<BacklogToolDto>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _uow.Repository<BacklogTool>().Query();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.IsSystemDefault ? 0 : 1).ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<BacklogToolDto>
        {
            Items = _mapper.Map<List<BacklogToolDto>>(items),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<BacklogToolDto?> GetByIdAsync(Guid id)
    {
        var e = await _uow.Repository<BacklogTool>().GetByIdAsync(id);
        return e is null ? null : _mapper.Map<BacklogToolDto>(e);
    }

    public async Task<IEnumerable<BacklogToolDto>> GetAllAsync()
    {
        var items = await _uow.Repository<BacklogTool>().Query()
            .OrderBy(x => x.IsSystemDefault ? 0 : 1).ThenBy(x => x.Name).ToListAsync();
        return _mapper.Map<IEnumerable<BacklogToolDto>>(items);
    }

    public async Task<BacklogToolDto> CreateAsync(BacklogToolFormDto dto)
    {
        var entity = _mapper.Map<BacklogTool>(dto);
        await _uow.Repository<BacklogTool>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<BacklogToolDto>(entity);
    }

    public async Task<BacklogToolDto> UpdateAsync(BacklogToolFormDto dto)
    {
        var entity = await _uow.Repository<BacklogTool>().GetByIdAsync(dto.Id!.Value)
            ?? throw new KeyNotFoundException("Ferramenta não encontrada.");

        if (entity.IsSystemDefault)
            throw new InvalidOperationException("Ferramentas padrão do sistema não podem ser editadas.");

        _mapper.Map(dto, entity);
        _uow.Repository<BacklogTool>().Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<BacklogToolDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _uow.Repository<BacklogTool>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Ferramenta não encontrada.");

        if (entity.IsSystemDefault)
            throw new InvalidOperationException("Ferramentas padrão do sistema não podem ser excluídas.");

        _uow.Repository<BacklogTool>().SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
