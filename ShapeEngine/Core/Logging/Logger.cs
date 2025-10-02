using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Logging;

/// <summary>
/// Provides logging functionality with support for console and file output, log levels, and block logging.
/// </summary>
public class Logger
{
    #region Members
    
    private bool enabled;
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled.
    /// </summary>
    /// <remarks>
    /// Disabling logging will terminate any active log block by calling <see cref="EndLogBlock"/>.
    /// </remarks>
    public bool Enabled
    {
        get => enabled;
        set
        {
            if(value == enabled) return;
            if (!value && isBlockLogging)
            {
                EndLogBlock();
            }
            enabled = value;
        }        
    }
    /// <summary>
    /// Gets or sets the minimum log level required for messages to be logged.
    /// </summary>
    public LogLevel MinimumLevel { get; set; }

    /// <summary>
    /// Specifies the output type for logging (e.g., Console, File, Both).
    /// </summary>
    public readonly LogOutputType OutputType;

    /// <summary>
    /// The file path used for logging to a file.
    /// </summary>
    public readonly string LogFilePath;

    /// <summary>
    /// Indicates whether the log file path is valid and usable.
    /// </summary>
    public readonly bool LogFilePathValid;
    
    #endregion
    
    #region Block System
    private bool isBlockLogging;
    private string blockTitle = string.Empty;
    private LogLevel blockLevel = LogLevel.Info;
    
    /// <summary>
    /// Starts a new log block with the specified title and log level.
    /// If a block is already active, it ends the current block before starting a new one.
    /// </summary>
    /// <param name="title">The title of the log block.</param>
    /// <param name="level">The log level for the block. Defaults to LogLevel.Info.</param>
    public void StartLogBlock(string title, LogLevel level = LogLevel.Info)
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
    public void EndLogBlock()
    {
        if (!isBlockLogging) return;
        isBlockLogging = false;
        Log($"[END BLOCK] <--- {blockTitle}", blockLevel);
        blockTitle = string.Empty;
    }
    #endregion
    
    #region Constructors
    public Logger(LoggerSettings settings)
    {
        enabled = settings.Enabled;
        MinimumLevel = settings.MinimumLevel;
        OutputType = settings.OutputType;
        if (settings.LogFilePath != string.Empty)
        {
            LogFilePathValid = true;
            LogFilePath = settings.LogFilePath;
        }
        else
        {
            LogFilePathValid = false;
            LogFilePath = string.Empty;
        }
    }
    /// <summary>
    /// Gets the current logger settings as a <see cref="LoggerSettings"/> object.
    /// </summary>
    public LoggerSettings GetSettings()
    {
        return new LoggerSettings(Enabled, MinimumLevel, LogFilePath, OutputType);
    }
    /// <summary>
    /// Creates a copy of the current <see cref="Logger"/> instance with the same settings.
    /// </summary>
    public Logger Copy()
    {
        return new Logger(GetSettings());
    }
    #endregion
    
    #region Log
    /// <summary>
    /// Logs a message with the specified log level.
    /// If logging is disabled or the level is below the minimum, the message is not logged.
    /// The output destination depends on the configured <see cref="OutputType"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log level for the message. Defaults to <see cref="LogLevel.Info"/>.</param>
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
    
    /// <summary>
    /// Logs a debug message (Debug: detailed information for diagnosing problems).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogDebug(string message) => Log(message, LogLevel.Debug);

    /// <summary>
    /// Logs an informational message (Info: general operational messages).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogInfo(string message) => Log(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message (Warning: indication of potential issues).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogWarning(string message) => Log(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message (Error: an error has occurred).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogError(string message) => Log(message, LogLevel.Error);

    /// <summary>
    /// Logs a fatal error message (Fatal: severe error causing premature termination).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogFatal(string message) => Log(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a trace message (Trace: fine-grained informational events).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogTrace(string message) => Log(message, LogLevel.Trace);

    /// <summary>
    /// Logs a notice message (Notice: normal but significant condition).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogNotice(string message) => Log(message, LogLevel.Notice);

    /// <summary>
    /// Logs an alert message (Alert: action must be taken immediately).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogAlert(string message) => Log(message, LogLevel.Alert);

    /// <summary>
    /// Logs an emergency message (Emergency: system is unusable).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="Log"/> function.
    /// </remarks>
    public void LogEmergency(string message) => Log(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log Console
    
    /// <summary>
    /// Logs a message to the console with the specified log level.
    /// If logging is disabled or the level is below the minimum, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the console.</param>
    /// <param name="level">The log level for the message. Defaults to <see cref="LogLevel.Info"/>.</param>
    public void LogConsole(string message, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;

        string logMsg = isBlockLogging ? $"--- {message}" : $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(logMsg);
    }
    
    /// <summary>
    /// Logs a debug message to the console (Debug: detailed information for diagnosing problems).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleDebug(string message) => LogConsole(message, LogLevel.Debug);

    /// <summary>
    /// Logs an informational message to the console (Info: general operational messages).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleInfo(string message) => LogConsole(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message to the console (Warning: indication of potential issues).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleWarning(string message) => LogConsole(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message to the console (Error: an error has occurred).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleError(string message) => LogConsole(message, LogLevel.Error);

    /// <summary>
    /// Logs a fatal error message to the console (Fatal: severe error causing premature termination).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleFatal(string message) => LogConsole(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a trace message to the console (Trace: fine-grained informational events).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleTrace(string message) => LogConsole(message, LogLevel.Trace);

    /// <summary>
    /// Logs a notice message to the console (Notice: normal but significant condition).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleNotice(string message) => LogConsole(message, LogLevel.Notice);

    /// <summary>
    /// Logs an alert message to the console (Alert: action must be taken immediately).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleAlert(string message) => LogConsole(message, LogLevel.Alert);

    /// <summary>
    /// Logs an emergency message to the console (Emergency: system is unusable).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the specified log level using the <see cref="LogConsole"/> function.
    /// </remarks>
    public void LogConsoleEmergency(string message) => LogConsole(message, LogLevel.Emergency);
    
    #endregion
    
    #region Log File
    
    /// <summary>
    /// Logs a message to the log file with the specified log level.
    /// If logging is disabled, the level is below the minimum, or the log file path is invalid, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the file.</param>
    /// <param name="level">The log level for the message. Defaults to <see cref="LogLevel.Info"/>.</param>
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
    
    /// <summary>
    /// Logs a message to the specified log file path with the given log level.
    /// If logging is disabled, the level is below the minimum, or the file path is invalid, the message is not logged.
    /// </summary>
    /// <param name="message">The message to log to the file.</param>
    /// <param name="path">The file path to write the log message to.</param>
    /// <param name="level">The log level for the message. Defaults to <see cref="LogLevel.Info"/>.</param>
    public void LogFile(string message, string path, LogLevel level = LogLevel.Info)
    {
        if (!Enabled || level < MinimumLevel) return;

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
    /// Logs a debug message to the log file (Debug: detailed information for diagnosing problems).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Debug level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileDebug(string message) => LogFile(message, LogLevel.Debug);

    /// <summary>
    /// Logs an informational message to the log file (Info: general operational messages).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Info level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileInfo(string message) => LogFile(message, LogLevel.Info);

    /// <summary>
    /// Logs a warning message to the log file (Warning: indication of potential issues).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Warning level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileWarning(string message) => LogFile(message, LogLevel.Warning);

    /// <summary>
    /// Logs an error message to the log file (Error: an error has occurred).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Error level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileError(string message) => LogFile(message, LogLevel.Error);

    /// <summary>
    /// Logs a fatal error message to the log file (Fatal: severe error causing premature termination).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Fatal level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileFatal(string message) => LogFile(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a trace message to the log file (Trace: fine-grained informational events).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Trace level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileTrace(string message) => LogFile(message, LogLevel.Trace);

    /// <summary>
    /// Logs a notice message to the log file (Notice: normal but significant condition).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Notice level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileNotice(string message) => LogFile(message, LogLevel.Notice);

    /// <summary>
    /// Logs an alert message to the log file (Alert: action must be taken immediately).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Alert level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileAlert(string message) => LogFile(message, LogLevel.Alert);

    /// <summary>
    /// Logs an emergency message to the log file (Emergency: system is unusable).
    /// </summary>
    /// <remarks>
    /// This is a shortcut for logging with the Emergency level using the <see cref="LogFile(string, LogLevel)"/> function.
    /// </remarks>
    public void LogFileEmergency(string message) => LogFile(message, LogLevel.Emergency);
    
    #endregion
    
}