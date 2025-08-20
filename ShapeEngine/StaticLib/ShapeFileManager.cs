
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
    
    #region Directories
    /// <summary>
    /// Creates a directory in the user's application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateApplicationDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(ApplicationDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory in the user's local application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateApplicationLocalDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(ApplicationLocalDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory in the common application data folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateApplicationCommonDataDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(ApplicationCommonDataPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory in the user's documents folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateApplicationDocumentsDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(ApplicationDocumentsPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory in the common documents folder.
    /// </summary>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateApplicationCommonDocumentsDirectory(string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(ApplicationCommonDocumentsPath, directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    /// <summary>
    /// Creates a directory in a specified special system folder.
    /// </summary>
    /// <param name="folder">The special folder in which to create the directory.</param>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateSpecialFolderDirectory(Environment.SpecialFolder folder, string directoryName, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(directoryName)) return -1;
        string absolutePath = CombinePath(GetSpecialFolderPath(folder), directoryName);
        return CreateDirectory(absolutePath, overrideIfExists);
    }
    
    /// <summary>
    /// Checks if the specified directory exists.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public static bool DirectoryExists(string absolutePath) => Directory.Exists(absolutePath);
    /// <summary>
    /// Creates a new directory at the specified absolute path.
    /// </summary>
    /// <param name="absolutePath">The absolute directory path.</param>
    /// <param name="overrideIfExists">If true, deletes and recreates the directory if it exists.</param>
    /// <returns>Returns 1 if the directory was created, 0 if it already existed, or -1 if the path was invalid.</returns>
    public static int CreateDirectory(string absolutePath, bool overrideIfExists = false)
    {
        if (string.IsNullOrWhiteSpace(absolutePath)) return -1;
        if (Directory.Exists(absolutePath))
        {
            if (overrideIfExists)
            {
                Directory.Delete(absolutePath, true);
            }
            return 0;
        }
        Directory.CreateDirectory(absolutePath);
        return 1;
    }
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
    #endregion
    
    //TODO: Update with StreamReaders for robustness and performance
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
    
    
    //TODO: Update with StreamReaders for robustness and performance
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

}

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

