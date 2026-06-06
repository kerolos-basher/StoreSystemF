using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.Products.Dtos;

namespace Application.Products.Queries.GetQRCode;

public sealed class GetQRCodeQueryHandler(
    IApplicationDbContext context,
    IQRCodeService qrCodeService)
    : IQueryHandler<GetQRCodeQuery, QRCodeDto>
{
    public async Task<QRCodeDto> Handle(
        GetQRCodeQuery request,
        CancellationToken cancellationToken)
    {
        var details = await context.ProductDetails
            .AsNoTracking()
            .Include(d => d.Product)
            .FirstOrDefaultAsync(d => d.Id == request.ProductDetailsId, cancellationToken)
            ?? throw new Exception("تفاصيل المنتج غير موجودة.");

        var barcode = details.BarCode;
        var png = qrCodeService.GeneratePng(barcode);

        return new QRCodeDto(
            details.ProductId.ToString(),
            details.Id.ToString(),
            barcode,
            Convert.ToBase64String(png),
            "image/png");
    }
}
