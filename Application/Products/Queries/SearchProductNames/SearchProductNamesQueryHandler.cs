using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Products.Queries.SearchProductNames;

public sealed class SearchProductNamesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchProductNamesQuery, IReadOnlyList<ProductNameLookupDto>>
{
    public async Task<IReadOnlyList<ProductNameLookupDto>> Handle(
        SearchProductNamesQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Term?.Trim().ToLower() ?? string.Empty;
        var limit = request.Limit < 1 ? 10 : Math.Min(request.Limit, 25);

        var query = context.Product.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(p => p.ProductName.ToLower().Contains(term));

        return await query
            .OrderBy(p => p.ProductName)
            .Take(limit)
            .Select(p => new ProductNameLookupDto(p.Id, p.ProductName))
            .ToListAsync(cancellationToken);
    }
}
