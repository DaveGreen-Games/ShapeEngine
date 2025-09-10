using System.Collections;
using System.IO.Compression;
using System.Resources;

namespace ResourcePacker;

/// <summary>
/// Provides static methods for packing and unpacking resources and directories
/// into text files or resource files, as well as extracting them.
/// </summary>
public static class ResourcePackManager
{
    #region Simple Txt Packer

    public static bool PackToText(string outputPath, string sourcePath, List<string>? extensionExceptions = null)
    {
        if(!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"Source Directory not found: {sourcePath}");
            return false;
        }

        if (!Path.HasExtension(outputPath))
        {
            Console.WriteLine($"No output file extension found: {outputPath}");
            return false;
        }

        if (Path.GetExtension(outputPath) != ".txt")
        {
            Console.WriteLine($"Output file extension must be .txt: {outputPath}");
            return false;
        }
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (outputDirectory == null)
        {
            Console.WriteLine($"Directory not found: {outputPath}");
            return false;
        }
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"Created output directory: {outputDirectory}");
        }
        
        string[] files = Directory.GetFiles(sourcePath, "", SearchOption.AllDirectories);
        var lines = new List<string>();
        foreach (string file in files)
        {
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file))) continue;
            
            lines.Add(Path.GetRelativePath(sourcePath, file));
            byte[] d = File.ReadAllBytes(file);
            lines.Add(Convert.ToBase64String(Compress(d)));
        }
        File.WriteAllLines(outputPath, lines);
        return true;
    }
    
    public static bool UnpackFromText(string outputDirectory, string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            Console.WriteLine($"Source file not found: {sourcePath}");
            return false;
        }

        if (Path.GetExtension(sourcePath) != ".txt")
        {
            Console.WriteLine($"Source file must have a .txt extension: {sourcePath}");
            return false;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Directory created: {outputDirectory}");
            Directory.CreateDirectory(outputDirectory);
        }

        var lines = File.ReadAllLines(sourcePath);
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            var relativePath = lines[i];
            var base64Data = lines[i + 1];
            var filePath = Path.Combine(outputDirectory, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if(dirPath == null) continue;
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var compressedData = Convert.FromBase64String(base64Data);
            File.WriteAllBytes(filePath, Decompress(compressedData));
        }
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
    
    /// <summary>
    /// Compresses binary data using the Deflate algorithm using compression level <see cref="CompressionLevel.Optimal"/>.
    /// </summary>
    /// <seealso cref="DeflateStream"/>
    /// <seealso cref="MemoryStream"/>
    /// <param name="data">The binary data to compress.</param>
    /// <returns>The compressed binary data.</returns>
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
    
    #region Create Resource File
    
    public static bool CreateResourcePackFromFile(string outputResPath, string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"File not found: {sourceFilePath}");
            return false;
        }
        
        var resxDir = Path.GetDirectoryName(outputResPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }

        if (!Directory.Exists(resxDir))
        {
            Directory.CreateDirectory(resxDir);
            Console.WriteLine($"Created output directory: {resxDir}");
        }

        var fileName = Path.GetFileName(sourceFilePath);
        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
        {
            writer.AddResource(fileName, File.ReadAllBytes(sourceFilePath));
            writer.Generate();
            writer.Close();
        }
        
        CompressGzip(memStream, outputResPath);
        
        return true;
    }

    public static bool CreateResourcePackFromDirectory(string outputResPath, string sourceDirectoryPath, List<string>? extensionExceptions = null)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        if (!Path.HasExtension(outputResPath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputResPath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputResPath);
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
    
        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
        {
            foreach (var file in files)
            {
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file))) continue;
                var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
                writer.AddResource(relativeName, File.ReadAllBytes(file));
            }
            writer.Generate();
            writer.Close(); // Ensure all data is written
        }

        CompressGzip(memStream, outputResPath);

        return true;
    }
    #endregion

    #region Append Resource File
    
    public static bool AddFileToPack(string outputResPath, string sourceFilePath)
    {
        if (!Path.HasExtension(sourceFilePath) || !File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Source File not found: {sourceFilePath}");
            return false;
        }

        var resxDir = Path.GetDirectoryName(outputResPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
        
        var resources = new Dictionary<string, byte[]>();
        
        if (Directory.Exists(resxDir))
        {
            // Read existing resources
            if (File.Exists(outputResPath))
            {
                using var decompressMemStream = DecompressGzip(outputResPath);

                using var reader = new ResourceReader(decompressMemStream);
                foreach (DictionaryEntry entry in reader)
                {
                    var fileName = entry.Key.ToString();
                    if (fileName == null) continue;
                    if (entry.Value is byte[] bytes) resources[fileName] = bytes;
                }
            }
        }
        else
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
        
        var resourceName = Path.GetFileName(sourceFilePath);
        resources[resourceName] = File.ReadAllBytes(sourceFilePath);

        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
        {
            foreach (var kvp in resources)
            {
                writer.AddResource(kvp.Key, kvp.Value);
            }
            writer.Generate();
            writer.Close();
        }

        CompressGzip(memStream, outputResPath);
        return true;
    }
    
    public static bool AddDirectoryToPack(string outputResPath, string sourceDirectoryPath, List<string>? extensionExceptions = null)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
        
        string? resxDir = Path.GetDirectoryName(outputResPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
        
        var resources = new Dictionary<string, byte[]>();
        
        if (Directory.Exists(resxDir))
        {
            // Read existing resources
            if (File.Exists(outputResPath))
            {
                using var decompressMemStream = DecompressGzip(outputResPath);
                using var reader = new ResourceReader(decompressMemStream);
                foreach (DictionaryEntry entry in reader)
                {
                    var fileName = entry.Key.ToString();
                    if (fileName == null) continue;
                    if(entry.Value is byte[] bytes) resources[fileName] = bytes;
                }
            }
        }
        else
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }

        string[] files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file))) continue;
            
            string relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
            resources[relativeName] = File.ReadAllBytes(file);
        }

        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
        {
            foreach (var kvp in resources)
            {
                writer.AddResource(kvp.Key, kvp.Value);
            }
            writer.Generate();
            writer.Close();
        }

        CompressGzip(memStream, outputResPath);
        return true;
    }
   
    #endregion
    
    #region Unpack Resouce File
    public static bool Unpack(string outputDirectory, string sourceResPath)
    {
        if (!Path.HasExtension(sourceResPath) || !File.Exists(sourceResPath))
        {
            Console.WriteLine($"Source File not found: {sourceResPath}");
            return false;
        }
    
        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Directory created: {outputDirectory}");
            Directory.CreateDirectory(outputDirectory);
        }
    
        using var decompressMemStream = DecompressGzip(sourceResPath);
        using var reader = new ResourceReader(decompressMemStream);
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            string filePath = Path.Combine(outputDirectory, fileName);
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null) continue;
            Directory.CreateDirectory(directoryPath);
            if (entry.Value is byte[] bytes) File.WriteAllBytes(filePath, bytes);
        }
    
        return true;
    }
    public static bool UnpackDebug(string outputDirectory, string sourceResPath)
    {
        if (!Path.HasExtension(sourceResPath) || !File.Exists(sourceResPath))
        {
            Console.WriteLine($"Source File not found: {sourceResPath}");
            return false;
        }
    
        using var decompressMemStream = DecompressGzip(sourceResPath);
        using var reader = new ResourceReader(decompressMemStream);
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            string filePath = Path.Combine(outputDirectory, fileName);
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null) continue;
    
            if (entry.Value is byte[] bytes)
            {
                Console.WriteLine($"File read -> Filename: {fileName} | FilePath: {filePath} | DirectoryPath: {directoryPath} with {bytes.Length} bytes");
            }
        }
    
        return true;
    }
    

    #endregion
    
    #region Compress/Decompress Helpers
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
    
    #endregion
    
}