using Application.Abstractions.Services;
using QRCoder;

namespace Infrastructure.Services.QRCode;

public sealed class QRCodeService : IQRCodeService
{
    public byte[] GeneratePng(string content, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}
