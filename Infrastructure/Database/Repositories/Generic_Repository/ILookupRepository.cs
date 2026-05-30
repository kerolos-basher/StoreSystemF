
using System.Linq.Expressions;

namespace Infrastructure.Database.Repositories.Generic_Repository;

public interface ILookupRepository<T> where T : class, new()
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllWithExpressionAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
}
