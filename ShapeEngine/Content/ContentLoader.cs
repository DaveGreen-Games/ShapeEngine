using System.Text;
using Raylib_cs;
using ShapeEngine.Core.GameDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Content;

/// <summary>
/// Provides a simple wrapper to load all types of Raylib resources and JSON strings.
/// </summary>
/// <remarks>
/// Resources and content refer to assets shipped with the game, such as textures, fonts, audio files, shaders, and similar files.
/// Text-based data files (e.g. XML, JSON) included with the game can be loaded as strings using this class.
/// For loading previously saved game data, use the <see cref="StaticLib.ShapeFileManager"/> class instead.
/// </remarks>
public static class ContentLoader
{
    /// <summary>
    /// Defines the types of resources that can be loaded
    /// </summary>
    public enum ContentType
    {
        Font,
        ShaderFragment,
        ShaderVertex,
        Texture,
        Wave,
        Sound,
        Music,
        Text
    }
    
    /// <summary>
    /// Specifies the number of font glyphs to load. Default value is 0, which loads the default character set.
    /// </summary>
    public static int GLYPH_COUNT = 0;

    /// <summary>
    /// Valid file extensions for different resource types
    /// </summary>
    private static readonly Dictionary<ContentType, HashSet<string>> ValidExtensions = new()
    {
        { ContentType.Font, [".ttf", ".otf"] },
        { ContentType.ShaderFragment, [".fs", ".glsl", ".frag"] },
        { ContentType.ShaderVertex, [".vs", ".glsl",  ".vert"] },
        { ContentType.Texture, [".png", ".bmp", ".tga", ".jpg", ".jpeg", ".gif", ".psd", ".pkm", ".ktx", ".pvr", ".dds", ".hdr"] },
        { ContentType.Wave, [".wav", ".mp3", ".ogg", ".flac"] },
        { ContentType.Sound, [".wav", ".mp3", ".ogg", ".flac"] },
        { ContentType.Music, [".mp3", ".ogg", ".flac", ".mod", ".xm"] },
        { ContentType.Text, [".txt", ".json", ".xml", ".csv"] }
    };

    #region Paths
    /// <summary>
    /// Resolves the full path to a resource file when running inside a macOS application bundle.
    /// </summary>
    /// <param name="relativePath">The relative path to the resource.</param>
    /// <returns>
    /// If running in a macOS .app bundle, returns the full path within the bundle's Resources directory.
    /// Otherwise, returns the original relativePath unchanged.
    /// </returns>
    /// <remarks>
    /// In macOS .app bundles, the executable is located in Contents/MacOS/ while resources are in Contents/Resources/.
    /// This method handles the path conversion for proper resource loading in both development and deployed scenarios.
    /// </remarks>
    private static string GetMacOsAppBundleResourcePath(string relativePath)
    {
        if (!Game.OSXIsRunningInAppBundle()) return relativePath;
        
        // macOS .app bundle: executable is in Contents/MacOS/
        // Resources are in Contents/Resources/
        string exeDir = AppContext.BaseDirectory; // This is Contents/MacOS/
        string resourcesDir = Path.Combine(exeDir, "..", "Resources");//".." goes up one level to Contents
        string fullPath = Path.GetFullPath(Path.Combine(resourcesDir, relativePath));
        ShapeLogger.LogInfo($"MacOS app bundle loading resource from path: {fullPath}");
        return fullPath;
    }
    /// <summary>
    /// Checks if a file is a valid resource based on its extension.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="contentType">The type of resource to validate against.</param>
    /// <returns>True if the file has a valid extension for the specified resource type.</returns>
    public static bool IsValidResourceFile(string filePath, ContentType contentType)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;
            
        string extension = Path.GetExtension(filePath).ToLower();
        return ValidExtensions[contentType].Contains(extension);
    }
    /// <summary>
    /// Gets all valid resource file paths in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <param name="contentType">The type of resource to filter for.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories.</param>
    /// <returns>An array of valid file paths.</returns>
    private static string[] GetResourceFilePaths(string directoryPath, ContentType contentType, bool recursive)
    {
        if (Game.IsOSX())
        {
            directoryPath = GetMacOsAppBundleResourcePath(directoryPath);
        }
        
        if (!Directory.Exists(directoryPath)) return [];
            
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        return Directory.GetFiles(directoryPath, "*.*", searchOption)
            .Where(file => IsValidResourceFile(file, contentType))
            .ToArray();
    }
    #endregion
    
    #region Extensions
    /// <summary>
    /// Gets the <see cref="ContentType"/> associated with a given file extension.
    /// </summary>
    /// <param name="extension">The file extension to check (e.g. ".png").</param>
    /// <returns>
    /// The corresponding <see cref="ContentType"/> if the extension is valid; otherwise, null.
    /// </returns>
    public static ContentType? GetContentType(string extension)
    {
        foreach (var kvp in ValidExtensions)
        {
            if(kvp.Value.Contains(extension)) return kvp.Key;
        }

        return null;
    }
    /// <summary>
    /// Checks if the given file extension is valid for the specified content type.
    /// </summary>
    /// <param name="extension">The file extension to check (e.g. ".png").</param>
    /// <param name="contentType">The content type to validate against.</param>
    /// <returns>True if the extension is valid for the content type; otherwise, false.</returns>
    public static bool IsValidExtension(string extension, ContentType contentType)
    {
        return ValidExtensions.ContainsKey(contentType) && ValidExtensions[contentType].Contains(extension);
    }
    /// <summary>
    /// Gets a list of valid file extensions for the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to retrieve valid extensions for.</param>
    /// <returns>A list of valid file extensions for the given content type.</returns>
    public static List<string> GetValidFileExtensions(ContentType contentType)
    {
        return ValidExtensions.TryGetValue(contentType, out var extension) ? extension.ToList() : [];
    }
    /// <summary>
    /// Adds a valid file extension for the specified <see cref="ContentType"/>.
    /// Automatically adds a '.' prefix if missing.
    /// </summary>
    /// <param name="contentType">The content type to associate the extension with.</param>
    /// <param name="extension">The file extension to add (e.g. ".png").</param>
    /// <returns>True if the extension was added; false if it already existed.</returns>
    public static bool AddValidFileExtension(ContentType contentType, string extension)
    {
        if (!extension.StartsWith('.'))
        {
            ShapeLogger.LogInfo($"ContentLoader AddValidFileExtension: Automatically added '.' prefix to extension {extension}.");
            extension = "." + extension;
        }

        if (ValidExtensions.TryGetValue(contentType, out var value))
        {
            return value.Add(extension);
        }
        
        ValidExtensions[contentType] = [extension];
        return true;
    }
    /// <summary>
    /// Adds multiple valid file extensions for the specified <see cref="ContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type to associate the extensions with.</param>
    /// <param name="extensions">An array of file extensions to add (e.g. ".png", ".jpg").</param>
    /// <returns>The number of extensions that were added (not already present).</returns>
    public static int AddValidFileExtensions(ContentType contentType, params string[] extensions)
    {
        var addedCount = 0;

        if(ValidExtensions.TryGetValue(contentType, out var value))
        {
            foreach (string ext in extensions)
            {
               if(value.Add(ext)) addedCount++;
            }
        }
        else
        {
            addedCount = extensions.Length;
            ValidExtensions[contentType] = new HashSet<string>(extensions);
        }
        return addedCount;
    }
    /// <summary>
    /// Removes a valid file extension for the specified <see cref="ContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type to remove the extension from.</param>
    /// <param name="extension">The file extension to remove (e.g. ".png").</param>
    /// <returns>True if the extension was removed; false if it did not exist.</returns>
    public static bool RemoveValidFileExtension(ContentType contentType, string extension)
    {
        return ValidExtensions.TryGetValue(contentType, out var value) && value.Remove(extension);
    }
    /// <summary>
    /// Removes multiple valid file extensions for the specified <see cref="ContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type to remove extensions from.</param>
    /// <param name="extensions">An array of file extensions to remove (e.g. ".png", ".jpg").</param>
    /// <returns>The number of extensions that were removed (existed and were deleted).</returns>
    public static int RemoveValidFileExtensions(ContentType contentType, params string[] extensions)
    {
        var removedCount = 0;
        if (!ValidExtensions.TryGetValue(contentType, out var value)) return removedCount;
        foreach (string ext in extensions)
        {
            if (value.Remove(ext)) removedCount++;
        }
        return removedCount;
    }
    /// <summary>
    /// Sets the valid file extensions for the specified <see cref="ContentType"/>.
    /// Replaces any existing extensions with the provided ones.
    /// </summary>
    /// <param name="contentType">The content type to set extensions for.</param>
    /// <param name="extensions">An array of file extensions to associate (e.g. ".png", ".jpg").</param>
    public static void SetValidFileExtensions(ContentType contentType, params string[] extensions)
    {
        ValidExtensions[contentType] = new HashSet<string>(extensions);
    }
    /// <summary>
    /// Prints all valid file extensions for each <see cref="ContentType"/> to the console.
    /// </summary>
    public static void PrintValidFileExtensions()
    {
        foreach (var kvp in ValidExtensions)
        {
            string extensions = string.Join(", ", kvp.Value);
            ShapeLogger.LogInfo($"{kvp.Key}: [{extensions}]");
        }
    }
    #endregion
    
    #region Load
    
    /// <summary>
    /// Loads a font from a file with the specified font size and texture filter.
    /// </summary>
    /// <param name="filePath">The path to the font file.</param>
    /// <param name="fontSize">The size of the font to load. Default is 100.</param>
    /// <param name="textureFilter">The texture filter to apply to the font texture. Default is Trilinear.</param>
    /// <returns>The loaded Font object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Font LoadFont(string filePath, int fontSize = 100, TextureFilter textureFilter = TextureFilter.Trilinear)
    {
        TryLoadFont(filePath, out var font, fontSize, textureFilter);
        return font;
    }
    /// <summary>
    /// Loads a fragment shader from a file.
    /// </summary>
    /// <param name="filePath">The path to the fragment shader file.</param>
    /// <returns>The loaded Shader object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Shader LoadFragmentShader(string filePath)
    {
        TryLoadFragmentShader(filePath, out var shader);
        return shader;
    }
    /// <summary>
    /// Loads a vertex shader from a file.
    /// </summary>
    /// <param name="filePath">The path to the vertex shader file.</param>
    /// <returns>The loaded Shader object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Shader LoadVertexShader(string filePath)
    {
        TryLoadVertexShader(filePath, out var shader);
        return shader;
    }
    /// <summary>
    /// Loads a texture from a file.
    /// </summary>
    /// <param name="filePath">The path to the texture file.</param>
    /// <returns>The loaded Texture2D object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Texture2D LoadTexture(string filePath)
    {
        TryLoadTexture(filePath, out var texture);
        return texture;
    }
    /// <summary>
    /// Loads an image from a file.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>The loaded Image object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Image LoadImage(string filePath)
    {
        TryLoadImage(filePath, out var image);
        return image;
    }
    /// <summary>
    /// Loads a wave sound from a file.
    /// </summary>
    /// <param name="filePath">The path to the wave sound file.</param>
    /// <returns>The loaded Wave object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Wave LoadWave(string filePath)
    {
        TryLoadWave(filePath, out var wave);
        return wave;
    }
    /// <summary>
    /// Loads a sound from a file.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <returns>The loaded Sound object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Sound LoadSound(string filePath)
    {
        TryLoadSound(filePath, out var sound);
        return sound;
    }
    /// <summary>
    /// Loads a music stream from a file.
    /// </summary>
    /// <param name="filePath">The path to the music file.</param>
    /// <returns>The loaded Music object.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    /// </list>
    /// </example>
    public static Music LoadMusic(string filePath)
    {
        TryLoadMusic(filePath, out var music);
        return music;
    }
    /// <summary>
    /// Loads a text file as a string.
    /// This should not be used for game save data,
    /// but rather for static text files like JSON or XML that were shipped with the game!
    /// </summary>
    /// <param name="filePath">The path to the text file.</param>
    /// <returns>The content of the text file as a string.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// By default, file paths should be relative to the executable's directory.
    /// </remarks>
    ///<example>
    /// If all resources are in a `Resources` folder at the project root, the following paths would be valid:
    /// <list type="bullet">
    ///   <item><description>"Resources/Fonts/font.ttf"</description></item>
    ///   <item><description>"Resources/Audio/Music/BackgroundMusic.mp3"</description></item>
    ///   <item><description>"Resources/Audio/Sounds/Jump.wav"</description></item>
    ///   <item><description>"Resources/Images/Background.png"</description></item>
    ///   <item><description>"Resources/Data/EnemySpawnData.xml"</description></item>
    /// </list>
    /// </example>
    public static string LoadText(string filePath)
    {
        TryLoadText(filePath, out string text);
        return text;
    }

    #endregion
    
    #region TryLoad
    
    /// <summary>
    /// Attempts to load a font from the specified file path with the given font size and texture filter.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the font file.</param>
    /// <param name="font">The loaded Font object if successful; otherwise, a default Font.</param>
    /// <param name="fontSize">The size of the font to load. Default is 100.</param>
    /// <param name="textureFilter">The texture filter to apply to the font texture. Default is Trilinear.</param>
    /// <returns>True if the font was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadFont(string filePath, out Font font, int fontSize = 100, TextureFilter textureFilter = TextureFilter.Trilinear)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }

        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            font = new();
            return false;
        }
        
        var extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Font;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for Font: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            font = new();
            return false;
        }
        
        try
        {
            font = Raylib.LoadFontEx(filePath, fontSize, [], GLYPH_COUNT);
            Raylib.SetTextureFilter(font.Texture, textureFilter);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading font from {filePath}: {ex.Message}");
            font = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a fragment shader from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the fragment shader file.</param>
    /// <param name="shader">The loaded Shader object if successful; otherwise, a default Shader.</param>
    /// <returns>True if the fragment shader was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadFragmentShader(string filePath, out Shader shader)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            shader = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.ShaderFragment;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            shader = new();
            return false;
        }
        
        try
        {
            shader = Raylib.LoadShader(null, filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading fragment shader from {filePath}: {ex.Message}");
            shader = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a vertex shader from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the vertex shader file.</param>
    /// <param name="shader">The loaded Shader object if successful; otherwise, a default Shader.</param>
    /// <returns>True if the vertex shader was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadVertexShader(string filePath, out Shader shader)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            shader = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.ShaderVertex;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            shader = new();
            return false;
        }
        
        try
        {
            shader = Raylib.LoadShader(filePath, "");
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading vertex shader from {filePath}: {ex.Message}");
            shader = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a texture from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the texture file.</param>
    /// <param name="texture">The loaded Texture2D object if successful; otherwise, a default Texture2D.</param>
    /// <returns>True if the texture was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadTexture(string filePath, out Texture2D texture)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            texture = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Texture;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            texture = new();
            return false;
        }
        
        try
        {
            texture = Raylib.LoadTexture(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading texture from {filePath}: {ex.Message}");
            texture = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load an image from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <param name="image">The loaded Image object if successful; otherwise, a default Image.</param>
    /// <returns>True if the image was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadImage(string filePath, out Image image)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            image = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Texture;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            image = new();
            return false;
        }
        
        try
        {
            image = Raylib.LoadImage(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading image from {filePath}: {ex.Message}");
            image = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a wave sound from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the wave sound file.</param>
    /// <param name="wave">The loaded Wave object if successful; otherwise, a default Wave.</param>
    /// <returns>True if the wave was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadWave(string filePath, out Wave wave)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
       
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            wave = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Wave;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            wave = new();
            return false;
        }
        
        try
        {
            wave = Raylib.LoadWave(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading wave from {filePath}: {ex.Message}");
            wave = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a sound from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="sound">The loaded Sound object if successful; otherwise, a default Sound.</param>
    /// <returns>True if the sound was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadSound(string filePath, out Sound sound)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            sound = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Sound;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            sound = new();
            return false;
        }
        
        try
        {
            sound = Raylib.LoadSound(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading sound from {filePath}: {ex.Message}");
            sound = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a music stream from the specified file path.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the music file.</param>
    /// <param name="music">The loaded Music object if successful; otherwise, a default Music.</param>
    /// <returns>True if the music stream was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadMusic(string filePath, out Music music)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            music = new();
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Music;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            music = new();
            return false;
        }
        
        try
        {
            music = Raylib.LoadMusicStream(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading music stream from {filePath}: {ex.Message}");
            music = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a text file as a string.
    /// Automatically resolves resource paths for macOS application bundles.
    /// </summary>
    /// <param name="filePath">The path to the text file.</param>
    /// <param name="text">The loaded text content if successful; otherwise, an empty string.</param>
    /// <returns>True if the text was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadText(string filePath, out string text)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        if (!File.Exists(filePath))
        {
            ShapeLogger.LogError($"Loading resource failed, file does not exist: {filePath}");
            text = string.Empty;
            return false;
        }
        
        string extension = Path.GetExtension(filePath).ToLower();
        var contentType = ContentType.Text;
        if (!IsValidExtension(extension, contentType))
        {
            ShapeLogger.LogError($"Loading resource failed, invalid file extension '{extension}' for {contentType}: {filePath}. Current valid extensions for {contentType} are: {string.Join(", ", GetValidFileExtensions(contentType))}");
            text = string.Empty;
            return false;
        }
        
        try
        {
            text = File.ReadAllText(filePath);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading text from {filePath}: {ex.Message}");
            text = string.Empty;
            return false;
        }
    }

    #endregion
    
    #region TryLoad from Memory

    /// <summary>
    /// Attempts to load a texture from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png").
    /// Will be checked against valid extensions for <see cref="ContentType.Texture"/></param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <param name="texture">The loaded Texture2D object if successful; otherwise, a default Texture2D.</param>
    /// <returns>True if the texture was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Texture"/> can be managed via <see cref="AddValidFileExtension"/>,
    /// <see cref="RemoveValidFileExtension"/>,and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadTextureFromMemory(string extension, byte[] data, out Texture2D texture)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            texture = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            texture = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Texture))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Texture} are: {string.Join(", ", GetValidFileExtensions(ContentType.Texture))}");
            texture = new();
            return false;
        }
        
        try
        {
            if (!TryLoadImageFromMemory(extension, data, out var image))
            {
                texture = new();
                return false;
            }
            texture = Raylib.LoadTextureFromImage(image);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading texture from memory: {ex.Message}");
            texture = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load an image from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png"). Will be checked against valid extensions for <see cref="ContentType.Texture"/>.</param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <param name="image">The loaded Image object if successful; otherwise, a default Image.</param>
    /// <returns>True if the image was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Texture"/> can be managed via <see cref="AddValidFileExtension"/>,
    /// <see cref="RemoveValidFileExtension"/>,and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadImageFromMemory(string extension, byte[] data, out Image image)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            image = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            image = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Texture))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Texture} are: {string.Join(", ", GetValidFileExtensions(ContentType.Texture))}");
            image = new();
            return false;
        }
        
        try
        {
            image = Raylib.LoadImageFromMemory(extension, data);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading image from memory: {ex.Message}");
            image = new();
            return false;
        }
    }

    /// <summary>
    /// Attempts to load a font from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".ttf"). Will be checked against valid extensions for <see cref="ContentType.Font"/>.</param>
    /// <param name="data">The raw font data as a byte array.</param>
    /// <param name="font">The loaded Font object if successful; otherwise, a default Font.</param>
    /// <param name="fontSize">The size of the font to load. Default is 100.</param>
    /// <returns>True if the font was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Font"/> can be managed via <see cref="AddValidFileExtension"/>,
    /// <see cref="RemoveValidFileExtension"/>,and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadFontFromMemory(string extension, byte[] data, out Font font, int fontSize = 100)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            font = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            font = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Font))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Font} are: {string.Join(", ", GetValidFileExtensions(ContentType.Font))}");
            font = new();
            return false;
        }
        
        try
        {
            font = Raylib.LoadFontFromMemory(extension, data, fontSize, [], GLYPH_COUNT);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading font from memory: {ex.Message}");
            font = new();
            return false;
        }
    }

    /// <summary>
    /// Attempts to load a wave sound from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav"). Will be checked against valid extensions for <see cref="ContentType.Wave"/>.</param>
    /// <param name="data">The raw wave data as a byte array.</param>
    /// <param name="wave">The loaded Wave object if successful; otherwise, a default Wave.</param>
    /// <returns>True if the wave was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Wave"/> can be managed via <see cref="AddValidFileExtension"/>,
    /// <see cref="RemoveValidFileExtension"/>,and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadWaveFromMemory(string extension, byte[] data, out Wave wave)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            wave = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            wave = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Wave))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Wave} are: {string.Join(", ", GetValidFileExtensions(ContentType.Wave))}");
            wave = new();
            return false;
        }
        
        try
        {
            wave = Raylib.LoadWaveFromMemory(extension, data);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading wave from memory: {ex.Message}");
            wave = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a sound from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav"). Will be checked against valid extensions for <see cref="ContentType.Sound"/>.</param>
    /// <param name="data">The raw sound data as a byte array.</param>
    /// <param name="sound">The loaded Sound object if successful; otherwise, a default Sound.</param>
    /// <returns>True if the sound was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Sound"/> can be managed via <see cref="AddValidFileExtension"/>,
    /// <see cref="RemoveValidFileExtension"/>, and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadSoundFromMemory(string extension, byte[] data, out Sound sound)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            sound = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            sound = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Sound))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Sound} are: {string.Join(", ", GetValidFileExtensions(ContentType.Sound))}");
            sound = new();
            return false;
        }
        
        try
        {
            sound = Raylib.LoadSoundFromWave(Raylib.LoadWaveFromMemory(extension, data));
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading sound from memory: {ex.Message}");
            sound = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a music stream from raw content data in memory.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".mp3", ".ogg", ".flac", ".mod", ".xm"). Will be checked against valid extensions for <see cref="ContentType.Music"/>.</param>
    /// <param name="data">The raw music data as a byte array.</param>
    /// <param name="music">The loaded Music object if successful; otherwise, a default Music.</param>
    /// <returns>True if the music stream was loaded successfully; otherwise, false.</returns>
    /// <remarks>
    /// Valid extensions for <see cref="ContentType.Music"/> can be managed via <see cref="AddValidFileExtension"/>, <see cref="RemoveValidFileExtension"/>, and <see cref="SetValidFileExtensions"/>.
    /// </remarks>
    public static bool TryLoadMusicFromMemory(string extension, byte[] data, out Music music)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            music = new();
            return false;
        }
        if (extension == string.Empty)
        {
            ShapeLogger.LogError($"Loading resource failed, extension is empty.");
            music = new();
            return false;
        }
        if (!IsValidExtension(extension, ContentType.Music))
        {
            ShapeLogger.LogError($"Loading resource failed, file extension is invalid: {extension}. Current Valid extensions for {ContentType.Music} are: {string.Join(", ", GetValidFileExtensions(ContentType.Music))}");
            music = new();
            return false;
        }
        
        try
        {
            music = Raylib.LoadMusicStreamFromMemory(extension, data);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading music from memory: {ex.Message}");
            music = new();
            return false;
        }
    }

    /// <summary>
    /// Attempts to load a fragment shader from raw content data in memory.
    /// </summary>
    /// <param name="data">The raw fragment shader data as a byte array.</param>
    /// <param name="shader">The loaded Shader object if successful; otherwise, a default Shader.</param>
    /// <returns>True if the fragment shader was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadFragmentShaderFromMemory(byte[] data, out Shader shader)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            shader = new();
            return false;
        }
        
        try
        {
            string file = Encoding.Default.GetString(data);
            shader = Raylib.LoadShaderFromMemory(null, file);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading fragment shader from memory: {ex.Message}");
            shader = new();
            return false;
        }
    }

    /// <summary>
    /// Attempts to load a vertex shader from raw content data in memory.
    /// </summary>
    /// <param name="data">The raw vertex shader data as a byte array.</param>
    /// <param name="shader">The loaded Shader object if successful; otherwise, a default Shader.</param>
    /// <returns>True if the vertex shader was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadVertexShaderFromMemory(byte[] data, out Shader shader)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            shader = new();
            return false;
        }
        
        try
        {
            string file = Encoding.Default.GetString(data);
            shader = Raylib.LoadShaderFromMemory(file, null);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading vertex shader from memory: {ex.Message}");
            shader = new();
            return false;
        }
    }
    /// <summary>
    /// Attempts to load a text string from raw content data in memory.
    /// </summary>
    /// <param name="data">The raw text data as a byte array.</param>
    /// <param name="text">The loaded text content if successful; otherwise, an empty string.</param>
    /// <returns>True if the text was loaded successfully; otherwise, false.</returns>
    public static bool TryLoadTextFromMemory(byte[] data, out string text)
    {
        if (data.Length <= 0)
        {
            ShapeLogger.LogError($"Loading resource failed, data has zero length.");
            text = string.Empty;
            return false;
        }
        
        try
        {
            text = Encoding.Default.GetString(data);
            return true;
        }
        catch (Exception ex)
        {
            ShapeLogger.LogError($"Error loading text from memory: {ex.Message}");
            text = string.Empty;
            return false;
        }
    }
    #endregion
    
    #region Load from Memory
    
    /// <summary>
    /// Loads a texture from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png").</param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <returns>The loaded Texture2D object.</returns>
    public static Texture2D LoadTextureFromMemory(string extension, byte[] data)
    {
        TryLoadTextureFromMemory(extension, data, out var texture);
        return texture;
    }

    /// <summary>
    /// Loads an image from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png").</param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <returns>The loaded Image object.</returns>
    public static Image LoadImageFromMemory(string extension, byte[] data)
    {
        TryLoadImageFromMemory(extension, data, out var image);
        return image;
    }

    /// <summary>
    /// Loads a font from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".ttf").</param>
    /// <param name="data">The raw font data as a byte array.</param>
    /// <param name="fontSize">The size of the font to load. Default is 100.</param>
    /// <returns>The loaded Font object.</returns>
    public static Font LoadFontFromMemory(string extension, byte[] data, int fontSize = 100)
    {
        TryLoadFontFromMemory(extension, data, out var font, fontSize);
        return font;
    }

    /// <summary>
    /// Loads a wave sound from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav").</param>
    /// <param name="data">The raw wave data as a byte array.</param>
    /// <returns>The loaded Wave object.</returns>
    public static Wave LoadWaveFromMemory(string extension, byte[] data)
    {
        TryLoadWaveFromMemory(extension, data, out var wave);
        return wave;
    }

    /// <summary>
    /// Loads a sound from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav").</param>
    /// <param name="data">The raw sound data as a byte array.</param>
    /// <returns>The loaded Sound object.</returns>
    public static Sound LoadSoundFromMemory(string extension, byte[] data)
    {
        TryLoadSoundFromMemory(extension, data, out var sound);
        return sound;
    }

    /// <summary>
    /// Loads a music stream from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".mp3").</param>
    /// <param name="data">The raw music data as a byte array.</param>
    /// <returns>The loaded Music object.</returns>
    public static Music LoadMusicFromMemory(string extension, byte[] data)
    {
        TryLoadMusicFromMemory(extension, data, out var music);
        return music;
    }

    /// <summary>
    /// Loads a fragment shader from raw content data.
    /// </summary>
    /// <param name="data">The raw shader data as a byte array.</param>
    /// <returns>The loaded Shader object.</returns>
    public static Shader LoadFragmentShaderFromMemory(byte[] data)
    {
        TryLoadFragmentShaderFromMemory(data, out var shader);
        return shader;
    }

    /// <summary>
    /// Loads a vertex shader from raw content data.
    /// </summary>
    /// <param name="data">The raw shader data as a byte array.</param>
    /// <returns>The loaded Shader object.</returns>
    public static Shader LoadVertexShaderFromMemory(byte[] data)
    {
        TryLoadVertexShaderFromMemory(data, out var shader);
        return shader;
    }

    /// <summary>
    /// Loads text content from raw data.
    /// </summary>
    /// <param name="data">The raw text data as a byte array.</param>
    /// <returns>The loaded text as a string.</returns>
    public static string LoadTextFromMemory(byte[] data)
    {
        TryLoadTextFromMemory(data, out string text);
        return text;
    }
    #endregion
    
    #region Load Directories
    
    /// <summary>
    /// Loads all fonts from a directory and returns a dictionary mapping relative file paths to Font objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing font files.</param>
    /// <param name="fontSize">The size of the fonts to load. Default is 100.</param>
    /// <param name="textureFilter">The texture filter to apply to the font textures. Default is Trilinear.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Font objects.</returns>
    /// <remarks>
    /// Only loads files with valid font extensions [".ttf", ".otf"].
    /// The key is the relative path from the directory to the font file.
    /// </remarks>
    public static Dictionary<string, Font> LoadFontsFromDirectory(string directoryPath, int fontSize = 100, TextureFilter textureFilter = TextureFilter.Trilinear, bool recursive = false)
    {
        string[] fontPaths = GetResourceFilePaths(directoryPath, ContentType.Font, recursive);
        var fontDict = new Dictionary<string, Font>(fontPaths.Length);

        foreach (string path in fontPaths)
        {
            if (!TryLoadFont(path, out var font)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            fontDict[relativePath] = font;
        }

        return fontDict;
    }
    
    /// <summary>
    /// Loads all fragment shaders from a directory and returns a dictionary mapping relative file paths to Shader objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing fragment shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Shader objects.</returns>
    /// <remarks>
    /// Only loads files with valid fragment shader extensions [".fs", ".glsl", ".frag"].
    /// The key is the relative path from the directory to the shader file.
    /// </remarks>
    public static Dictionary<string, Shader> LoadFragmentShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderFragment, recursive);
        var shaderDict = new Dictionary<string, Shader>(shaderPaths.Length);

        foreach (string path in shaderPaths)
        {
            if (!TryLoadFragmentShader(path, out var shader)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            shaderDict[relativePath] = shader;
        }

        return shaderDict;
    }
    
    /// <summary>
    /// Loads all vertex shaders from a directory and returns a dictionary mapping relative file paths to Shader objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing vertex shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Shader objects.</returns>
    /// <remarks>
    /// Only loads files with valid vertex shader extensions [".vs", ".glsl", ".vert"].
    /// The key is the relative path from the directory to the shader file.
    /// </remarks>
    public static Dictionary<string, Shader> LoadVertexShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderVertex, recursive);
        var shaderDict = new Dictionary<string, Shader>(shaderPaths.Length);

        foreach (string path in shaderPaths)
        {
            if (!TryLoadVertexShader(path, out var shader)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            shaderDict[relativePath] = shader;
        }

        return shaderDict;
    }
    
    /// <summary>
    /// Loads all textures from a directory and returns a dictionary mapping relative file paths to Texture2D objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing texture files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Texture2D objects.</returns>
    /// <remarks>
    /// Only loads files with valid texture extensions [".png", ".bmp", ".tga", ".jpg", ".jpeg", ".gif", ".psd", ".pkm", ".ktx", ".pvr", ".dds", ".hdr"].
    /// The key is the relative path from the directory to the texture file.
    /// </remarks>
    public static Dictionary<string, Texture2D> LoadTexturesFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] texturePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var textureDict = new Dictionary<string, Texture2D>(texturePaths.Length);

        foreach (string path in texturePaths)
        {
            if (!TryLoadTexture(path, out var texture)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            textureDict[relativePath] = texture;
        }

        return textureDict;
    }
    
    /// <summary>
    /// Loads all images from a directory and returns a dictionary mapping relative file paths to Image objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing image files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Image objects.</returns>
    /// <remarks>
    /// Only loads files with valid image extensions [".png", ".bmp", ".tga", ".jpg", ".jpeg", ".gif", ".psd", ".pkm", ".ktx", ".pvr", ".dds", ".hdr"].
    /// The key is the relative path from the directory to the image file.
    /// </remarks>
    public static Dictionary<string, Image> LoadImagesFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] imagePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var imageDict = new Dictionary<string, Image>(imagePaths.Length);

        foreach (string path in imagePaths)
        {
            if (!TryLoadImage(path, out var image)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            imageDict[relativePath] = image;
        }

        return imageDict;
    }
    
    /// <summary>
    /// Loads all wave sound files from a directory and returns a dictionary mapping relative file paths to Wave objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing wave sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Wave objects.</returns>
    /// <remarks>
    /// Only loads files with valid sound extensions [".wav", ".mp3", ".ogg", ".flac"].
    /// The key is the relative path from the directory to the wave file.
    /// </remarks>
    public static Dictionary<string, Wave> LoadWavesFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] wavePaths = GetResourceFilePaths(directoryPath, ContentType.Wave, recursive);
        var waveDict = new Dictionary<string, Wave>(wavePaths.Length);

        foreach (string path in wavePaths)
        {
            if (!TryLoadWave(path, out var wave)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            waveDict[relativePath] = wave;
        }

        return waveDict;
    }
    
    /// <summary>
    /// Loads all sound files from a directory and returns a dictionary mapping relative file paths to Sound objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Sound objects.</returns>
    /// <remarks>
    /// Only loads files with valid sound extensions [".wav", ".mp3", ".ogg", ".flac"].
    /// The key is the relative path from the directory to the sound file.
    /// </remarks>
    public static Dictionary<string, Sound> LoadSoundsFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] soundPaths = GetResourceFilePaths(directoryPath, ContentType.Sound, recursive);
        var soundDict = new Dictionary<string, Sound>(soundPaths.Length);

        foreach (string path in soundPaths)
        {
            if (!TryLoadSound(path, out var sound)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            soundDict[relativePath] = sound;
        }

        return soundDict;
    }
    
    /// <summary>
    /// Loads all music streams from a directory and returns a dictionary mapping relative file paths to Music objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing music files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded Music objects.</returns>
    /// <remarks>
    /// Only loads files with valid music extensions [".mp3", ".ogg", ".flac", ".mod", ".xm"].
    /// The key is the relative path from the directory to the music file.
    /// </remarks>
    public static Dictionary<string, Music> LoadMusicFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] musicPaths = GetResourceFilePaths(directoryPath, ContentType.Music, recursive);
        var musicDict = new Dictionary<string, Music>(musicPaths.Length);

        foreach (string path in musicPaths)
        {
            if (!TryLoadMusic(path, out var music)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            musicDict[relativePath] = music;
        }

        return musicDict;
    }
    
    /// <summary>
    /// Loads all text files from a directory and returns a dictionary mapping relative file paths to their contents.
    /// </summary>
    /// <param name="directoryPath">The directory path containing text files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping relative file paths to loaded text strings.</returns>
    /// <remarks>
    /// Only loads files with valid text extensions [".txt", ".json", ".xml", ".csv"].
    /// The key is the relative path from the directory to the text file.
    /// This should not be used for game save data, but rather for static text files that were shipped with the game.
    /// </remarks>
    public static Dictionary<string, string> LoadTextsFromDirectory(string directoryPath, bool recursive = false)
    {
        string[] textPaths = GetResourceFilePaths(directoryPath, ContentType.Text, recursive);
        var textDict = new Dictionary<string, string>(textPaths.Length);
        
        foreach (string path in textPaths)
        {
            if (!TryLoadText(path, out string text)) continue;
            string relativePath = Path.GetRelativePath(directoryPath, path);
            textDict[relativePath] = text;
        }
        
        return textDict;
    }
    #endregion
    
    #region Unload Content
    /// <summary>
    /// Unloads a font from memory.
    /// </summary>
    /// <param name="font">The Font object to unload.</param>
    public static void UnloadFont(Font font) { Raylib.UnloadFont(font); }

    /// <summary>
    /// Unloads multiple fonts from memory.
    /// </summary>
    /// <param name="fonts">The collection of Font objects to unload.</param>
    public static void UnloadFonts(IEnumerable<Font> fonts) { foreach (var font in fonts) UnloadFont(font); }

    /// <summary>
    /// Unloads a shader from memory.
    /// </summary>
    /// <param name="shader">The Shader object to unload.</param>
    public static void UnloadShader(Shader shader) { Raylib.UnloadShader(shader); }

    /// <summary>
    /// Unloads multiple shaders from memory.
    /// </summary>
    /// <param name="shaders">The collection of Shader objects to unload.</param>
    public static void UnloadShaders(IEnumerable<Shader> shaders) { foreach (var shader in shaders) UnloadShader(shader); }

    /// <summary>
    /// Unloads a texture from memory.
    /// </summary>
    /// <param name="texture">The Texture2D object to unload.</param>
    public static void UnloadTexture(Texture2D texture) { Raylib.UnloadTexture(texture); }

    /// <summary>
    /// Unloads multiple textures from memory.
    /// </summary>
    /// <param name="textures">The collection of Texture2D objects to unload.</param>
    public static void UnloadTextures(IEnumerable<Texture2D> textures) { foreach (var texture in textures) UnloadTexture(texture); }

    /// <summary>
    /// Unloads an image from memory.
    /// </summary>
    /// <param name="image">The Image object to unload.</param>
    public static void UnloadImage(Image image) { Raylib.UnloadImage(image); }

    /// <summary>
    /// Unloads multiple images from memory.
    /// </summary>
    /// <param name="images">The collection of Image objects to unload.</param>
    public static void UnloadImages(IEnumerable<Image> images) { foreach (var image in images) UnloadImage(image); }

    /// <summary>
    /// Unloads a wave sound from memory.
    /// </summary>
    /// <param name="wave">The Wave object to unload.</param>
    public static void UnloadWave(Wave wave) { Raylib.UnloadWave(wave); }

    /// <summary>
    /// Unloads multiple wave sounds from memory.
    /// </summary>
    /// <param name="waves">The collection of Wave objects to unload.</param>
    public static void UnloadWaves(IEnumerable<Wave> waves) { foreach (var wave in waves) UnloadWave(wave); }

    /// <summary>
    /// Unloads a sound from memory.
    /// </summary>
    /// <param name="sound">The Sound object to unload.</param>
    public static void UnloadSound(Sound sound) { Raylib.UnloadSound(sound); }

    /// <summary>
    /// Unloads multiple sounds from memory.
    /// </summary>
    /// <param name="sounds">The collection of Sound objects to unload.</param>
    public static void UnloadSounds(IEnumerable<Sound> sounds) { foreach (var sound in sounds) UnloadSound(sound); }

    /// <summary>
    /// Unloads a music stream from memory.
    /// </summary>
    /// <param name="musicStream">The Music object to unload.</param>
    public static void UnloadMusicStream(Music musicStream) { Raylib.UnloadMusicStream(musicStream); }

    /// <summary>
    /// Unloads multiple music streams from memory.
    /// </summary>
    /// <param name="musicStreams">The collection of Music objects to unload.</param>
    public static void UnloadMusicStreams(IEnumerable<Music> musicStreams) { foreach (var musicStream in musicStreams) UnloadMusicStream(musicStream); }
    #endregion

}
