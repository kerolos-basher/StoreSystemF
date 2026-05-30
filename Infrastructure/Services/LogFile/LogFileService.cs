
using System.Diagnostics;
using System.Reflection;

namespace Infrastructure.Services.LogFile;

public class LogFileService
{
    private static readonly object LockLogObj = new object();

    private readonly IConfiguration Configuration;

    public LogFileService(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void LogException(Exception ex, string comment = "")
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string fileName = $"{currentDate}_log.txt";

        var logBuilder = new StringBuilder();

        logBuilder.AppendLine("========================================");
        logBuilder.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        logBuilder.AppendLine($"Exception Type: {ex.GetType().FullName}");
        logBuilder.AppendLine($"Message: {ex.Message}");
        logBuilder.AppendLine($"Source: {ex.Source}");
        if (!string.IsNullOrEmpty(comment))
            logBuilder.AppendLine($"comment: {comment}");

        // Get the method and line number where the exception occurred
        var stackTrace = new StackTrace(ex, true); // 'true' enables file line info
        var frame = stackTrace.GetFrame(0); // First frame usually contains the error location

        if (frame != null)
        {
            MethodBase method = frame.GetMethod();
            int line = frame.GetFileLineNumber();
            string fileNameError = frame.GetFileName();

            logBuilder.AppendLine($"Method: {method?.DeclaringType?.FullName}.{method?.Name}");
            logBuilder.AppendLine($"File: {fileNameError}");
            logBuilder.AppendLine($"Line: {line}");
        }

        logBuilder.AppendLine("Stack Trace:");
        logBuilder.AppendLine(ex.StackTrace);

        // Capture all Inner Exceptions
        Exception inner = ex.InnerException;
        while (inner != null)
        {
            logBuilder.AppendLine("------ Inner Exception ------");
            logBuilder.AppendLine($"Type: {inner.GetType().FullName}");
            logBuilder.AppendLine($"Message: {inner.Message}");
            logBuilder.AppendLine($"Source: {inner.Source}");

            var innerStackTrace = new StackTrace(inner, true);
            var innerFrame = innerStackTrace.GetFrame(0);
            if (innerFrame != null)
            {
                MethodBase innerMethod = innerFrame.GetMethod();
                int innerLine = innerFrame.GetFileLineNumber();
                string innerFileName = innerFrame.GetFileName();

                logBuilder.AppendLine($"Inner Method: {innerMethod?.DeclaringType?.FullName}.{innerMethod?.Name}");
                logBuilder.AppendLine($"Inner File: {innerFileName}");
                logBuilder.AppendLine($"Inner Line: {innerLine}");
            }

            logBuilder.AppendLine("Inner Stack Trace:");
            logBuilder.AppendLine(inner.StackTrace);
            inner = inner.InnerException;
        }

        logBuilder.AppendLine("========================================");

        WriteToTextFile(logBuilder.ToString(), fileName);
    }
    public void LogExceptionString(string ex)
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string fileName = currentDate + "_logException.txt";


        WriteToTextFile(ex, fileName);
    }

    public void LogInformation(string info)
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string fileName = currentDate + "_Info.txt";
        string logMessage = $@"

                                ======================
                                [Info]{info}
                              
                                ======================
                                ";
        WriteToTextFile(logMessage, fileName);
    }
    public void LogValidation(AggregateException ex)
    {
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        string fileName = currentDate + "_validations.txt";
        string logMessage = $@"
                                ======================
                                Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
                                Message: {ex.Message}
                                Data: {ex.Data}
                                Exception Type: {ex.GetType().FullName}
                                Source: {ex.Source}
                                StackTrace: {ex.StackTrace}
                                Target Site: {ex.TargetSite}
                                InnerException: {(ex.InnerException != null ? ex.InnerException.ToString() : "N/A")}
                                ======================
                                ";
        WriteToTextFile(logMessage, fileName);
    }


    #region Private

    private void WriteToTextFile(string ex_Txt, string fileName)
    {
        if (!Directory.Exists(this.LogsPath))
            Directory.CreateDirectory(this.LogsPath);

        lock (LockLogObj)
        {
            File.AppendAllText(Path.Combine(this.LogsPath, fileName), DateTime.Now.ToString() + Environment.NewLine + ex_Txt + Environment.NewLine + Environment.NewLine);
        }
    }

    private string LogsPath
    {
        get
        {
            return this.Configuration["LogsPath"];
        }
    }
    #endregion
}
