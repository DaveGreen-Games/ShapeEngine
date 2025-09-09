
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
        
        
        if (args.Length <= 0)
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
                                "  ./ResourcePacker --help\n" +
                                "  ./ResourcePacker packDir <sourceDirectory> <targetResourceFile>\n" +
                                "  ./ResourcePacker packFile <sourceFile> <targetResourceFile>\n" +
                                "  ./ResourcePacker addFile <sourceFile> <targetResourceFile>\n" +
                                "  ./ResourcePacker addDir <sourceDirectory> <targetResourceFile>\n" +
                                "  ./ResourcePacker unpack <resourceFile> <outputDirectory>\n" +
                                "  ./ResourcePacker packDirText <sourceDirectory> <targetTextFile>\n" +
                            "Flags:\n" +
                                "  --exceptions <.ext1> <.ext2> ... : Optional flag to specify file extensions to exclude when packing directories.\n" +
                            "Examples:\n" +
                                "  ./ResourcePacker packDir ./backgroundMusic ./backgroundMusic.resource\n" +
                                "  ./ResourcePacker packFile ./logo.png ./output.resource\n" +
                                "  ./ResourcePacker packDireText ./assets ./packedResources.txt\n" +
                                "  ./ResourcePacker packDir ./assets ./build/release/packedAssets.resource --exceptions .txt .png .wav\n"
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
        
        int exceptionsIndex = Array.IndexOf(args, "--exceptions");
        List<string>? extensionExceptions = null;
        if (exceptionsIndex > 2)
        {
            extensionExceptions = [];
            for (int i = exceptionsIndex + 1; i < args.Length; i++)
            {
                extensionExceptions.Add(args[i]);
            }
        }
        
        var command = args[0].ToLower();

        if (command == "packdir")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                if (ResourcePackManager.CreateResourcePackFromDirectory(targetFile, sourceDir, extensionExceptions))
                {
                    Console.WriteLine($"Packed '{sourceDir}' to '{targetFile}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Packing '{sourceDir}' to '{targetFile}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Packing directory '{sourceDir}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "packfile")
        {
            var sourceFile = args[1];
            var targetFile = args[2];
            try
            {
                if (ResourcePackManager.CreateResourcePackFromFile(targetFile, sourceFile))
                {
                    Console.WriteLine($"Packed '{sourceFile}' to '{targetFile}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Packing '{sourceFile}' to '{targetFile}'!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Packing file '{sourceFile}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "addfile")
        {
            var sourceFile = args[1];
            var targetFile = args[2];
            try
            {
                if (ResourcePackManager.AddFileToPack(targetFile, sourceFile))
                {
                    Console.WriteLine($"Added '{sourceFile}' to '{targetFile}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Adding '{sourceFile}' to '{targetFile}'!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Adding file '{sourceFile}' to '{targetFile}': {ex.Message}");
            }
            
        }
        else if (command == "adddir")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                if (ResourcePackManager.AddDirectoryToPack(targetFile, sourceDir, extensionExceptions))
                {
                    Console.WriteLine($"Added directory '{sourceDir}' to '{targetFile}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Adding directory '{sourceDir}' to '{targetFile}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Adding directory '{sourceDir}' to '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "packdirtext")
        {
            var sourceDir = args[1];
            var targetFile = args[2];
            try
            {
                if (ResourcePackManager.PackToText(targetFile, sourceDir, extensionExceptions))
                {
                    Console.WriteLine($"Packed '{sourceDir}' to text file '{targetFile}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Packing '{sourceDir}' to text file '{targetFile}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Packing directory '{sourceDir}' to text file '{targetFile}': {ex.Message}");
            }
        }
        else if (command == "unpack")
        {
            var resxFile = args[1];
            var outputDir = args[2];

            try
            {
                if (ResourcePackManager.Unpack(outputDir, resxFile))
                {
                    Console.WriteLine($"Unpacked '{resxFile}' to '{outputDir}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Unpacking '{resxFile}' to '{outputDir}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unpacking resource pack {resxFile} to {outputDir} with exception: {ex.Message}");
            }
        }
        else if (command == "unpackdebug")
        {
            var resxFile = args[1];
            var outputDir = args[2];

            try
            {
                if (ResourcePackManager.UnpackDebug(outputDir, resxFile))
                {
                    Console.WriteLine($"Unpacked debug '{resxFile}' to '{outputDir}'.");
                }
                else
                {
                    Console.WriteLine($"Error: Unpacking debug '{resxFile}' to '{outputDir}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unpacking debug resource pack {resxFile} to {outputDir} with exception: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}. Use '--help' for usage instructions.");
        }
    }
}