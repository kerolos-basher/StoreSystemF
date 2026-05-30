namespace Infrastructure.Services.Authentication;

public class AuThConfiguration
{
    public string ValidIssuer { get; set; } = string.Empty;
    public string ValidAudience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public int ExpireInDays { get; set; }
    public int NoOfFailedTrials { get; set; }
}
