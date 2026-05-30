namespace Application.Abstractions.Services;

public interface IQRCodeService
{
    byte[] GeneratePng(string content, int pixelsPerModule = 10);
}
