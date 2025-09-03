namespace ShapeEngine.Core.Logging;

/// <summary>
/// Specifies the output destination for log messages.
/// </summary>
/// <remarks>
/// <see cref="LogOutputType.File"/> and <see cref="LogOutputType.Both"/> need valid file paths to write to a file.
/// </remarks>
public enum LogOutputType
{
    /// <summary>Log messages are written to the console.</summary>
    Console,
    /// <summary>Log messages are written to a file.</summary>
    File,
    /// <summary>Log messages are written to both the console and a file.</summary>
    Both
}