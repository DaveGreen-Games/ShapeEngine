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
    //TODO: add progress bars to pack functions
    
    //TODO: fix memory stream too long error when packing large directories
    
    
    
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
        Console.WriteLine($"File packing started with {files.Length} files.");
        
        var lines = new List<string>();
        foreach (string file in files)
        {
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
            {
                if (debug)
                {
                    Console.WriteLine($"File skipped due to extension: {Path.GetFileName(file)}");
                }
                continue;
            }
            
            lines.Add(Path.GetRelativePath(sourceDirectoryPath, file));
            byte[] d = File.ReadAllBytes(file);
            lines.Add(Convert.ToBase64String(Compress(d)));

            if (debug)
            {
                Console.WriteLine($" -File {file} has been packed with {d.Length} bytes.");
            }
        }
        File.WriteAllLines(outputFilePath, lines);
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
        Console.WriteLine($"File unpacking started with {lines.Length} lines.");
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            var relativePath = lines[i];
            
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug)
                {
                    Console.WriteLine($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                }
                continue;
            }
            
            var filePath = Path.Combine(outputDirectoryPath, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if(dirPath == null) continue;
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                if(debug) Console.WriteLine($"Output Directory created: {dirPath}");
            }
            var base64Data = lines[i + 1];
            var compressedData = Convert.FromBase64String(base64Data);
            File.WriteAllBytes(filePath, Decompress(compressedData));
            if (debug)
            {
                Console.WriteLine($" -File {filePath} has been unpacked with {compressedData.Length} bytes.");
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
        
        Console.WriteLine($"Pack to file {outputFilePath} started with {files.Length} files found in {sourceDirectoryPath}.");
        
        //VARIANT 1
        using var memStream = new MemoryStream();
        using (var writer = new ResourceWriter(memStream))
        {
            foreach (var file in files)
            {
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                {
                    if (debug)
                    {
                        Console.WriteLine($"File skipped due to extension: {Path.GetFileName(file)}");
                    }
                    continue;
                }
                var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
                writer.AddResource(relativeName, File.ReadAllBytes(file));
                if (debug)
                {
                    Console.WriteLine($" -File {file} has been packed with {new FileInfo(file).Length} bytes.");
                }
            }
            writer.Generate();
            writer.Close(); // Ensure all data is written
        }
        
        CompressGzip(memStream, outputFilePath);

        //VARIANT 2
        // using (var fileStream = new FileStream(outputResPath, FileMode.Create, FileAccess.Write))
        // using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
        // using (var writer = new ResourceWriter(gzipStream))
        // {
        //     foreach (var file in files)
        //     {
        //         if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file))) continue;
        //         var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
        //         writer.AddResource(relativeName, File.ReadAllBytes(file));
        //     }
        //     writer.Generate();
        //     writer.Close(); // Ensure all data is written
        // }
        
        //VARIANT 3
        // using (var memStream = new MemoryStream())
        // {
        //     using (var writer = new ResourceWriter(memStream))
        //     {
        //         foreach (var file in files)
        //         {
        //             if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file))) continue;
        //             var relativeName = Path.GetRelativePath(sourceDirectoryPath, file);
        //             writer.AddResource(relativeName, File.ReadAllBytes(file));
        //         }
        //         writer.Generate();
        //         writer.Close();
        //     }
        //     memStream.Position = 0;
        //     using (var fileStream = new FileStream(outputResPath, FileMode.Create, FileAccess.Write))
        //     using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
        //     {
        //         memStream.CopyTo(gzipStream);
        //     }
        // }
        
        Console.WriteLine("Pack to file finished.");
        
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
        
        
        Console.WriteLine($"Unpack from file {sourceFilePath} started to {outputDirectoryPath}.");
        
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(fileName)))
            {
                if (debug)
                {
                    Console.WriteLine($"File skipped due to extension: {Path.GetFileName(fileName)}");
                }
                continue;
            }
            
            string filePath = Path.Combine(outputDirectoryPath, fileName);
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null) continue;
            Directory.CreateDirectory(directoryPath);
            if (entry.Value is byte[] bytes) File.WriteAllBytes(filePath, bytes);
            if (debug)
            {
                Console.WriteLine($" -File {filePath} has been unpacked with {new FileInfo(filePath).Length} bytes.");
            }
        }
    
        Console.WriteLine("Unpack from file finished.");
        
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