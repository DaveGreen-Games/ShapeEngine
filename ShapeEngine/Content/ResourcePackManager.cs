using System.Collections;
using System.Resources;

namespace ShapeEngine.Content;

public static class ResourcePackManager
{
    
    #region Create Resource File

    public static void CreateResourcePackFromFile(string resxPath, string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir != null && !Directory.Exists(resxDir)) throw new DirectoryNotFoundException($"Directory not found: {resxDir}");
        if (Path.GetExtension(resxPath) != ".resource") throw new ArgumentException($"Path does not point to a .resource file: {resxPath}");

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
        if (!Directory.Exists(directoryPath)) throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
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
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir != null && !Directory.Exists(resxDir)) throw new DirectoryNotFoundException($"Directory not found: {resxDir}");
        if (Path.GetExtension(resxPath) != ".resource") throw new ArgumentException($"Path does not point to a .resource file: {resxPath}");

        var resourceName = Path.GetFileName(filePath);

        // Read existing resources
        var resources = new Dictionary<string, byte[]>();
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
        // Add/replace the new resource
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
        // Read existing resources
        var resources = new Dictionary<string, byte[]>();
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
        Directory.CreateDirectory(outputDirectory);
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