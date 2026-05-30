namespace Infrastructure.Database.Repositories;

public interface IRepository<T> where T : class
{



    #region Write
    void Add(T entity);
    Task AddAsync(T entity);
    T Update(T entity);
    #endregion

    #region Get Entity

    Task<T> GetByIdAsync(object id, Expression<Func<T, object>> includeExpression = null, bool Tracked = false);
    Task<T> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool tracked = true, bool enableLazyLoading = false);
    Task<T> GetAsNoTrackingAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);


    #endregion

    #region Get Many

    #region Not Async
    IQueryable<T> GetMany();
    IQueryable<TResult> GetManyWithSelector<TResult>(
      Expression<Func<T, bool>> predicate,
      Expression<Func<T, TResult>> selector);
    IQueryable<T> GetMany(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetAsNoTracking();
    Task<List<TResult>> GetManyProjectedAsNoTracking<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
    #endregion

    #endregion


    #region StoredProcedures

    public Task<List<T>> ExecuteStoredProcedureAsync(string storedProcedure, params SqlParameter[] parameters);
    public Task<bool> ExecuteSqlCommand(string query, params SqlParameter[] parameters);
    #endregion

}
