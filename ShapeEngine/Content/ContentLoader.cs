using System.Text;
using Raylib_cs;
using ShapeEngine.Core.GameDef;


namespace ShapeEngine.Content;

//TODO: Add ShapeLogger logging instead of Console.WriteLine

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
    /// Specifies the number of font glyphs to load. Default value is 0, which loads the default character set.
    /// </summary>
    public static int GLYPH_COUNT = 0;

    /// <summary>
    /// Valid file extensions for different resource types
    /// </summary>
    private static readonly Dictionary<ContentType, string[]> ValidExtensions = new()
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
        Console.WriteLine($"--- MacOS app bundle loading resource from path: {fullPath}");
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

    public static bool IsValidExtension(string extension, ContentType contentType)
    {
        return ValidExtensions.ContainsKey(contentType) && ValidExtensions[contentType].Contains(extension);
    }

    public static ContentType? GetContentType(string extension)
    {
        foreach (var kvp in ValidExtensions)
        {
            if(kvp.Value.Contains(extension)) return kvp.Key;
        }

        return null;
    }

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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            var f = Raylib.LoadFontEx(filePath, fontSize, [], GLYPH_COUNT);
            Raylib.SetTextureFilter(f.Texture, textureFilter);
            return f;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading font from {filePath}: {ex.Message}");
        }
        
        return default;
        
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        try
        {
            return Raylib.LoadShader(null, filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fragment shader from {filePath}: {ex.Message}");
        }
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadShader(filePath, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading vertex shader from {filePath}: {ex.Message}");
        }
        
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadTexture(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading texture from {filePath}: {ex.Message}");
        }
        
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadImage(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image from {filePath}: {ex.Message}");
        }
        
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadWave(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading wave from {filePath}: {ex.Message}");
        }
        
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadSound(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sound from {filePath}: {ex.Message}");
        }
        
        return default;
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
    public static Music LoadMusicStream(string filePath)
    {
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return Raylib.LoadMusicStream(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading music from {filePath}: {ex.Message}");
        }
        
        return default;
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
        if (Game.IsOSX())
        {
            filePath = GetMacOsAppBundleResourcePath(filePath);
        }
        
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading text from {filePath}: {ex.Message}");
        }
        
        return string.Empty;
    }

    #endregion
    
    #region Load from Data
    
    /// <summary>
    /// Loads a texture from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png").</param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <returns>The loaded Texture2D object.</returns>
    public static Texture2D LoadTextureFromMemory(string extension, byte[] data)
    {
        return Raylib.LoadTextureFromImage(LoadImageFromMemory(extension, data));
    }

    /// <summary>
    /// Loads an image from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".png").</param>
    /// <param name="data">The raw image data as a byte array.</param>
    /// <returns>The loaded Image object.</returns>
    public static Image LoadImageFromMemory(string extension, byte[] data)
    {
        return Raylib.LoadImageFromMemory(extension, data);
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
        return Raylib.LoadFontFromMemory(extension, data, fontSize, [], GLYPH_COUNT);
    }

    /// <summary>
    /// Loads a wave sound from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav").</param>
    /// <param name="data">The raw wave data as a byte array.</param>
    /// <returns>The loaded Wave object.</returns>
    public static Wave LoadWaveFromMemory(string extension, byte[] data)
    {
        return Raylib.LoadWaveFromMemory(extension, data);
    }

    /// <summary>
    /// Loads a sound from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".wav").</param>
    /// <param name="data">The raw sound data as a byte array.</param>
    /// <returns>The loaded Sound object.</returns>
    public static Sound LoadSoundFromMemory(string extension, byte[] data)
    {
        return Raylib.LoadSoundFromWave(LoadWaveFromMemory(extension, data));
    }

    /// <summary>
    /// Loads a music stream from raw content data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".mp3").</param>
    /// <param name="data">The raw music data as a byte array.</param>
    /// <returns>The loaded Music object.</returns>
    public static Music LoadMusicFromMemory(string extension, byte[] data)
    {
        return Raylib.LoadMusicStreamFromMemory(extension, data);
    }

    /// <summary>
    /// Loads a fragment shader from raw content data.
    /// </summary>
    /// <param name="data">The raw shader data as a byte array.</param>
    /// <returns>The loaded Shader object.</returns>
    public static Shader LoadFragmentShaderFromMemory(byte[] data)
    {
        string file = Encoding.Default.GetString(data);
        return Raylib.LoadShaderFromMemory(null, file);
    }

    /// <summary>
    /// Loads a vertex shader from raw content data.
    /// </summary>
    /// <param name="data">The raw shader data as a byte array.</param>
    /// <returns>The loaded Shader object.</returns>
    public static Shader LoadVertexShaderFromMemory(byte[] data)
    {
        string file = Encoding.Default.GetString(data);
        return Raylib.LoadShaderFromMemory(file, null);
    }

    /// <summary>
    /// Loads text content from raw data.
    /// </summary>
    /// <param name="extension">The file extension (e.g. ".txt").</param>
    /// <param name="data">The raw text data as a byte array.</param>
    /// <returns>The loaded text as a string.</returns>
    public static string LoadTextFromMemory(string extension, byte[] data)
    {
        return Encoding.Default.GetString(data);
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
        var fontPaths = GetResourceFilePaths(directoryPath, ContentType.Font, recursive);
        var fontDict = new Dictionary<string, Font>(fontPaths.Length);

        foreach (var path in fontPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                fontDict[relativePath] = LoadFont(path, fontSize, textureFilter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading font from {path}: {ex.Message}");
            }
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
        var shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderFragment, recursive);
        var shaderDict = new Dictionary<string, Shader>(shaderPaths.Length);

        foreach (var path in shaderPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                shaderDict[relativePath] = LoadFragmentShader(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fragment shader from {path}: {ex.Message}");
            }
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
        var shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderVertex, recursive);
        var shaderDict = new Dictionary<string, Shader>(shaderPaths.Length);

        foreach (var path in shaderPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                shaderDict[relativePath] = LoadVertexShader(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vertex shader from {path}: {ex.Message}");
            }
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
        var texturePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var textureDict = new Dictionary<string, Texture2D>(texturePaths.Length);

        foreach (var path in texturePaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                textureDict[relativePath] = LoadTexture(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading texture from {path}: {ex.Message}");
            }
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
        var imagePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var imageDict = new Dictionary<string, Image>(imagePaths.Length);

        foreach (var path in imagePaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                imageDict[relativePath] = LoadImage(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from {path}: {ex.Message}");
            }
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
        var wavePaths = GetResourceFilePaths(directoryPath, ContentType.Wave, recursive);
        var waveDict = new Dictionary<string, Wave>(wavePaths.Length);

        foreach (var path in wavePaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                waveDict[relativePath] = LoadWave(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading wave sound from {path}: {ex.Message}");
            }
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
        var soundPaths = GetResourceFilePaths(directoryPath, ContentType.Sound, recursive);
        var soundDict = new Dictionary<string, Sound>(soundPaths.Length);

        foreach (var path in soundPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                soundDict[relativePath] = LoadSound(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound from {path}: {ex.Message}");
            }
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
        var musicPaths = GetResourceFilePaths(directoryPath, ContentType.Music, recursive);
        var musicDict = new Dictionary<string, Music>(musicPaths.Length);

        foreach (var path in musicPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                musicDict[relativePath] = LoadMusicStream(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading music stream from {path}: {ex.Message}");
            }
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
        var textPaths = GetResourceFilePaths(directoryPath, ContentType.Text, recursive);
        var textDict = new Dictionary<string, string>(textPaths.Length);
        
        foreach (var path in textPaths)
        {
            try
            {
                string relativePath = Path.GetRelativePath(directoryPath, path);
                textDict[relativePath] = LoadText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading text from {path}: {ex.Message}");
            }
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
