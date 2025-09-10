
namespace ResourcePacker;

//TODO: make final AI check for issues

class Program
{
    /// <summary>
    /// Commands:
    /// <list type="bullet">
    /// <item>--help</item>
    /// <item>resourcePacker pack sourceDirectoryPath outputFilePath</item>
    /// <item>resourcePacker unpack sourceFilePath outputDirectoryPath</item>
    /// </list>
    /// Flags:
    /// <list type="bullet">
    /// <item>--debug</item>
    /// <item>--extensions .txt .json .xml and so on</item>
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
                          " - unpack packed binary files into directories.\n" +
                          " - pack directories into text files with each file taking up 2 lines:\n" +
                          "  - 1st line = filename.\n" +
                          "  - 2nd line =  binary data of the file compressed to a string.\n" +
                          " - unpack text files into directories.\n" +
                          " - resource packs are always compressed.\n");
        Console.WriteLine("Use '--help' for usage instructions.");
        Console.WriteLine("Remarks:\n" +
                          " - ResourcePacker does not modify or delete any source files or directories.\n" +
                          " - Using .txt extension for the sourceFilePath in unpack or the outputFilePath uses the text based packing system. Any other extensions will use the binary packing system.\n" +
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
                                "  ./ResourcePacker pack <sourceDirectoryPath> <outputFilePath>\n" +
                                "  ./ResourcePacker unpack <sourceFilePath> <outputDirectoryPath>\n" +
                            "Flags:\n" +
                                "  --exceptions <.ext1> <.ext2> ... : Optional flag to specify file extensions to exclude when packing/unpacking directories.\n" +
                                "  --debug : Optional flag to enable debug mode for detailed logging during operations.\n" +
                            "Examples:\n" +
                                "  ./ResourcePacker pack ./backgroundMusic ./packedMusic.res\n" +
                                "  ./ResourcePacker pack ./assets ./packedResources.txt\n" +
                                "  ./ResourcePacker pack ./assets ./build/release/packedAssets.res --exceptions .txt .png .wav --debug\n"
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

        bool isDebug = false;
        int debugIndex = Array.IndexOf(args, "--debug");
        if(debugIndex > 2) isDebug = true;
        
        int exceptionsIndex = Array.IndexOf(args, "--exceptions");
        List<string>? extensionExceptions = null;
        if (exceptionsIndex > 2)
        {
            extensionExceptions = [];
            for (int i = exceptionsIndex + 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.StartsWith('.')) break;
                extensionExceptions.Add(args[i]);
            }
        }
        
        var command = args[0].ToLower();

        if (command == "pack")
        {
            var sourceDirectoryPath = args[1];
            var outputFilePath = args[2];
            try
            {
                if (!ResourcePackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, isDebug))
                {
                    Console.WriteLine($"Error: Packing '{sourceDirectoryPath}' to '{outputFilePath}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Packing directory '{sourceDirectoryPath}' to '{outputFilePath}': {ex.Message}");
            }
        }
        else if (command == "unpack")
        {
            var sourceFilePath = args[1];
            var outputDirectoryPath = args[2];

            try
            {
                if (!ResourcePackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, isDebug))
                {
                    Console.WriteLine($"Error: Unpacking '{sourceFilePath}' to '{outputDirectoryPath}' failed!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unpacking resource pack {sourceFilePath} to {outputDirectoryPath} with exception: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}. Use '--help' for usage instructions.");
        }
    }
}