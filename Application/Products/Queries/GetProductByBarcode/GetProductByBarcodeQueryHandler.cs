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
        var barcode = request.Barcode.Trim();

        var details = await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Product)
            .FirstOrDefaultAsync(d => d.BarCode == barcode, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        if (details.Product.IsDeleted)
            throw new Exception("المنتج غير متاح.");

        return new ProductByBarcodeDto(
            details.ProductId,
            details.Id,
            details.Product.ProductName,
            details.BarCode,
            details.SeLingPrice,
            details.RemainingQuantity,
            string.Empty);
    }
}
