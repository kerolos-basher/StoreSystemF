using Application.Abstractions.Services;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace Infrastructure.Services.Barcode;

public sealed class BarcodeImageService : IBarcodeImageService
{
    public byte[] GeneratePng(string content, int width = 400, int height = 100)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 10,
                PureBarcode = false
            }
        };

        using var bitmap = writer.Write(content);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}
