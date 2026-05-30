using Infrastructure.Database.Context;
using System.Linq.Expressions;

namespace Infrastructure.Database.Repositories.Generic_Repository;

public class LookupRepository<T>(StoreContext context) : ILookupRepository<T>
    where T : class, new()
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<T>> GetAllWithExpressionAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);


    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(entity, cancellationToken);

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
}
