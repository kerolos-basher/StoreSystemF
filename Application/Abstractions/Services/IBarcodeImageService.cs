namespace Application.Abstractions.Services;

public interface IBarcodeImageService
{
    byte[] GeneratePng(string content, int width = 400, int height = 100);
}
