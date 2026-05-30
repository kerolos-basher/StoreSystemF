namespace Domain;
public class SystemConfiguration
{
    public int Id { get; private set; }
    public SystemConfigurationKeyEnum ConfigurationKeyId { get; private set; }
    public string Value { get; private set; }
    public virtual SystemConfigurationKey ConfigurationKey { get; private set; }
}
