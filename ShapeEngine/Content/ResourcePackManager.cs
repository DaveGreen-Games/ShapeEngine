using System.Collections;
using System.Resources;

namespace ShapeEngine.Content;

public static class ResourcePackManager
{
    
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

    public static Dictionary<string, ContentInfo> ExtractPack(string resxPath)
    {
        var contentInfos = new Dictionary<string, ContentInfo>();
        if (!File.Exists(resxPath)) return contentInfos;
        
        using var reader = new ResourceReader(resxPath);
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key?.ToString();
            if (fileName == null) continue;
            if (entry.Value is byte[] bytes)
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                contentInfos[name] = new ContentInfo(fileName, bytes);
            }
        }
        return contentInfos;
    }

}