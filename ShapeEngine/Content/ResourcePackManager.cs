using System.Collections;
using System.Resources;

namespace ShapeEngine.Content;

public class ResourcePackManager
{
    //todo: add to overloads with Directory directory instead of string path
    
    //creates a new .resource file or overwrites an existing one
    public void CreateResourcePackFromFile(string resxPath, string filePath, string? resourceName = null)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir != null && !Directory.Exists(resxDir)) throw new DirectoryNotFoundException($"Directory not found: {resxDir}");
        if (Path.GetExtension(resxPath) != ".resource") throw new ArgumentException($"Path does not point to a .resource file: {resxPath}");

        resourceName ??= Path.GetFileNameWithoutExtension(filePath);
        using var writer = new ResourceWriter(resxPath);
        writer.AddResource(resourceName, File.ReadAllBytes(filePath));
        writer.Generate();
    }
    //creates a new .resource file or overwrites an existing one
    public void CreateResourcePackFromDirectory(string resxPath, string directoryPath)
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
    
    public void AddFileToPack(string resxPath, string filePath, string? resourceName = null)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");
        
        var resxDir = Path.GetDirectoryName(resxPath);
        if (resxDir != null && !Directory.Exists(resxDir)) throw new DirectoryNotFoundException($"Directory not found: {resxDir}");
        if (Path.GetExtension(resxPath) != ".resource") throw new ArgumentException($"Path does not point to a .resource file: {resxPath}");

        resourceName ??= Path.GetFileNameWithoutExtension(filePath);

        // Read existing resources
        var resources = new Dictionary<string, byte[]>();
        if (File.Exists(resxPath))
        {
            using var reader = new ResourceReader(resxPath);
            foreach (DictionaryEntry entry in reader)
            {
                resources[entry.Key.ToString() ?? ""] = (byte[])entry.Value;
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

    public void AddDirectoryToPack(string resxPath, string directoryPath)
    {
        // Read existing resources
        var resources = new Dictionary<string, byte[]>();
        if (File.Exists(resxPath))
        {
            using var reader = new ResourceReader(resxPath);
            foreach (DictionaryEntry entry in reader)
            {
                resources[entry.Key.ToString() ?? ""] = (byte[])entry.Value;
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
    
    public void ExtractPack(string resxPath, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        using var reader = new ResourceReader(resxPath);
        foreach (DictionaryEntry entry in reader)
        {
            var filePath = Path.Combine(outputDirectory, entry.Key.ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllBytes(filePath, (byte[])entry.Value);
        }
    }

}