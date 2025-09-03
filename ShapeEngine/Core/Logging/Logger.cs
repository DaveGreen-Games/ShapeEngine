using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Logging;

public class Logger
{
    #region Members
    
    public bool Enabled { get; set; }
    public LogLevel MinimumLevel { get; set; }
    public readonly LogOutputType OutputType;
    public readonly string LogFilePath;
    public readonly bool LogFilePathValid;
    
    #endregion
    
    #region Block System
    private bool isBlockLogging = false;
    private string blockTitle = string.Empty;
    private LogLevel blockLevel = LogLevel.Info;


    public void StartLogBlock(string title, LogLevel level = LogLevel.Info)
    {
        if (isBlockLogging) EndLogBlock();
        blockTitle = title;
        blockLevel = level;
        Log($"[START BLOCK] ---> {title}:", level);
        isBlockLogging = true;
    }
    
    public void EndLogBlock()
    {
        if (!isBlockLogging) return;
        isBlockLogging = false;
        Log($"[END BLOCK] <--- {blockTitle}", blockLevel);
        blockTitle = string.Empty;
    }
    #endregion
    
    #region Constructors
    
    public Logger()
    {
        Enabled = true;
        MinimumLevel = LogLevel.Info;
        OutputType = LogOutputType.Console;
        LogFilePath = string.Empty;
        LogFilePathValid = false;
    }
    public Logger(LogLevel minimumLevel)
    {
        Enabled = true;
        MinimumLevel = minimumLevel;
        OutputType = LogOutputType.Console;
        LogFilePath = string.Empty;
        LogFilePathValid = false;
    }
    public Logger(string logFilePath, LogLevel minimumLevel, bool logToConsole = false)
    {
        Enabled = true;
        if (ShapeFileManager.CreateFile(logFilePath, ".txt"))
        {
            LogFilePath = logFilePath;
            LogFilePathValid = true;
            OutputType = logToConsole ? LogOutputType.Both : LogOutputType.File;
            
            //file exists and is valid, clear it before starting logging
            File.WriteAllText(logFilePath, string.Empty); // Clear the file before logging
            
        }
        else
        {
            OutputType = LogOutputType.Console;
            LogFilePath = string.Empty;
            LogFilePathValid = false;
        }

        MinimumLevel = minimumLevel;
    }

    #endregion
    
    #region Log
    
    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;

        // string logMsg = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        
        if(OutputType is LogOutputType.Console or LogOutputType.Both)
        {
            Console.WriteLine(logMsg);
            if(OutputType == LogOutputType.Console || !LogFilePathValid) return;
        }
        bool LogToFile = OutputType is LogOutputType.File or LogOutputType.Both && LogFilePathValid;
        if (LogToFile)
        {
            try
            {
                File.AppendAllText(LogFilePath, logMsg + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Logger Error] Failed to write to log file {LogFilePath}: {ex.Message}");
            }
        }
    }
    public void LogDebug(string message) => Log(message, LogLevel.Debug);
    public void LogInfo(string message) => Log(message, LogLevel.Info);
    public void LogWarning(string message) => Log(message, LogLevel.Warning);
    public void LogError(string message) => Log(message, LogLevel.Error);
    public void LogFatal(string message) => Log(message, LogLevel.Fatal);
    public void LogTrace(string message) => Log(message, LogLevel.Trace);
    public void LogNotice(string message) => Log(message, LogLevel.Notice);
    public void LogAlert(string message) => Log(message, LogLevel.Alert);
    public void LogEmergency(string message) => Log(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log Console
    
    public void LogConsole(string message, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;

        string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(logMsg);
    }
    public void LogConsoleDebug(string message) => LogConsole(message, LogLevel.Debug);
    public void LogConsoleInfo(string message) => LogConsole(message, LogLevel.Info);
    public void LogConsoleWarning(string message) => LogConsole(message, LogLevel.Warning);
    public void LogConsoleError(string message) => LogConsole(message, LogLevel.Error);
    public void LogConsoleFatal(string message) => LogConsole(message, LogLevel.Fatal);
    public void LogConsoleTrace(string message) => LogConsole(message, LogLevel.Trace);
    public void LogConsoleNotice(string message) => LogConsole(message, LogLevel.Notice);
    public void LogConsoleAlert(string message) => LogConsole(message, LogLevel.Alert);
    public void LogConsoleEmergency(string message) => LogConsole(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log File
    
    public void LogFile(string message, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;
        if (!LogFilePathValid) return;
        try
        {
            string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(LogFilePath, logMsg + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Logger Error] Failed to write to log file {LogFilePath}: {ex.Message}");
        }
    }
    public void LogFile(string message, string path, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;

        if (!ShapeFileManager.CreateFile(path, ".txt")) return;
        
        try
        {
            string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(LogFilePath, logMsg + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Logger Error] Failed to write to log file {path}: {ex.Message}");
        }
    }
    public void LogFileDebug(string message) => LogFile(message, LogLevel.Debug);
    public void LogFileInfo(string message) => LogFile(message, LogLevel.Info);
    public void LogFileWarning(string message) => LogFile(message, LogLevel.Warning);
    public void LogFileError(string message) => LogFile(message, LogLevel.Error);
    public void LogFileFatal(string message) => LogFile(message, LogLevel.Fatal);
    public void LogFileTrace(string message) => LogFile(message, LogLevel.Trace);
    public void LogFileNotice(string message) => LogFile(message, LogLevel.Notice);
    public void LogFileAlert(string message) => LogFile(message, LogLevel.Alert);
    public void LogFileEmergency(string message) => LogFile(message, LogLevel.Emergency);
    
    #endregion
    
}