using System.Text;
using Raylib_cs;
using ShapeEngine.Core.GameDef;


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
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;
            
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
        
        if (!Directory.Exists(directoryPath))
            return Array.Empty<string>();
            
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        return Directory.GetFiles(directoryPath, "*.*", searchOption)
            .Where(file => IsValidResourceFile(file, contentType))
            .ToArray();
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
        var f = Raylib.LoadFontEx(filePath, fontSize, [], GLYPH_COUNT);
        Raylib.SetTextureFilter(f.Texture, textureFilter);
        return f;
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
        return Raylib.LoadShader(null, filePath);
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
        return Raylib.LoadShader(filePath, "");
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
        return Raylib.LoadTexture(filePath);
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
        return Raylib.LoadImage(filePath);
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
        return Raylib.LoadWave(filePath);
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
        return Raylib.LoadSound(filePath);
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
        return Raylib.LoadMusicStream(filePath);
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
        return File.ReadAllText(filePath);
    }

    #endregion
    
    #region Load from ContentInfo
    
    /// <summary>
    /// Loads a texture from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing texture data and extension.</param>
    /// <returns>The loaded Texture2D object.</returns>
    public static Texture2D LoadTextureFromContent(ContentInfo content)
    {
        return Raylib.LoadTextureFromImage(LoadImageFromContent(content));
    }
    /// <summary>
    /// Loads an image from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing image data and extension.</param>
    /// <returns>The loaded Image object.</returns>
    public static Image LoadImageFromContent(ContentInfo content)
    {
        byte[] data = content.data;
        string extension = content.extension;
        return Raylib.LoadImageFromMemory(extension, data);
    }
    /// <summary>
    /// Loads a font from ContentInfo data with the specified font size.
    /// </summary>
    /// <param name="content">The ContentInfo object containing font data and extension.</param>
    /// <param name="fontSize">The size of the font to load. Default is 100.</param>
    /// <returns>The loaded Font object.</returns>
    public static Font LoadFontFromContent(ContentInfo content, int fontSize = 100)
    {
        byte[] data = content.data;
        string extension = content.extension;
        return Raylib.LoadFontFromMemory(extension, data, fontSize, Array.Empty<int>(), GLYPH_COUNT);
        
    }
    /// <summary>
    /// Loads a wave sound from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing wave sound data and extension.</param>
    /// <returns>The loaded Wave object.</returns>
    public static Wave LoadWaveFromContent(ContentInfo content)
    {
        byte[] data = content.data;
        string extension = content.extension;
        return Raylib.LoadWaveFromMemory(extension, data);
    }
    /// <summary>
    /// Loads a sound from ContentInfo data by first converting it to a Wave object.
    /// </summary>
    /// <param name="content">The ContentInfo object containing sound data and extension.</param>
    /// <returns>The loaded Sound object.</returns>
    public static Sound LoadSoundFromContent(ContentInfo content)
    {
        return Raylib.LoadSoundFromWave(LoadWaveFromContent(content));

    }
    /// <summary>
    /// Loads a music stream from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing music data and extension.</param>
    /// <returns>The loaded Music object.</returns>
    public static Music LoadMusicFromContent(ContentInfo content)
    {
        byte[] data = content.data;
        string extension = content.extension;
        return Raylib.LoadMusicStreamFromMemory(extension, data);
    }
    /// <summary>
    /// Loads a fragment shader from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing shader code as binary data.</param>
    /// <returns>The loaded Shader object configured as a fragment shader.</returns>
    public static Shader LoadFragmentShaderFromContent(ContentInfo content)
    {
        string file = Encoding.Default.GetString(content.data);
        return Raylib.LoadShaderFromMemory(null, file);
    }
    /// <summary>
    /// Loads a vertex shader from ContentInfo data.
    /// </summary>
    /// <param name="content">The ContentInfo object containing shader code as binary data.</param>
    /// <returns>The loaded Shader object configured as a vertex shader.</returns>
    public static Shader LoadVertexShaderFromContent(ContentInfo content)
    {
        string file = Encoding.Default.GetString(content.data);
        return Raylib.LoadShaderFromMemory(file, null);
    }
    /// <summary>
    /// Decodes binary text data from a ContentInfo object to a string.
    /// This should not be used for game save data,
    /// but rather for static text files like JSON or XML that were shipped with the game!
    /// </summary>
    /// <param name="content">The ContentInfo object containing text data as a byte array.</param>
    /// <returns>The decoded string from the content data.</returns>
    public static string LoadTextFromContent(ContentInfo content)
    {
        return Encoding.Default.GetString(content.data);
    }

    #endregion
    
    #region Load Directories

    /// <summary>
    /// Loads all fonts from a directory with the specified font size and texture filter.
    /// </summary>
    /// <param name="directoryPath">The directory path containing font files.</param>
    /// <param name="fontSize">The size of the fonts to load. Default is 100.</param>
    /// <param name="textureFilter">The texture filter to apply to the font textures. Default is Trilinear.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Font objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid font extensions (.ttf, .otf).
    /// </remarks>
    public static List<Font> LoadFontsFromDirectory(string directoryPath, int fontSize = 100, TextureFilter textureFilter = TextureFilter.Trilinear, bool recursive = false)
    {
        var fontPaths = GetResourceFilePaths(directoryPath, ContentType.Font, recursive);
        var fonts = new List<Font>(fontPaths.Length);
        
        foreach (var path in fontPaths)
        {
            try
            {
                fonts.Add(LoadFont(path, fontSize, textureFilter));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading font from {path}: {ex.Message}");
            }
        }
        
        return fonts;
    }
    
    /// <summary>
    /// Loads all fragment shaders from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing fragment shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Shader objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid shader extensions (.fs, .frag, etc.).
    /// </remarks>
    public static List<Shader> LoadFragmentShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderFragment, recursive);
        var shaders = new List<Shader>(shaderPaths.Length);
        
        foreach (var path in shaderPaths)
        {
            try
            {
                shaders.Add(LoadFragmentShader(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fragment shader from {path}: {ex.Message}");
            }
        }
        
        return shaders;
    }
    
    /// <summary>
    /// Loads all vertex shaders from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing vertex shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Shader objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid shader extensions (.vs, .vert, etc.).
    /// </remarks>
    public static List<Shader> LoadVertexShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var shaderPaths = GetResourceFilePaths(directoryPath, ContentType.ShaderVertex, recursive);
        var shaders = new List<Shader>(shaderPaths.Length);
        
        foreach (var path in shaderPaths)
        {
            try
            {
                shaders.Add(LoadVertexShader(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vertex shader from {path}: {ex.Message}");
            }
        }
        
        return shaders;
    }
    
    /// <summary>
    /// Loads all textures from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing texture files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Texture2D objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid texture extensions (.png, .jpg, etc.).
    /// </remarks>
    public static List<Texture2D> LoadTexturesFromDirectory(string directoryPath, bool recursive = false)
    {
        var texturePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var textures = new List<Texture2D>(texturePaths.Length);
        
        foreach (var path in texturePaths)
        {
            try
            {
                textures.Add(LoadTexture(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading texture from {path}: {ex.Message}");
            }
        }
        
        return textures;
    }
    
    /// <summary>
    /// Loads all images from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing image files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Image objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid image extensions (.png, .jpg, etc.).
    /// </remarks>
    public static List<Image> LoadImagesFromDirectory(string directoryPath, bool recursive = false)
    {
        var imagePaths = GetResourceFilePaths(directoryPath, ContentType.Texture, recursive);
        var images = new List<Image>(imagePaths.Length);
        
        foreach (var path in imagePaths)
        {
            try
            {
                images.Add(LoadImage(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from {path}: {ex.Message}");
            }
        }
        
        return images;
    }
    
    /// <summary>
    /// Loads all wave sounds from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing wave sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Wave objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid sound extensions (.wav, .mp3, etc.).
    /// </remarks>
    public static List<Wave> LoadWavesFromDirectory(string directoryPath, bool recursive = false)
    {
        var wavePaths = GetResourceFilePaths(directoryPath, ContentType.Sound, recursive);
        var waves = new List<Wave>(wavePaths.Length);
        
        foreach (var path in wavePaths)
        {
            try
            {
                waves.Add(LoadWave(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading wave sound from {path}: {ex.Message}");
            }
        }
        
        return waves;
    }
    
    /// <summary>
    /// Loads all sounds from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Sound objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid sound extensions (.wav, .mp3, etc.).
    /// </remarks>
    public static List<Sound> LoadSoundsFromDirectory(string directoryPath, bool recursive = false)
    {
        var soundPaths = GetResourceFilePaths(directoryPath, ContentType.Sound, recursive);
        var sounds = new List<Sound>(soundPaths.Length);
        
        foreach (var path in soundPaths)
        {
            try
            {
                sounds.Add(LoadSound(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound from {path}: {ex.Message}");
            }
        }
        
        return sounds;
    }
    
    /// <summary>
    /// Loads all music streams from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing music files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded Music objects.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid music extensions (.mp3, .ogg, etc.).
    /// </remarks>
    public static List<Music> LoadMusicStreamsFromDirectory(string directoryPath, bool recursive = false)
    {
        var musicPaths = GetResourceFilePaths(directoryPath, ContentType.Music, recursive);
        var musicStreams = new List<Music>(musicPaths.Length);
        
        foreach (var path in musicPaths)
        {
            try
            {
                musicStreams.Add(LoadMusicStream(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading music stream from {path}: {ex.Message}");
            }
        }
        
        return musicStreams;
    }
    
    /// <summary>
    /// Loads all text files from a directory as strings.
    /// </summary>
    /// <param name="directoryPath">The directory path containing text files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A list of loaded text strings.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid text extensions (.txt, .json, etc.).
    /// This should not be used for game save data, but rather for static text files that were shipped with the game!
    /// </remarks>
    public static List<string> LoadTextsFromDirectory(string directoryPath, bool recursive = false)
    {
        var textPaths = GetResourceFilePaths(directoryPath, ContentType.Text, recursive);
        var texts = new List<string>(textPaths.Length);
        
        foreach (var path in textPaths)
        {
            try
            {
                texts.Add(LoadText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading text from {path}: {ex.Message}");
            }
        }
        
        return texts;
    }
    
    /// <summary>
    /// Loads all text files from a directory as key-value pairs, where the key is the file name (without extension) and the value is the file content.
    /// </summary>
    /// <param name="directoryPath">The directory path containing text files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to their contents.</returns>
    /// <remarks>
    /// Automatically resolves resource paths for macOS application bundles.
    /// Only loads files with valid text extensions (.txt, .json, etc.).
    /// This should not be used for game save data, but rather for static text files that were shipped with the game!
    /// </remarks>
    public static Dictionary<string, string> LoadTextsWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var textPaths = GetResourceFilePaths(directoryPath, ContentType.Text, recursive);
        var textDict = new Dictionary<string, string>(textPaths.Length);
        
        foreach (var path in textPaths)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                textDict[fileName] = LoadText(path);
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
