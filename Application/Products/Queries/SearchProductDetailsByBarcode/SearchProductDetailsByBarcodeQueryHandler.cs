using Application.Abstractions.Persistence;

namespace Application.Products.Queries.SearchProductDetailsByBarcode;

public sealed class SearchProductDetailsByBarcodeQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchProductDetailsByBarcodeQuery, ProductDetailsSearchDto?>
{
    public async Task<ProductDetailsSearchDto?> Handle(
        SearchProductDetailsByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        var barcode = request.Barcode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return await context.ProductDetails
            .AsNoTracking()
            .Include(pd => pd.Product)
            .Include(pd => pd.Supplier)
            .Include(pd => pd.Category)
            .Where(pd => pd.BarCode == barcode)
            .Select(pd => new ProductDetailsSearchDto(
                pd.Id,
                pd.ProductId,
                pd.Product.ProductName,
                pd.BarCode,
                pd.Price,
                pd.SeLingPrice,
                pd.Supplier != null ? pd.Supplier.Name : string.Empty,
                pd.Category != null ? pd.Category.Name : string.Empty,
                pd.RemainingQuantity,
                pd.Notes))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
