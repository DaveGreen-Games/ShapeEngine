using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Resources;

namespace ResourcePacker;

//TODO: make final AI check for issues
//TODO: add documentation comments


//TODO: There should only be a non-parallel and a parallel version of each packer / unpacker
// the non-parallel version is the most optimal for low memory usage
// the parallel version is the most optimal for speed


/// <summary>
/// Provides static methods for packing and unpacking resources and directories
/// into text files or resource files, as well as extracting them.
/// </summary>
public static class ResourcePackManager
{
    private const double progressIntervalMilliseconds = 10.0;
    
    #region Public Interface
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool parallel = false, bool batching = false, bool debug = false)
    {
        if (Path.GetExtension(outputFilePath) == ".txt")
        {
            if (batching)
            {
                return  PackToTextBatching(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
            }

            if (parallel)
            {
                return PackToTextParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
            }
            
            return PackToText(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }

        if (batching)
        {
            return PackToFileBatching(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        if (parallel)
        {
            return PackToFileParallel(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        
        return PackToFile(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
    }
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool parallel = false, bool batching = false, bool debug = false)
    {
        if (Path.GetExtension(sourceFilePath) == ".txt")
        {
            // return UnpackFromTextBatchingParallel(outputDirectoryPath, sourceFilePath, extensionExceptions, debug, 16);
            
            // var value = UnpackFromTextBatchingParallel(sourceFilePath, extensionExceptions, debug, 16);
            //
            // Console.WriteLine($"Going to sleep for 15 seconds to allow async operations to complete...");
            // Thread.Sleep(15000);
            //
            // return value.Count > 0;
            
            if (batching)
            {
                return UnpackFromTextBatching(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
            }

            if (parallel)
            {
                return UnpackFromTextParallel(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
            }
            return UnpackFromText(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
        }
        return UnpackFromFile(outputDirectoryPath, sourceFilePath, extensionExceptions, debug);
    }
    private static void PrintProgressBar(int current, int total, double seconds, int barWidth = 30)
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
    
    #region Text Packer
    //TODO: Combine into 2 pack and 2 unpack functions (parallel and non-parallel) (always batching)
    
    private static bool PackToText(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if(!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Source Directory not found: {sourceDirectoryPath}");
            return false;
        }

        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"No output file extension found: {outputFilePath}");
            return false;
        }

        if (Path.GetExtension(outputFilePath) != ".txt")
        {
            Console.WriteLine($"Output file extension must be .txt: {outputFilePath}");
            return false;
        }
        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (outputDirectory == null)
        {
            Console.WriteLine($"Directory not found: {outputFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"Created output directory: {outputDirectory}");
        }
        var debugWatch = Stopwatch.StartNew();
        
        string[] files = Directory.GetFiles(sourceDirectoryPath, "", SearchOption.AllDirectories);

        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        int total = files.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"Text File packing started with {total} files.");
        
        var lines = new List<string>();
        foreach (string file in files)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
            {
                PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
            
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
            {
                if (debug)
                {
                    debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                }
                continue;
            }
            
            lines.Add(Path.GetRelativePath(sourceDirectoryPath, file));
            byte[] d = File.ReadAllBytes(file);
            totalBytesPacked += d.Length;
            totalFilesPacked++;
            lines.Add(Convert.ToBase64String(Compress(d)));
            if (debug)
            {
                debugMessages.Add($" -File {file} has been packed with {d.Length} bytes.");
            }
        }
        //Data -> Empty Line -> Number of files packed
        File.WriteAllLines(outputFilePath, lines.Concat(["", totalFilesPacked.ToString()]));
        
        PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        Console.WriteLine($"File packing finished. With {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    private static bool PackToTextBatching(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if(!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Source Directory not found: {sourceDirectoryPath}");
            return false;
        }

        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"No output file extension found: {outputFilePath}");
            return false;
        }

        if (Path.GetExtension(outputFilePath) != ".txt")
        {
            Console.WriteLine($"Output file extension must be .txt: {outputFilePath}");
            return false;
        }
        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (outputDirectory == null)
        {
            Console.WriteLine($"Directory not found: {outputFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"Created output directory: {outputDirectory}");
        }

        var debugWatch = Stopwatch.StartNew();
        string[] files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"Text File packing started with {total} files. Batch");
        using var writer = new StreamWriter(outputFilePath, false);

        foreach (string file in files)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
            {
                PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }

            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
            {
                if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                continue;
            }

            writer.WriteLine(Path.GetRelativePath(sourceDirectoryPath, file));
            byte[] d = File.ReadAllBytes(file);
            totalBytesPacked += d.Length;
            totalFilesPacked++;
            writer.WriteLine(Convert.ToBase64String(Compress(d)));

            if (debug) debugMessages.Add($" -File {file} has been packed with {d.Length} bytes.");
        }

        writer.WriteLine("");
        writer.WriteLine(totalFilesPacked.ToString());
        
        PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        Console.WriteLine($"File packing finished. With {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    private static bool PackToTextParallel(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if(!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Source Directory not found: {sourceDirectoryPath}");
            return false;
        }

        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"No output file extension found: {outputFilePath}");
            return false;
        }

        if (Path.GetExtension(outputFilePath) != ".txt")
        {
            Console.WriteLine($"Output file extension must be .txt: {outputFilePath}");
            return false;
        }
        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (outputDirectory == null)
        {
            Console.WriteLine($"Directory not found: {outputFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"Created output directory: {outputDirectory}");
        }
    
        var debugWatch = Stopwatch.StartNew();
        string[] files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int processorCount = Environment.ProcessorCount;
        var tempFiles = new string[processorCount];
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
        int total = files.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();
        int totalFilesPacked = 0;
        long totalBytesPacked = 0;
        Console.WriteLine($"Text File packing started with {total} files. Parallel on {processorCount} threads.");
    
        Parallel.For(0, processorCount, i =>
        {
            tempFiles[i] = Path.Combine(outputDirectory, $"__packtemp_{i}.txt");
            using var writer = new StreamWriter(tempFiles[i], false);
            for (int j = i; j < files.Length; j += processorCount)
            {
                int progress = Interlocked.Increment(ref current);
                if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || progress == total)
                {
                    PrintProgressBar(progress, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
    
                var file = files[j];
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                {
                    debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                    continue;
                }
                writer.WriteLine(Path.GetRelativePath(sourceDirectoryPath, file));
                byte[] d = File.ReadAllBytes(file);
                writer.WriteLine(Convert.ToBase64String(Compress(d)));
                Interlocked.Increment(ref totalFilesPacked);
                Interlocked.Add(ref totalBytesPacked, d.Length);
                debugMessages?.Add($" -File {file} has been packed with {d.Length} bytes.");
            }
        });
    
        using (var outputWriter = new StreamWriter(outputFilePath, false))
        {
            foreach (var tempFile in tempFiles)
            {
                foreach (var line in File.ReadLines(tempFile))
                    outputWriter.WriteLine(line);
                File.Delete(tempFile);
            }
            outputWriter.WriteLine("");
            outputWriter.WriteLine(totalFilesPacked.ToString());
        }
    
        PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages != null && debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
        Console.WriteLine($"File packing finished. {total} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return true;
    }
    private static bool UnpackFromText(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }

        if (!Directory.Exists(outputDirectoryPath))
        {
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
            Directory.CreateDirectory(outputDirectoryPath);
        }

        var debugWatch = Stopwatch.StartNew();
        
        var lines = File.ReadAllLines(sourceFilePath);
        
        long totalBytesUnpacked = 0;
        int totalFilesUnpacked = 0;
        int total = int.Parse(lines[^1]); // parse last line for total files packed
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total / 2) : [];
        Console.WriteLine($"File unpacking started with {total} lines.");
        
        //last 2 lines are meta data
        for (int i = 0; i < lines.Length - 2; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            if (lines[i] == string.Empty) continue;
            
            current += 1;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
            {
                PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
            
            var relativePath = lines[i];
            
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug)
                {
                    debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                }
                continue;
            }
            
            var filePath = Path.Combine(outputDirectoryPath, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if(dirPath == null) continue;
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                if(debug) debugMessages.Add($"Output Directory created: {dirPath}");
            }
            var base64Data = lines[i + 1];
            var compressedData = Convert.FromBase64String(base64Data);
            var data = Decompress(compressedData);
            totalBytesUnpacked += data.Length;
            totalFilesUnpacked++;
            File.WriteAllBytes(filePath, data);
            if (debug)
            {
                debugMessages.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
            }
        }
        
        PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        Console.WriteLine($"File unpacking finished. With {totalFilesUnpacked} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    private static bool UnpackFromTextParallel(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }
        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
        }
    
        var debugWatch = Stopwatch.StartNew();
        var lines = File.ReadAllLines(sourceFilePath);
        int total = int.Parse(lines[^1]); // parse last line for total files packed
        int current = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
        int totalFilesUnpacked = 0;
        long totalBytesUnpacked = 0;
        Console.WriteLine($"Parallel text unpacking started with {total} files.");
    
        Parallel.For(0, total, i =>
        {
            if (i * 2 + 1 >= lines.Length - 2) return;
            
            int progress = Interlocked.Add(ref current, 1);
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || progress == total)
            {
                PrintProgressBar(progress, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
    
            var relativePath = lines[i * 2];
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                return;
            }
    
            var filePath = Path.Combine(outputDirectoryPath, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
    
            var base64Data = lines[i * 2 + 1];
            var compressedData = Convert.FromBase64String(base64Data);
            var data = Decompress(compressedData);
            Interlocked.Add(ref totalBytesUnpacked, data.Length);
            Interlocked.Increment(ref totalFilesUnpacked);
            File.WriteAllBytes(filePath, data);
    
            if (debug) debugMessages?.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
        });
    
        PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);

        if (debugMessages != null && debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
    
        Console.WriteLine($"Parallel unpacking finished. {totalFilesUnpacked} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds with {totalBytesUnpacked} total bytes unpacked.");
        return true;
    }
    private static bool UnpackFromTextBatching(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }
        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
        }

        var debugWatch = Stopwatch.StartNew();
        int current = 0;
        int totalFiles = int.Parse(File.ReadLines(sourceFilePath).Last());
        if (totalFiles <= 0)
        {
            Console.WriteLine("No files to unpack.");
            return false;
        }

        bool finished = false;
        int totalFilesRead = 0;
        long totalBytesUnpacked = 0;
        int totalFilesUnpacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>() : [];
        Console.WriteLine($"Batch unpacking started. Batch size: {batchSize}");

        using var reader = new StreamReader(sourceFilePath);
        var batchLines = new List<string>(batchSize * 2);

        while (!reader.EndOfStream && !finished)
        {
            batchLines.Clear();
            var firstLine = string.Empty;
            for (int i = 0; i < batchSize * 2 && !reader.EndOfStream; i++)
            {
                if (totalFilesRead >= totalFiles)
                {
                    finished = true;
                    break;
                }
                var line = reader.ReadLine();
                if (line != null)
                {
                    if (firstLine == string.Empty)
                    {
                        firstLine = line;
                    }
                    else
                    {
                        //only add complete pairs of two to batch lines
                        batchLines.Add(firstLine);
                        firstLine = string.Empty;
                        batchLines.Add(line);
                        totalFilesRead++;
                    }
                }
            }

            if (firstLine != string.Empty)
            {
                Console.WriteLine("Odd number of lines in file, last line ignored.");
            }

            int filesInBatch = batchLines.Count / 2;
            for (int i = 0; i < filesInBatch; i++)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds)
                {
                    PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
                
                var relativePath = batchLines[i * 2];
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    continue;
                }

                var filePath = Path.Combine(outputDirectoryPath, relativePath);
                var dirPath = Path.GetDirectoryName(filePath);
                if (dirPath != null && !Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var base64Data = batchLines[i * 2 + 1];
                var compressedData = Convert.FromBase64String(base64Data);
                var data = Decompress(compressedData);
                File.WriteAllBytes(filePath, data);
                totalBytesUnpacked += data.Length;
                totalFilesUnpacked++;
                if (debug) debugMessages.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
            }
        }

        PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        Console.WriteLine($"Batch unpacking finished. {totalFilesUnpacked} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    private static bool UnpackFromTextBatchingParallel(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }
        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
        }

        bool finished = false;
        var debugWatch = Stopwatch.StartNew();
        int current = 0;
        int totalFiles = int.Parse(File.ReadLines(sourceFilePath).Last());
        if (totalFiles <= 0)
        {
            Console.WriteLine("No files to unpack.");
            return false;
        }
        int totalFilesRead = 0;
        long totalBytesUnpacked = 0;
        int totalFilesUnpacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>() : [];
        Console.WriteLine($"Batch Parallel unpacking started. Batch size: {batchSize}");

        using var reader = new StreamReader(sourceFilePath);
        var batchLines = new List<string>(batchSize * 2);

        while (!reader.EndOfStream && !finished)
        {
            batchLines.Clear();
            var firstLine = string.Empty;
            for (int i = 0; i < batchSize * 2 && !reader.EndOfStream; i++)
            {
                if (totalFilesRead >= totalFiles)
                {
                    finished = true;
                    break;
                }
                
                var line = reader.ReadLine();
                if (line != null)
                {
                    if(firstLine == string.Empty) firstLine = line;
                    else
                    {
                        //only add complete pairs of two to batch lines
                        batchLines.Add(firstLine);
                        firstLine = string.Empty;
                        batchLines.Add(line);
                        totalFilesRead++;
                    }
                }
            }

            if (firstLine != string.Empty)
            {
                Console.WriteLine("Odd number of lines in file, last line ignored.");
            }

            int filesInBatch = batchLines.Count / 2;
            Parallel.For(0, filesInBatch, i =>
            {
                var relativePath = batchLines[i * 2];
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    if (debug) lock (debugMessages) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    return;
                }

                var filePath = Path.Combine(outputDirectoryPath, relativePath);
                var dirPath = Path.GetDirectoryName(filePath);
                if (dirPath != null && !Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var base64Data = batchLines[i * 2 + 1];
                var compressedData = Convert.FromBase64String(base64Data);
                var data = Decompress(compressedData);
                File.WriteAllBytes(filePath, data);

                Interlocked.Add(ref totalBytesUnpacked, data.Length);
                Interlocked.Increment(ref totalFilesUnpacked);
                if (debug) lock (debugMessages) debugMessages.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
                Interlocked.Increment(ref current);
                if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds)
                {
                    PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
            });
        }

        PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        Console.WriteLine($"Batch unpacking finished. {totalFilesUnpacked} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }

    public static Dictionary<string, byte[]> UnpackFromText(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return result;
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        var lines = File.ReadAllLines(sourceFilePath);
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;

        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;

            var relativePath = lines[i];

            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug) Console.WriteLine($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }

            var base64Data = lines[i + 1];
            var compressedData = Convert.FromBase64String(base64Data);
            // var data = Decompress(compressedData);
            
            using var input = new MemoryStream(compressedData);
            using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflateStream.CopyTo(output);
            byte[] data = output.ToArray();
            
            totalBytesUnpacked += data.Length;
            result[relativePath] = data;
            unpackedFiles++;
            if (debug) Console.WriteLine($"Unpacked {relativePath} ({compressedData.Length} bytes)");
        }

        Console.WriteLine($"Unpacking packed text file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    public static Dictionary<string, byte[]> UnpackFromTextBatchingParallel(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        var result = new ConcurrentDictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        var debugWatch = Stopwatch.StartNew();
        int totalFiles = int.Parse(File.ReadLines(sourceFilePath).Last());
        if (totalFiles <= 0)
        {
            Console.WriteLine("No files to unpack.");
            return new Dictionary<string, byte[]>();
        }

        Console.WriteLine($"Batch Parallel unpacking to memory started. Batch size: {batchSize}");
        bool finished = false;
        int totalFilesRead = 0;
        var debugMessages = debug ? new ConcurrentBag<string>() : null;

        using var reader = new StreamReader(sourceFilePath);
        var batchLines = new List<string>(batchSize * 2);

        while (!reader.EndOfStream && !finished)
        {
            batchLines.Clear();
            var firstLine = string.Empty;
            for (int i = 0; i < batchSize * 2 && !reader.EndOfStream; i++)
            {
                if (totalFilesRead >= totalFiles)
                {
                    finished = true;
                    break;
                }
                var line = reader.ReadLine();
                if (line != null)
                {
                    if (firstLine == string.Empty)
                    {
                        firstLine = line;
                    }
                    else
                    {
                        batchLines.Add(firstLine);
                        firstLine = string.Empty;
                        batchLines.Add(line);
                        totalFilesRead++;
                    }
                }
            }

            if (firstLine != string.Empty)
            {
                Console.WriteLine("Odd number of lines in file, last line ignored.");
            }

            int filesInBatch = batchLines.Count / 2;
            Parallel.For(0, filesInBatch, i =>
            {
                var relativePath = batchLines[i * 2];
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    if (debug) debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    return;
                }

                var base64Data = batchLines[i * 2 + 1];
                var compressedData = Convert.FromBase64String(base64Data);
                using var input = new MemoryStream(compressedData);
                using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);
                byte[] data = output.ToArray();

                result[relativePath] = data;
                if (debug) debugMessages?.Add($"Unpacked {relativePath} ({compressedData.Length} bytes)");
            });
        }

        Console.WriteLine($"Unpacking packed text file {sourceFilePath} to memory finished. {result.Count} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        if (debugMessages != null && debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
        return new Dictionary<string, byte[]>(result);
    }
    
    public static byte[] GetDataFromText(string sourceFilePath, long byteOffset)
    {
        using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        fs.Seek(byteOffset, SeekOrigin.Begin);
        using var reader = new StreamReader(fs);
        var base64Data = reader.ReadLine();
        if (base64Data == null) throw new IndexOutOfRangeException("Index out of range.");
        var compressedData = Convert.FromBase64String(base64Data);
        using var input = new MemoryStream(compressedData);
        using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflateStream.CopyTo(output);
        return output.ToArray();
    }

    public static Dictionary<string, long> CreateIndexFromText(string sourceFilePath, bool debug = false)
    {
        var index = new Dictionary<string, long>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return index;
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return index;
        }

        var debugWatch = Stopwatch.StartNew();
        int indexedFiles = 0;

        
        using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs);
        
        while (!reader.EndOfStream)
        {
            // long offset = fs.Position;
            string? path = reader.ReadLine();
            long dataOffset = fs.Position;
            string? data = reader.ReadLine();
            if (path != null && data != null)
            {
                index[path] = dataOffset;
                if (debug)
                {
                    Console.WriteLine($"Indexed File {path} with offset {dataOffset})");
                }
            }
            indexedFiles++;
        }

        Console.WriteLine($"Indexing packed text file {sourceFilePath} finished. {indexedFiles} files indexed in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return index;
    }
    
    private static byte[] Decompress(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflateStream.CopyTo(output);
        return output.ToArray();
    }
    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    
    #endregion
    
    #region Binary Packer
    //TODO: test parallel / batching speed and memory usage (parallel should be the fastest, batching should use the least memory)
    private static bool PackToFile(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
        
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
        
        var debugWatch = Stopwatch.StartNew();
        
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Binary File packing started with {total} files.");
        
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        //Create placeholder for writing total files packed after packing is done
        fileStream.Write(new byte[4], 0, 4); 
        // using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        
        using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            foreach (var file in files)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
                {
                    PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
        
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                {
                    if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                    continue;
                }
        
                var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
                var data = File.ReadAllBytes(file);
    
                totalFilesPacked++;
                totalBytesPacked += data.Length;
                // Write header: [path length][path][data length][data]
                gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
                gzipStream.Write(pathBytes, 0, pathBytes.Length);
                gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                gzipStream.Write(data, 0, data.Length);
        
                if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
            }
        }
        
        //write to placeholder at the start of the file
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
        
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    private static bool PackToFileParallel(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
    
        var debugWatch = Stopwatch.StartNew();
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Parallel Binary File packing started with {total} files.");
        
        // Parallel read and buffer file data
        var fileDataList = new List<(string RelativePath, byte[] Data)>(files.Length);
        Parallel.ForEach(files, file =>
        {
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                return;
    
            var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
            var data = File.ReadAllBytes(file);
            lock (fileDataList)
            {
                fileDataList.Add((relativePath, data));
            }
        });
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        fileStream.Write(new byte[4], 0, 4); // Placeholder for total files
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true);
    
        foreach (var (relativePath, data) in fileDataList)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
            {
                PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
    
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
            gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            gzipStream.Write(pathBytes, 0, pathBytes.Length);
            gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            gzipStream.Write(data, 0, data.Length);
    
            totalFilesPacked++;
            totalBytesPacked += data.Length;
            if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
        }
    
        // Write total files packed at the start
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    private static bool PackToFileBatching(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
    
        var debugWatch = Stopwatch.StartNew();
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
        int batchSize = 512;
    
        Console.WriteLine($"Binary File packing started with {total} files. Batch Size: {batchSize}");
        
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        fileStream.Write(new byte[4], 0, 4); // Placeholder for total files
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true);
    
        for (int i = 0; i < files.Length; i += batchSize)
        {
            var batch = files.Skip(i).Take(batchSize);
            var fileDataList = new List<(string RelativePath, byte[] Data)>();
    
            foreach (var file in batch)
            {
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                    continue;
    
                var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
                var data = File.ReadAllBytes(file);
                fileDataList.Add((relativePath, data));
            }
    
            foreach (var (relativePath, data) in fileDataList)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
                {
                    PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
    
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
                gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
                gzipStream.Write(pathBytes, 0, pathBytes.Length);
                gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                gzipStream.Write(data, 0, data.Length);
    
                totalFilesPacked++;
                totalBytesPacked += data.Length;
                if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
            }
        }
    
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    
    //TODO: implement parallel / batching for unpack as well
    public static bool UnpackFromFile(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Created output directory: {outputDirectoryPath}");
        }

        var debugWatch = Stopwatch.StartNew();
        
        var debugMessages = debug ? new List<string>() : [];
        
        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        var totalFilesBytes = new byte[4];
        fileStream.ReadExactly(totalFilesBytes, 0, 4);
        int totalFiles = BitConverter.ToInt32(totalFilesBytes, 0);
        Console.WriteLine("Total files to unpack: " + totalFiles);
        
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;
        int currentFile = 0;
        var sw = Stopwatch.StartNew();

        bool failed = false;
        while (currentFile < totalFiles)
        {
            var pathLenBytes = new byte[4];
            gzipStream.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);

            var pathBytes = new byte[pathLen];
            gzipStream.ReadExactly(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);

            var dataLenBytes = new byte[4];
            gzipStream.ReadExactly(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);

            var data = new byte[dataLen];
            gzipStream.ReadExactly(data, 0, dataLen);

            totalBytesUnpacked += dataLen;
            currentFile++;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || currentFile == totalFiles)
            {
                PrintProgressBar(currentFile, totalFiles, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
            //this needs to come after reading data to avoid breaking the stream
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }

            var filePath = Path.Combine(outputDirectoryPath, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            File.WriteAllBytes(filePath, data);
            unpackedFiles++;
            if (debug) debugMessages.Add($"Unpacked {relativePath} ({dataLen} bytes)");
        }

        if (debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        if (failed)
        {
            Console.WriteLine($"Unpacking terminated due to errors after {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
            return false;
        }
        
        Console.WriteLine($"Unpacking finished. {unpackedFiles} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    
    #endregion

    #region External Unpacking
    
    //TODO: add async versions
    
    public static Dictionary<string, byte[]> UnpackFromFile(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;

        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        var totalFilesBytes = new byte[4];
        fileStream.ReadExactly(totalFilesBytes, 0, 4);
        int totalFiles = BitConverter.ToInt32(totalFilesBytes, 0);

        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        for (int i = 0; i < totalFiles; i++)
        {
            var pathLenBytes = new byte[4];
            gzipStream.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);

            var pathBytes = new byte[pathLen];
            gzipStream.ReadExactly(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);

            var dataLenBytes = new byte[4];
            gzipStream.ReadExactly(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);

            var data = new byte[dataLen];
            gzipStream.ReadExactly(data, 0, dataLen);

            //this needs to come after reading data to avoid breaking the stream
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug) Console.WriteLine($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }

            result[relativePath] = data;
            totalBytesUnpacked += dataLen;
            unpackedFiles++;
            if (debug) Console.WriteLine($"Unpacked {relativePath} ({dataLen} bytes)");
        }

        Console.WriteLine($"Unpacking from packed file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    #endregion
}