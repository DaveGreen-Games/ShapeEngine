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
                ResourcePackManager.CreateResourcePackFromDirectory(targetFile, sourceDir);
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
                ResourcePackManager.CreateResourcePackFromFile(targetFile, sourceFile);
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
                ResourcePackManager.AddFileToPack(targetFile, sourceFile);
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
                ResourcePackManager.AddDirectoryToPack(targetFile, sourceDir);
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
                ResourcePackManager.ExtractPack(resxFile, outputDir);
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
}