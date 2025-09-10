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
    //TODO: fix memory stream too long error when packing large directories

    private const double progressInterval = 100.0;
    
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
    
    private static void PrintProgressBar(int current, int total, int barWidth = 30, string title = "File")
    {
        double percent = (double)current / total;
        int filled = (int)(barWidth * percent);
        int empty = barWidth - filled;
        string bar = new string('/', filled) + new string('_', empty);
        
        if (current == total)
        {
            Console.Write($"\r[{title} {current}/{total}] [{bar}] [{(percent * 100):F1}%]\n");
        }
        else
        {
            Console.Write($"\r[{title} {current}/{total}] [{bar}] [{(percent * 100):F1}%]");
        }
    }
    
    #region Simple Txt Packer

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
        
        string[] files = Directory.GetFiles(sourceDirectoryPath, "", SearchOption.AllDirectories);
        
        int total = files.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"File packing started with {total} files.");
        
        var lines = new List<string>();
        foreach (string file in files)
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
            
            lines.Add(Path.GetRelativePath(sourceDirectoryPath, file));
            byte[] d = File.ReadAllBytes(file);
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
        Console.WriteLine($"File packing finished. With {lines.Count / 2} files packed to {outputFilePath}");
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

        var lines = File.ReadAllLines(sourceFilePath);
        
        int total = lines.Length;
        int current = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total / 2) : [];
        Console.WriteLine($"File unpacking started with {total} lines.");
        
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            
            current += 2;
            if (sw.Elapsed.TotalMilliseconds >= progressInterval || current == total)
            {
                PrintProgressBar(current, total);
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
            File.WriteAllBytes(filePath, Decompress(compressedData));
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
        Console.WriteLine($"File unpacking finished. With {lines.Length / 2} files unpacked to {outputDirectoryPath}");
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
        var output = new MemoryStream();
        using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    
    #endregion
    
    #region Resource File Packer
    
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
    
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        
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
        
        Console.WriteLine($"File packing finished. With {packedFiles} files packed to {outputFilePath}");
        
        return true;
    }
    
    private static bool PackToFileLarge(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
    
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
                if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                continue;
            }
    
            var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
            var data = File.ReadAllBytes(file);
    
            // Write header: [path length][path][data length][data]
            gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            gzipStream.Write(pathBytes, 0, pathBytes.Length);
            gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            gzipStream.Write(data, 0, data.Length);
    
            if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
        }
        gzipStream.Write(BitConverter.GetBytes(total), 0, 4);
        
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {total} files packed to {outputFilePath}");
        return true;
    }
    
    private static bool UnpackFromFile(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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
    
        using var decompressMemStream = DecompressGzip(sourceFilePath);
        using var reader = new ResourceReader(decompressMemStream);
        
        int total = reader.Cast<DictionaryEntry>().Count();
        int current = 0;
        int unpackedFiles = 0;
        var sw = Stopwatch.StartNew();

        var debugMessages = debug ? new List<string>(total) : [];
        Console.WriteLine($"Unpack from file {sourceFilePath}  to {outputDirectoryPath} with {total} files started.");
        
        foreach (DictionaryEntry entry in reader)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressInterval || current == total)
            {
                PrintProgressBar(current, total);
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
        
        Console.WriteLine("");
        if (debugMessages.Count > 0)
        {
            foreach (string message in debugMessages)
            {
                Console.WriteLine(message);
            }
        }
    
        Console.WriteLine($"File unpacking finished. With {unpackedFiles} files unpacked to {outputDirectoryPath}");
        
        return true;
    }

    public static bool UnpackFromFileLarge(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
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

        // Read total files from the last 4 bytes of the file
        fileStream.Seek(-4, SeekOrigin.End);
        var totalBytes = new byte[4];
        fileStream.ReadExactly(totalBytes, 0, 4);
        int total = BitConverter.ToInt32(totalBytes, 0);

        // Reset to start for unpacking
        fileStream.Seek(0, SeekOrigin.Begin);

        int unpackedFiles = 0;
        int current = 0;
        var sw = Stopwatch.StartNew();

        long dataEnd = fileStream.Length - 4; // Exclude last 4 bytes
        while (gzipStream.Position < dataEnd)
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

            current++;
            if (sw.Elapsed.TotalMilliseconds >= progressInterval || current == total)
            {
                PrintProgressBar(current, total, 30, "Unpack");
                sw.Restart();
            }

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
    }
    
    public static Dictionary<string, byte[]> UnpackFromFileLarge(string packedFilePath, List<string>? extensionExceptions = null)
    {
        var result = new Dictionary<string, byte[]>();
        if (!File.Exists(packedFilePath))
            return result;

        using var fileStream = new FileStream(packedFilePath, FileMode.Open, FileAccess.Read);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

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
                continue;

            result[relativePath] = data;
        }
        return result;
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