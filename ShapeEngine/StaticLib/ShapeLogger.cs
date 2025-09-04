using ShapeEngine.Core.Logging;

namespace ShapeEngine.StaticLib;


/// <summary>
/// Provides static methods for logging messages to the console and/or a file with configurable log levels and output types.
/// </summary>
public static class ShapeLogger
{
    
    #region Members
    
    private static bool loggingEnabled = true;
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled.
    /// When set to false, logging is disabled and any active log block is ended.
    /// </summary>
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
    /// Starts a new log block with the specified title and log level.
    /// If a log block is already active, it ends the current block before starting a new one.
    /// </summary>
    /// <param name="title">The title of the log block.</param>
    /// <param name="level">The log level for the block. Defaults to LogLevel.Info.</param>
    public static void StartLogBlock(string title, LogLevel level = LogLevel.Info)
    {
        if (isBlockLogging) EndLogBlock();
        blockTitle = title;
        blockLevel = level;
        Log($"[START BLOCK] ---> {title}:", level);
        isBlockLogging = true;
    }

    /// <summary>
    /// Ends the current log block if one is active, logs the block end message, and resets block state.
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
    
    /// <summary>
    /// Logs a message with the specified log level to the configured output(s).
    /// If logging is disabled or the log level is below the minimum, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The severity level of the log message. Defaults to LogLevel.Info.</param>
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

    /// <summary>
    /// Logs a debug message. Debug level is used for detailed diagnostic information.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Debug"/>.
    /// </remarks>
    public static void LogDebug(string message) => Log(message, LogLevel.Debug);
    
    /// <summary>
    /// Logs an informational message. Info level is used for general operational entries.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Info"/>.
    /// </remarks>
    public static void LogInfo(string message) => Log(message, LogLevel.Info);
    
    /// <summary>
    /// Logs a warning message. Warning level indicates a potential issue or important situation.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Warning"/>.
    /// </remarks>
    public static void LogWarning(string message) => Log(message, LogLevel.Warning);
    
    /// <summary>
    /// Logs an error message. Error level indicates a failure or problem that needs attention.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Error"/>.
    /// </remarks>
    public static void LogError(string message) => Log(message, LogLevel.Error);
    
    /// <summary>
    /// Logs a fatal error message. Fatal level indicates a critical failure causing program termination.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Fatal"/>.
    /// </remarks>
    public static void LogFatal(string message) => Log(message, LogLevel.Fatal);
    
    /// <summary>
    /// Logs a trace message. Trace level is used for very fine-grained informational events.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Trace"/>.
    /// </remarks>
    public static void LogTrace(string message) => Log(message, LogLevel.Trace);
    
    /// <summary>
    /// Logs a notice message. Notice level is used for normal but significant conditions.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Notice"/>.
    /// </remarks>
    public static void LogNotice(string message) => Log(message, LogLevel.Notice);
    
    /// <summary>
    /// Logs an alert message. Alert level indicates action must be taken immediately.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Alert"/>.
    /// </remarks>
    public static void LogAlert(string message) => Log(message, LogLevel.Alert);
    
    /// <summary>
    /// Logs an emergency message. Emergency level indicates a system-wide failure.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="Log"/> with <see cref="LogLevel.Emergency"/>.
    /// </remarks>
    public static void LogEmergency(string message) => Log(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log Console
 
    /// <summary>
    /// Logs a message to the console with the specified log level.
    /// If logging is disabled or the log level is below the minimum, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the console.</param>
    /// <param name="level">The severity level of the log message. Defaults to LogLevel.Info.</param>
    public static void LogConsole(string message, LogLevel level = LogLevel.Info)
    {
        if (!LoggingEnabled || level < MinimumLevel) return;

        string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(logMsg);
    }
    
    /// <summary>
    /// Logs a debug message to the console. Debug level is used for detailed diagnostic information.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Debug</c>.
    /// </remarks>
    public static void LogConsoleDebug(string message) => LogConsole(message, LogLevel.Debug);

    /// <summary>
    /// Logs an informational message to the console. Info level is used for general operational entries.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Info</c>.
    /// </remarks>
    public static void LogConsoleInfo(string message) => LogConsole(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message to the console. Warning level indicates a potential issue or important situation.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Warning</c>.
    /// </remarks>
    public static void LogConsoleWarning(string message) => LogConsole(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message to the console. Error level indicates a failure or problem that needs attention.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Error</c>.
    /// </remarks>
    public static void LogConsoleError(string message) => LogConsole(message, LogLevel.Error);

    /// <summary>
    /// Logs a fatal error message to the console. Fatal level indicates a critical failure causing program termination.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Fatal</c>.
    /// </remarks>
    public static void LogConsoleFatal(string message) => LogConsole(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a trace message to the console. Trace level is used for very fine-grained informational events.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Trace</c>.
    /// </remarks>
    public static void LogConsoleTrace(string message) => LogConsole(message, LogLevel.Trace);

    /// <summary>
    /// Logs a notice message to the console. Notice level is used for normal but significant conditions.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Notice</c>.
    /// </remarks>
    public static void LogConsoleNotice(string message) => LogConsole(message, LogLevel.Notice);

    /// <summary>
    /// Logs an alert message to the console. Alert level indicates action must be taken immediately.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Alert</c>.
    /// </remarks>
    public static void LogConsoleAlert(string message) => LogConsole(message, LogLevel.Alert);

    /// <summary>
    /// Logs an emergency message to the console. Emergency level indicates a system-wide failure.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <c>LogConsole</c> with <c>LogLevel.Emergency</c>.
    /// </remarks>
    public static void LogConsoleEmergency(string message) => LogConsole(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log File
    /// <summary>
    /// Logs a message to the log file with the specified log level.
    /// If logging is disabled, the log level is below the minimum, or the log file path is invalid, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the file.</param>
    /// <param name="level">The severity level of the log message. Defaults to LogLevel.Info.</param>
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
    
    /// <summary>
    /// Logs a message to the specified log file path with the given log level.
    /// If logging is disabled, the log level is below the minimum, or the file path is invalid, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the file.</param>
    /// <param name="path">The file path to log the message to.</param>
    /// <param name="level">The severity level of the log message. Defaults to LogLevel.Info.</param>
    public static void LogFile(string message, string path, LogLevel level = LogLevel.Info)
    {
        if (!LoggingEnabled || level < MinimumLevel) return;

        if (!ShapeFileManager.CreateFile(path, ".txt")) return;
        
        try
        {
            string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(path, logMsg + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Logger Error] Failed to write to log file {path}: {ex.Message}");
        }
    }
   
    /// <summary>
    /// Logs a debug message to the log file. Debug level is used for detailed diagnostic information.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Debug"/>.
    /// </remarks>
    public static void LogFileDebug(string message) => LogFile(message, LogLevel.Debug);

    /// <summary>
    /// Logs an informational message to the log file. Info level is used for general operational entries.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Info"/>.
    /// </remarks>
    public static void LogFileInfo(string message) => LogFile(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message to the log file. Warning level indicates a potential issue or important situation.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Warning"/>.
    /// </remarks>
    public static void LogFileWarning(string message) => LogFile(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message to the log file. Error level indicates a failure or problem that needs attention.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Error"/>.
    /// </remarks>
    public static void LogFileError(string message) => LogFile(message, LogLevel.Error);

    /// <summary>
    /// Logs a fatal error message to the log file. Fatal level indicates a critical failure causing program termination.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Fatal"/>.
    /// </remarks>
    public static void LogFileFatal(string message) => LogFile(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a trace message to the log file. Trace level is used for very fine-grained informational events.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Trace"/>.
    /// </remarks>
    public static void LogFileTrace(string message) => LogFile(message, LogLevel.Trace);

    /// <summary>
    /// Logs a notice message to the log file. Notice level is used for normal but significant conditions.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Notice"/>.
    /// </remarks>
    public static void LogFileNotice(string message) => LogFile(message, LogLevel.Notice);

    /// <summary>
    /// Logs an alert message to the log file. Alert level indicates action must be taken immediately.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Alert"/>.
    /// </remarks>
    public static void LogFileAlert(string message) => LogFile(message, LogLevel.Alert);

    /// <summary>
    /// Logs an emergency message to the log file. Emergency level indicates a system-wide failure.
    /// </summary>
    /// <remarks>
    /// Shortcut for calling <see cref="LogFile(string, LogLevel)"/> with <see cref="LogLevel.Emergency"/>.
    /// </remarks>
    public static void LogFileEmergency(string message) => LogFile(message, LogLevel.Emergency);

    #endregion
}