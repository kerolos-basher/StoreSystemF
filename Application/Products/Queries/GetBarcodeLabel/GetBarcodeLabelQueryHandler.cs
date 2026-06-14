using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetBarcodeLabel;

public sealed class GetBarcodeLabelQueryHandler(
    IApplicationDbContext context,
    IBarcodeImageService barcodeImageService)
    : IQueryHandler<GetBarcodeLabelQuery, BarcodeLabelDto>
{
    public async Task<BarcodeLabelDto> Handle(
        GetBarcodeLabelQuery request,
        CancellationToken cancellationToken)
    {
        var details = await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Product)
            .FirstOrDefaultAsync(d => d.Id == request.ProductDetailsId, cancellationToken)
            ?? throw new Exception("تفاصيل المنتج غير موجودة.");

        var barcode = details.BarCode;
        var png = barcodeImageService.GeneratePng(barcode);

        return new BarcodeLabelDto(
            details.ProductId.ToString(),
            details.Id.ToString(),
            barcode,
            Convert.ToBase64String(png),
            "image/png");
    }
}
