using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.DTOs;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Application.Services;

public class AgentProfileService : IAgentProfileService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AgentProfileService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<AgentProfileDto>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var repo = _uow.Repository<AgentProfile>();
        var query = repo.Query();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.KnowledgeDomain.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.IsSystemDefault ? 0 : 1).ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<AgentProfileDto>
        {
            Items = _mapper.Map<List<AgentProfileDto>>(items),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AgentProfileDto?> GetByIdAsync(Guid id)
    {
        var entity = await _uow.Repository<AgentProfile>().GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<AgentProfileDto>(entity);
    }

    public async Task<IEnumerable<AgentProfileDto>> GetAllAsync()
    {
        var items = await _uow.Repository<AgentProfile>().Query()
            .OrderBy(x => x.IsSystemDefault ? 0 : 1).ThenBy(x => x.Name)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AgentProfileDto>>(items);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null)
    {
        var query = _uow.Repository<AgentProfile>().Query()
            .Where(x => x.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<AgentProfileDto> CloneAsync(Guid id)
    {
        var source = await _uow.Repository<AgentProfile>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        var baseName = $"{source.Name} (Cópia)";
        var cloneName = baseName;
        var suffix = 2;
        while (await ExistsWithNameAsync(cloneName))
            cloneName = $"{baseName} {suffix++}";

        var clone = new AgentProfile
        {
            Name             = cloneName,
            Role             = source.Role,
            KnowledgeDomain  = source.KnowledgeDomain,
            Tone             = source.Tone,
            DefaultConstraints = source.DefaultConstraints,
            IsSystemDefault  = false
        };

        await _uow.Repository<AgentProfile>().AddAsync(clone);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AgentProfileDto>(clone);
    }

    public async Task<AgentProfileDto> CreateAsync(AgentProfileFormDto dto)
    {
        if (await ExistsWithNameAsync(dto.Name))
            throw new InvalidOperationException($"Já existe um perfil com o nome \"{dto.Name}\".");

        var entity = _mapper.Map<AgentProfile>(dto);
        await _uow.Repository<AgentProfile>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AgentProfileDto>(entity);
    }

    public async Task<AgentProfileDto> UpdateAsync(AgentProfileFormDto dto)
    {
        var entity = await _uow.Repository<AgentProfile>().GetByIdAsync(dto.Id!.Value)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        if (entity.IsSystemDefault)
            throw new InvalidOperationException("Perfis padrão do sistema não podem ser editados.");

        if (await ExistsWithNameAsync(dto.Name, dto.Id))
            throw new InvalidOperationException($"Já existe um perfil com o nome \"{dto.Name}\".");

        _mapper.Map(dto, entity);
        _uow.Repository<AgentProfile>().Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AgentProfileDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _uow.Repository<AgentProfile>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        if (entity.IsSystemDefault)
            throw new InvalidOperationException("Perfis padrão do sistema não podem ser excluídos.");

        _uow.Repository<AgentProfile>().SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
