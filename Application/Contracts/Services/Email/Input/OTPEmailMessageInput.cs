
namespace Infrastructure.Services.Email.Input;

public class OTPEmailMessageInput : BaseEmailMessageInput
{
    public string Code { get; set; }
}
