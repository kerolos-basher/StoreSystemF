using Application.Abstractions.Persistence;

namespace Application.Products.Queries.GetProductsAutocomplete;

public sealed class GetProductsAutocompleteQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductsAutocompleteQuery, IReadOnlyList<ProductAutoCompleteDto>>
{
    public async Task<IReadOnlyList<ProductAutoCompleteDto>> Handle(
        GetProductsAutocompleteQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Term?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return [];

        var lowered = term.ToLower();

        return await context.Product
            .AsNoTracking()
            .Where(p => p.ProductName.ToLower().Contains(lowered))
            .OrderBy(p => p.ProductName)
            .Take(10)
            .Select(p => new ProductAutoCompleteDto(p.Id, p.ProductName))
            .ToListAsync(cancellationToken);
    }
}
