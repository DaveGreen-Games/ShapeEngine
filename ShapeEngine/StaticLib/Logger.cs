using ShapeEngine.Core.GameDef;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Specifies the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>General informational messages.</summary>
    Info = 1,
    /// <summary>Detailed debugging information.</summary>
    Debug = 2,
    /// <summary>Fine-grained informational events.</summary>
    Trace = 4,
    /// <summary>Normal but significant conditions.</summary>
    Notice = 8,
    /// <summary>Potentially harmful situations.</summary>
    Warning = 16,
    /// <summary>Immediate action required.</summary>
    Alert = 32,
    /// <summary>Error events that might still allow the application to continue running.</summary>
    Error = 64,
    /// <summary>System is unusable.</summary>
    Emergency = 128,
    /// <summary>Critical error causing program termination.</summary>
    Fatal = 256
}

public static class Logger
{
    /// <summary>
    /// Gets or sets the minimum severity level required for a message to be logged.
    /// </summary>
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
    

    // public static bool LogToFile { get; set; } = false;
    /// <summary>
    /// Indicates whether logging to a file is enabled.
    /// </summary>
    private static bool logToFile = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether log messages should be written to a file.
    /// Only enabled if the log file path is valid.
    /// </summary>
    public static bool LogToFile
    {
        get => logFilePathValid && logToFile;
        set
        {
            if (value && !logFilePathValid)
            {
                logToFile = false;
            }
            else
            {
                logToFile = value;
            }
        }
    }
    
    /// <summary>
    /// Indicates whether the log file path is valid.
    /// </summary>
    private static bool logFilePathValid = false;
    
    /// <summary>
    /// Stores the path to the log file.
    /// </summary>
    private static string logFilePath = string.Empty;
    
    /// <summary>
    /// Gets the current log file path.
    /// </summary>
    public static string LogFilePath => logFilePath;
    
    /// <summary>
    /// Sets the log file path if the provided path is valid.
    /// Returns true if the path is valid and set; otherwise, false.
    /// </summary>
    /// <param name="path">The file path to set for logging.</param>
    /// <returns>True if the path is valid and set; otherwise, false.</returns>
    public static bool SetLogFilePath(string path)
    {
        if (!IsValidTxtFilePath(path)) return false;
        logFilePath = path;
        logFilePathValid = true;
        return true;
    }

    /// <summary>
    /// Logs a message with the specified severity level.
    /// If no level is provided, defaults to Info.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The severity level of the log message.</param>
    /// <remarks>
    /// Messages with a severity level lower than the configured MinimumLevel will be ignored.
    /// If LogToFile is enabled and the log file path is valid, messages will be appended to the specified log file.
    /// Otherwise, messages will be printed to the console.
    /// </remarks>
    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (level < MinimumLevel) return;
        
        if (LogToFile)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Logger Error] Failed to write to log file {LogFilePath}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
        }
    }

    /// <summary>
    /// Logs a message with Debug severity.
    /// </summary>
    public static void Debug(string message) => Log(message, LogLevel.Debug);

    /// <summary>
    /// Logs a message with Info severity.
    /// </summary>
    public static void Info(string message) => Log(message, LogLevel.Info);

    /// <summary>
    /// Logs a message with Warning severity.
    /// </summary>
    public static void Warning(string message) => Log(message, LogLevel.Warning);

    /// <summary>
    /// Logs a message with Error severity.
    /// </summary>
    public static void Error(string message) => Log(message, LogLevel.Error);

    /// <summary>
    /// Logs a message with Fatal severity.
    /// </summary>
    public static void Fatal(string message) => Log(message, LogLevel.Fatal);

    /// <summary>
    /// Logs a message with Trace severity.
    /// </summary>
    public static void Trace(string message) => Log(message, LogLevel.Trace);

    /// <summary>
    /// Logs a message with Notice severity.
    /// </summary>
    public static void Notice(string message) => Log(message, LogLevel.Notice);

    /// <summary>
    /// Logs a message with Alert severity.
    /// </summary>
    public static void Alert(string message) => Log(message, LogLevel.Alert);

    /// <summary>
    /// Logs a message with Emergency severity.
    /// </summary>
    public static void Emergency(string message) => Log(message, LogLevel.Emergency);
    
    /// <summary>
    /// Checks if the provided path is a valid .txt file path.
    /// Returns true if the path is non-empty, ends with .txt, and contains no invalid path characters.
    /// </summary>
    private static bool IsValidTxtFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (!path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) return false;
        foreach (char c in Path.GetInvalidPathChars())
        {
            if (path.Contains(c)) return false;
        }
        return true;
    }
}