using System.Collections;
using System.Resources;

namespace ResourcePacker;

class Program
{
    /// <summary>
    /// Commands:
    /// <list type="bullet">
    /// <item>--help</item>
    /// <item>resourcePacker packDir sourceDirectory targetResourceFile</item>
    /// <item>resourcePacker packFile sourceFile targetResourceFile</item>
    /// <item>resourcePacker addFile sourceFile targetResourceFile</item>
    /// <item>resourcePacker addDir sourceDirectory targetResourceFile</item>
    /// <item>resourcePacker extract resourceFile outputDirectory</item>
    /// </list>
    /// </summary>
    /// <param name="args">command line arguments</param>
    static void Main(string[] args)
    {
        Console.WriteLine("ResourcePacker: Packs and manages .resource files.\nUse '--help' for usage instructions.");
        
        if (args.Length is <= 0 or > 3)
        {
            Console.WriteLine("Invalid number of arguments. Use '--help' for usage instructions.");
            return;
        }
        
        if (args.Length == 1)
        {
            if(args[0].Equals("--help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                            "Commands:\n" +
                                "  resourcepacker --help\n" +
                                "  resourcepacker packDir <sourceDirectory> <targetResourceFile>\n" +
                                "  resourcepacker packFile <sourceFile> <targetResourceFile>\n" +
                                "  resourcepacker addFile <sourceFile> <targetResourceFile>\n" +
                                "  resourcepacker addDir <sourceDirectory> <targetResourceFile>\n" +
                                "  resourcepacker extract <resourceFile> <outputDirectory>\n" +
                            "Examples:\n" +
                                "  resourcepacker packDir ./backgroundMusic ./backgroundMusic.resource\n" +
                                "  resourcepacker packFile ./logo.png ./output.resource\n"
                            );
            }
            else
            {
                Console.WriteLine("Invalid argument. Use '--help' for usage instructions.");
            }
            return;
        }

        if (args.Length == 2)
        {
            Console.WriteLine("Wrong number of arguments. Use '--help' for usage instructions.");
            return;
        }
        
        var command = args[0].ToLower();

        if (command == "packdir")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                CreateResourcePackFromDirectory(targetFile, sourceDir);
                Console.WriteLine($"Packed '{sourceDir}' to '{targetFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error packing directory '{sourceDir}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "packfile")
        {
            var sourceFile = args[1];
            var targetFile = args[2];
            try
            {
                CreateResourcePackFromFile(targetFile, sourceFile);
                Console.WriteLine($"Packed '{sourceFile}' to '{targetFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error packing file '{sourceFile}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "addfile")
        {
            var sourceFile = args[1];
            var targetFile = args[2];
            try
            {
                AddFileToPack(targetFile, sourceFile);
                Console.WriteLine($"Added '{sourceFile}' to '{targetFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding file '{sourceFile}' to '{targetFile}': {ex.Message}");
            }
            
        }
        else if (command == "adddir")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                AddDirectoryToPack(targetFile, sourceDir);
                Console.WriteLine($"Added directory '{sourceDir}' to '{targetFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding directory '{sourceDir}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "extract")
        {
            
            var resxFile = args[1];
            var outputDir = args[2];

            try
            {
                ExtractPack(resxFile, outputDir);
                Console.WriteLine($"Extracted '{resxFile}' to '{outputDir}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting resource pack {resxFile} to {outputDir} with exception: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}. Use '--help' for usage instructions.");
        }
    }
    
    
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
}