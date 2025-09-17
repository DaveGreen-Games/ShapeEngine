using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;

namespace ResourcePacker;

//TODO: add documentation comments
public static class BinaryPackManager
{
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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
    
        string? outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
        if (outputDirectoryPath == null)
        {
            Console.WriteLine($"Directory not found: {outputDirectoryPath}");
            return false;
        }
    
        if (!Directory.Exists(outputDirectoryPath))
        {
            Console.WriteLine($"Created output directory: {outputDirectoryPath}");
            Directory.CreateDirectory(outputDirectoryPath);
        }
    
        var debugWatch = Stopwatch.StartNew();
        string[] files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        var totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Binary File packing started with {total} files.");
    
        // Index: path -> (offset, compressed length)
        var index = new Dictionary<string, long>();
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        // Reserve 12 bytes: [total files (4)][index offset (8)]
        fileStream.Write(new byte[12], 0, 12);
    
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
    
            string relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
            byte[] data = File.ReadAllBytes(file);
    
            // Compress data individually
            byte[] compressedData;
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                {
                    gzip.Write(data, 0, data.Length);
                }
                compressedData = ms.ToArray();
            }
    
            long offset = fileStream.Position;
    
            // Write header and compressed data
            fileStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            fileStream.Write(pathBytes, 0, pathBytes.Length);
            fileStream.Write(BitConverter.GetBytes(compressedData.Length), 0, 4);
            fileStream.Write(compressedData, 0, compressedData.Length);
    
            index[relativePath] = offset;
    
            totalFilesPacked++;
            totalBytesPacked += compressedData.Length;
            if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes, compressed {compressedData.Length} bytes)");
        }
    
        // Write index at the end
        long indexOffset = fileStream.Position;
        fileStream.Write(BitConverter.GetBytes(index.Count), 0, 4);
        foreach (var kvp in index)
        {
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key);
            fileStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            fileStream.Write(pathBytes, 0, pathBytes.Length);
            fileStream.Write(BitConverter.GetBytes(kvp.Value), 0, 8);
        }
    
        // Seek back and write total files and index offset
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
        fileStream.Write(BitConverter.GetBytes(indexOffset), 0, 8);

        if (debugMessages.Count > 0)
        {
            foreach (string msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        Console.WriteLine($"Index written at offset {indexOffset}");
        return true;
    }
    public static bool PackParallel(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
    
        Console.WriteLine($"Binary File packing started with {total} files.");
    
        var index = new Dictionary<string, long>();
        var queue = new BlockingCollection<(string path, byte[] compressed, int originalSize)>(boundedCapacity: Environment.ProcessorCount * 2);
    
        // Producer: compress files in parallel and add to queue
        var producer = Task.Run(() =>
        {
            Parallel.ForEach(files, file =>
            {
                if (extensionExceptions != null && IsExtensionException(Path.GetExtension(file), extensionExceptions)) return;
                
                var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
                var data = File.ReadAllBytes(file);
    
                using var ms = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                    gzip.Write(data, 0, data.Length);
    
                queue.Add((relativePath, ms.ToArray(), data.Length));
            });
            queue.CompleteAdding();
        });
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        fileStream.Write(new byte[12], 0, 12); // Reserve header
    
        foreach (var entry in queue.GetConsumingEnumerable())
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
            {
                ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
    
            long offset = fileStream.Position;
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(entry.path);
            fileStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            fileStream.Write(pathBytes, 0, pathBytes.Length);
            fileStream.Write(BitConverter.GetBytes(entry.compressed.Length), 0, 4);
            fileStream.Write(entry.compressed, 0, entry.compressed.Length);
            index[entry.path] = offset;
    
            totalFilesPacked++;
            totalBytesPacked += entry.compressed.Length;
            if (debug) debugMessages?.Add($"Packed {entry.path} ({entry.originalSize} bytes, compressed {entry.compressed.Length} bytes)");
        }
    
        producer.Wait();
    
        // Write index at the end
        long indexOffset = fileStream.Position;
        fileStream.Write(BitConverter.GetBytes(index.Count), 0, 4);
        foreach (var kvp in index)
        {
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key);
            fileStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            fileStream.Write(pathBytes, 0, pathBytes.Length);
            fileStream.Write(BitConverter.GetBytes(kvp.Value), 0, 8);
        }
    
        // Seek back and write total files and index offset
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
        fileStream.Write(BitConverter.GetBytes(indexOffset), 0, 8);

        if (debugMessages is { Count: > 0 })
        {
            foreach (string msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        Console.WriteLine($"Index written at offset {indexOffset}");
        return true;
    }
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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
        var header = new byte[12];
        fileStream.ReadExactly(header, 0, 12);
        int totalFiles = BitConverter.ToInt32(header, 0);
    
        int unpackedFiles = 0;
        long totalBytesUnpacked = 0;
        int currentFile = 0;
        var sw = Stopwatch.StartNew();
    
        while (currentFile < totalFiles)
        {
            currentFile++;
            
            var pathLenBytes = new byte[4];
            fileStream.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
            var pathBytes = new byte[pathLen];
            fileStream.ReadExactly(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);
    
            var dataLenBytes = new byte[4];
            fileStream.ReadExactly(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);
            
            if (extensionExceptions != null && IsExtensionException(Path.GetExtension(relativePath), extensionExceptions))
            {
                if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                fileStream.Position += dataLen; // skip data
                continue;
            }
            
            var compressedData = new byte[dataLen];
            fileStream.ReadExactly(compressedData, 0, dataLen);
    
            using var ms = new MemoryStream(compressedData);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            gzip.CopyTo(outMs);
            var data = outMs.ToArray();
            
            unpackedFiles++;
            totalBytesUnpacked += data.Length;
    
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || currentFile == totalFiles)
            {
                ResourcePackManager.PrintProgressBar(currentFile, totalFiles, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
    
            var filePath = Path.Combine(outputDirectoryPath, relativePath);
           
            //check for path traversal, eg "../../somefile.txt" would extract outside of outputDirectoryPath
            var fullOutputDir = Path.GetFullPath(outputDirectoryPath);
            var fullFilePath = Path.GetFullPath(filePath);
            if (!fullFilePath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
            {
                if (debug) debugMessages.Add($"Skipped file due to path traversal: {relativePath}. File would be extracted outside of the output directory {outputDirectoryPath}.");
                continue;
            }
            
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
    
            File.WriteAllBytes(filePath, data);
            
            if (debug) debugMessages.Add($"Unpacked {relativePath} ({data.Length} bytes)");
        }
    
        if (debugMessages.Count > 0)
            foreach (var msg in debugMessages) Console.WriteLine(msg);
    
        Console.WriteLine($"Unpacking finished. {unpackedFiles} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    public static bool UnpackParallel(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
        var entries = new List<(string path, long dataOffset, int dataLen)>();
    
        // First pass: read metadata for all files
        using (var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var header = new byte[12];
            fileStream.ReadExactly(header, 0, 12);
            int totalFiles = BitConverter.ToInt32(header, 0);
    
            for (int i = 0; i < totalFiles; i++)
            {
                var pathLenBytes = new byte[4];
                fileStream.ReadExactly(pathLenBytes, 0, 4);
                int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
                var pathBytes = new byte[pathLen];
                fileStream.ReadExactly(pathBytes, 0, pathLen);
                string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);
    
                var dataLenBytes = new byte[4];
                fileStream.ReadExactly(dataLenBytes, 0, 4);
                int dataLen = BitConverter.ToInt32(dataLenBytes, 0);
    
                long dataOffset = fileStream.Position;
    
                if (extensionExceptions != null && IsExtensionException(Path.GetExtension(relativePath), extensionExceptions))
                {
                    debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    fileStream.Position += dataLen; // skip data
                    continue;
                }
    
                entries.Add((relativePath, dataOffset, dataLen));
                fileStream.Position += dataLen;
            }
        }
    
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;
        int currentFile = 0;
        var sw = Stopwatch.StartNew();

        using (var mmf = MemoryMappedFile.CreateFromFile(sourceFilePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
        {
            // Second pass: decompress and write in parallel
            Parallel.ForEach(entries, entry =>
            {
                // ReSharper disable once AccessToDisposedClosure
                using var viewStream = mmf.CreateViewStream(entry.dataOffset, entry.dataLen, MemoryMappedFileAccess.Read);
                // using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                // fs.Position = entry.dataOffset;
                var compressedData = new byte[entry.dataLen];
                viewStream.ReadExactly(compressedData, 0, entry.dataLen);
                // fs.ReadExactly(compressedData, 0, entry.dataLen);

                using var ms = new MemoryStream(compressedData);
                using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                gzip.CopyTo(outMs);
                byte[] data = outMs.ToArray();

                string filePath = Path.Combine(outputDirectoryPath, entry.path);

                //check for path traversal, eg "../../somefile.txt" would extract outside of outputDirectoryPath
                string fullOutputDir = Path.GetFullPath(outputDirectoryPath);
                string fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
                {
                    if (debug)
                        debugMessages?.Add(
                            $"Skipped file due to path traversal: {entry.path}. File would be extracted outside of the output directory {outputDirectoryPath}.");
                    return;
                }

                string? dirPath = Path.GetDirectoryName(filePath);
                if (dirPath != null && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.WriteAllBytes(filePath, data);

                Interlocked.Add(ref totalBytesUnpacked, data.Length);
                Interlocked.Increment(ref unpackedFiles);
                int fileNum = Interlocked.Increment(ref currentFile);

                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || fileNum == entries.Count)
                {
                    ResourcePackManager.PrintProgressBar(fileNum, entries.Count, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }

                debugMessages?.Add($"Unpacked {entry.path} ({data.Length} bytes)");
            });
            GC.KeepAlive(mmf); // Ensures mmf is not collected/disposed before parallel tasks finish
        }


        if (debugMessages != null)
        {
            foreach (string msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }
    
        Console.WriteLine($"Unpacking (parallel) finished. {unpackedFiles} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    

    private static bool IsExtensionException(string extension, List<string> extensionExceptions)
    {
        return extensionExceptions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase));
    }
}