


namespace Application.Services.Email;

public class Attachment
{
    public byte[] base64 { get; set; }
    public string name { get; set; }
}
public class EmailMessage
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public List<Attachment>? attachments { get; set; }
}



