

namespace Infrastructure.Services.LogFile;

public interface ILogFileService
{
    void LogException(string ex);
    void LogValidation(string ex);
    void LogTrace(string ex);
}
