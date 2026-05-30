namespace Application.Services.Attachment;

public class AttachmentDTO
{
    public long ID { get; set; }
    public string DocumentName { get; set; } = null!;
    public string RelativePath { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string Data { get; set; } = null!;
    public bool IsImage { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}
