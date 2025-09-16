using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Resources;

namespace ResourcePacker;

//TODO: test all pack/unpack functions
//TODO: make final AI check for issues
//TODO: add documentation comments


/// <summary>
/// Provides static methods for packing and unpacking resources and directories
/// into text files or resource files, as well as extracting them.
/// </summary>
public static class ResourcePackManager
{
    public const double ProgressIntervalMilliseconds = 10.0;
    
    #region Public Interface
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool parallel = false, bool batching = false, bool debug = false)
    {
        if (Path.GetExtension(outputFilePath) == ".txt")
        {
            if (parallel)
            {
                return TextPackManager.PackParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
            }
            
            return TextPackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }

        if (batching)
        {
            return BinaryPackManager.PackBatching(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        if (parallel)
        {
            return BinaryPackManager.PackParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        return BinaryPackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
    }
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool parallel = false, bool batching = false, bool debug = false)
    {
        if (Path.GetExtension(sourceFilePath) == ".txt")
        {
            if (parallel)
            {
                return TextPackManager.UnpackParallel(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
            }
            return TextPackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
        }
        return BinaryPackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
    }
    public static void PrintProgressBar(int current, int total, double seconds, int barWidth = 30)
    {
        double percent = (double)current / total;
        int filled = (int)(barWidth * percent);
        int empty = barWidth - filled;
        string bar = new string('/', filled) + new string('_', empty);
        
        if (current == total)
        {
            Console.Write($"\r[File {current}/{total}] [{bar}] [{(percent * 100):F1}%] [{seconds:F2}s]\n");
        }
        else
        {
            Console.Write($"\r[File {current}/{total}] [{bar}] [{(percent * 100):F1}%] [{seconds:F2}s]");
        }
    }
    
    #endregion
}