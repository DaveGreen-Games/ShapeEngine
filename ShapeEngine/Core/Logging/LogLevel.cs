namespace ShapeEngine.Core.Logging;

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