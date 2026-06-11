using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductDetails;

public sealed class GetProductDetailsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductDetailsQuery, ProductDetailsDto>
{
    public async Task<ProductDetailsDto> Handle(
        GetProductDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Product
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new Exception("Product not found.");

        var lines = await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Include(d => d.Category)
            .Where(d => d.ProductId == request.ProductId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new ProductDetailLineDto(
                d.Id,
                d.BarCode,
                d.SupplierId,
                d.Supplier != null ? d.Supplier.Name : "—",
                d.CategoryId,
                d.Category != null ? d.Category.Name : "—",
                d.Price,
                d.SeLingPrice,
                d.Quantity,
                d.RemainingQuantity,
                d.CreatedAt,
                d.Notes ?? string.Empty))
            .ToListAsync(cancellationToken);

        var totalQuantity = lines.Sum(x => x.RemainingQuantity);
        var inventoryValue = lines.Sum(x => x.RemainingQuantity * x.PurchasePrice);
        var supplierCount = lines
            .Select(x => x.Supplier)
            .Where(x => x != "—")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return new ProductDetailsDto(
            product.Id,
            product.ProductName,
            totalQuantity,
            inventoryValue,
            lines.Count,
            supplierCount,
            lines);
    }
}
