using System.Collections;
using System.Diagnostics;
using System.IO.Compression;
using System.Resources;

namespace ResourcePacker;

//TODO: make final AI check for issues
//TODO: add documentation comments


/// <summary>
/// Provides static methods for packing and unpacking resources and directories
/// into text files or resource files, as well as extracting them.
/// </summary>
public static class ResourcePackManager
{
    private const double progressIntervalMilliseconds = 100.0;
    
    #region Public Interface
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (Path.GetExtension(outputFilePath) == ".txt")
        {
            return PackToText(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
        }
        return PackToFile(outputFilePath, sourceDirectoryPath, extensionExceptions, debug);
    }
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (Path.GetExtension(sourceFilePath) == ".txt")
        {
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
        int total = files.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"File packing started with {total} files.");
        
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
            lines.Add(Convert.ToBase64String(Compress(d)));

            if (debug)
            {
                debugMessages.Add($" -File {file} has been packed with {d.Length} bytes.");
            }
        }
        File.WriteAllLines(outputFilePath, lines);

        Console.WriteLine("");
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        Console.WriteLine($"File packing finished. With {lines.Count / 2} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes packed: {totalBytesPacked}");
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
        int total = lines.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total / 2) : [];
        Console.WriteLine($"File unpacking started with {total} lines.");
        
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            
            current += 2;
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
            File.WriteAllBytes(filePath, data);
            if (debug)
            {
                debugMessages.Add($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
            }
        }
        
        Console.WriteLine("");
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        Console.WriteLine($"File unpacking finished. With {lines.Length / 2} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
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
            var data = Decompress(compressedData);
            totalBytesUnpacked += data.Length;
            result[relativePath] = data;
            unpackedFiles++;
            if (debug) Console.WriteLine($"Unpacked {relativePath} ({compressedData.Length} bytes)");
        }

        Console.WriteLine($"Unpacking packed text file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
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
    
    #region Legacy Binary Packer
    private static bool PackToFileLegacy(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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

        long totalBytes = 0;
        int total = files.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();
        int packedFiles = 0;
        
        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"Pack to file {outputFilePath} started with {total} files found in {sourceDirectoryPath}.");
        
        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
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
                    if (debug)
                    {
                        debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                    }
                    continue;
                }
                var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
                var data = File.ReadAllBytes(file);
                totalBytes += data.Length;
                writer.AddResource(relativeName, data);
                packedFiles++;
                if (debug)
                {
                    debugMessages.Add($" -File {file} has been packed with {new FileInfo(file).Length} bytes.");
                }
            }
            
            writer.Generate();
            writer.Close(); // Ensure all data is written
        }
        
        CompressGzip(memStream, outputFilePath);
        
        Console.WriteLine("");
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
        
        Console.WriteLine($"File packing finished. With {packedFiles} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes packed: {totalBytes}");
        
        return true;
    }
    private static bool UnpackFromFileLegacy(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Path.HasExtension(sourceFilePath) || !File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source File not found: {sourceFilePath}");
            return false;
        }
    
        if (!Directory.Exists(outputDirectoryPath))
        {
            Console.WriteLine($"Directory created: {outputDirectoryPath}");
            Directory.CreateDirectory(outputDirectoryPath);
        }
    
        var debugWatch = Stopwatch.StartNew();
        
        using var decompressMemStream = DecompressGzip(sourceFilePath);
        using var reader = new ResourceReader(decompressMemStream);
        
        long totalBytesUnpacked = 0;
        int total = reader.Cast<DictionaryEntry>().Count();
        int current = 0;
        int unpackedFiles = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"Unpack from file {sourceFilePath}  to {outputDirectoryPath} with {total} files started.");
        
        foreach (DictionaryEntry entry in reader)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressIntervalMilliseconds || current == total)
            {
                PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
            
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(fileName)))
            {
                if (debug)
                {
                    debugMessages.Add($"File skipped due to extension: {Path.GetFileName(fileName)}");
                }
                continue;
            }
            
            string filePath = Path.Combine(outputDirectoryPath, fileName);
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null) continue;
            Directory.CreateDirectory(directoryPath);
            if (entry.Value is byte[] bytes)
            {
                totalBytesUnpacked += bytes.Length;
                File.WriteAllBytes(filePath, bytes);
                unpackedFiles++;
                if (debug)
                {
                    debugMessages.Add($" -File {filePath} has been unpacked with {new FileInfo(filePath).Length} bytes.");
                }
            }
            else
            {
                debugMessages.Add($" -File {filePath} has unknown data type and was skipped.");
            }
            
        }
        
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
    
        Console.WriteLine($"File unpacking finished. With {unpackedFiles} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        
        return true;
    }
    private static MemoryStream DecompressGzip(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        var memStream = new MemoryStream();
        gzipStream.CopyTo(memStream);
        memStream.Position = 0;
        return memStream;
    }
    private static void CompressGzip(MemoryStream memStream, string outputPath)
    {
        var buffer = memStream.ToArray();
        using var bufferStream = new MemoryStream(buffer);
        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        bufferStream.CopyTo(gzipStream);
    }

    #endregion
}




/*
private static bool PackToFile_OLD2(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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

    var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);

    int total = files.Length;
    int current = 0;
    var sw = Stopwatch.StartNew();
    int packedFiles = 0;

    var debugMessages = debug ? new List<string>(total) : [];
    Console.WriteLine($"Pack to file {outputFilePath} started with {total} files found in {sourceDirectoryPath}.");

    // Write directly to file stream with GZip compression
    using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
    using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
    using (var writer = new ResourceWriter(gzipStream))
    {
        foreach (var file in files)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressInterval || current == total)
            {
                PrintProgressBar(current, total);
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
            var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
            writer.AddResource(relativeName, File.ReadAllBytes(file));
            packedFiles++;
            if (debug)
            {
                debugMessages.Add($" -File {file} has been packed with {new FileInfo(file).Length} bytes.");
            }
        }

        writer.Generate();
        writer.Close();
    }

    Console.WriteLine("");
    if (debugMessages.Count > 0)
    {
        foreach (string message in debugMessages)
        {
            Console.WriteLine(message);
        }
    }

    Console.WriteLine($"File packing finished. With {packedFiles} files packed to {outputFilePath}");

    return true;
}

private static bool PackToFile_OLD3(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
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

    var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);

    int total = files.Length;
    int current = 0;
    var sw = Stopwatch.StartNew();
    int packedFiles = 0;

    var debugMessages = debug ? new List<string>(total) : [];
    Console.WriteLine($"Pack to file {outputFilePath} started with {total} files found in {sourceDirectoryPath}.");

    // Step 1: Write resources to a temporary file
    string tempFile = Path.GetTempFileName();
    try
    {
        using (var tempStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
        using (var writer = new ResourceWriter(tempStream))
        {
            foreach (var file in files)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= progressInterval || current == total)
                {
                    PrintProgressBar(current, total);
                    sw.Restart();
                    if (current == total)
                    {
                        Console.WriteLine("");
                    }
                }

                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                {
                    if (debug)
                    {
                        debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                    }
                    continue;
                }
                var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
                writer.AddResource(relativeName, File.ReadAllBytes(file));
                packedFiles++;
                if (debug)
                {
                    debugMessages.Add($" -File {file} has been packed with {new FileInfo(file).Length} bytes.");
                }
            }
            writer.Generate();
            writer.Close();
        }

        // Step 2: Compress the temporary file to the final output
        using (var tempStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
        using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
        {
            tempStream.CopyTo(gzipStream);
        }
    }
    finally
    {
        // Step 3: Delete the temporary file
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }

    
    if (debugMessages.Count > 0)
    {
        foreach (string message in debugMessages)
        {
            Console.WriteLine(message);
        }
    }

    Console.WriteLine($"File packing finished. With {packedFiles} files packed to {outputFilePath}");

    return true;
}
*/

/*public static bool UnpackFromFileLarge_OLD(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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

    var debugMessages = debug ? new List<string>() : [];
    using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

    int unpackedFiles = 0;
    while (gzipStream.Position < fileStream.Length)
    {
        var pathLenBytes = new byte[4];
        if (gzipStream.Read(pathLenBytes, 0, 4) < 4) break;
        int pathLen = BitConverter.ToInt32(pathLenBytes, 0);

        var pathBytes = new byte[pathLen];
        if (gzipStream.Read(pathBytes, 0, pathLen) < pathLen) break;
        string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);

        var dataLenBytes = new byte[4];
        if (gzipStream.Read(dataLenBytes, 0, 4) < 4) break;
        int dataLen = BitConverter.ToInt32(dataLenBytes, 0);

        var data = new byte[dataLen];
        if (gzipStream.Read(data, 0, dataLen) < dataLen) break;

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

    if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
    Console.WriteLine($"Unpacking finished. {unpackedFiles} files unpacked to {outputDirectoryPath}");
    return true;
}*/
    