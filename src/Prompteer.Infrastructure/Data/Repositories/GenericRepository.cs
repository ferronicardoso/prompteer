using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Common;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Infrastructure.Data.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
        => await _set.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _set.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _set.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _set.FirstOrDefaultAsync(predicate);

    public async Task AddAsync(T entity)
        => await _set.AddAsync(entity);

    public void Update(T entity)
        => _set.Update(entity);

    public void Remove(T entity)
        => _set.Remove(entity);

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        _set.Update(entity);
    }

    public IQueryable<T> Query()
        => _set.AsQueryable();
}
