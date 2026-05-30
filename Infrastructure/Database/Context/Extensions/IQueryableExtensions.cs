// Ignore Spelling: Queryable

namespace Infrastructure.Database.Extensions;
public static class IQueryableExtensions
{
    public static PaginatedResult<T> Paginate<T>(this IQueryable<T> source, int pageIndex, int pageSize)
    {
        int totalCount = source.Count();

        var data = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<T>(data, totalCount, pageSize);
    }
}

public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public PaginatedResult(IEnumerable<T> data, int totalCount, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}