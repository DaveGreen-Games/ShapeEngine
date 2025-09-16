using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace ResourcePacker;

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
    
        // Index: path -> (offset, compressed length)
        var index = new Dictionary<string, long>();
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        // Reserve 12 bytes: [total files (4)][index offset (8)]
        fileStream.Write(new byte[12], 0, 12);
    
        foreach (var file in files)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
            {
                ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
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
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
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
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Binary File packing started with {total} files.");
    
        var index = new Dictionary<string, long>();
        var queue = new BlockingCollection<(string path, byte[] compressed, int originalSize)>(boundedCapacity: Environment.ProcessorCount * 2);
    
        // Producer: compress files in parallel and add to queue
        var producer = Task.Run(() =>
        {
            Parallel.ForEach(files, file =>
            {
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                    return;
    
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
            if (debug) debugMessages.Add($"Packed {entry.path} ({entry.originalSize} bytes, compressed {entry.compressed.Length} bytes)");
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
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        Console.WriteLine($"Index written at offset {indexOffset}");
        return true;
    }
    
    
    
    public static Dictionary<string, long> ReadIndexFromPackedFile(string packedFilePath)
    {
        var index = new Dictionary<string, long>();
        using var fs = new FileStream(packedFilePath, FileMode.Open, FileAccess.Read);
    
        // Read total files (4 bytes) and index offset (8 bytes)
        var header = new byte[12];
        fs.ReadExactly(header, 0, 12);
        long indexOffset = BitConverter.ToInt64(header, 4);
    
        fs.Position = indexOffset;
        var countBytes = new byte[4];
        fs.ReadExactly(countBytes, 0, 4);
        int count = BitConverter.ToInt32(countBytes, 0);
    
        for (int i = 0; i < count; i++)
        {
            var pathLenBytes = new byte[4];
            fs.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
            var pathBytes = new byte[pathLen];
            fs.ReadExactly(pathBytes, 0, pathLen);
            string path = System.Text.Encoding.UTF8.GetString(pathBytes);
    
            var offsetBytes = new byte[8];
            fs.ReadExactly(offsetBytes, 0, 8);
            long offset = BitConverter.ToInt64(offsetBytes, 0);

            index[path] = offset;
        }
        return index;
    }
    public static byte[] LoadFileFromPackedFile(string packedFilePath, string relativePath, Dictionary<string, long> index)
    {
        if (!index.TryGetValue(relativePath, out long offset))
        {
            throw new FileNotFoundException($"File '{relativePath}' not found in packed file.");
        }
    
        using var fs = new FileStream(packedFilePath, FileMode.Open, FileAccess.Read);
        fs.Position = offset;
    
        // Read header: [path length][path][data length]
        var pathLenBytes = new byte[4];
        fs.ReadExactly(pathLenBytes, 0, 4);
        int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
        fs.Position += pathLen; // skip path bytes
    
        var dataLenBytes = new byte[4];
        fs.ReadExactly(dataLenBytes, 0, 4);
        int dataLen = BitConverter.ToInt32(dataLenBytes, 0);
    
        var compressedData = new byte[dataLen];
        fs.ReadExactly(compressedData, 0, dataLen);
    
        using var ms = new MemoryStream(compressedData);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gzip.CopyTo(outMs);
        return outMs.ToArray();
    }
    
    
    
    
    //TODO: look at performance and memory usage
    // can batching or parallel improve it?
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
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || currentFile == totalFiles)
            {
                ResourcePackManager.PrintProgressBar(currentFile, totalFiles, debugWatch.Elapsed.TotalSeconds);
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

}