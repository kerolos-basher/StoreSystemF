using Application.Abstractions.Persistence;

namespace Application.Products.Queries.GetProductDetailsAutocomplete;

public sealed class GetProductDetailsAutocompleteQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductDetailsAutocompleteQuery, IReadOnlyList<ProductDetailsAutoCompleteDto>>
{
    public async Task<IReadOnlyList<ProductDetailsAutoCompleteDto>> Handle(
        GetProductDetailsAutocompleteQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Term?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return [];

        var lowered = term.ToLower();

        return await context.ProductDetails
            .AsNoTracking()
            .Include(pd => pd.Product)
            .Include(pd => pd.Supplier)
            .Where(pd => pd.RemainingQuantity > 0 && pd.Product.ProductName.ToLower().Contains(lowered))
            .OrderBy(pd => pd.Product.ProductName)
            .Take(15)
            .Select(pd => new ProductDetailsAutoCompleteDto(
                pd.Id,
                pd.Product.ProductName,
                pd.Supplier != null ? pd.Supplier.Name : string.Empty,
                pd.SeLingPrice,
                pd.Price,
                pd.RemainingQuantity,
                pd.ProductId,
                pd.BarCode))
            .ToListAsync(cancellationToken);
    }
}
