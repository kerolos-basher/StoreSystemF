using Application.Abstractions.Persistence;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductByBarcode;

public sealed class GetProductByBarcodeQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductByBarcodeQuery, ProductByBarcodeDto>
{
    public async Task<ProductByBarcodeDto> Handle(
        GetProductByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Barcode.Trim(), out var barcode))
            throw new Exception("الباركود غير صالح.");

        var product = await context.Product
            .AsNoTracking()
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.BarCode == barcode, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        if (product.IsDeleted)
            throw new Exception("المنتج غير متاح.");

        var available = product.ProductDetails.Sum(x => x.RemainingQuantity);
        var latestSelling = product.ProductDetails
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.SeLingPrice)
            .FirstOrDefault();

        return new ProductByBarcodeDto(
            product.Id.ToString(),
            product.ProductName,
            product.BarCode.ToString(),
            latestSelling,
            available,
            string.Empty);
    }
}
