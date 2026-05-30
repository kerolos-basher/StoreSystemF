

namespace Infrastructure.Services.Email.Input;

public class DailyRequestEmailInput : BaseEmailMessageInput
{
    public byte[] AttachmentBase64 { get; set; }
}
