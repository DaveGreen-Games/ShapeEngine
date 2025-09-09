using System.Collections;
using System.IO.Compression;
using System.Resources;

namespace ResourcePacker;

public static class ResourcePackManager
{
    #region Simple Txt Packer
    /// <summary>
    /// Pack a folder structure of various content types into a single txt file.
    /// </summary>
    /// <param name="sourcePath">The path to the folder that should be packed.
    /// Goes through all subfolders as well.</param>
    /// <param name="outputPath">The path where the resulting txt file should be saved.</param>
    /// <param name="outputFilename">The name of the resulting txt file.</param>
    public static void Pack(string sourcePath, string outputPath, string outputFilename = "resources.txt")
    {
        string[] files = Directory.GetFiles(sourcePath, "", SearchOption.AllDirectories);
        List<string> lines = new List<string>();
        foreach (var file in files)
        {
            lines.Add(Path.GetFileName(file));
            var d = File.ReadAllBytes(file);
            lines.Add(Convert.ToBase64String(Compress(d)));
        }
        File.WriteAllLines(outputPath + outputFilename, lines);
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

    public static void CreateResourcePackFromFile(string resxPath, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return;
        }
        
        if (!Directory.Exists(resxDir)) Directory.CreateDirectory(resxDir);

        var fileName = Path.GetFileName(filePath);
        using var writer = new ResourceWriter(resxPath);
        writer.AddResource(fileName, File.ReadAllBytes(filePath));
        writer.Generate();
    }
    public static void CreateResourcePackFromFile(string resxPath, FileInfo file)
    {
        CreateResourcePackFromFile(resxPath, file.FullName);
    }
    public static void CreateResourcePackFromDirectory(string resxPath, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Directory not found: {directoryPath}");
            return;
        }
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return;
        }
        
        if (!Directory.Exists(resxDir)) Directory.CreateDirectory(resxDir);
        
        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        using var writer = new ResourceWriter(resxPath);
        foreach (var file in files)
        {
            var relativeName = Path.GetRelativePath(directoryPath, file);
            writer.AddResource(relativeName, File.ReadAllBytes(file));
        }
        writer.Generate();
    }
    public static void CreateResourcePackFromDirectory(string resxPath, DirectoryInfo directory)
    {
        CreateResourcePackFromDirectory(resxPath, directory.FullName);
    }
    
    #endregion

    #region Append Resource File
    
    public static void AddFileToPack(string resxPath, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return;
        }
        
        var resources = new Dictionary<string, byte[]>();
        
        if (Directory.Exists(resxDir))
        {
            // Read existing resources
            if (File.Exists(resxPath))
            {
                using var reader = new ResourceReader(resxPath);
                foreach (DictionaryEntry entry in reader)
                {
                    var fileName = entry.Key.ToString();
                    if (fileName == null) continue;
                    if(entry.Value is byte[] bytes) resources[fileName] = bytes;
                }
            }
        }
        else Directory.CreateDirectory(resxDir);
        
        var resourceName = Path.GetFileName(filePath);
        resources[resourceName] = File.ReadAllBytes(filePath);

        using var writer = new ResourceWriter(resxPath);
        foreach (var kvp in resources)
        {
            writer.AddResource(kvp.Key, kvp.Value);
        }
        writer.Generate();
    }
    public static void AddFileToPack(string resxPath, FileInfo file)
    {
        AddFileToPack(resxPath, file.FullName);
    }
    public static void AddDirectoryToPack(string resxPath, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Directory not found: {directoryPath}");
            return;
        }
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return;
        }
        
        var resources = new Dictionary<string, byte[]>();
        
        if (Directory.Exists(resxDir))
        {
            // Read existing resources
            if (File.Exists(resxPath))
            {
                using var reader = new ResourceReader(resxPath);
                foreach (DictionaryEntry entry in reader)
                {
                    var fileName = entry.Key.ToString();
                    if (fileName == null) continue;
                    if(entry.Value is byte[] bytes) resources[fileName] = bytes;
                }
            }
        }
        else Directory.CreateDirectory(resxDir);

        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativeName = Path.GetRelativePath(directoryPath, file);
            resources[relativeName] = File.ReadAllBytes(file);
        }

        using var writer = new ResourceWriter(resxPath);
        foreach (var kvp in resources)
        {
            writer.AddResource(kvp.Key, kvp.Value);
        }
        writer.Generate();
    }
    public static void AddDirectoryToPack(string resxPath, DirectoryInfo directory)
    {
        AddDirectoryToPack(resxPath, directory.FullName);
    }
   
    #endregion
    
    #region Extract Resouce File
    public static void ExtractPack(string resxPath, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        using var reader = new ResourceReader(resxPath);
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            string filePath = Path.Combine(outputDirectory, fileName);
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null) continue;
            Directory.CreateDirectory(directoryPath);
            if(entry.Value is byte[] bytes) File.WriteAllBytes(filePath, bytes);
        }
    }


    
    #endregion
    
    #endregion
}