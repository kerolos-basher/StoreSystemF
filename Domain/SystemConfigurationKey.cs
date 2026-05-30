using Utilities.Enums.SystemConfigurationKeyEnum;

namespace Domain;
public class SystemConfigurationKey
{
    [Key]
    public SystemConfigurationKeyEnum Id { get; private set; }
    public string ConfigurationEn { get; private set; }
}
