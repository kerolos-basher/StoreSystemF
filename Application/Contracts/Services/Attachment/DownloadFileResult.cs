namespace Application.Services.Attachment;

public class DownloadFileResult
{
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
