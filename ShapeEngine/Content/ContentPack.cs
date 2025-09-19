using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Logging;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Content;


//TODO: change console write line to debuglogger
//TODO: add logging for errors and info in most functions
public abstract class ContentPack
{
    public enum UnpackMode
    {
        None,
        Memory,
        Indexed
    }
    
    #region Members
    public bool IsLoaded => CurrentUnpackMode != UnpackMode.None;
    public UnpackMode CurrentUnpackMode { get; private set; } = UnpackMode.None;
    public readonly string SourceFilePath;
    public readonly bool IsTextPack;
    public long CacheSize { get; private set; }
    
    public bool DebugLogging = false;
    
    private Dictionary<string, byte[]> cache = new();
    private Dictionary<string, long> index = new();
    #endregion

    #region Public
    
    public ContentPack(string sourceFilePath)
    {
        if(!Path.HasExtension(sourceFilePath))
        {
            throw new ArgumentException("Source file path must have a valid extension. (.txt for text packs, others for binary packs)");
        }
        
        SourceFilePath = sourceFilePath;
        IsTextPack = Path.GetExtension(sourceFilePath).Equals(".txt", StringComparison.CurrentCultureIgnoreCase);
    }
    
    public byte[]? GetFileData(string filePath)
    {
        if(!IsLoaded) return null;
        if(!HasFile(filePath)) return null;
        if(CurrentUnpackMode == UnpackMode.Memory) return cache[filePath];
        
        if(IsTextPack) return index.TryGetValue(filePath, out long offsetText) ? GetDataFromTextIndex(offsetText) : null;
        return index.TryGetValue(filePath, out long offsetFile) ? GetDataFromFileIndex(offsetFile) : null;
    }
    public bool HasFile(string filePath)
    {
        if(!IsLoaded) return false;
        return CurrentUnpackMode == UnpackMode.Memory ? cache.ContainsKey(filePath) : index.ContainsKey(filePath);
    }

    public bool CreateCache(bool parallelProcessing = false, List<string>? extensionExceptions = null, int textFileUnpackingBatchSize = 16)
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
                cache = UnpackTextToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, DebugLogging, textFileUnpackingBatchSize);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackTextToMemory(SourceFilePath, out long cacheSize, extensionExceptions, DebugLogging);
                CacheSize = cacheSize;
            }
        }
        else
        {
            if (parallelProcessing)
            {
                cache = UnpackFileToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, DebugLogging);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackFileToMemory(SourceFilePath, out long cacheSize, extensionExceptions, DebugLogging);
                CacheSize = cacheSize;
            }
        }

        if (cache.Count <= 0)
        {
            CacheSize = 0;
            return false;
        }

        CurrentUnpackMode = UnpackMode.Memory;
        return true;
        
    }
    
    public bool CreateIndex()
    {
        if (IsLoaded) return false;
        if (!File.Exists(SourceFilePath))
        {
            ShapeLogger.LogError($"ContentPackMemory LoadContent() failed.  Source file not found: {SourceFilePath}");
            return false;
        }
        
        if (IsTextPack)
        {
            index = CreateTextIndex();
        }
        else
        {
            index = CreateFileIndex();
        }

        if (index.Count <= 0)
        {
            return false;
        }

        CurrentUnpackMode = UnpackMode.Indexed;
        return true;
        
    }
    
    public bool Clear()
    {
        if (!IsLoaded) return false;
        CacheSize = 0;
        cache.Clear();
        index.Clear();
        CurrentUnpackMode = UnpackMode.None;
        return true;
    }
    
    #endregion
    
    #region Unpack To Memory
    
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
    
    #region Index
    private Dictionary<string, long> CreateFileIndex()
    {
        var result = new Dictionary<string, long>();
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
    
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

            result[path] = offset;
        }
        return result;
    }
    private Dictionary<string, long> CreateTextIndex()
    {
        var result = new Dictionary<string, long>();

        if (!File.Exists(SourceFilePath))
        {
            Console.WriteLine($"Source file not found: {SourceFilePath}");
            return result;
        }

        if (Path.GetExtension(SourceFilePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {SourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        int indexedFiles = 0;

        
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs);
        
        while (!reader.EndOfStream)
        {
            // long offset = fs.Position;
            string? path = reader.ReadLine();
            long dataOffset = fs.Position;
            string? data = reader.ReadLine();
            if (path != null && data != null)
            {
                result[path] = dataOffset;
                if (DebugLogging)
                {
                    Console.WriteLine($"Indexed File {path} with offset {dataOffset})");
                }
            }
            indexedFiles++;
        }

        Console.WriteLine($"Indexing packed text file {SourceFilePath} finished. {indexedFiles} files indexed in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return result;
    }
    
    private byte[] GetDataFromFileIndex(long byteOffset)
    {
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
        fs.Position = byteOffset;
    
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
    private byte[] GetDataFromTextIndex(long byteOffset)
    {
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
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
    
    #endregion
}


/*
 
// Deprecated classes, kept for reference
public record PackedTextIndex
{
    private readonly Dictionary<string, long> index;
    private readonly string sourceFilePath;
    private readonly bool debug;
    public PackedTextIndex(string sourceFilePath, bool debug = false)
    {
        this.sourceFilePath = sourceFilePath;
        this.debug = debug;
        this.index = CreateIndex();
    }
    
    public byte[] GetData(string path)
    {
        return !index.TryGetValue(path, out long value) ? throw new KeyNotFoundException($"Path not found in index: {path}") : GetDataFromIndex(value);
    }
    
    private byte[] GetDataFromIndex(long byteOffset)
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
    private Dictionary<string, long> CreateIndex()
    {
        var result = new Dictionary<string, long>();

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
                result[path] = dataOffset;
                if (debug)
                {
                    Console.WriteLine($"Indexed File {path} with offset {dataOffset})");
                }
            }
            indexedFiles++;
        }

        Console.WriteLine($"Indexing packed text file {sourceFilePath} finished. {indexedFiles} files indexed in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return result;
    }
}
public record PackedBinaryIndex
{
    private readonly Dictionary<string, long> index;
    private readonly string sourceFilePath;
    private bool debug;
    public PackedBinaryIndex(string sourceFilePath, bool debug = false)
    {
        this.sourceFilePath = sourceFilePath;
        this.debug = debug;
        this.index = CreateIndex();
    }
    
    public byte[] GetData(string path)
    {
        return !index.TryGetValue(path, out long value) ? throw new KeyNotFoundException($"Path not found in index: {path}") : GetDataFromIndex(value);
    }
    private Dictionary<string, long> CreateIndex()
    {
        var result = new Dictionary<string, long>();
        using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
    
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

            result[path] = offset;
        }
        return result;
    }
    private byte[] GetDataFromIndex(long offset)
    {
        using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
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
}
public class TextUnpacker
{
    public static Dictionary<string, byte[]> UnpackToMemorySimple(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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
    public static Dictionary<string, byte[]> UnpackToMemory(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
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
    
}
public class BinaryUnpacker
{
    public static Dictionary<string, byte[]> UnpackToMemory(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
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
        long totalBytesUnpacked = 0;
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
    public static Dictionary<string, byte[]> UnpackToMemoryParallel(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
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
}
*/
