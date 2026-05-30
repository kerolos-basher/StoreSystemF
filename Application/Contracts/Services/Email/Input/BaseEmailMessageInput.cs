
namespace Infrastructure.Services.Email.Input;
public class BaseEmailMessageInput
{
    public string To { get; set; }
    public string Title { get; set; }
    public string Subject { get; set; }
    public string Template { get; set; }
}
