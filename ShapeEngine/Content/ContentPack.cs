using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using Raylib_cs;
using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Content;

/// <summary>
/// Represents a content pack that can load, cache, index, and provide access to various types of content files.
/// Supports both binary and text-based packs, with options for memory or indexed unpacking.
/// </summary>
/// <remarks>
/// This class only supports content packs created with the ShapeEngine ResourcePacker tool, available in the ShapeEngine GitHub repository.
/// </remarks>
public sealed class ContentPack
{
    /// <summary>
    /// Event triggered when the asynchronous cache loading operation completes.
    /// Subscribers can use this to perform actions after the content pack has finished loading its cache.
    /// </summary>
    public event Action<ContentPack>? OnAsyncCacheLoaded;
    
    /// <summary>
    /// Specifies the mode used to unpack content files in the ContentPack.
    /// </summary>
    public enum UnpackMode
    {
        /// <summary>
        /// No unpacking performed.
        /// </summary>
        None,
        /// <summary>
        /// All files unpacked into memory.
        /// </summary>
        Memory,
        /// <summary>
        /// Files are indexed for on-demand access.
        /// </summary>
        Indexed
    }
    
    #region Members
    /// <summary>
    /// Indicates whether the content pack is loaded (i.e., unpacked or indexed).
    /// </summary>
    public bool IsLoaded => CurrentUnpackMode != UnpackMode.None;
    
    /// <summary>
    /// Gets the current <see cref="UnpackMode"/> of the content pack.
    /// </summary>
    /// <remarks>
    /// When <see cref="CurrentUnpackMode"/> is <see cref="UnpackMode.None"/>, <see cref="IsLoaded"/> will be false.
    /// </remarks>
    public UnpackMode CurrentUnpackMode { get; private set; } = UnpackMode.None;
    
    /// <summary>
    /// The source file path for the content pack.
    /// </summary>
    public readonly string SourceFilePath;
    
    /// <summary>
    /// Indicates if the content pack is a text pack (.txt extension).
    /// </summary>
    public readonly bool IsTextPack;
    
    /// <summary>
    /// The total size of the cached content in bytes.
    /// </summary>
    /// <remarks>
    /// Only applicable when using <see cref="UnpackMode.Memory"/>.
    /// Will be 0 if the content pack is not loaded or if using <see cref="UnpackMode.Indexed"/>.
    /// </remarks>
    public long CacheSize { get; private set; }
    
    /// <summary>
    /// Enables or disables debug logging for content pack operations.
    /// </summary>
    public bool DebugLogging = false;
    
    /// <summary>
    /// Stores cached file data when unpacked into memory.
    /// </summary>
    private Dictionary<string, byte[]> cache = new();
    
    /// <summary>
    /// Stores file offsets for indexed access.
    /// </summary>
    private Dictionary<string, long> index = new();
    
    #endregion

    #region Public
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPack"/> class with the specified source file path.
    /// The source file path must have a valid extension (.txt for text packs, others for binary packs).
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed source file.</param>
    /// <remarks>
    /// Will throw and <see cref="ArgumentException"/> if the source file path does not have a valid extension.
    /// </remarks>
    public ContentPack(string sourceFilePath)
    {
        if(!Path.HasExtension(sourceFilePath))
        {
            throw new ArgumentException("Source file path must have a valid extension. (.txt for text packs, others for binary packs)");
        }
        
        SourceFilePath = sourceFilePath;
        IsTextPack = Path.GetExtension(sourceFilePath).Equals(".txt", StringComparison.CurrentCultureIgnoreCase);
    }
    
    /// <summary>
    /// Retrieves the file data for the specified file path from the content pack.
    /// Returns null if the content pack is not loaded or the file does not exist.
    /// </summary>
    /// <param name="filePath">The relative path of the file within the content pack.</param>
    /// <returns>The file data as a byte array, or null if not found.</returns>
    /// <remarks>
    /// For example, if the following directory structure was packed:
    /// <list type="bullet">
    ///   <item>Resources/</item>
    ///   <item>Resources/Textures/...</item>
    ///   <item>Resources/Sounds/...</item>
    ///   <item>Resources/Music/</item>
    ///   <item>Resources/Music/myBackgroundMusic.mp3</item>
    ///   <item>Resources/...</item>
    /// </list>
    /// The relative <paramref name="filePath"/> stored in the pack to access myBackgroundMusic.mp3 would be <c>Music/myBackgroundMusic.mp3</c>.
    /// </remarks>
    public byte[]? GetFileData(string filePath)
    {
        if(!IsLoaded) return null;
        if(!HasFile(filePath)) return null;
        
        if(CurrentUnpackMode == UnpackMode.Memory) return cache[filePath];
        
        if(IsTextPack) return index.TryGetValue(filePath, out long offsetText) ? GetDataFromTextIndex(offsetText) : null;
        return index.TryGetValue(filePath, out long offsetFile) ? GetDataFromFileIndex(offsetFile) : null;
    }
    /// <summary>
    /// Checks if the specified file exists in the content pack.
    /// Returns true if the content pack is loaded and the file is present in the cache or index.
    /// </summary>
    public bool HasFile(string filePath)
    {
        if(!IsLoaded) return false;
        return CurrentUnpackMode == UnpackMode.Memory ? cache.ContainsKey(filePath) : index.ContainsKey(filePath);
    }
    
    /// <summary>
    /// Unpacks the content pack into memory, optionally using parallel processing.
    /// Skips files with extensions listed in <paramref name="extensionExceptions"/>.
    /// For text packs, allows batch size control via <paramref name="textFileUnpackingBatchSize"/>.
    /// Returns true if cache creation succeeds and files are loaded.
    /// </summary>
    /// <param name="parallelProcessing">If true, uses parallel processing for unpacking.</param>
    /// <param name="extensionExceptions">List of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="textFileUnpackingBatchSize">Batch size for parallel text file unpacking.</param>
    /// <returns>True if cache was created and files loaded; otherwise, false.</returns>
    /// <remarks>
    /// Returns false if the content pack is already loaded or if the source file does not exist.
    /// Use <see cref="IsLoaded"/> to check load status.
    /// Use <see cref="Clear"/> to unload and clear the cache/index.
    /// </remarks>
    public bool CreateCache(bool parallelProcessing = false, List<string>? extensionExceptions = null, string directoryRestriction = "", int textFileUnpackingBatchSize = 16)
    {
        if (IsLoaded) return false;
        if (!File.Exists(SourceFilePath))
        {
            Game.Instance.Logger.LogError($"ContentPackMemory LoadContent() failed.  Source file not found: {SourceFilePath}");
            return false;
        }

        if (IsTextPack)
        {
            if (parallelProcessing)
            {
                cache = UnpackTextToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, directoryRestriction, textFileUnpackingBatchSize, DebugLogging);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackTextToMemory(SourceFilePath, out long cacheSize, extensionExceptions, directoryRestriction, DebugLogging);
                CacheSize = cacheSize;
            }
        }
        else
        {
            if (parallelProcessing)
            {
                cache = UnpackFileToMemoryParallel(SourceFilePath, out long cacheSize, extensionExceptions, directoryRestriction, DebugLogging);
                CacheSize = cacheSize;
            }
            else
            {
                cache = UnpackFileToMemory(SourceFilePath, out long cacheSize, extensionExceptions, directoryRestriction, DebugLogging);
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
    
    /// <summary>
    /// Asynchronously unpacks the content pack into memory, optionally using parallel processing.
    /// Use <see cref="OnAsyncCacheLoaded"/> event to get notified when loading is complete.
    /// Alternatively, await this method (see Example).
    /// </summary>
    /// <param name="parallelProcessing">If true, uses parallel processing for unpacking.</param>
    /// <param name="extensionExceptions">List of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="textFileUnpackingBatchSize">Batch size for parallel text file unpacking.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>True if cache was created and files loaded; otherwise, false.</returns>
    /// <remarks>
    /// Returns false if the content pack is already loaded or if the source file does not exist.
    /// Use <see cref="IsLoaded"/> to check load status.
    /// Use <see cref="Clear"/> to unload and clear the cache/index.
    /// </remarks>
    /// <example>
    /// How to use CreateCacheAsync():
    /// <code>
    ///public async Task LoadAndUseContentPackAsync()
    ///{
    ///    var pack = new ContentPack("Resources/Pack.bin");
    ///    bool loaded = await pack.CreateCacheAsync();
    ///
    ///    if (loaded)
    ///    {
    ///        // Notify: ContentPack is ready
    ///        // Now use pack normally
    ///        var data = pack.GetFileData("Textures/Logo.png");
    ///        // ... use data
    ///    }
    ///    else
    ///    {
    ///        // Notify: loading failed
    ///    }
    ///}
    /// </code>
    /// </example>
    public async Task<bool> CreateCacheAsync(bool parallelProcessing = false, List<string>? extensionExceptions = null, 
        string directoryRestriction = "", int textFileUnpackingBatchSize = 16, CancellationToken cancellationToken = default)
    {
        if (IsLoaded) return false;
        if (!File.Exists(SourceFilePath))
        {
            Game.Instance.Logger.LogError($"ContentPackMemory LoadContentAsync() failed. Source file not found: {SourceFilePath}");
            return false;
        }
    
        long cacheSize = 0;
        if (IsTextPack)
        {
            if (parallelProcessing)
            {
                cache = await UnpackTextToMemoryParallelAsync(
                    SourceFilePath,
                    size => cacheSize = size,
                    extensionExceptions,
                    directoryRestriction,
                    textFileUnpackingBatchSize,
                    DebugLogging);
            }
            else
            {
                cache = await UnpackTextToMemoryAsync(
                    SourceFilePath,
                    size => cacheSize = size,
                    extensionExceptions,
                    directoryRestriction,
                    DebugLogging);
            }
        }
        else
        {
            if (parallelProcessing)
            {
                cache = await UnpackFileToMemoryParallelAsync(
                    SourceFilePath,
                    size => cacheSize = size,
                    extensionExceptions,
                    directoryRestriction,
                    DebugLogging);
            }
            else
            {
                cache = await UnpackFileToMemoryAsync(
                    SourceFilePath,
                    size => cacheSize = size,
                    extensionExceptions,
                    directoryRestriction,
                    DebugLogging);
            }
        }
    
        CacheSize = cacheSize;
        if (cache.Count <= 0)
        {
            CacheSize = 0;
            return false;
        }
    
        CurrentUnpackMode = UnpackMode.Memory;
        
        OnAsyncCacheLoaded?.Invoke(this); // Fire event
        
        return true;
    }
    
    /// <summary>
    /// Creates an index for the content pack, allowing on-demand access to files without loading them into memory.
    /// Returns true if the index was successfully created; otherwise, false.
    /// Returns false if the content pack is already loaded or the source file does not exist.
    /// Use <see cref="IsLoaded"/> to check load status.
    /// Use <see cref="Clear"/> to unload and clear the cache/index.
    /// </summary>
    public bool CreateIndex()
    {
        if (IsLoaded) return false;
        if (!File.Exists(SourceFilePath))
        {
            Game.Instance.Logger.LogError($"ContentPackMemory LoadContent() failed.  Source file not found: {SourceFilePath}");
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
    /// <summary>
    /// Clears the content pack by unloading all cached and indexed files.
    /// Resets the cache size and unpack mode to None.
    /// Returns true if the content pack was loaded and is now cleared; otherwise, false.
    /// </summary>
    /// <remarks>
    /// Call this method to unload the content pack and free memory when it is no longer necessary.
    /// Indexed mode uses significantly less memory than memory mode, but file access is slower.
    /// </remarks>
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
    
    /// <summary>
    /// Unpacks a binary content pack file into memory.
    /// Reads all files from the specified <paramref name="sourceFilePath"/>, decompresses them, and stores them in a dictionary.
    /// Skips files with extensions listed in <paramref name="extensionExceptions"/>.
    /// If <paramref name="debug"/> is true, logs detailed unpacking information.
    /// Returns a dictionary mapping relative file paths to their decompressed byte data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed binary file.</param>
    /// <param name="totalBytesUnpacked">Outputs the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackFileToMemory(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Packed file not found: {sourceFilePath}");
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
            
            if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
            {
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
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackFileToMemory Debug Info");
            foreach (var msg in debugMessages)
            {
                Game.Instance.Logger.LogInfo(msg);
            }
            Game.Instance.Logger.EndLogBlock();
        }
        
        Game.Instance.Logger.LogInfo($"Unpacking to memory finished. {unpackedFiles} files loaded from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    /// <summary>
    /// Unpacks a binary content pack file into memory using parallel processing.
    /// Reads all files from the specified <paramref name="sourceFilePath"/>, decompresses them, and stores them in a dictionary.
    /// Skips files with extensions listed in <paramref name="extensionExceptions"/>.
    /// If <paramref name="debug"/> is true, logs detailed unpacking information.
    /// Returns a dictionary mapping relative file paths to their decompressed byte data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed binary file.</param>
    /// <param name="totalBytesUnpacked">Outputs the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackFileToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Packed file not found: {sourceFilePath}");
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
    
                if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
                {
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
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackFileToMemoryParallel Debug Info");
            foreach (string msg in debugMessages)
            {
                Game.Instance.Logger.LogInfo(msg);
                
            }
            Game.Instance.Logger.EndLogBlock();
        }
            
    
        Game.Instance.Logger.LogInfo($"Unpacking to memory (parallel) finished. {result.Count} files loaded and {totalBytesUnpacked} bytes unpacked from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return new Dictionary<string, byte[]>(result);
    }
    
    /// <summary>
    /// Unpacks a packed text file into memory.
    /// Each file is stored as two lines: the relative path and the base64-encoded compressed data.
    /// Skips files with extensions listed in <paramref name="extensionExceptions"/>.
    /// If <paramref name="debug"/> is true, logs detailed unpacking information.
    /// Returns a dictionary mapping relative file paths to their decompressed byte data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed text file (.txt).</param>
    /// <param name="totalBytesUnpacked">Outputs the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackTextToMemory(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Source file not found: {sourceFilePath}");
            return result;
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Game.Instance.Logger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        var lines = File.ReadAllLines(sourceFilePath);
        
        int unpackedFiles = 0;

        if(debug) Game.Instance.Logger.StartLogBlock("ContentPack UnpackTextToMemory Debug Info");
        
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;

            var relativePath = lines[i];

            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug)
                {
                    Game.Instance.Logger.LogInfo($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                }
                continue;
            }

            if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
            {
                if (debug)
                {
                    Game.Instance.Logger.LogInfo($"File skipped due to directory restriction {directoryRestriction}: {relativePath}");
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
                Game.Instance.Logger.LogInfo($"Unpacked {relativePath} ({compressedData.Length} bytes)");
            }
        }
        if (debug) Game.Instance.Logger.EndLogBlock();

        Game.Instance.Logger.LogInfo($"Unpacking packed text file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    
    /// <summary>
    /// Unpacks a packed text file into memory using parallel processing and batching.
    /// Each file is stored as two lines: the relative path and the base64-encoded compressed data.
    /// Skips files with extensions listed in <paramref name="extensionExceptions"/>.
    /// If <paramref name="debug"/> is true, logs detailed unpacking information.
    /// Returns a dictionary mapping relative file paths to their decompressed byte data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed text file (.txt).</param>
    /// <param name="totalBytesUnpacked">Outputs the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="batchSize">Batch size for parallel text file unpacking.</param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackTextToMemoryParallel(string sourceFilePath, out long totalBytesUnpacked, List<string>? extensionExceptions = null, string directoryRestriction = "", int batchSize = 16, bool debug = false)
    {
        totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Source file not found: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Game.Instance.Logger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            return new Dictionary<string, byte[]>();
        }

        var debugWatch = Stopwatch.StartNew();
        int totalFiles = int.Parse(File.ReadLines(sourceFilePath).Last());
        if (totalFiles <= 0)
        {
            Game.Instance.Logger.LogWarning("No files to unpack.");
            return new Dictionary<string, byte[]>();
        }

        Game.Instance.Logger.LogInfo($"ContentPack Batch Parallel unpacking to memory started. Batch size: {batchSize}");
        
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
                        
                        if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(firstLine)))
                        {
                            if (debug)
                            {
                                debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(firstLine)}");
                            }
                            continue;
                        }
                        
                        if (!string.IsNullOrEmpty(directoryRestriction) && !firstLine.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
                        {
                            if (debug)
                            {
                                Game.Instance.Logger.LogInfo($"File skipped due to directory restriction {directoryRestriction}: {firstLine}");
                            }
                            continue;
                        }
                        
                        batchLines.Add(firstLine);
                        firstLine = string.Empty;
                        batchLines.Add(line);
                        totalFilesRead++;
                    }
                }
            }

            if (firstLine != string.Empty)
            {
                Game.Instance.Logger.LogWarning("Odd number of lines in file, last line ignored.");
            }

           
            int filesInBatch = batchLines.Count / 2;
            Parallel.For(0, filesInBatch, i =>
            {
                var relativePath = batchLines[i * 2];
                
                //Is now done above when creating the batch lines
                // if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                // {
                //     if (debug)
                //     {
                //         debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                //     }
                //     return;
                // }

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
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackTextToMemoryParallel Debug Info");
            foreach (string msg in debugMessages)
            {
                Game.Instance.Logger.LogInfo(msg);
            }
            Game.Instance.Logger.EndLogBlock();
        }
        
        Game.Instance.Logger.LogInfo($"Unpacking packed text file {sourceFilePath} to memory finished. {result.Count} files with {totalBytesUnpacked} total bytes unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        
        return new Dictionary<string, byte[]>(result);
    }
    #endregion
    
    #region Unpack To Memory Async
    
    /// <summary>
    /// Asynchronously unpacks a binary content pack file into memory.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed binary file.</param>
    /// <param name="setTotalBytesUnpacked">Optional callback to set the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    /// <example>
    /// <code>
    ///public async Task LoadContentPackAsync()
    ///{
    ///    string packPath = "Resources/Pack.bin";
    ///    Dictionary string, byte[] files = await ContentPack.UnpackFileToMemoryAsync(packPath);
    ///
    ///    foreach (var kvp in files)
    ///    {
    ///        string filePath = kvp.Key;
    ///        byte[] data = kvp.Value;
    ///        // Use filePath and data as needed
    ///        Console.WriteLine($"Loaded {filePath} ({data.Length} bytes)");
    ///    }
    ///}
    /// </code>
    /// </example>
    public static async Task<Dictionary<string, byte[]>> UnpackFileToMemoryAsync(string sourceFilePath, Action<long>? setTotalBytesUnpacked = null, 
        List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        long totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Packed file not found: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return result;
        }
    
        var debugWatch = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>() : null;
        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var header = new byte[12];
        await fileStream.ReadExactlyAsync(header, 0, 12);
        int totalFiles = BitConverter.ToInt32(header, 0);
    
        int unpackedFiles = 0;
        int currentFile = 0;
    
        while (currentFile < totalFiles)
        {
            currentFile++;
    
            var pathLenBytes = new byte[4];
            await fileStream.ReadExactlyAsync(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
            var pathBytes = new byte[pathLen];
            await fileStream.ReadExactlyAsync(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);
    
            var dataLenBytes = new byte[4];
            await fileStream.ReadExactlyAsync(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);
    
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                fileStream.Position += dataLen;
                continue;
            }
    
            if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
            {
                fileStream.Position += dataLen;
                continue;
            }
    
            var compressedData = new byte[dataLen];
            await fileStream.ReadExactlyAsync(compressedData, 0, dataLen);
    
            var data = await Task.Run(() =>
            {
                using var ms = new MemoryStream(compressedData);
                using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                gzip.CopyTo(outMs);
                return outMs.ToArray();
            });
    
            result[relativePath] = data;
            unpackedFiles++;
            totalBytesUnpacked += data.Length;
    
            debugMessages?.Add($"Unpacked {relativePath} ({data.Length} bytes)");
        }
    
        if (debugMessages is { Count: > 0 })
        {
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackFileToMemoryAsync Debug Info");
            foreach (var msg in debugMessages)
            {
                Game.Instance.Logger.LogInfo(msg);
            }
            Game.Instance.Logger.EndLogBlock();
        }
    
        Game.Instance.Logger.LogInfo($"Unpacking to memory finished. {unpackedFiles} files loaded from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        setTotalBytesUnpacked?.Invoke(totalBytesUnpacked);
        return result;
    }
   
    /// <summary>
    /// Asynchronously unpacks a binary content pack file into memory using parallel processing.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed binary file.</param>
    /// <param name="setTotalBytesUnpacked">Optional callback to set the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    /// <example>
    /// <code>
    ///public async Task LoadContentPackAsync()
    ///{
    ///    string packPath = "Resources/Pack.bin";
    ///    Dictionary string, byte[] files = await ContentPack.UnpackFileToMemoryParallelAsync(packPath);
    ///
    ///    foreach (var kvp in files)
    ///    {
    ///        string filePath = kvp.Key;
    ///        byte[] data = kvp.Value;
    ///        // Use filePath and data as needed
    ///        Console.WriteLine($"Loaded {filePath} ({data.Length} bytes)");
    ///    }
    ///}
    /// </code>
    /// </example>
    public static async Task<Dictionary<string, byte[]>> UnpackFileToMemoryParallelAsync(string sourceFilePath, Action<long>? setTotalBytesUnpacked = null, 
        List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        long totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Packed file not found: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return new Dictionary<string, byte[]>();
        }
    
        var debugWatch = Stopwatch.StartNew();
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
        var entries = new List<(string path, long dataOffset, int dataLen)>();
    
        using (var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            var header = new byte[12];
            await fileStream.ReadExactlyAsync(header, 0, 12);
            int totalFiles = BitConverter.ToInt32(header, 0);
    
            for (int i = 0; i < totalFiles; i++)
            {
                var pathLenBytes = new byte[4];
                await fileStream.ReadExactlyAsync(pathLenBytes, 0, 4);
                int pathLen = BitConverter.ToInt32(pathLenBytes, 0);
    
                var pathBytes = new byte[pathLen];
                await fileStream.ReadExactlyAsync(pathBytes, 0, pathLen);
                string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);
    
                var dataLenBytes = new byte[4];
                await fileStream.ReadExactlyAsync(dataLenBytes, 0, 4);
                int dataLen = BitConverter.ToInt32(dataLenBytes, 0);
    
                long dataOffset = fileStream.Position;
    
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                {
                    debugMessages?.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                    fileStream.Position += dataLen;
                    continue;
                }
    
                if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
                {
                    fileStream.Position += dataLen;
                    continue;
                }
    
                entries.Add((relativePath, dataOffset, dataLen));
                fileStream.Position += dataLen;
            }
        }
    
        var tasks = entries.Select(async entry =>
        {
            using var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            fs.Position = entry.dataOffset;
            var compressedData = new byte[entry.dataLen];
            await fs.ReadExactlyAsync(compressedData, 0, entry.dataLen);
    
            var data = await Task.Run(() =>
            {
                using var ms = new MemoryStream(compressedData);
                using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                gzip.CopyTo(outMs);
                return outMs.ToArray();
            });
    
            result[entry.path] = data;
            Interlocked.Add(ref totalBytesUnpacked, data.Length);
            debugMessages?.Add($"Unpacked {entry.path} ({data.Length} bytes)");
        });
    
        await Task.WhenAll(tasks);
    
        if (debugMessages is { Count: > 0 })
        {
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackFileToMemoryParallelAsync Debug Info");
            foreach (string msg in debugMessages)
                Game.Instance.Logger.LogInfo(msg);
            Game.Instance.Logger.EndLogBlock();
        }
    
        Game.Instance.Logger.LogInfo($"Unpacking to memory (parallel) finished. {result.Count} files loaded and {totalBytesUnpacked} bytes unpacked from {sourceFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        setTotalBytesUnpacked?.Invoke(totalBytesUnpacked);
        return new Dictionary<string, byte[]>(result);
    }
    /// <summary>
    /// Asynchronously unpacks a packed text file (.txt) into memory.
    /// Each file is stored as two lines: the relative path and the base64-encoded compressed data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed text file (.txt).</param>
    /// <param name="setTotalBytesUnpacked">Optional callback to set the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    /// <example>
    /// <code>
    ///public async Task LoadContentPackAsync()
    ///{
    ///    string packPath = "Resources/Pack.bin";
    ///    Dictionary string, byte[] files = await ContentPack.UnpackTextToMemoryAsync(packPath);
    ///
    ///    foreach (var kvp in files)
    ///    {
    ///        string filePath = kvp.Key;
    ///        byte[] data = kvp.Value;
    ///        // Use filePath and data as needed
    ///        Console.WriteLine($"Loaded {filePath} ({data.Length} bytes)");
    ///    }
    ///}
    /// </code>
    /// </example>
    public static async Task<Dictionary<string, byte[]>> UnpackTextToMemoryAsync(string sourceFilePath, Action<long>? setTotalBytesUnpacked = null, 
        List<string>? extensionExceptions = null, string directoryRestriction = "", bool debug = false)
    {
        long totalBytesUnpacked = 0;
        var result = new Dictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Source file not found: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return result;
        }
    
        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Game.Instance.Logger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return result;
        }
    
        var debugWatch = Stopwatch.StartNew();
        string[] lines = await File.ReadAllLinesAsync(sourceFilePath);
    
        var unpackedFiles = 0;
        if (debug) Game.Instance.Logger.StartLogBlock("ContentPack UnpackTextToMemoryAsync Debug Info");
    
        for (var i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
    
            string relativePath = lines[i];
    
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug)
                    Game.Instance.Logger.LogInfo($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }
    
            if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
            {
                if (debug)
                    Game.Instance.Logger.LogInfo($"File skipped due to directory restriction {directoryRestriction}: {relativePath}");
                continue;
            }
    
            string base64Data = lines[i + 1];
            byte[] compressedData = Convert.FromBase64String(base64Data);
    
            byte[] data = await Task.Run(() =>
            {
                using var input = new MemoryStream(compressedData);
                using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);
                return output.ToArray();
            });
    
            totalBytesUnpacked += data.Length;
            result[relativePath] = data;
            unpackedFiles++;
            if (debug)
                Game.Instance.Logger.LogInfo($"Unpacked {relativePath} ({compressedData.Length} bytes)");
        }
        if (debug) Game.Instance.Logger.EndLogBlock();
    
        Game.Instance.Logger.LogInfo($"Unpacking packed text file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        setTotalBytesUnpacked?.Invoke(totalBytesUnpacked);
        return result;
    }
    
    /// <summary>
    /// Asynchronously unpacks a packed text file (.txt) into memory using parallel processing and batching.
    /// Each file is stored as two lines: the relative path and the base64-encoded compressed data.
    /// </summary>
    /// <param name="sourceFilePath">The path to the packed text file (.txt).</param>
    /// <param name="setTotalBytesUnpacked">Optional callback to set the total number of bytes unpacked.</param>
    /// <param name="extensionExceptions">Optional list of file extensions to skip during unpacking.</param>
    /// <param name="directoryRestriction">
    /// If set, only files whose relative path starts with this directory restriction will be unpacked.
    /// Example: setting to `Audio/Music/Background` will only load resources within that directory path.
    /// </param>
    /// <param name="batchSize">Batch size for parallel text file unpacking.</param>
    /// <param name="debug">If true, enables debug logging.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a dictionary of relative file paths and their corresponding decompressed byte arrays.</returns>
    /// <example>
    /// <code>
    ///public async Task LoadContentPackAsync()
    ///{
    ///    string packPath = "Resources/Pack.bin";
    ///    Dictionary string, byte[] files = await ContentPack.UnpackTextToMemoryParallelAsync(packPath);
    ///
    ///    foreach (var kvp in files)
    ///    {
    ///        string filePath = kvp.Key;
    ///        byte[] data = kvp.Value;
    ///        // Use filePath and data as needed
    ///        Console.WriteLine($"Loaded {filePath} ({data.Length} bytes)");
    ///    }
    ///}
    /// </code>
    /// </example>
    public static async Task<Dictionary<string, byte[]>> UnpackTextToMemoryParallelAsync(string sourceFilePath, Action<long>? setTotalBytesUnpacked = null, 
        List<string>? extensionExceptions = null, string directoryRestriction = "", int batchSize = 16, bool debug = false)
    {
        long totalBytesUnpacked = 0;
        var result = new ConcurrentDictionary<string, byte[]>();
    
        if (!File.Exists(sourceFilePath))
        {
            Game.Instance.Logger.LogError($"Source file not found: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return new Dictionary<string, byte[]>();
        }
    
        if (Path.GetExtension(sourceFilePath) != ".txt")
        {
            Game.Instance.Logger.LogError($"Source file must have a .txt extension: {sourceFilePath}");
            setTotalBytesUnpacked?.Invoke(0);
            return new Dictionary<string, byte[]>();
        }
    
        var debugWatch = Stopwatch.StartNew();
        string[] lines = await File.ReadAllLinesAsync(sourceFilePath);
        int totalFiles = (lines.Length - 1) / 2;
        if (totalFiles <= 0)
        {
            Game.Instance.Logger.LogWarning("No files to unpack.");
            setTotalBytesUnpacked?.Invoke(0);
            return new Dictionary<string, byte[]>();
        }
    
        Game.Instance.Logger.LogInfo($"ContentPack Batch Parallel unpacking to memory started. Batch size: {batchSize}");
    
        var debugMessages = debug ? new ConcurrentBag<string>() : null;
        var entries = new List<(string path, string base64Data)>();
    
        for (int i = 0; i < lines.Length - 1; i += 2)
        {
            string relativePath = lines[i];
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
                continue;
            if (!string.IsNullOrEmpty(directoryRestriction) && !relativePath.StartsWith(directoryRestriction, StringComparison.OrdinalIgnoreCase))
                continue;
            entries.Add((relativePath, lines[i + 1]));
        }
    
        var tasks = entries.Select(async entry =>
        {
            byte[] compressedData = Convert.FromBase64String(entry.base64Data);
            byte[] data = await Task.Run(() =>
            {
                using var input = new MemoryStream(compressedData);
                using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);
                return output.ToArray();
            });
            result[entry.path] = data;
            Interlocked.Add(ref totalBytesUnpacked, data.Length);
            if (debug)
                debugMessages?.Add($"Unpacked {entry.path} ({compressedData.Length} bytes)");
        });
    
        await Task.WhenAll(tasks);
    
        if (debugMessages is { Count: > 0 })
        {
            Game.Instance.Logger.StartLogBlock("ContentPack UnpackTextToMemoryParallelAsync Debug Info");
            foreach (string msg in debugMessages)
                Game.Instance.Logger.LogInfo(msg);
            Game.Instance.Logger.EndLogBlock();
        }
    
        Game.Instance.Logger.LogInfo($"Unpacking packed text file {sourceFilePath} to memory finished. {result.Count} files with {totalBytesUnpacked} total bytes unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        setTotalBytesUnpacked?.Invoke(totalBytesUnpacked);
        return new Dictionary<string, byte[]>(result);
    }
    
    #endregion
    
    #region Index
    /// <summary>
    /// Creates an index for a binary content pack file, mapping relative file paths to their byte offsets in the file.
    /// Reads the index offset from the file header and then iterates through the index entries.
    /// Returns a dictionary where each key is a file path and each value is the offset for that file's data.
    /// </summary>
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
    /// <summary>
    /// Creates an index for a packed text file (.txt), mapping each relative file path to its offset in the file.
    /// Each file entry consists of two lines: the relative path and the base64-encoded compressed data.
    /// Returns a dictionary where the key is the file path and the value is the offset for the data line.
    /// </summary>
    private Dictionary<string, long> CreateTextIndex()
    {
        var result = new Dictionary<string, long>();

        if (!File.Exists(SourceFilePath))
        {
            Game.Instance.Logger.LogError($"Source file not found: {SourceFilePath}");
            return result;
        }

        if (Path.GetExtension(SourceFilePath) != ".txt")
        {
            Game.Instance.Logger.LogError($"Source file must have a .txt extension: {SourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        int indexedFiles = 0;

        
        using var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs);
        if(DebugLogging) Game.Instance.Logger.StartLogBlock("ContentPack CreateTextIndex Debug Info");
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
                    Game.Instance.Logger.LogInfo($"Indexed File {path} with offset {dataOffset})");
                }
            }
            indexedFiles++;
        }
        if(DebugLogging) Game.Instance.Logger.EndLogBlock();
        
        Game.Instance.Logger.LogInfo($"Indexing packed text file {SourceFilePath} finished. {indexedFiles} files indexed in {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
        return result;
    }
    
    /// <summary>
    /// Retrieves and decompresses file data from a binary content pack at the specified byte offset.
    /// The offset should point to the start of the file entry in the pack.
    /// Returns the decompressed byte array for the file.
    /// </summary>
    /// <param name="byteOffset">The byte offset in the content pack file where the file entry starts.</param>
    /// <returns>The decompressed file data as a byte array.</returns>
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
    /// <summary>
    /// Retrieves and decompresses file data from a packed text file (.txt) at the specified byte offset.
    /// The offset should point to the start of the base64-encoded compressed data line.
    /// Returns the decompressed byte array for the file.
    /// </summary>
    /// <param name="byteOffset">The byte offset in the text file where the base64 data line starts.</param>
    /// <returns>The decompressed file data as a byte array.</returns>
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
    /// <summary>
    /// Loads a <see cref="Texture2D"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the texture file within the content pack.</param>
    /// <returns>The loaded <see cref="Texture2D"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Texture2D LoadTexture(string filePath)
    {
        TryLoadTexture(filePath, out var texture);
        return texture;
    }

    /// <summary>
    /// Loads an <see cref="Image"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the image file within the content pack.</param>
    /// <returns>The loaded <see cref="Image"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Image LoadImage(string filePath)
    {
        TryLoadImage(filePath, out var image);
        return image;
    }

    /// <summary>
    /// Loads a <see cref="Font"/> from the content pack using the specified file path and font size.
    /// </summary>
    /// <param name="filePath">The relative path of the font file within the content pack.</param>
    /// <param name="fontSize">The desired font size. Default is 100.</param>
    /// <returns>The loaded <see cref="Font"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Font LoadFont(string filePath, int fontSize = 100)
    {
        TryLoadFont(filePath, out var font, fontSize);
        return font;
    }

    /// <summary>
    /// Loads a <see cref="Wave"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the wave file within the content pack.</param>
    /// <returns>The loaded <see cref="Wave"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Wave LoadWave(string filePath)
    {
        TryLoadWave(filePath, out var wave);
        return wave;
    }

    /// <summary>
    /// Loads a <see cref="Sound"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the sound file within the content pack.</param>
    /// <returns>The loaded <see cref="Sound"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Sound LoadSound(string filePath)
    {
        TryLoadSound(filePath, out var sound);
        return sound;
    }

    /// <summary>
    /// Loads a <see cref="Music"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the music file within the content pack.</param>
    /// <returns>The loaded <see cref="Music"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Music LoadMusic(string filePath)
    {
        TryLoadMusic(filePath, out var music);
        return music;
    }

    /// <summary>
    /// Loads a fragment <see cref="Shader"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the fragment shader file within the content pack.</param>
    /// <returns>The loaded <see cref="Shader"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Shader LoadFragmentShader(string filePath)
    {
        TryLoadFragmentShader(filePath, out var shader);
        return shader;
    }

    /// <summary>
    /// Loads a vertex <see cref="Shader"/> from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the vertex shader file within the content pack.</param>
    /// <returns>The loaded <see cref="Shader"/> instance.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Shader LoadVertexShader(string filePath)
    {
        TryLoadVertexShader(filePath, out var shader);
        return shader;
    }

    /// <summary>
    /// Loads a text file from the content pack using the specified file path.
    /// </summary>
    /// <param name="filePath">The relative path of the text file within the content pack.</param>
    /// <returns>The loaded text as a <see cref="string"/>.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method,
    /// otherwise it will fail and return a default value.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public string LoadText(string filePath)
    {
        TryLoadText(filePath, out var text);
        return text;
    }
    #endregion
    
    #region TryLoad
    /// <summary>
    /// Attempts to load a <see cref="Texture2D"/> from the content pack using the specified file path.
    /// Returns true if the texture is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the texture file within the content pack.</param>
    /// <param name="texture">The loaded <see cref="Texture2D"/> instance if successful;
    /// otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> has to be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadTexture(string filePath, out Texture2D texture)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadTexture() failed. Content pack not loaded!");
            texture = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadTexture() failed. File {filePath} not found in pack!");
            texture = new();
            return false;
        }

        string extension = Path.GetExtension(filePath);
        texture = ContentLoader.LoadTextureFromMemory(extension, data);
        return true;
    }
    /// <summary>
    /// Attempts to load an <see cref="Image"/> from the content pack using the specified file path.
    /// Returns true if the image is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the image file within the content pack.</param>
    /// <param name="image">The loaded <see cref="Image"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadImage(string filePath, out Image image)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadImage() failed. Content pack not loaded!");
            image = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadImage() failed. File {filePath} not found in pack!");
            image = new();
            return false;
        }

        string extension = Path.GetExtension(filePath);
        image = ContentLoader.LoadImageFromMemory(extension, data);
        return true;
    }
    /// <summary>
    /// Attempts to load a <see cref="Font"/> from the content pack using the specified file path and font size.
    /// Returns true if the font is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the font file within the content pack.</param>
    /// <param name="font">The loaded <see cref="Font"/> instance if successful; otherwise, a default value.</param>
    /// <param name="fontSize">The desired font size. Default is 100.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadFont(string filePath, out Font font, int fontSize = 100)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadFont() failed. Content pack not loaded!");
            font = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadFont() failed. File {filePath} not found in pack!");
            font = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        font = ContentLoader.LoadFontFromMemory(extension, data, fontSize);
        return true;
    }
    /// <summary>
    /// Attempts to load a <see cref="Wave"/> from the content pack using the specified file path.
    /// Returns true if the wave is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the wave file within the content pack.</param>
    /// <param name="wave">The loaded <see cref="Wave"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadWave(string filePath, out Wave wave)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadWave() failed. Content pack not loaded!");
            wave = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadWave() failed. File {filePath} not found in pack!");
            wave = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        wave =  ContentLoader.LoadWaveFromMemory(extension, data);
        return true;
    }
    /// <summary>
    /// Attempts to load a <see cref="Sound"/> from the content pack using the specified file path.
    /// Returns true if the sound is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the sound file within the content pack.</param>
    /// <param name="sound">The loaded <see cref="Sound"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadSound(string filePath, out Sound sound)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadSound() failed. Content pack not loaded!");
            sound = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadSound() failed. File {filePath} not found in pack!");
            sound = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        sound = ContentLoader.LoadSoundFromMemory(extension, data);
        return true;

    }
    
    /// <summary>
    /// Attempts to load a <see cref="Music"/> from the content pack using the specified file path.
    /// Returns true if the music is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the music file within the content pack.</param>
    /// <param name="music">The loaded <see cref="Music"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadMusic(string filePath, out Music music)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadMusic() failed. Content pack not loaded!");
            music = new();   
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadMusic() failed. File {filePath} not found in pack!");
            music = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath);
        music = ContentLoader.LoadMusicFromMemory(extension, data);
        return true;
    }
    /// <summary>
    /// Attempts to load a fragment <see cref="Shader"/> from the content pack using the specified file path.
    /// Returns true if the shader is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the fragment shader file within the content pack.</param>
    /// <param name="shader">The loaded <see cref="Shader"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadFragmentShader(string filePath, out Shader shader)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadFragmentShader() failed. Content pack not loaded!");
            shader = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadFragmentShader() failed. File {filePath} not found in pack!");
            shader = new();
            return false;
        }
        
        shader = ContentLoader.LoadFragmentShaderFromMemory(data);
        return true;
    }
    
    /// <summary>
    /// Attempts to load a vertex <see cref="Shader"/> from the content pack using the specified file path.
    /// Returns true if the shader is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the vertex shader file within the content pack.</param>
    /// <param name="shader">The loaded <see cref="Shader"/> instance if successful; otherwise, a default value.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadVertexShader(string filePath, out Shader shader)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadVertexShader() failed. Content pack not loaded!");
            shader = new();
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadVertexShader() failed. File {filePath} not found in pack!");
            shader = new();
            return false;
        }
        
        shader = ContentLoader.LoadVertexShaderFromMemory(data);
        return true;
    }
   
    /// <summary>
    /// Attempts to load a text file from the content pack using the specified file path.
    /// Returns true if the text is successfully loaded; otherwise, false.
    /// </summary>
    /// <param name="filePath">The relative path of the text file within the content pack.</param>
    /// <param name="text">The loaded text as a <see cref="string"/> if successful; otherwise, an empty string.</param>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and return false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public bool TryLoadText(string filePath, out string text)
    {
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError("ContentPack LoadText() failed. Content pack not loaded!");
            text = string.Empty;
            return false;
        }
        
        byte[]? data = GetFileData(filePath);
        if (data == null)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadText() failed. File {filePath} not found in pack!");
            text = string.Empty;
            return false;
        }
        
        // string extension = Path.GetExtension(filePath);
        text = ContentLoader.LoadTextFromMemory(data);
        return true;
    }
    #endregion
    
    #region Load All
    /// <summary>
    /// Loads all <see cref="Texture2D"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded textures.
    /// The <paramref name="success"/> output parameter is set to true if at least one texture is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if textures were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Texture2D"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Texture2D> LoadAllTextures(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Texture2D>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllTextures() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllTextures() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all <see cref="Image"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded images.
    /// The <paramref name="success"/> output parameter is set to true if at least one image is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if images were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Image"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Image> LoadAllImages(out bool success)
        {
            success = false;
            var result = new Dictionary<string, Image>();
            if (!IsLoaded)
            {
                Game.Instance.Logger.LogError($"ContentPack LoadAllImages() failed. Content pack not loaded!");
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
                Game.Instance.Logger.LogWarning($"ContentPack LoadAllImages() found no valid content in pack!");
                return result;
            }
            
            success = true;
            return result;
        }
    /// <summary>
    /// Loads all <see cref="Font"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded fonts.
    /// The <paramref name="success"/> output parameter is set to true if at least one font is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if fonts were loaded; otherwise, false.</param>
    /// <param name="fontSize">The desired font size. Default is 100.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Font"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Font> LoadAllFonts(out bool success, int fontSize = 100)
    {
        success = false;
        var result = new Dictionary<string, Font>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllFonts() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllFonts() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all <see cref="Wave"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded waves.
    /// The <paramref name="success"/> output parameter is set to true if at least one wave is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if waves were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Wave"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Wave> LoadAllWaves(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Wave>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllWaves() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllWaves() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all <see cref="Sound"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded sounds.
    /// The <paramref name="success"/> output parameter is set to true if at least one sound is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if sounds were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Sound"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Sound> LoadAllSounds(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Sound>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllSounds() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllSounds() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all <see cref="Music"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded music instances.
    /// The <paramref name="success"/> output parameter is set to true if at least one music file is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if music files were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Music"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Music> LoadAllMusic(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Music>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllMusic() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllMusic() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all fragment <see cref="Shader"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded fragment shaders.
    /// The <paramref name="success"/> output parameter is set to true if at least one fragment shader is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if fragment shaders were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Shader"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Shader> LoadAllFragmentShaders(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Shader>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllFragmentShaders() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllFragmentShader() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all vertex <see cref="Shader"/> assets from the content pack.
    /// Returns a dictionary mapping file paths to loaded vertex shaders.
    /// The <paramref name="success"/> output parameter is set to true if at least one vertex shader is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if vertex shaders were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding <see cref="Shader"/> instances.</returns>
    /// <remarks>
    /// ContentPack <see cref="IsLoaded"/> must be true before calling this method, otherwise it will fail and <paramref name="success"/> will be false.
    /// Use <see cref="CreateCache"/> or <see cref="CreateIndex"/> to load the content pack first.
    /// </remarks>
    public Dictionary<string, Shader> LoadAllVertexShaders(out bool success)
    {
        success = false;
        var result = new Dictionary<string, Shader>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"ContentPack LoadAllVertexShaders() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllVertexShaders() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    /// <summary>
    /// Loads all text files from the content pack.
    /// Returns a dictionary mapping file paths to their loaded text content.
    /// The <paramref name="success"/> output parameter is set to true if at least one text file is loaded; otherwise, false.
    /// </summary>
    /// <param name="success">True if text files were loaded; otherwise, false.</param>
    /// <returns>Dictionary of file paths and their corresponding text content as <see cref="string"/>.</returns>
    public Dictionary<string, string> LoadAllText(out bool success)
    {
        success = false;
        var result = new Dictionary<string, string>();
        if (!IsLoaded)
        {
            Game.Instance.Logger.LogError($"Content Pack LoadAllText() failed. Content pack not loaded!");
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
            Game.Instance.Logger.LogWarning($"ContentPack LoadAllText() found no valid content in pack!");
            return result;
        }
        
        success = true;
        return result;
    }
    #endregion
}
