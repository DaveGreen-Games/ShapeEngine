namespace ResourcePacker;

//TODO: Implement double clicking the exe and writing commands in terminal window (see github copilot suggestions)

class Program
{
    /// <summary>
    /// Commands:
    /// <list type="bullet">
    /// <item>help</item>
    /// <item>pack sourceDirectoryPath outputFilePath</item>
    /// <item>unpack sourceFilePath outputDirectoryPath</item>
    /// </list>
    /// Flags:
    /// <list type="bullet">
    /// <item>--debug</item>
    /// <item>--exceptions .txt .json .xml and so on</item>
    /// <item>--parallel for parallel processing</item>
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
                          " - pack directories into binary files using gzip with this layout:\n" +
                          "  - Total Files packed (4 bytes).\n" +
                          "  - Total Bytes packed (8 bytes). Used for offset to skip data block.\n" +
                          "  - Data block with [path length][path][data length][data] layout. path length & data length have 4 bytes.\n" +
                          "  - Index with [path length][path][data length] layout used for fast random access.\n" +
                          " - unpack packed binary files into directories.\n" +
                          " - pack directories into text files with this layout:\n" +
                          "  - Data block with each resource file taking up 2 lines:\n" +
                          "   - 1st line = filename.\n" +
                          "   - 2nd line = binary data of the file compressed to a string.\n" +
                          "  - Empty Line.\n" +
                          "  - Number of packed files.\n" +
                          " - unpack text files into directories.\n");
        Console.WriteLine("Use 'help' for usage instructions.");
        Console.WriteLine("Open a terminal at the folder containing ResourcePacker.exe and run commands starting with './ResourcePacker'.");
        Console.WriteLine("Remarks:\n" +
                          " - ResourcePacker does not modify or delete any source files or directories.\n" +
                          " - Using .txt extension for the sourceFilePath in unpack or the outputFilePath in pack uses the text based packing system. Any other extensions will use the binary packing system.\n" +
                          " - Ensure you have the necessary permissions for file operations in the specified directories.");
        Console.WriteLine("------------------------------------------------------------");
        
        if (args.Length == 0)
        {
            var commandFinished = false;
            while (!commandFinished)
            {
                Console.WriteLine("Please enter a command (help, pack, unpack) or type 'exit' to quit:");
                Console.Write("> ");
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    args = input.Split(' ');

                    if (args.Length > 0)
                    {
                        var exitIndex = Array.IndexOf(args, "exit");
                        if (exitIndex >= 0)
                        {
                            commandFinished = true;
                            args = []; //to avoid processing arguments after exit
                            Console.WriteLine("Exiting ResourcePacker. Goodbye!");
                        }
                        else
                        {
                            var helpIndex = Array.IndexOf(args, "help");
                            if (helpIndex >= 0)
                            {
                                PrintHelp();
                            }
                            else
                            {
                                if (args.Length >= 3)
                                {
                                    var success = ProcessArguments(args);
                                    if (success)
                                    {
                                        commandFinished = true;
                                        args = []; //to avoid processing arguments again
                                        Console.WriteLine("Command executed successfully. Exiting ResourcePacker. Goodbye!");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Processing arguments {args} failed. Please try again or type 'exit' to quit.");
                                    }
                                }
                                else
                                {
                                    var packIndex = Array.IndexOf(args, "pack");
                                    if (packIndex >= 0)
                                    {
                                        args = ConstructCommand(true);
                                        if(args.Length <= 0) continue;
                                        var success = ProcessArguments(args);
                                        if (success)
                                        {
                                            commandFinished = true;
                                            args = []; //to avoid processing arguments again
                                            Console.WriteLine("Command executed successfully. Exiting ResourcePacker. Goodbye!");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Processing arguments {args} failed. Please try again or type 'exit' to quit.");
                                        }

                                        continue;
                                    }
                                    var unpackIndex = Array.IndexOf(args, "unpack");
                                    if (unpackIndex >= 0)
                                    {
                                        args = ConstructCommand(false);
                                        if (args.Length <= 0) continue;
                                        var success = ProcessArguments(args);
                                        if (success)
                                        {
                                            commandFinished = true;
                                            args = []; //to avoid processing arguments again
                                            Console.WriteLine("Command executed successfully. Exiting ResourcePacker. Goodbye!");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Processing arguments {args} failed. Please try again or type 'exit' to quit.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No command entered. Please try again.");
                }
            }
        }

        if (args.Length > 0)
        { 
            ProcessArguments(args);
        }
    }

    private static string[] ConstructCommand(bool pack)
    {
        if (pack)
        {
            List<string> commandParts = ["pack"];
            bool commandFinished = false;
            
            Console.WriteLine("Starting packing command construction.");
            
            while (!commandFinished)
            {
                if (commandParts.Count == 1)
                {
                    Console.WriteLine($"Current command entry: {string.Join(' ', commandParts)}");
                    Console.WriteLine("write 'exit', 'quit', 'close' or 'done' to stop command construction.");
                    Console.WriteLine("Please enter the source directory path to pack:");
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if(input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("quit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("close", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("done", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting command construction.");
                            return [];
                        }
                        
                        if (Path.HasExtension(input))
                        {
                            Console.WriteLine("Source directory path should not have a file extension. Please try again.");
                        }
                        else if(Directory.Exists(input))
                        {
                            commandParts.Add(input);
                            Console.WriteLine($"Source directory path added to command: {input}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid source directory path. Please try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No input detected. Please try again.");
                    }
                }
                else if (commandParts.Count == 2)
                {
                    Console.WriteLine($"Current command entry: {string.Join(' ', commandParts)}");
                    Console.WriteLine("write 'exit', 'quit', 'close' or 'done' to stop command construction.");
                    Console.WriteLine("Please enter the output file path:");
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if(input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("quit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("close", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("done", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting command construction.");
                            return [];
                        }
                        
                        if (Path.HasExtension(input))
                        {
                            commandParts.Add(input);
                            Console.WriteLine($"Output file path added to command: {input}");
                            commandFinished = true;
                        }
                        else
                        {
                            Console.WriteLine("Output file path should have a file extension. Please try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No input detected. Please try again.");
                    }
                }
                else
                {
                    //TODO: add possibility to add flags like --debug, --exceptions .ext1 .ext2 and so on
                }
            }
            return commandParts.ToArray();
        }
        else
        {
            List<string> commandParts = ["unpack"];
            bool commandFinished = false;
            
            Console.WriteLine("Starting unpacking command construction.");
            
            while (!commandFinished)
            {
                if (commandParts.Count == 1)
                {
                    Console.WriteLine($"Current command entry: {string.Join(' ', commandParts)}");
                    Console.WriteLine("write 'exit', 'quit', 'close' or 'done' to stop command construction.");
                    Console.WriteLine("Please enter the source file path to unpack:");
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if(input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("quit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("close", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("done", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting command construction.");
                            return [];
                        }
                        
                        if (Path.HasExtension(input))
                        {
                            commandParts.Add(input);
                            Console.WriteLine($"Source file path added to command: {input}");
                            commandFinished = true;
                        }
                        else
                        {
                            Console.WriteLine("Source file path should have a file extension. Please try again.");
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("No input detected. Please try again.");
                    }
                }
                else if (commandParts.Count == 2)
                {
                    Console.WriteLine($"Current command entry: {string.Join(' ', commandParts)}");
                    Console.WriteLine("write 'exit', 'quit', 'close' or 'done' to stop command construction.");
                    Console.WriteLine("Please enter the output directory  path:");
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if(input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("quit", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("close", StringComparison.OrdinalIgnoreCase) || 
                           input.Equals("done", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting command construction.");
                            return [];
                        }
                        
                        if (Path.HasExtension(input))
                        {
                            Console.WriteLine("Output directory path should not have a file extension. Please try again.");
                        }
                        else if(Directory.Exists(input))
                        {
                            commandParts.Add(input);
                            Console.WriteLine($"Output directory path added to command: {input}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid output directory path. Please try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No input detected. Please try again.");
                    }
                }
                else
                {
                    //TODO: add possibility to add flags like --debug, --exceptions .ext1 .ext2 and so on
                }
            }
            return commandParts.ToArray();
        }
        
    }
    private static void PrintHelp()
    {
        Console.WriteLine(
            "Commands:\n" +
            "  ./ResourcePacker --help\n" +
            "  ./ResourcePacker pack <sourceDirectoryPath> <outputFilePath>\n" +
            "  ./ResourcePacker unpack <sourceFilePath> <outputDirectoryPath>\n" +
            "Flags:\n" +
            "  --exceptions <.ext1> <.ext2> ... : Optional flag to specify file extensions to exclude when packing/unpacking directories.\n" +
            "  --debug : Optional flag to enable debug mode for detailed logging during operations.\n" +
            "  --parallel : Optional flag to enable parallel packing/unpacking possible increasing speed at the cost of increase memory usage.\n" +
            "Examples:\n" +
            "  ./ResourcePacker pack ./backgroundMusic ./packedMusic.res\n" +
            "  ./ResourcePacker pack ./assets ./packedResources.txt\n" +
            "  ./ResourcePacker pack ./assets ./build/release/packedAssets.res --exceptions .txt .png .wav --debug\n"
        );
    }
    private static bool ProcessArguments(string[] args)
    {
        if (args.Length <= 0)
        {
            Console.WriteLine("No command entered. Please try again.");
            return false;
        }
        
        if (args.Length == 1)
        {
            if(args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
            }
            else
            {
                Console.WriteLine("Invalid argument. Use 'help' for usage instructions.");
            }
            return false;
        }

        if (args.Length == 2)
        {
            Console.WriteLine("Wrong number of arguments. Use 'help' for usage instructions.");
            return false;
        }
        
        var command = args[0].ToLower();
        if (!command.Equals("pack", StringComparison.OrdinalIgnoreCase) && !command.Equals("unpack", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Wrong command entered: {command}. Use 'help' for usage instructions.");
            return false;
        }
        
        bool isDebug = false;
        int debugIndex = Array.IndexOf(args, "--debug");
        if(debugIndex > 0) isDebug = true;
        
        bool isParallel = false;
        int parallelIndex = Array.IndexOf(args, "--parallel");
        if(parallelIndex > 0) isParallel = true;
        
        int exceptionsIndex = Array.IndexOf(args, "--exceptions");
        List<string>? extensionExceptions = null;
        if (exceptionsIndex > 0)
        {
            extensionExceptions = [];
            for (int i = exceptionsIndex + 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.StartsWith('.')) break;
                extensionExceptions.Add(args[i]);
            }
        }
        
        if (command == "pack")
        {
            var sourceDirectoryPath = args[1];
            var outputFilePath = args[2];
            try
            {
                if (!ResourcePackManager.Pack(outputFilePath, sourceDirectoryPath, extensionExceptions, isParallel, isDebug))
                {
                    Console.WriteLine($"Error: Packing '{sourceDirectoryPath}' to '{outputFilePath}' failed!");
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Packing directory '{sourceDirectoryPath}' to '{outputFilePath}': {ex.Message}");
                return false;
            }
        }
        
        if (command == "unpack")
        {
            var sourceFilePath = args[1];
            var outputDirectoryPath = args[2];

            try
            {
                if (!ResourcePackManager.Unpack(outputDirectoryPath, sourceFilePath, extensionExceptions, isParallel, isDebug))
                {
                    Console.WriteLine($"Error: Unpacking '{sourceFilePath}' to '{outputDirectoryPath}' failed!");
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unpacking resource pack {sourceFilePath} to {outputDirectoryPath} with exception: {ex.Message}");
                return false;
            }
        }
        
        Console.WriteLine($"Unknown command: {command}. Use 'help' for usage instructions.");
        return false;
    }
}