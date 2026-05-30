using Application.Services.Attachment;
using Infrastructure.Services.Attachment;
using Utilities.Enums.SystemConfigurationKeyEnum;

namespace Infrastructure.Services.Attachement.Input;

public class UploadAttachmentInput
{
    public AttachmentDTO AttachmentDto { get; set; } = null!;
    public Func<SystemConfigurationKeyEnum, string> FindConfigurationValue { get; set; } = null!;
}
