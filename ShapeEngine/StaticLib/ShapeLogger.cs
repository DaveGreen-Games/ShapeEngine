using ShapeEngine.Core.Logging;

namespace ShapeEngine.StaticLib;

public static class ShapeLogger
{
    
    #region Members
    
    /// <summary>
    /// Indicates whether logging is currently enabled.
    /// </summary>
    private static bool loggingEnabled = true;

    /// <summary>
    /// Gets or sets a value indicating whether logging is currently enabled.
    /// </summary>
    /// <remarks>
    /// Terminates any active log block if logging is disabled while a block is active.
    /// </remarks>
    public static bool LoggingEnabled
    {
        get => loggingEnabled;
        set
        {
            if(value == loggingEnabled) return;
            if (!value && isBlockLogging)
            {
                EndLogBlock();
            }
            loggingEnabled = value;
        }        
    }
    /// <summary>
    /// Gets or sets the minimum severity level required for a message to be logged.
    /// </summary>
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets the output destination for log messages.
    /// </summary>
    /// <remarks>
    /// <see cref="LogFilePath"/> has to be set and valid for <see cref="LogOutputType.File"/> or <see cref="LogOutputType.Both"/> to work.
    /// Use <see cref="SetLogFilePath"/> to set a new file path or change the existing one.
    /// Use <see cref="ClearLogFile"/> to clear the contents of the current log file.
    /// </remarks>
    public static LogOutputType OutputType { get; set; } = LogOutputType.Both;

    #endregion
    
    #region Log File Path
    
    /// <summary>
    /// Indicates whether the log file path is valid.
    /// </summary>
    public static bool LogFilePathValid { get; private set; } = false;
    
    /// <summary>
    /// Gets the current log file path used for logging output.
    /// </summary>
    public static string LogFilePath { get; private set; } = string.Empty;
    
    /// <summary>
    /// Sets the log file path if the provided path is valid.
    /// Returns true if the path is valid and set; otherwise, false.
    /// </summary>
    /// <param name="path">The file path to set for logging.</param>
    /// <returns>True if the path is valid and set; otherwise, false.</returns>
    /// <remarks>
    /// 
    /// </remarks>
    public static bool SetLogFilePath(string path)
    {
        if (!ShapeFileManager.CreateFile(path, ".txt")) return false;
        
        LogFilePath = path;
        LogFilePathValid = true;
        
        return true;
    }
    /// <summary>
    /// Clears the contents of the current log file if the log file path is valid.
    /// Returns true if the file was cleared; otherwise, false.
    /// </summary>
    public static bool ClearLogFile()
    {
        if (!LogFilePathValid) return false;
        File.WriteAllText(LogFilePath, string.Empty); // Clear the file before logging
        return true;
    }

    #endregion
    
    #region Block Logging
    
    private static bool isBlockLogging = false;
    private static string blockTitle = string.Empty;
    private static LogLevel blockLevel = LogLevel.Info;

    /// <summary>
    /// Starts a block of logging with a title and log level.
    /// </summary>
    public static void StartLogBlock(string title, LogLevel level = LogLevel.Info)
    {
        if (isBlockLogging) EndLogBlock();
        blockTitle = title;
        blockLevel = level;
        Log($"[START BLOCK] ---> {title}:", level);
        isBlockLogging = true;
    }

    /// <summary>
    /// Ends the current block of logging.
    /// </summary>
    public static void EndLogBlock()
    {
        if (!isBlockLogging) return;
        isBlockLogging = false;
        Log($"[END BLOCK] <--- {blockTitle}", blockLevel);
        blockTitle = string.Empty;
    }
    
    #endregion
    
    #region Log
    
    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (!loggingEnabled || level < MinimumLevel) return;

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

    public static void LogDebug(string message) => Log(message, LogLevel.Debug);
    public static void LogInfo(string message) => Log(message, LogLevel.Info);
    public static void LogWarning(string message) => Log(message, LogLevel.Warning);
    public static void LogError(string message) => Log(message, LogLevel.Error);
    public static void LogFatal(string message) => Log(message, LogLevel.Fatal);
    public static void LogTrace(string message) => Log(message, LogLevel.Trace);
    public static void LogNotice(string message) => Log(message, LogLevel.Notice);
    public static void LogAlert(string message) => Log(message, LogLevel.Alert);
    public static void LogEmergency(string message) => Log(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log Console
 
    public static void LogConsole(string message, LogLevel level = LogLevel.Info)
    {
        if (!LoggingEnabled || level < MinimumLevel) return;

        string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(logMsg);
    }
    public static void LogConsoleDebug(string message) => LogConsole(message, LogLevel.Debug);
    public static void LogConsoleInfo(string message) => LogConsole(message, LogLevel.Info);
    public static void LogConsoleWarning(string message) => LogConsole(message, LogLevel.Warning);
    public static void LogConsoleError(string message) => LogConsole(message, LogLevel.Error);
    public static void LogConsoleFatal(string message) => LogConsole(message, LogLevel.Fatal);
    public static void LogConsoleTrace(string message) => LogConsole(message, LogLevel.Trace);
    public static void LogConsoleNotice(string message) => LogConsole(message, LogLevel.Notice);
    public static void LogConsoleAlert(string message) => LogConsole(message, LogLevel.Alert);
    public static void LogConsoleEmergency(string message) => LogConsole(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log File
    
    public static void LogFile(string message, LogLevel level = LogLevel.Info)
    {
        if (!LoggingEnabled || level < MinimumLevel) return;
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
    public static void LogFile(string message, string path, LogLevel level = LogLevel.Info)
    {
        if (!LoggingEnabled || level < MinimumLevel) return;

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
    public static void LogFileDebug(string message) => LogFile(message, LogLevel.Debug);
    public static void LogFileInfo(string message) => LogFile(message, LogLevel.Info);
    public static void LogFileWarning(string message) => LogFile(message, LogLevel.Warning);
    public static void LogFileError(string message) => LogFile(message, LogLevel.Error);
    public static void LogFileFatal(string message) => LogFile(message, LogLevel.Fatal);
    public static void LogFileTrace(string message) => LogFile(message, LogLevel.Trace);
    public static void LogFileNotice(string message) => LogFile(message, LogLevel.Notice);
    public static void LogFileAlert(string message) => LogFile(message, LogLevel.Alert);
    public static void LogFileEmergency(string message) => LogFile(message, LogLevel.Emergency);

    #endregion
}