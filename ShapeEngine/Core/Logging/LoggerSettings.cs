using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Logging;

/// <summary>
/// Represents the configuration settings for the <see cref="Logger"/>.
/// Controls logging enablement, minimum log level, output type, and log file path.
/// </summary>
public readonly struct LoggerSettings
{
    #region Static Factory Methods
    public static readonly LoggerSettings Disabled = new();
    public static readonly LoggerSettings Default = new(LogLevel.Info);
    public static LoggerSettings LogToConsole(LogLevel minimumLogLevel) => new(minimumLogLevel);
    public static LoggerSettings LogToFile(LogLevel minimumLogLevel, string logFilePath) => new(minimumLogLevel, logFilePath, false);
    public static LoggerSettings LogToFileAndConsole(LogLevel minimumLogLevel, string logFilePath) => new(minimumLogLevel, logFilePath, true);
    
    #endregion
    
    #region Members
    /// <summary>
    /// Indicates whether logging is enabled.
    /// </summary>
    public readonly bool Enabled;

    /// <summary>
    /// The minimum log level required for messages to be logged.
    /// </summary>
    public readonly LogLevel MinimumLevel;

    /// <summary>
    /// The file path used for logging to a file.
    /// </summary>
    public readonly string LogFilePath;

    /// <summary>
    /// Specifies the output type for logging (e.g., Console, File, Both).
    /// </summary>
    public readonly LogOutputType OutputType;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSettings"/> struct with default values.
    /// <list type="bullet">
    /// <item><description><see cref="Enabled"/> is set to <c>false</c></description></item>
    /// <item><description><see cref="MinimumLevel"/> is set to <see cref="LogLevel.Info"/></description></item>
    /// <item><description><see cref="OutputType"/> is set to <see cref="LogOutputType.Console"/></description></item>
    /// <item><description><see cref="LogFilePath"/> is set to an empty string</description></item>
    /// </list>
    /// </summary>
    public LoggerSettings()
    {
        Enabled = false;
        MinimumLevel = LogLevel.Info;
        OutputType = LogOutputType.Console;
        LogFilePath = string.Empty;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSettings"/> struct with a specified minimum log level.
    /// <list type="bullet">
    /// <item><description><see cref="Enabled"/> is set to <c>true</c></description></item>
    /// <item><description><see cref="OutputType"/> is set to <see cref="LogOutputType.Console"/></description></item>
    /// <item><description><see cref="LogFilePath"/> is set to an empty string</description></item>
    /// </list>
    /// </summary>
    /// <param name="minimumLevel">The minimum log level required for messages to be logged. Messages below this level will be ignored.</param>
    public LoggerSettings(LogLevel minimumLevel)
    {
        Enabled = true;
        MinimumLevel = minimumLevel;
        OutputType = LogOutputType.Console;
        LogFilePath = string.Empty;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSettings"/> struct with a specified minimum log level,
    /// log file path, and an option to also log to the console.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level required for messages to be logged.</param>
    /// <param name="logFilePath">The file path to write log messages to.</param>
    /// <param name="logToConsole">If true, log messages will be written to both the console and the log file.</param>
    /// <remarks>
    /// If the <paramref name="logFilePath"/> is not valid the <see cref="LogOutputType"/>
    /// will be set to <see cref="LogOutputType.Console"/> and the <see cref="LogFilePath"/> will be empty.
    /// If the specified log file already exists it will be cleared before starting logging.
    /// </remarks>
    public LoggerSettings(LogLevel minimumLevel, string logFilePath, bool logToConsole = false)
    {
        Enabled = true;
        MinimumLevel = minimumLevel;
        
        if (ShapeFileManager.CreateFile(logFilePath, ".txt"))
        {
            LogFilePath = logFilePath;
            OutputType = logToConsole ? LogOutputType.Both : LogOutputType.File;
            
            //file exists and is valid, clear it before starting logging
            File.WriteAllText(logFilePath, string.Empty);
        }
        else
        {
            LogFilePath = string.Empty;
            OutputType = LogOutputType.Console;
        }
        
    }
    #endregion
}