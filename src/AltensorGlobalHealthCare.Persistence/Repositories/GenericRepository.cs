using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AltensorGlobalHealthCare.Application.Interfaces.Repositories;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Persistence.Contexts;

namespace AltensorGlobalHealthCare.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual IQueryable<T> Query()
    {
        return DbSet.AsQueryable();
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        if (entity is ISoftDelete softDelete)
        {
            softDelete.IsDeleted = true;
            softDelete.DeletedAtUtc = DateTime.UtcNow;
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }
}
