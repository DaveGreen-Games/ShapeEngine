
using System.Text;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides static methods for saving and loading text and data files, as well as accessing common application directories.
/// </summary>
public static class ShapeFileManager
{
    #region Paths
    /// <summary>
    /// Gets the path to the user's application data folder.
    /// </summary>
    public static string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    /// <summary>
    /// Gets the path to the local (common) application data folder.
    /// </summary>
    public static string ApplicationLocalDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    /// <summary>
    /// Gets the path to the common application data folder.
    /// </summary>
    public static string ApplicationCommonDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    /// <summary>
    /// Gets the path to the user's documents folder.
    /// </summary>
    public static string ApplicationDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    /// <summary>
    /// Gets the path to the common documents folder.
    /// </summary>
    public static string ApplicationCommonDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
    /// <summary>
    /// Gets the path to a special system folder.
    /// </summary>
    /// <param name="folder">The special folder to retrieve the path for.</param>
    /// <returns>The absolute path to the special folder.</returns>
    public static string GetSpecialFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
    /// <summary>
    /// Combines multiple path segments into a single path.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>The combined path.</returns>
    public static string CombinePath(params string[] paths) => Path.Combine(paths);
    /// <summary>
    /// Combines two path segments into a single path.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>The combined path.</returns>
    public static string CombinePath(string path1, string path2) => Path.Combine(path1, path2);

    #endregion

    #region Files
    /// <summary>
    /// Creates a file at the specified path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to create.</param>
    /// <param name="overrideIfExists">If true and the file exists, it will be deleted and recreated (WARNING: all contents will be lost).</param>
    /// <returns>The FileInfo of the created or existing file, or null if the operation failed.</returns>
    public static FileInfo? CreateFile(string absolutePath, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath)) return null;

        try
        {
            if (!File.Exists(absolutePath))
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? string.Empty);
                // Create file (will be empty)
                using (File.Create(absolutePath)) { }
                return new FileInfo(absolutePath);
            }

            if (!overrideIfExists) return new FileInfo(absolutePath);

            File.Delete(absolutePath);
            using (File.Create(absolutePath)) { }
            return new FileInfo(absolutePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create file at {absolutePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a file at the specified path and writes the provided text content to it.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the file to create.</param>
    /// <param name="text">The text content to write to the file.</param>
    /// <param name="overrideIfExists">If true and the file exists, it will be deleted and recreated with the new content.</param>
    /// <returns>The FileInfo of the created or existing file, or null if the operation failed.</returns>
    public static FileInfo? CreateFile(string absolutePath, string text, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath)) return null;

        try
        {
            if (!File.Exists(absolutePath))
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? string.Empty);
                // Create file with content
                File.WriteAllText(absolutePath, text);
                return new FileInfo(absolutePath);
            }

            if (!overrideIfExists) return new FileInfo(absolutePath);

            // Override existing file with new content
            File.WriteAllText(absolutePath, text);
            return new FileInfo(absolutePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create file at {absolutePath}: {ex.Message}");
            return null;
        }
    }
    
    
    
    /// <summary>
    /// Creates a file within the specified directory.
    /// </summary>
    /// <param name="directory">The directory in which to create the file.</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="overrideIfExists">If true and the file exists, it will be deleted and recreated (WARNING: all contents will be lost).</param>
    /// <returns>The FileInfo of the created or existing file, or null if the operation failed.</returns>
    public static FileInfo? CreateFile(DirectoryInfo directory, string fileName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        
        string absolutePath = Path.Combine(directory.FullName, fileName);
        return CreateFile(absolutePath, overrideIfExists);
    }
    
    /// <summary>
    /// Creates a file within the specified directory and writes the provided text content to it.
    /// </summary>
    /// <param name="directory">The directory in which to create the file.</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="text">The text content to write to the file.</param>
    /// <param name="overrideIfExists">If true and the file exists, it will be deleted and recreated with the new content.</param>
    /// <returns>The FileInfo of the created or existing file, or null if the operation failed.</returns>
    public static FileInfo? CreateFile(DirectoryInfo directory, string fileName, string text, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        
        string absolutePath = Path.Combine(directory.FullName, fileName);
        return CreateFile(absolutePath, text, overrideIfExists);
    }
    
    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public static bool FileExists(string absolutePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || string.IsNullOrWhiteSpace(fileName)) return false;
        
        string path = Path.Combine(absolutePath, fileName);
        return File.Exists(path);
    }
    
    #endregion
    
    #region Directories
    /// <summary>
    /// Creates a directory within the user's application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateApplicationDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(ApplicationDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory within the local application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateApplicationLocalDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(ApplicationLocalDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory within the common application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateApplicationCommonDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(ApplicationCommonDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory within the user's documents folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateApplicationDocumentsDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(ApplicationDocumentsPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory within the common documents folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateApplicationCommonDocumentsDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(ApplicationCommonDocumentsPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory within the specified special system folder.
    /// </summary>
    /// <param name="folder">The special system folder in which to create the directory.</param>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">
    /// If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).
    /// </param>
    /// <returns>
    /// A DirectoryInfo object representing the created or existing directory, or null if the operation failed.
    /// </returns>
    public static DirectoryInfo? CreateSpecialFolderDirectory(Environment.SpecialFolder folder, string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return null;
        string absolutePath = CombinePath(GetSpecialFolderPath(folder), directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    
    /// <summary>
    /// Gets a DirectoryInfo object for the specified path if the directory exists.
    /// </summary>
    /// <param name="absolutePath">The absolute path to the directory.</param>
    /// <returns>
    /// A DirectoryInfo object representing the directory at the specified path if it exists;
    /// otherwise, null.
    /// Returns null if the path is invalid or an error occurs.
    /// </returns>
    public static DirectoryInfo? GetDirectoryInfo(string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath)) return null;
        
        try
        {
            return Directory.Exists(absolutePath) ? new DirectoryInfo(absolutePath) : null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to get directory info for {absolutePath}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the directory to create.</param>
    /// <param name="overrideIfExists">If true and the directory exists, it will be deleted and recreated (WARNING: all contents will be lost).</param>
    /// <returns>The DirectoryInfo of the created or existing directory, or null if the operation failed.</returns>
    public static DirectoryInfo? CreateDirectory(string absolutePath, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath)) return null;
    
        try
        {
            if (!Directory.Exists(absolutePath)) return Directory.CreateDirectory(absolutePath);
            
            if (!overrideIfExists) return new DirectoryInfo(absolutePath);
        
            Directory.Delete(absolutePath, true);
            return Directory.CreateDirectory(absolutePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create directory at {absolutePath}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Checks if the specified directory exists.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public static bool DirectoryExists(string absolutePath) => Directory.Exists(absolutePath);
    #endregion

    #region Load/Process Directory
    
    /// <summary>
    /// Retrieves all file names in the specified directory matching the search pattern.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <returns>
    /// A list of file names matching the search pattern, or an empty list if the path is invalid or does not exist.
    /// </returns>
    public static List<string> GetAllFileNames(string absolutePath, string searchPattern = "*", bool recursive = false)
    {
       if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath))
           return [];

       var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
       
       try
       {
           return Directory.GetFiles(absolutePath, searchPattern, searchOption).ToList();
       }
       catch (Exception ex)
       {
           Console.Error.WriteLine($"[{ex.GetType().Name}] {absolutePath}: {ex.Message}");
           return [];
       }
    }
    
    /// <summary>
    /// Retrieves the contents of all files in the specified directory matching the search pattern.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <param name="encoding">The encoding to use when reading files (null uses the default encoding).</param>
    /// <returns>
    /// A list of file contents matching the search pattern, or an empty list if the path is invalid,
    /// does not exist, or contains no matching files.
    /// </returns>
    public static List<string> LoadDirectory(string absolutePath, string searchPattern = "*", bool recursive = false, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath))
            return new List<string>();

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(absolutePath, searchPattern, searchOption);
        if (files.Length <= 0) return new List<string>();
        
        var contents = new List<string>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using (var reader = new StreamReader(file, encoding ?? Encoding.Default))
                {
                    contents.Add(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file}: {ex.Message}");
                contents.Add(string.Empty);
            }
        }

        return contents;
    }
    /// <summary>
    /// Retrieves the contents of all files in the specified directory with their file paths.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <param name="encoding">The encoding to use when reading files (null uses the default encoding).</param>
    /// <returns>
    /// A dictionary mapping file paths to their contents, or an empty dictionary if the path is invalid,
    /// does not exist, or contains no matching files.
    /// </returns>
    public static Dictionary<string, string> LoadDirectoryWithPaths(string absolutePath, string searchPattern = "*", bool recursive = false, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath))
            return new Dictionary<string, string>();

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(absolutePath, searchPattern, searchOption);
        if (files.Length <= 0) return new Dictionary<string, string>();
        
        var contents = new Dictionary<string, string>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using (var reader = new StreamReader(file, encoding ?? Encoding.Default))
                {
                    contents[file] = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file}: {ex.Message}");
                contents[file] = string.Empty;
            }
        }

        return contents;
    }
    
    /// <summary>
    /// Processes all files in the specified directory using a custom processor function, suitable for large files.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the processor function.</typeparam>
    /// <param name="absolutePath">The absolute path of the directory to search.</param>
    /// <param name="processor">A function that processes each file stream and returns a result.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <returns>
    /// A list of results from processing each file, or an empty list if the path is invalid or does not exist.
    /// </returns>
    /// <example>
    /// <code>
    /// //Option 1 Using a separate method
    /// string ReadStreamToString(Stream stream)
    /// {
    ///    using var reader = new StreamReader(stream);
    ///    return reader.ReadToEnd();
    /// }
    /// var fileContents = ProcessAllFiles&lt;string&gt;(absolutePath, ReadStreamToString);
    ///
    /// //Option 2 Using a lambda function directly
    /// var fileContents = ProcessAllFiles&lt;string&gt;(absolutePath, stream =>
    /// {
    ///    using var reader = new StreamReader(stream);
    ///    return reader.ReadToEnd();
    /// });
    /// </code>
    /// </example>
    public static List<T> ProcessDirectory<T>(string absolutePath, Func<Stream, T> processor, string searchPattern = "*", bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath))
            return [];

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(absolutePath, searchPattern, searchOption);
        if (files.Length <= 0) return [];
    
        var results = new List<T>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using var stream = File.OpenRead(file);
                results.Add(processor(stream));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file}: {ex.Message}");
                results.Add(default!);
            }
        }

        return results;
    }
    /// <summary>
    /// Processes all files in the specified directory with their file paths using a custom processor function,
    /// suitable for large files.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the processor function.</typeparam>
    /// <param name="absolutePath">The absolute path of the directory to search.</param>
    /// <param name="processor">A function that processes each file stream and returns a result.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <returns>
    /// A dictionary mapping file paths to their processing results,
    /// or an empty dictionary if the path is invalid or does not exist.
    /// </returns>
    /// <example>
    /// <code>
    /// //Option 1 Using a separate method
    /// string ReadStreamToString(Stream stream)
    /// {
    ///    using var reader = new StreamReader(stream);
    ///    return reader.ReadToEnd();
    /// }
    /// var fileContents = ProcessAllFiles&lt;string&gt;(absolutePath, ReadStreamToString);
    ///
    /// //Option 2 Using a lambda function directly
    /// var fileContents = ProcessAllFiles&lt;string&gt;(absolutePath, stream =>
    /// {
    ///    using var reader = new StreamReader(stream);
    ///    return reader.ReadToEnd();
    /// });
    /// </code>
    /// </example>
    public static Dictionary<string, T> ProcessDirectoryWithPaths<T>(string absolutePath, Func<Stream, T> processor, string searchPattern = "*", bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath))
            return new Dictionary<string, T>();

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(absolutePath, searchPattern, searchOption);
        if (files.Length <= 0) return new Dictionary<string, T>();
    
        var results = new Dictionary<string, T>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using var stream = File.OpenRead(file);
                results[file] = processor(stream);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file}: {ex.Message}");
                results[file] = default!;
            }
        }

        return results;
    }
    
    
    /// <summary>
    /// Retrieves the contents of all files in the specified directory matching the search pattern.
    /// </summary>
    /// <param name="directory">The DirectoryInfo representing the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <param name="encoding">The encoding to use when reading files (null uses the default encoding).</param>
    /// <returns>
    /// A list of file contents matching the search pattern, or an empty list if the directory is null,
    /// does not exist, or contains no matching files.
    /// </returns>
    public static List<string> LoadDirectory(this DirectoryInfo directory, string searchPattern = "*", bool recursive = false, Encoding? encoding = null)
    {
        if (!directory.Exists) return [];

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = directory.GetFiles(searchPattern, searchOption);
        if (files.Length <= 0) return [];

        var contents = new List<string>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using (var reader = new StreamReader(file.FullName, encoding ?? Encoding.Default))
                {
                    contents.Add(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file.FullName}: {ex.Message}");
                contents.Add(string.Empty);
            }
        }

        return contents;
    }

    /// <summary>
    /// Retrieves the contents of all files in the specified directory with their file paths.
    /// </summary>
    /// <param name="directory">The DirectoryInfo representing the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <param name="encoding">The encoding to use when reading files (null uses the default encoding).</param>
    /// <returns>
    /// A dictionary mapping file paths to their contents, or an empty dictionary if the directory is null,
    /// does not exist, or contains no matching files.
    /// </returns>
    public static Dictionary<string, string> LoadDirectoryWithPaths(this DirectoryInfo directory, string searchPattern = "*", bool recursive = false, Encoding? encoding = null)
    {
        if (!directory.Exists) return new Dictionary<string, string>();

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = directory.GetFiles(searchPattern, searchOption);
        if (files.Length <= 0) return new Dictionary<string, string>();

        var contents = new Dictionary<string, string>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using (var reader = new StreamReader(file.FullName, encoding ?? Encoding.Default))
                {
                    contents[file.FullName] = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file.FullName}: {ex.Message}");
                contents[file.FullName] = string.Empty;
            }
        }

        return contents;
    }

    /// <summary>
    /// Processes all files in the specified directory using a custom processor function, suitable for large files.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the processor function.</typeparam>
    /// <param name="directory">The DirectoryInfo representing the directory to search.</param>
    /// <param name="processor">A function that processes each file stream and returns a result.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <returns>
    /// A list of results from processing each file, or an empty list if the directory is null or does not exist.
    /// </returns>
    public static List<T> ProcessDirectory<T>(this DirectoryInfo directory, Func<Stream, T> processor, string searchPattern = "*", bool recursive = false)
    {
        if (!directory.Exists) return [];

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = directory.GetFiles(searchPattern, searchOption);
        if (files.Length <= 0) return [];

        var results = new List<T>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using var stream = File.OpenRead(file.FullName);
                results.Add(processor(stream));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file.FullName}: {ex.Message}");
                results.Add(default!);
            }
        }

        return results;
    }

    /// <summary>
    /// Processes all files in the specified directory with their file paths using a custom processor function,
    /// suitable for large files.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the processor function.</typeparam>
    /// <param name="directory">The DirectoryInfo representing the directory to search.</param>
    /// <param name="processor">A function that processes each file stream and returns a result.</param>
    /// <param name="searchPattern">The search pattern to match files (default is "*").</param>
    /// <param name="recursive">If true, searches all subdirectories; otherwise, only the top directory.</param>
    /// <returns>
    /// A dictionary mapping file paths to their processing results,
    /// or an empty dictionary if the directory is null or does not exist.
    /// </returns>
    public static Dictionary<string, T> ProcessDirectoryWithPaths<T>(this DirectoryInfo directory, Func<Stream, T> processor, string searchPattern = "*", bool recursive = false)
    {
        if (!directory.Exists) return new Dictionary<string, T>();

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = directory.GetFiles(searchPattern, searchOption);
        if (files.Length <= 0) return new Dictionary<string, T>();

        var results = new Dictionary<string, T>(files.Length);

        foreach (var file in files)
        {
            try
            {
                using var stream = File.OpenRead(file.FullName);
                results[file.FullName] = processor(stream);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] {file.FullName}: {ex.Message}");
                results[file.FullName] = default!;
            }
        }

        return results;
    }
    
    #endregion
    
    #region Save
    
    /// <summary>
    /// Saves text to a file at the specified absolute path and file name.
    /// </summary>
    /// <param name="text">The text content to save.</param>
    /// <param name="absolutePath">The absolute directory path where the file will be saved.</param>
    /// <param name="fileName">The name of the file to save.</param>
    /// <param name="encoding">The encoding to use when writing the file (null uses the default encoding).</param>
    /// <param name="append">If true, appends to the file if it exists; otherwise, overwrites it.</param>
    /// <param name="createDirectory">If true, creates the directory if it doesn't exist.</param>
    /// <returns>True if the file was saved successfully; otherwise, false.</returns>
    public static bool SaveText(string text, string absolutePath, string fileName, Encoding? encoding = null, bool append = false, bool createDirectory = true)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || string.IsNullOrWhiteSpace(fileName)) return false;
    
        try
        {
            if (createDirectory) Directory.CreateDirectory(absolutePath);
            string path = CombinePath(absolutePath, fileName);
        
            using (var writer = new StreamWriter(path, append, encoding ?? Encoding.Default))
            {
                writer.Write(text);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to save {fileName} to {absolutePath}: {ex.Message}");
            return false;
        }
    }
    /// <summary>
    /// Saves text to a file using a FileInfo object.
    /// </summary>
    /// <param name="text">The text content to save.</param>
    /// <param name="file">The FileInfo representing the file to save to.</param>
    /// <param name="encoding">The encoding to use when writing the file (null uses the default encoding).</param>
    /// <param name="append">If true, appends to the file if it exists; otherwise, overwrites it.</param>
    /// <param name="createDirectory">If true, creates the parent directory if it doesn't exist.</param>
    /// <returns>True if the file was saved successfully; otherwise, false.</returns>
    public static bool SaveText(this FileInfo file, string text, Encoding? encoding = null, bool append = false, bool createDirectory = true)
    {
        try
        {
            if (createDirectory && file.Directory is { Exists: false }) file.Directory.Create();
                
            using (var writer = new StreamWriter(file.FullName, append, encoding ?? Encoding.Default))
            {
                writer.Write(text);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to save to {file.FullName}: {ex.Message}");
            return false;
        }
    }
    /// <summary>
    /// Saves text to a file within a specified directory.
    /// </summary>
    /// <param name="text">The text content to save.</param>
    /// <param name="directory">The DirectoryInfo representing the directory where the file will be saved.</param>
    /// <param name="fileName">The name of the file to save.</param>
    /// <param name="encoding">The encoding to use when writing the file (null uses the default encoding).</param>
    /// <param name="append">If true, appends to the file if it exists; otherwise, overwrites it.</param>
    /// <param name="createDirectory">If true, creates the directory if it doesn't exist.</param>
    /// <returns>True if the file was saved successfully; otherwise, false.</returns>
    public static bool SaveText(this DirectoryInfo directory, string fileName, string text, Encoding? encoding = null, bool append = false, bool createDirectory = true)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        try
        {
            if (createDirectory && !directory.Exists) directory.Create();
            string path = Path.Combine(directory.FullName, fileName);
            
            using (var writer = new StreamWriter(path, append, encoding ?? Encoding.Default))
            {
                writer.Write(text);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to save {fileName} to {directory.FullName}: {ex.Message}");
            return false;
        }
    }
    
    #endregion

    #region Load
    /// <summary>
    /// Loads a text file from the specified absolute path and file name.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <param name="fileName">The file name to load.</param>
    /// <param name="encoding">The encoding to use when reading the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or an empty string if not found or if an error occurs.</returns>
    public static string LoadText(string absolutePath, string fileName, Encoding? encoding = null)
    {
        string path = CombinePath(absolutePath, fileName);
        if (!File.Exists(path)) return string.Empty;
        
        try
        {
            using (var reader = new StreamReader(path, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {fileName} from {absolutePath}: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Loads a text file from the specified path, or creates it with default text if it doesn't exist.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <param name="fileName">The file name to load.</param>
    /// <param name="defaultText">The default text to write if the file needs to be created.</param>
    /// <param name="encoding">The encoding to use when reading/writing the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or the default text if the file was created, or an empty string if an error occurs.</returns>
    public static string LoadText(string absolutePath, string fileName, string defaultText, Encoding? encoding = null)
    {
        string path = CombinePath(absolutePath, fileName);
    
        if (!File.Exists(path))
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                // Create the file with default text
                File.WriteAllText(path, defaultText, encoding ?? Encoding.Default);
                return defaultText;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create file with default text at {path}: {ex.Message}");
                return string.Empty;
            }
        }
    
        try
        {
            using (var reader = new StreamReader(path, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {fileName} from {absolutePath}: {ex.Message}");
            return string.Empty;
        }
    }
    /// <summary>
    /// Loads a text file from the specified FileInfo.
    /// </summary>
    /// <param name="file">The FileInfo representing the file to load.</param>
    /// <param name="encoding">The encoding to use when reading the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or an empty string if not found or if an error occurs.</returns>
    public static string LoadText(this FileInfo file, Encoding? encoding = null)
    {
        if (!file.Exists) return string.Empty;
        
        try
        {
            using (var reader = new StreamReader(file.FullName, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {file.Name}: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Loads a text file from the specified FileInfo, or creates it with default text if it doesn't exist.
    /// </summary>
    /// <param name="file">The FileInfo representing the file to load.</param>
    /// <param name="defaultText">The default text to write if the file needs to be created.</param>
    /// <param name="encoding">The encoding to use when reading/writing the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or the default text if the file was created, or an empty string if an error occurs.</returns>
    public static string LoadText(this FileInfo file, string defaultText, Encoding? encoding = null)
    {
        if (!file.Exists)
        {
            try
            {
                // Ensure the directory exists
                if (file.Directory is { Exists: false }) file.Directory.Create();
                    
                // Create the file with default text
                File.WriteAllText(file.FullName, defaultText, encoding ?? Encoding.Default);
                return defaultText;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create file with default text at {file.FullName}: {ex.Message}");
                return string.Empty;
            }
        }
    
        try
        {
            using (var reader = new StreamReader(file.FullName, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {file.Name}: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Loads a text file from the specified directory and file name.
    /// </summary>
    /// <param name="directory">The DirectoryInfo representing the directory where the file is located.</param>
    /// <param name="fileName">The file name to load.</param>
    /// <param name="encoding">The encoding to use when reading the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or an empty string if not found or if an error occurs.</returns>
    public static string LoadText(this DirectoryInfo directory, string fileName, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
        
        string path = Path.Combine(directory.FullName, fileName);
        if (!File.Exists(path)) return string.Empty;
        
        try
        {
            using (var reader = new StreamReader(path, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {fileName} from {directory.FullName}: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Loads a text file from the specified directory and file name, or creates it with default text if it doesn't exist.
    /// </summary>
    /// <param name="directory">The DirectoryInfo representing the directory where the file is located.</param>
    /// <param name="fileName">The file name to load.</param>
    /// <param name="defaultText">The default text to write if the file needs to be created.</param>
    /// <param name="encoding">The encoding to use when reading/writing the file (null uses the default encoding).</param>
    /// <returns>The loaded text content, or the default text if the file was created, or an empty string if an error occurs.</returns>
    public static string LoadText(this DirectoryInfo directory, string fileName, string defaultText, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
        
        string path = Path.Combine(directory.FullName, fileName);
        
        if (!File.Exists(path))
        {
            try
            {
                // Ensure the directory exists
                if (!directory.Exists) directory.Create();
                
                // Create the file with default text
                File.WriteAllText(path, defaultText, encoding ?? Encoding.Default);
                return defaultText;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to create file with default text at {path}: {ex.Message}");
                return string.Empty;
            }
        }
    
        try
        {
            using (var reader = new StreamReader(path, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to load {fileName} from {directory.FullName}: {ex.Message}");
            return string.Empty;
        }
    }
    #endregion
    
    #region Read/Write to File
    /// <summary>
    /// Writes to a text file using the provided action.
    /// </summary>
    /// <param name="path">The directory path where the file should be saved.</param>
    /// <param name="fileName">The name of the file (must include a valid extension).</param>
    /// <param name="writeAction">Function that writes content using the provided StreamWriter.</param>
    /// <param name="encoding">The character encoding to use (null uses the default encoding).</param>
    /// <param name="append">If true, appends to the file if it exists; otherwise, overwrites it.</param>
    /// <param name="createDirectory">If true, creates the directory if it doesn't exist.</param>
    /// <returns>True if the write operation succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the filename has no valid extension.</exception>
    public static bool WriteToFile(string path, string fileName, Action<StreamWriter> writeAction, Encoding? encoding = null, bool append = false, bool createDirectory = true)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(fileName)) return false;
            
        if (!Path.HasExtension(fileName)) throw new ArgumentException("File name must have a valid extension.");

        try
        {
            if (createDirectory && !Directory.Exists(path)) Directory.CreateDirectory(path);
                
            string fullPath = CombinePath(path, fileName);
            
            using (var writer = new StreamWriter(fullPath, append, encoding ?? Encoding.Default))
            {
                writeAction(writer);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to write to {fileName} at {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reads from a text file using the provided action.
    /// </summary>
    /// <param name="path">The directory path where the file is located.</param>
    /// <param name="fileName">The name of the file (must include a valid extension).</param>
    /// <param name="readAction">Function that reads content using the provided StreamReader.</param>
    /// <param name="encoding">The character encoding to use (null uses the default encoding).</param>
    /// <returns>True if the read operation succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the filename has no valid extension.</exception>
    public static bool ReadFromFile(string path, string fileName, Action<StreamReader> readAction, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(fileName)) return false;
            
        if (!Path.HasExtension(fileName)) throw new ArgumentException("File name must have a valid extension.");

        string fullPath = CombinePath(path, fileName);
        
        if (!File.Exists(fullPath)) return false;
            
        try
        {
            using (var reader = new StreamReader(fullPath, encoding ?? Encoding.Default))
            {
                readAction(reader);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to read from {fileName} at {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Writes to a text file using the provided action.
    /// </summary>
    /// <param name="file">The FileInfo representing the file to write to.</param>
    /// <param name="writeAction">Function that writes content using the provided StreamWriter.</param>
    /// <param name="encoding">The character encoding to use (null uses the default encoding).</param>
    /// <param name="append">If true, appends to the file if it exists; otherwise, overwrites it.</param>
    /// <param name="createDirectory">If true, creates the directory if it doesn't exist.</param>
    /// <returns>True if the write operation succeeded; otherwise, false.</returns>
    public static bool WriteToFile(this FileInfo file, Action<StreamWriter> writeAction, Encoding? encoding = null, bool append = false, bool createDirectory = true)
    {
        try
        {
            if (createDirectory && !file.Directory!.Exists) file.Directory.Create();
                
            using (var writer = new StreamWriter(file.FullName, append, encoding ?? Encoding.Default))
            {
                writeAction(writer);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to write to {file.FullName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reads from a text file using the provided action.
    /// </summary>
    /// <param name="file">The FileInfo representing the file to read from.</param>
    /// <param name="readAction">Function that reads content using the provided StreamReader.</param>
    /// <param name="encoding">The character encoding to use (null uses the default encoding).</param>
    /// <returns>True if the read operation succeeded; otherwise, false.</returns>
    public static bool ReadFromFile(this FileInfo file, Action<StreamReader> readAction, Encoding? encoding = null)
    {
        if (!file.Exists) return false;
        
        try
        {
            using (var reader = new StreamReader(file.FullName, encoding ?? Encoding.Default))
            {
                readAction(reader);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{ex.GetType().Name}] Failed to read from {file.FullName}: {ex.Message}");
            return false;
        }
    }
    #endregion
}

/* Deprecated
    #region Save
   /// <summary>
   /// Saves a text file to the specified absolute path and file name.
   /// </summary>
   /// <param name="text">The text content to save.</param>
   /// <param name="absolutePath">The absolute directory path.</param>
   /// <param name="fileName">The file name to save as.</param>
   /// <returns>True if the file was saved successfully; otherwise, false.</returns>
   public static bool SaveText(string text, string absolutePath, string fileName)
   {
       if (absolutePath.Length <= 0 || fileName.Length <= 0) return false;
       Directory.CreateDirectory(absolutePath);
       File.WriteAllText(CombinePath(absolutePath, fileName), text);
       return true;
   }
   #endregion
   
   
   #region Load
   /// <summary>
   /// Loads a text file from the specified absolute path and file name.
   /// </summary>
   /// <param name="absolutePath">The absolute directory path.</param>
   /// <param name="fileName">The file name to load.</param>
   /// <returns>The loaded text content, or an empty string if not found.</returns>
   public static string LoadText(string absolutePath, string fileName)
   {
       string path = CombinePath(absolutePath, fileName);
       if (!File.Exists(path)) return String.Empty;
       return File.ReadAllText(path);
   }
   #endregion
 */

//DEPRECATED
// public static List<string>? GetAllFileContents(string absolutePath, string searchPattern = "*", bool recursive = false)
// {
//     if (string.IsNullOrWhiteSpace(absolutePath) || !Directory.Exists(absolutePath)) return null;
//
//     var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
//     var files = Directory.GetFiles(absolutePath, searchPattern, searchOption);
//     if(files.Length <= 0) return null;
//         
//     var contents = new List<string>();
//
//     foreach (var file in files)
//     {
//         try
//         {
//             contents.Add(File.ReadAllText(file));
//         }
//         catch (PathTooLongException ex)
//         {
//             Console.Error.WriteLine($"[PathTooLongException] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//         catch (DirectoryNotFoundException ex)
//         {
//             Console.Error.WriteLine($"[DirectoryNotFoundException] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//         catch (FileNotFoundException ex)
//         {
//             Console.Error.WriteLine($"[FileNotFoundException] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//         catch (UnauthorizedAccessException ex)
//         {
//             Console.Error.WriteLine($"[UnauthorizedAccessException] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//         catch (IOException ex)
//         {
//             Console.Error.WriteLine($"[IOException] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//         catch (Exception ex)
//         {
//             Console.Error.WriteLine($"[Exception] {file}: {ex.Message}");
//             contents.Add(string.Empty);
//         }
//     }
//
//     return contents;
// }

//use new serializer classes to get a string and then use this class to save the string to file!

// /// <summary>
// /// Saves an object as JSON to the specified absolute path and file name.
// /// </summary>
// /// <typeparam name="T">The type of the object to save.</typeparam>
// /// <param name="data">The data object to serialize and save.</param>
// /// <param name="absolutePath">The absolute directory path.</param>
// /// <param name="fileName">The file name to save as.</param>
// /// <returns>True if the file was saved successfully; otherwise, false.</returns>
// public static bool Save<T>(T data, string absolutePath, string fileName)
// {
//     if (data == null) return false;
//     if (absolutePath.Length <= 0 || fileName.Length <= 0) return false;
//     Directory.CreateDirectory(absolutePath);
//
//     string data_string = JsonSerializer.Serialize(data);
//     if (data_string.Length <= 0) return false;
//     File.WriteAllText(CombinePath(absolutePath, fileName), data_string);
//     return true;
// }
// /// <summary>
// /// Loads an object of type <typeparamref name="T"/> from a JSON file at the specified path and file name.
// /// </summary>
// /// <typeparam name="T">The type of the object to load.</typeparam>
// /// <param name="absolutePath">The absolute directory path.</param>
// /// <param name="fileName">The file name to load.</param>
// /// <returns>The deserialized object, or default if not found or invalid.</returns>
// public static T? Load<T>(string absolutePath, string fileName)
// {
//     string path = CombinePath(absolutePath, fileName);
//     if (!File.Exists(path)) return default;
//
//     var data_string = File.ReadAllText(path);
//
//     return JsonSerializer.Deserialize<T>(data_string);
// }

