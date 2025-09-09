
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
    /// <item>resourcePacker unpack resourceFile outputDirectory</item>
    /// <item>resourcePacker packDirText sourceDirectory targetTextFile</item>
    /// </list>
    /// </summary>
    /// <param name="args">command line arguments</param>
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to ResourcePacker v1.0.0");
        Console.WriteLine("ResourcePacker is licensed under the MIT License.");
        Console.WriteLine("Copyright (c) 2025 David Grueneis");
        Console.WriteLine("Source code available at: https://github.com/DaveGreen-Games/ShapeEngine");
        Console.WriteLine("Use at your own risk. No warranties provided.");
        Console.WriteLine("ResourcePacker can:\n" +
                          " - pack directories or single files into binary files.\n" +
                          " - add files/directories to existing packed binary files.\n" +
                          " - unpack packed binary files into directories.\n" +
                          " - pack directories into text files with each file taking up 2 lines:\n" +
                          "  - 1st line = filename.\n" +
                          "  - 2nd line =  binary data of the file compressed to a string.\n");
        Console.WriteLine("Use '--help' for usage instructions.");
        Console.WriteLine("Remarks:\n" +
                          " - ResourcePacker does not modify or delete any source files or directories.\n" +
                          " - The <packDirText> command always expects a .txt target file extension.\n" +
                          " - The other pack commands do not care about the target file extension but .resources is a good default.\n" +
                          " - Ensure you have the necessary permissions for file operations in the specified directories.");
        Console.WriteLine("------------------------------------------------------------");
        
        
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
                                "  resourcepacker unpack <resourceFile> <outputDirectory>\n" +
                                "  resourcepacker packDirText <sourceDirectory> <targetTextFile>\n" +
                            "Examples:\n" +
                                "  resourcepacker packDir ./backgroundMusic ./backgroundMusic.resource\n" +
                                "  resourcepacker packFile ./logo.png ./output.resource\n" +
                                "  resourcepacker packDireText ./assets ./packedResources.txt\n"
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
        else if (command == "packdirtext")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                ResourcePackManager.PackToText(sourceDir, targetFile);
                Console.WriteLine($"Packed '{sourceDir}' to text file '{targetFile}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error packing directory '{sourceDir}' to text file '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "unpack")
        {
            
            var resxFile = args[1];
            var outputDir = args[2];

            try
            {
                ResourcePackManager.Unpack(resxFile, outputDir);
                Console.WriteLine($"Unpacked '{resxFile}' to '{outputDir}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unpacking resource pack {resxFile} to {outputDir} with exception: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}. Use '--help' for usage instructions.");
        }
    }
}