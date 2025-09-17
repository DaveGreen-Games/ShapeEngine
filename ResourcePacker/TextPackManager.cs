using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace ResourcePacker;

public static class TextPackManager
{
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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

        if (!IsExtensionValid(Path.GetExtension(outputFilePath)))
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
        Console.WriteLine($"Text File packing started with {total} files.");
        using var writer = new StreamWriter(outputFilePath, false);

        foreach (string file in files)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
            {
                ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }

            if (extensionExceptions != null && IsExtensionException(Path.GetExtension(file), extensionExceptions))
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
        
        ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
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
    public static bool PackParallel(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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

        if (!IsExtensionValid(Path.GetExtension(outputFilePath)))
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
                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || progress == total)
                {
                    ResourcePackManager.PrintProgressBar(progress, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
    
                var file = files[j];
                if (extensionExceptions != null && IsExtensionException(Path.GetExtension(file), extensionExceptions))
                {
                    if (debug) debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(file)}");
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
    
        ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
        
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
    
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }
        if (!IsExtensionValid(Path.GetExtension(sourceFilePath)))
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
        }

        var lastLine = File.ReadLines(sourceFilePath).LastOrDefault();
        if (lastLine == null || !int.TryParse(lastLine, out int totalFiles) || totalFiles <= 0)
        {
            Console.WriteLine("Pack file is malformed or contains no files to unpack.");
            return false;
        }

        var debugWatch = Stopwatch.StartNew();
        int current = 0;
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
            for (var i = 0; i < filesInBatch; i++)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds)
                {
                    ResourcePackManager.PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
                
                string relativePath = batchLines[i * 2];
                if (extensionExceptions != null && IsExtensionException(Path.GetExtension(relativePath), extensionExceptions))
                {
                    if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    continue;
                }

                string filePath = Path.Combine(outputDirectoryPath, relativePath);
                
                //check for path traversal, eg "../../somefile.txt" would extract outside of outputDirectoryPath
                string fullOutputDir = Path.GetFullPath(outputDirectoryPath);
                string fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
                {
                    if (debug) debugMessages.Add($"Skipped file due to path traversal: {relativePath}. File would be extracted outside of the output directory {outputDirectoryPath}.");
                    continue;
                }
                
                string? dirPath = Path.GetDirectoryName(filePath);
                if (dirPath != null && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string base64Data = batchLines[i * 2 + 1];
                byte[] compressedData = Convert.FromBase64String(base64Data);
                byte[] data = Decompress(compressedData);
                File.WriteAllBytes(filePath, data);
                totalBytesUnpacked += data.Length;
                totalFilesUnpacked++;
                if (debug) debugMessages.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
            }
        }

        ResourcePackManager.PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
        
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
    public static bool UnpackParallel(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source file not found: {sourceFilePath}");
            return false;
        }
        if (!IsExtensionValid(Path.GetExtension(sourceFilePath)))
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
        }
        
        var lastLine = File.ReadLines(sourceFilePath).LastOrDefault();
        if (lastLine == null || !int.TryParse(lastLine, out int totalFiles) || totalFiles <= 0)
        {
            Console.WriteLine("Pack file is malformed or contains no files to unpack.");
            return false;
        }
        
        bool finished = false;
        var debugWatch = Stopwatch.StartNew();
        int current = 0;
        int totalFilesRead = 0;
        long totalBytesUnpacked = 0;
        int totalFilesUnpacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
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
                string relativePath = batchLines[i * 2];
                if (extensionExceptions != null && IsExtensionException(Path.GetExtension(relativePath), extensionExceptions))
                {
                    if (debug) debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    return;
                }

                string filePath = Path.Combine(outputDirectoryPath, relativePath);
                
                //check for path traversal, eg "../../somefile.txt" would extract outside of outputDirectoryPath
                var fullOutputDir = Path.GetFullPath(outputDirectoryPath);
                var fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
                {
                    if (debug) debugMessages?.Add($"Skipped file due to path traversal: {relativePath}. File would be extracted outside of the output directory {outputDirectoryPath}.");
                    return;
                }
                
                string? dirPath = Path.GetDirectoryName(filePath);
                if (dirPath != null && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string base64Data = batchLines[i * 2 + 1];
                byte[] compressedData = Convert.FromBase64String(base64Data);
                byte[] data = Decompress(compressedData);
                File.WriteAllBytes(filePath, data);

                Interlocked.Add(ref totalBytesUnpacked, data.Length);
                Interlocked.Increment(ref totalFilesUnpacked);
                if (debug)  debugMessages?.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
                Interlocked.Increment(ref current);
                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds)
                {
                    ResourcePackManager.PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
            });
        }

        ResourcePackManager.PrintProgressBar(current, totalFiles, debugWatch.Elapsed.TotalSeconds);
        
        if (debugMessages != null && debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        Console.WriteLine($"Batch unpacking finished. {totalFilesUnpacked} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
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
        using var output = new MemoryStream();
        using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    
    private static bool IsExtensionValid(string extension, string validExtension = ".txt")
    {
        return string.Equals(extension, validExtension, StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsExtensionException(string extension, List<string> extensionExceptions)
    {
        return extensionExceptions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase));
    }

}