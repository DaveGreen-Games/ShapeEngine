namespace ResourcePacker;

/// <summary>
/// Provides static methods for packing and unpacking resources and directories
/// into text files or resource files, as well as extracting them.
/// </summary>
public static class ResourcePackManager
{
    /// <summary>
    /// The interval in milliseconds between progress updates when packing or unpacking resources.
    /// </summary>
    public const double ProgressIntervalMilliseconds = 10.0;
    
    #region Public Interface
    
    /// <summary>
    /// Packs the contents of a source directory into a file.
    /// Supports packing as text or binary, with optional parallel processing and debug output.
    /// </summary>
    /// <param name="outputFilePath">The path to the output file (.txt for text, other for binary).</param>
    /// <param name="sourceDirectoryPath">The path to the source directory to pack.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to exclude from packing.</param>
    /// <param name="parallel">If true, enables parallel packing.</param>
    /// <param name="debug">If true, enables debug output.</param>
    /// <returns>True if packing succeeds; otherwise, false.</returns>
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool parallel = false, bool debug = false)
    {
        if (Path.GetExtension(outputFilePath) == ".txt")
        {
            if (parallel)
            {
                return TextPackManager.PackParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
            }
            
            return TextPackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        if (parallel)
        {
            return BinaryPackManager.PackParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        return BinaryPackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
    }
    
    /// <summary>
    /// Unpacks the contents of a packed resource file into a directory.
    /// Supports unpacking as text or binary, with optional parallel processing and debug output.
    /// </summary>
    /// <param name="outputDirectoryPath">The path to the output directory where files will be unpacked.</param>
    /// <param name="sourceFilePath">The path to the packed resource file (.txt for text, other for binary).</param>
    /// <param name="extensionExceptions">Optional list of file extensions to exclude from unpacking.</param>
    /// <param name="parallel">If true, enables parallel unpacking.</param>
    /// <param name="debug">If true, enables debug output.</param>
    /// <returns>True if unpacking succeeds; otherwise, false.</returns>
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool parallel = false, bool debug = false)
    {
        if (Path.GetExtension(sourceFilePath) == ".txt")
        {
            if (parallel)
            {
                return TextPackManager.UnpackParallel(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
            }
            return TextPackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
        }
        if (parallel)
        {
            return BinaryPackManager.UnpackParallel(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
        }
        return BinaryPackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
    }
    
    /// <summary>
    /// Prints a progress bar to the console indicating the current progress of a file operation.
    /// </summary>
    /// <param name="current">The current progress value (e.g., number of files processed).</param>
    /// <param name="total">The total value representing completion (e.g., total number of files).</param>
    /// <param name="seconds">Elapsed time in seconds.</param>
    /// <param name="barWidth">The width of the progress bar in characters. Default is 30.</param>
    public static void PrintProgressBar(int current, int total, double seconds, int barWidth = 30)
    {
        if (total <= 0) return;
        
        double percent = (double)current / total;
        int filled = (int)(barWidth * percent);
        int empty = barWidth - filled;
        string bar = new string('/', filled) + new string('_', empty);
        
        if (current == total)
        {
            Console.WriteLine($"\r[File {current}/{total}] [{bar}] [{(percent * 100):F1}%] [{seconds:F2}s]\n");
        }
        else
        {
            Console.Write($"\r[File {current}/{total}] [{bar}] [{(percent * 100):F1}%] [{seconds:F2}s]");
        }
    }
    
    #endregion
}