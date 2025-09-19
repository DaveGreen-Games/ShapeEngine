using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Logging;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Content;

//NOTE: Handles loading from packed files (text or binary) to memory
//NOTE: Handles index creation and data retrieval


//TODO: Unify into ContentPack -> Constructor allows to set unpack mode (memory/indexed) and source file path
// this way abstract is no needed and the only class is ContentPack

public abstract class ContentPack
{
    public bool IsLoaded { get; protected set; } = false;
    
    public abstract byte[]? GetFileData(string filePath);
    public abstract bool HasFile(string filePath);
    
    public abstract bool LoadContent(bool parallelProcessing = false, List<string>? extensionExceptions = null,  bool debugLogging = false);
    public abstract bool ClearCache();
}

public sealed class ContentPackMemory : ContentPack
{
    private Dictionary<string, byte[]> cache = new();

    public int TextFileUnpackBatchSize = 16;
    public long CacheSize { get; private set; } = 0;
    public readonly bool IsTextPack;
    public readonly string SourceFilePath;

    public ContentPackMemory(string sourceFilePath)
    {
        if(!Path.HasExtension(sourceFilePath))
        {
            throw new ArgumentException("Source file path must have a valid extension. (.txt for text packs, others for binary packs)");
        }
        
        SourceFilePath = sourceFilePath;
        IsTextPack = Path.GetExtension(sourceFilePath).Equals(".txt", StringComparison.CurrentCultureIgnoreCase);
    }
    
    public override byte[]? GetFileData(string filePath)
    {
        return !HasFile(filePath) ? null : cache[filePath];
    }
    public override bool HasFile(string filePath)
    {
        return cache.ContainsKey(filePath);
    }

    public override bool LoadContent(bool parallelProcessing = false, List<string>? extensionExceptions = null,  bool debugLogging = false)
    {
        if (IsLoaded) return false;
        if (!File.Exists(SourceFilePath))
        {
            ShapeLogger.LogError($"ContentPackMemory LoadContent() failed.  Source file not found: {SourceFilePath}");
            return false;
        }

        if (IsTextPack)
        {
            if (parallelProcessing)
            {
                cache = UnpackTextToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, debugLogging, TextFileUnpackBatchSize);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackTextToMemory(SourceFilePath, out long cacheSize, extensionExceptions, debugLogging);
                CacheSize = cacheSize;
            }
        }
        else
        {
            if (parallelProcessing)
            {
                cache = UnpackFileToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, debugLogging);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackFileToMemory(SourceFilePath, out long cacheSize, extensionExceptions, debugLogging);
                CacheSize = cacheSize;
            }
        }

        if (cache.Count <= 0)
        {
            CacheSize = 0;
            IsLoaded = false;
            return false;
        }

        IsLoaded = true;
        return true;
    }

    public override bool ClearCache()
    {
        if (!IsLoaded) return false;
        IsLoaded = false;
        CacheSize = 0;
        cache.Clear();
        return true;
    }
    
    #region Unpacking
    //TODO: change console write line to debuglogger
    public static Dictionary<string, byte[]> UnpackFileToMemory(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>() : [];
        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        var header = new byte[12];
        fileStream.ReadExactly(header, 0, 12);
        int totalFiles = BitConverter.ToInt32(header, 0);

        int unpackedFiles = 0;
        
        int currentFile = 0;

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

            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
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

            result[relativePath] = data;
            unpackedFiles++;
            totalBytesUnpacked += data.Length;

            if (debug) debugMessages.Add($"Unpacked {relativePath} ({data.Length} bytes)");
        }

        if (debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        Console.WriteLine($"Unpacking to memory finished. {unpackedFiles} files loaded from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    public static Dictionary<string, byte[]> UnpackFileToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false)
    {
        //TODO: Implement
        totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
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
    
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    fileStream.Position += dataLen; // skip data
                    continue;
                }
    
                entries.Add((relativePath, dataOffset, dataLen));
                fileStream.Position += dataLen;
            }
        }
    
        // Second pass: decompress in parallel
        Parallel.ForEach(entries, entry =>
        {
            using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.Position = entry.dataOffset;
            var compressedData = new byte[entry.dataLen];
            fs.ReadExactly(compressedData, 0, entry.dataLen);
    
            using var ms = new MemoryStream(compressedData);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            gzip.CopyTo(outMs);
            var data = outMs.ToArray();
    
            result[entry.path] = data;
            debugMessages?.Add($"Unpacked {entry.path} ({data.Length} bytes)");
        });
    
        if (debugMessages != null)
            foreach (var msg in debugMessages) Console.WriteLine(msg);
    
        Console.WriteLine($"Unpacking to memory (parallel) finished. {result.Count} files loaded from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return new Dictionary<string, byte[]>(result);
    }
    
    public static Dictionary<string, byte[]> UnpackTextToMemory(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false)
    {
        totalBytesUnpacked = 0;
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
    public static Dictionary<string, byte[]> UnpackTextToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        //TODO: Implement
        totalBytesUnpacked = 0;
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
    #endregion
}

public sealed class ContentPackIndexed : ContentPack
{
    public override byte[]? GetFileData(string filePath)
    {
        throw new NotImplementedException();
    }

    public override bool HasFile(string filePath)
    {
        throw new NotImplementedException();
    }

    public override bool LoadContent(bool parallelProcessing = false, List<string>? extensionExceptions = null,  bool debugLogging = false)
    {
        throw new NotImplementedException();
    }

    public override bool ClearCache()
    {
        throw new NotImplementedException();
    }
}