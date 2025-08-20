
namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides static methods for saving and loading text and data files, as well as accessing common application directories.
/// </summary>
public static class ShapeFileManager
{
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
   

}

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

