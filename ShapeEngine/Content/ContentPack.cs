using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using Raylib_cs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Content;
//TODO: Docs!
public sealed class ContentPack
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
            ShapeLogger.LogError($"Packed file not found: {sourceFilePath}");
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
            ShapeLogger.StartLogBlock("ContentPack UnpackFileToMemory Debug Info");
            foreach (var msg in debugMessages)
            {
                ShapeLogger.LogInfo(msg);
            }
            ShapeLogger.EndLogBlock();
        }
        
        ShapeLogger.LogInfo($"Unpacking to memory finished. {unpackedFiles} files loaded from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    public static Dictionary<string, byte[]> UnpackFileToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            ShapeLogger.LogError($"Packed file not found: {sourceFilePath}");
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
    
        long bytesProcessed = 0;
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
            bytesProcessed += data.Length;
            result[entry.path] = data;
            debugMessages?.Add($"Unpacked {entry.path} ({data.Length} bytes)");
        });
    
        totalBytesUnpacked = bytesProcessed;

        if (debugMessages is { Count: > 0 })
        {
            ShapeLogger.StartLogBlock("ContentPack UnpackFileToMemoryParallel Debug Info");
            foreach (string msg in debugMessages)
            {
                ShapeLogger.LogInfo(msg);
                
            }
            ShapeLogger.EndLogBlock();
        }
            
    
        ShapeLogger.LogInfo($"Unpacking to memory (parallel) finished. {result.Count} files loaded and {totalBytesUnpacked} bytes unpacked from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return new Dictionary<string, byte[]>(result);
    }
    
    public static Dictionary<string, byte[]> UnpackTextToMemory(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            ShapeLogger.LogError($"Source file not found: {sourceFilePath}");
            return result;
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            ShapeLogger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        var lines = File.ReadAllLines(sourceFilePath);
        
        int unpackedFiles = 0;

        if(debug) ShapeLogger.StartLogBlock("ContentPack UnpackTextToMemory Debug Info");
        
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;

            var relativePath = lines[i];

            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug)
                {
                    ShapeLogger.LogInfo($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                }
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
            if (debug)
            {
                ShapeLogger.LogInfo($"Unpacked {relativePath} ({compressedData.Length} bytes)");
            }
        }
        if (debug) ShapeLogger.EndLogBlock();

        ShapeLogger.LogInfo($"Unpacking packed text file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    public static Dictionary<string, byte[]> UnpackTextToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, bool debug = false, int batchSize = 16)
    {
        totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            ShapeLogger.LogError($"Source file not found: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            ShapeLogger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        var debugWatch = Stopwatch.StartNew();
        int totalFiles = int.Parse(File.ReadLines(sourceFilePath).Last());
        if (totalFiles <= 0)
        {
            ShapeLogger.LogWarning("No files to unpack.");
            return new Dictionary<string, byte[]>();
        }

        ShapeLogger.LogInfo($"ContentPack Batch Parallel unpacking to memory started. Batch size: {batchSize}");
        
        bool finished = false;
        int totalFilesRead = 0;
        var debugMessages = debug ? new ConcurrentBag<string>() : null;

        using var reader = new StreamReader(sourceFilePath);
        var batchLines = new List<string>(batchSize * 2);
        
        long bytesProcessed = 0;
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
                ShapeLogger.LogWarning("Odd number of lines in file, last line ignored.");
            }

           
            int filesInBatch = batchLines.Count / 2;
            Parallel.For(0, filesInBatch, i =>
            {
                var relativePath = batchLines[i * 2];
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    if (debug)
                    {
                        debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    }
                    return;
                }

                var base64Data = batchLines[i * 2 + 1];
                var compressedData = Convert.FromBase64String(base64Data);
                using var input = new MemoryStream(compressedData);
                using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);
                byte[] data = output.ToArray();
                bytesProcessed += data.Length;
                result[relativePath] = data;
                if (debug)
                {
                    debugMessages?.Add($"Unpacked {relativePath} ({compressedData.Length} bytes)");
                }
            });
        }
        
        totalBytesUnpacked += bytesProcessed;

       
        if (debugMessages is { Count: > 0 })
        {
            ShapeLogger.StartLogBlock("ContentPack UnpackTextToMemoryParallel Debug Info");
            foreach (string msg in debugMessages)
            {
                ShapeLogger.LogInfo(msg);
            }
            ShapeLogger.EndLogBlock();
        }
        
        ShapeLogger.LogInfo($"Unpacking packed text file {sourceFilePath} to memory finished. {result.Count} files with {totalBytesUnpacked} total bytes unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        
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
            ShapeLogger.LogError($"Source file not found: {SourceFilePath}");
            return result;
        }

        if (Path.GetExtension(SourceFilePath) != ".txt")
        {
            ShapeLogger.LogError($"Source file must have a .txt extension: {SourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        int indexedFiles = 0;

        
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs);
        if(DebugLogging) ShapeLogger.StartLogBlock("ContentPack CreateTextIndex Debug Info");
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
                    ShapeLogger.LogInfo($"Indexed File {path} with offset {dataOffset})");
                }
            }
            indexedFiles++;
        }
        if(DebugLogging) ShapeLogger.EndLogBlock();
        
        ShapeLogger.LogInfo($"Indexing packed text file {SourceFilePath} finished. {indexedFiles} files indexed in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
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
    
    #region Load
    public Texture2D LoadTexture(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadTexture() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadTexture() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var t = ContentLoader.LoadTextureFromMemory(extension, data);
        return t;
    }
    public Image LoadImage(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadImage() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadImage() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var i = ContentLoader.LoadImageFromMemory(extension, data);
        return i;
    }
    public Font LoadFont(string filePath, int fontSize = 100)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadFont() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadFont() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var f = ContentLoader.LoadFontFromMemory(extension, data, fontSize);
        return f;
    }
    public Wave LoadWave(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadWave() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadWave() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var w =  ContentLoader.LoadWaveFromMemory(extension, data);
        return w;
    }
    public Sound LoadSound(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadSound() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadSound() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var s = ContentLoader.LoadSoundFromMemory(extension, data);
        return s;

    }
    public Music LoadMusic(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadMusic() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadMusic() failed. File {filePath} not found in pack!");
            return new();
        }
        
        string extension = Path.GetExtension(filePath);
        var m = ContentLoader.LoadMusicFromMemory(extension, data);
        return m;
    }
    public Shader LoadFragmentShader(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadFragmentShader() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadFragmentShader() failed. File {filePath} not found in pack!");
            return new();
        }
        
        var fs = ContentLoader.LoadFragmentShaderFromMemory(data);
        return fs;
    }
    public Shader LoadVertexShader(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadVertexShader() failed. Content pack not loaded!");
            return new();
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadVertexShader() failed. File {filePath} not found in pack!");
            return new();
        }
        
        var vs = ContentLoader.LoadVertexShaderFromMemory(data);
        return vs;
    }
    public string LoadText(string filePath)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadText() failed. Content pack not loaded!");
            return string.Empty;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadText() failed. File {filePath} not found in pack!");
            return string.Empty;
        }
        
        // string extension = Path.GetExtension(filePath);
        return ContentLoader.LoadTextFromMemory(data);
    }

    #endregion
    
    #region TryLoad
    public bool TryLoadTexture(string filePath, out Texture2D texture)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadTexture() failed. Content pack not loaded!");
            texture = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadTexture() failed. File {filePath} not found in pack!");
            texture = new();
            return false;
        }

        string extension = Path.GetExtension(filePath);
        texture = ContentLoader.LoadTextureFromMemory(extension, data);
        return true;
    }
    public bool TryLoadImage(string filePath, out Image image)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadImage() failed. Content pack not loaded!");
            image = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadImage() failed. File {filePath} not found in pack!");
            image = new();
            return false;
        }

        string extension = Path.GetExtension(filePath);
        image = ContentLoader.LoadImageFromMemory(extension, data);
        return true;
    }
    public bool TryLoadFont(string filePath, out Font font, int fontSize = 100)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadFont() failed. Content pack not loaded!");
            font = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadFont() failed. File {filePath} not found in pack!");
            font = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        font = ContentLoader.LoadFontFromMemory(extension, data, fontSize);
        return true;
    }
    public bool TryLoadWave(string filePath, out Wave wave)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadWave() failed. Content pack not loaded!");
            wave = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadWave() failed. File {filePath} not found in pack!");
            wave = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        wave =  ContentLoader.LoadWaveFromMemory(extension, data);
        return true;
    }
    public bool TryLoadSound(string filePath, out Sound sound)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadSound() failed. Content pack not loaded!");
            sound = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadSound() failed. File {filePath} not found in pack!");
            sound = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        sound = ContentLoader.LoadSoundFromMemory(extension, data);
        return true;

    }
    public bool TryLoadMusic(string filePath, out Music music)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadMusic() failed. Content pack not loaded!");
            music = new();   
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadMusic() failed. File {filePath} not found in pack!");
            music = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        music = ContentLoader.LoadMusicFromMemory(extension, data);
        return true;
    }
    public bool TryLoadFragmentShader(string filePath, out Shader shader)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadFragmentShader() failed. Content pack not loaded!");
            shader = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadFragmentShader() failed. File {filePath} not found in pack!");
            shader = new();
            return false;
        }
        
        shader = ContentLoader.LoadFragmentShaderFromMemory(data);
        return true;
    }
    public bool TryLoadVertexShader(string filePath, out Shader shader)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadVertexShader() failed. Content pack not loaded!");
            shader = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadVertexShader() failed. File {filePath} not found in pack!");
            shader = new();
            return false;
        }
        
        shader = ContentLoader.LoadVertexShaderFromMemory(data);
        return true;
    }
    public bool TryLoadText(string filePath, out string text)
    {
        if (!IsLoaded)
        {
            ShapeLogger.LogError("ContentPack LoadText() failed. Content pack not loaded!");
            text = string.Empty;
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            ShapeLogger.LogError($"ContentPack LoadText() failed. File {filePath} not found in pack!");
            text = string.Empty;
            return false;
        }
        
        // string extension = Path.GetExtension(filePath);
        text = ContentLoader.LoadTextFromMemory(data);
        return true;
    }

    #endregion
    
    #region Load All
    public Dictionary<string, Texture2D> LoadAllTextures(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Texture2D>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllTextures() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Texture)) continue;
            if (TryLoadTexture(file, out var texture))
            {
                result[file] = texture;
            }
        }

        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllTextures() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    
    public Dictionary<string, Image> LoadAllImages(out bool success)
        {
            success = false;
            var result = new Dictionary<string, Image>();
            if (!IsLoaded)
            {
                ShapeLogger.LogError($"ContentPack LoadAllImages() failed. Content pack not loaded!");
                return result;
            }
    
            var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
            foreach (string file in list)
            {
                string ext = Path.GetExtension(file);
                if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Texture)) continue;
                if (TryLoadImage(file, out var image))
                {
                    result[file] = image;
                }
            }
            
            if (result.Count <= 0)
            {
                ShapeLogger.LogWarning($"ContentPack LoadAllImages() found no valid content in pack!");
                return result;
            }
            
            success = true;
            return result;
        }
    
    public Dictionary<string, Font> LoadAllFonts(out bool success, int fontSize = 100)
    {
        success = false;
        var result = new Dictionary<string, Font>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllFonts() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Font)) continue;
            if (TryLoadFont(file, out var font, fontSize))
            {
                result[file] = font;
            }
        }
        
        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllFonts() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, Wave> LoadAllWaves(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Wave>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllWaves() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Wave)) continue;
            if (TryLoadWave(file, out var wave))
            {
                result[file] = wave;
            }
        }
        
        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllWaves() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, Sound> LoadAllSounds(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Sound>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllSounds() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Sound)) continue;
            if (TryLoadSound(file, out var sound))
            {
                result[file] = sound;
            }
        }
        
        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllSounds() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, Music> LoadAllMusic(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Music>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllMusic() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Music)) continue;
            if (TryLoadMusic(file, out var music))
            {
                result[file] = music;
            }
        }
        
        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllMusic() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, Shader> LoadAllFragmentShaders(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Shader>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllFragmentShaders() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.ShaderFragment)) continue;
            if (TryLoadFragmentShader(file, out var shader))
            {
                result[file] = shader;
            }
        }

        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllFragmentShader() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, Shader> LoadAllVertexShaders(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Shader>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"ContentPack LoadAllVertexShaders() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (string file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.ShaderVertex)) continue;
            if (TryLoadVertexShader(file, out var shader))
            {
                result[file] = shader;
            }
        }
        
        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllVertexShaders() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }

    public Dictionary<string, string> LoadAllText(out bool success)
    {
        success = false;
        var result = new Dictionary<string, string>();
        if (!IsLoaded)
        {
            ShapeLogger.LogError($"Content Pack LoadAllText() failed. Content pack not loaded!");
            return result;
        }

        var list = CurrentUnpackMode == UnpackMode.Memory ? cache.Keys.ToList() : index.Keys.ToList();
        foreach (var file in list)
        {
            string ext = Path.GetExtension(file);
            if (!ContentLoader.IsValidExtension(ext, ContentLoader.ContentType.Text)) continue;
            if (TryLoadText(file, out string text))
            {
                result[file] = text;
            }
        }

        if (result.Count <= 0)
        {
            ShapeLogger.LogWarning($"ContentPack LoadAllText() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    #endregion
}
