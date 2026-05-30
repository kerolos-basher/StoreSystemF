using Utilities.Enums.SystemConfigurationKeyEnum;

namespace Infrastructure.Services.Attachement.Input;

public class SaveAttachmentInput
{
    public bool OnlyImage { get; set; }
    public string DocumentBase64 { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public string NameWithoutExtension { get; set; } = null!;
    public long EntityId { get; set; }
    public Func<SystemConfigurationKeyEnum, string> FindConfigurationValue { get; set; } = null!;
}
