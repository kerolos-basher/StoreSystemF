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
        var product = await context.Product
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        var barcode = product.BarCode.ToString();
        var png = qrCodeService.GeneratePng(barcode);

        return new QRCodeDto(
            product.Id.ToString(),
            barcode,
            Convert.ToBase64String(png),
            "image/png");
    }
}
