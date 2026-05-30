

namespace Infrastructure.Database.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class
{
    private DbSet<T> dbSet;
    protected readonly StoreContext _context;
    private readonly IConfiguration _configuration;

    public RepositoryBase(StoreContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        dbSet = _context.Set<T>();
    }


    #region Get Key

    #endregion

    #region Write
    public virtual void Add(T entity)
    {
        dbSet.Add(entity);
    }
    public virtual async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public T Update(T entity)
    {
        entity = dbSet.Update(entity).Entity;
        return entity;
    }

    #endregion



    #region Get Entity


    public async Task<T> GetByIdAsync(object id, Expression<Func<T, object>> includeExpression = null, bool Tracked = false)
    {
        var item = dbSet.FindAsync(id).Result;
        if (includeExpression != null)
        {
            if (item == null)
                return null;

            await _context.Entry(item).Reference(propertyExpression: includeExpression).LoadAsync();
        }
        if (!Tracked)
            _context.ChangeTracker.Clear();
        return item;
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool tracked = true, bool enableLazyLoading = false)
    {
        if (enableLazyLoading)
        {
            _context.ChangeTracker.LazyLoadingEnabled = enableLazyLoading;
        }
        if (tracked == false)
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        var entity = await GetQueryable(predicate, include).FirstOrDefaultAsync();
        if (tracked == false)
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        return entity;
    }

    public Task<T> GetAsNoTrackingAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        return GetQueryable(predicate, include).AsNoTracking().FirstOrDefaultAsync();
    }


    #endregion

    #region Get Many

    #region Not Async
    public IQueryable<T> GetMany()
    {
        return GetManyHelper(null, null, null, null, null);
    }

    public IQueryable<T> GetMany(Expression<Func<T, bool>> predicate)
    {
        return GetManyHelper(predicate, null, null, null, null);
    }
    public IQueryable<T> GetAsNoTracking()
    {
        return dbSet.AsNoTracking();

    }
    public IQueryable<TResult> GetManyWithSelector<TResult>(
      Expression<Func<T, bool>> predicate,
      Expression<Func<T, TResult>> selector)
    {
        return GetManyHelper(predicate, null, null, null, null)
            .Select(selector);
    }
    public async Task<List<TResult>> GetManyProjectedAsNoTracking<TResult>(
    Expression<Func<T, bool>> predicate,
    Expression<Func<T, TResult>> selector)
    {
        return await dbSet
            .Where(predicate)
            .AsNoTracking()
            .Select(selector)
            .ToListAsync();
    }

    #endregion

    #endregion


    #region Helper Methods
    private IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        IQueryable<T> query = dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        if (include != null)
            query = include(query);


        return query;
    }

    private IQueryable<T> GetManyHelper(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? pageNumber = null, int? pageSize = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        if (orderBy != null)
            query = orderBy(query);

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            pageNumber = pageNumber - 1;
            int skip = pageNumber.Value * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return query;
    }

    #endregion


    #region Stored Procedures

    public async Task<List<T>> ExecuteStoredProcedureAsync(string storedProcedure, params SqlParameter[] parameters)
    {
        //return await _context.Set<T>().FromSqlRaw($"EXEC {storedProcedure}", parameters).ToListAsync();

        string paramPlaceholders = string.Join(", ", parameters.Select(p => p.ParameterName));
        return await _context.Set<T>().FromSqlRaw($"EXEC {storedProcedure} {paramPlaceholders}", parameters).ToListAsync();
    }

    public async Task<bool> ExecuteSqlCommand(string query, params SqlParameter[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(query, parameters) > 0;
    }
    #endregion

}
