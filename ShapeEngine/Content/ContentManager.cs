using Raylib_cs;
using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Content;


/// <summary>
/// Manages loading, caching, and unloading of various content types (textures, shaders, fonts, sounds, etc.)
/// from disk and content packs for the game.
/// Provides methods to retrieve and manage content efficiently.
/// </summary>
public sealed class ContentManager
{
    #region Members
    /// <summary>
    /// The root directory for all content managed by this ContentManager.
    /// </summary>
    public readonly string RootDirectory;

    /// <summary>
    /// Caches loaded shaders by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Shader> loadedShaders = new();

    /// <summary>
    /// Caches loaded textures by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Texture2D> loadedTextures = new();

    /// <summary>
    /// Caches loaded images by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Image> loadedImages = new();

    /// <summary>
    /// Caches loaded fonts by their relative file path and font size.
    /// </summary>
    private readonly Dictionary<string, Dictionary<int, Font>> loadedFonts = new();

    /// <summary>
    /// Caches loaded sounds by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Sound> loadedSounds = new();

    /// <summary>
    /// Caches loaded music streams by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Music> loadedMusicStreams = new();

    /// <summary>
    /// Caches loaded wave data by their relative file path.
    /// </summary>
    private readonly Dictionary<string, Wave> loadedWaves = new();

    /// <summary>
    /// Caches loaded text files by their relative file path.
    /// </summary>
    private readonly Dictionary<string, string> loadedTexts = new();

    /// <summary>
    /// Stores all loaded content packs, indexed by their root path.
    /// </summary>
    private readonly Dictionary<string, ContentPack> contentPacks = new();

    /// <summary>
    /// Helper list for splitting and processing content paths.
    /// </summary>
    private readonly List<string> pathParts = [];

    /// <summary>
    /// Helper list for storing possible root paths when searching for content packs.
    /// </summary>
    private readonly List<string> possibleRoots = [];

    /// <summary>
    /// Directory separator characters used for splitting paths.
    /// </summary>
    private static readonly char[] separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class with the specified root directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory for all content managed by this ContentManager.</param>
    /// <remarks>
    /// The <paramref name="rootDirectory"/> parameter specifies the base directory from which all content paths are resolved.
    /// It acts as the root for loading, caching, and managing content files.
    /// All relative file paths used by the ContentManager are interpreted with respect to this directory.
    /// If a relative path used for loading a resource contains the <see cref="RootDirectory"/>,
    /// the <see cref="RootDirectory"/> will be stripped from the path before further processing.
    /// If no content pack is found for a given relative path, the ContentManager will attempt to load the resource
    /// directly from the file system using the combined path of <see cref="RootDirectory"/> and the relative path.
    /// </remarks>
    public ContentManager(string rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    #endregion
    
    #region Content Packs
    /// <summary>
    /// Adds a content pack to the manager with the specified root path.
    /// </summary>
    /// <param name="pack">The content pack to add.</param>
    /// <param name="root">The root path under which the content pack will be accessible.</param>
    /// <returns>True if the content pack was added successfully; false if the root is empty or already exists.</returns>
    public bool AddContentPack(ContentPack pack, string root)
    {
        if (root == string.Empty)
        {
            Game.Instance.Logger.LogWarning("Content pack root cannot be empty!");
            return false;
        }
        
        if (contentPacks.TryAdd(root, pack)) return true;
        Game.Instance.Logger.LogWarning($"Content pack with root '{root}' is already loaded.");
        return false;
    }
    /// <summary>
    /// Removes a content pack from the manager by its root path.
    /// </summary>
    /// <param name="root">The root path of the content pack to remove.</param>
    /// <param name="clear">If true, clears the content pack before removal.</param>
    /// <returns>True if the content pack was removed; false if not found.</returns>
    public bool RemoveContentPack(string root, bool clear = true)
    {
        if (contentPacks.Remove(root, out var pack))
        {
            if(clear) pack.Clear();
            return true;
        }
        
        Game.Instance.Logger.LogWarning($"No content pack with root '{root}' found.");
        return false;
    }
    /// <summary>
    /// Removes a content pack from the manager by its instance.
    /// </summary>
    /// <param name="pack">The content pack instance to remove.</param>
    /// <param name="clear">If true, clears the content pack before removal.</param>
    /// <returns>True if the content pack was removed; false if not found.</returns>
    public bool RemoveContentPack(ContentPack pack, bool clear = true)
    {
        var item = contentPacks.FirstOrDefault(x => x.Value == pack);
        if (item.Value != null)
        {
            if(clear) pack.Clear();
            return contentPacks.Remove(item.Key);
        }
        
        Game.Instance.Logger.LogWarning("Content pack not found.");
        return false;
    }
    /// <summary>
    /// Removes all content packs from the manager.
    /// </summary>
    /// <param name="clear">If true, clears each content pack before removal.</param>
    public void RemoveAllContentPacks(bool clear = true)
    {
        if (clear) UnloadAllContentPacks();
        contentPacks.Clear();
    }
    /// <summary>
    /// Gets a list of all content packs managed by this ContentManager.
    /// </summary>
    public List<ContentPack> GetAllContentPacks()
    {
        return contentPacks.Values.ToList();
    }
    /// <summary>
    /// Gets a list of all loaded content packs managed by this ContentManager.
    /// Only content packs with <c>IsLoaded</c> set to true are included.
    /// </summary>
    public List<ContentPack> GetAllLoadedContentPacks()
    {
        return contentPacks.Values.Where(x => x.IsLoaded).ToList();
    }
    /// <summary>
    /// Checks if a content pack with the specified root path exists in the manager.
    /// </summary>
    /// <param name="root">The root path of the content pack to check for existence.</param>
    /// <returns>True if the content pack exists; otherwise, false.</returns>
    public bool HasContentPack(string root)
    {
        return contentPacks.ContainsKey(root);
    }
    
    /// <summary>
    /// Tries to get a content pack by its root path.
    /// </summary>
    /// <param name="root">The root path of the content pack.</param>
    /// <param name="pack">The found content pack, or null if not found.</param>
    /// <returns>True if the content pack exists; otherwise, false.</returns>
    public bool TryGetContentPack(string root, out ContentPack? pack)
    {
        return contentPacks.TryGetValue(root, out pack);
    }   
    #endregion
    
    #region Try Load Content
    /// <summary>
    /// Attempts to load a texture from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the texture to load.</param>
    /// <param name="texture">The loaded <see cref="Texture2D"/> if successful; otherwise, default.</param>
    /// <returns>True if the texture was loaded successfully; otherwise, false.</returns>
    public bool TryLoadTexture(string filePath, out Texture2D texture)
    {
        texture = default;
    
        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;
    
        // Check cache first
        if (loadedTextures.TryGetValue(relativePath, out texture))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadTexture(packFilePath, out var packTexture))
            {
                Game.Instance.Logger.LogInfo($"Texture '{relativePath}' loaded from content pack {pack.SourceFilePath}.");
                texture = packTexture;
                loadedTextures[relativePath] = packTexture;
                return true;
            }
        }
        
        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadTexture(relativePath, out var packTexture))
            {
                Game.Instance.Logger.LogInfo($"Texture '{relativePath}' loaded from root content pack {rootPack.SourceFilePath}.");
                texture = packTexture;
                loadedTextures[relativePath] = packTexture;
                return true;
            }
        }
        
        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadTexture(contentPath, out var loadedTexture))
        {
            return false;
        }
        
        Game.Instance.Logger.LogInfo($"Texture '{relativePath}' loaded from file system.");
        loadedTextures[relativePath] = loadedTexture;
        texture = loadedTexture;
        return true;
    }
    /// <summary>
    /// Attempts to load a fragment shader from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the fragment shader to load.</param>
    /// <param name="shader">The loaded <see cref="Shader"/> if successful; otherwise, default.</param>
    /// <returns>True if the fragment shader was loaded successfully; otherwise, false.</returns>
    public bool TryLoadFragmentShader(string filePath, out Shader shader)
    {
        shader = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedShaders.TryGetValue(relativePath, out shader))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadFragmentShader(packFilePath, out var packShader))
            {
                Game.Instance.Logger.LogInfo($"Fragment shader '{relativePath}' loaded from content pack  {pack.SourceFilePath}.");
                shader = packShader;
                loadedShaders[relativePath] = packShader;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadFragmentShader(relativePath, out var packShader))
            {
                Game.Instance.Logger.LogInfo($"Fragment shader '{relativePath}' loaded from root content pack  {rootPack.SourceFilePath}.");
                shader = packShader;
                loadedShaders[relativePath] = packShader;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadFragmentShader(contentPath, out var loadedShader)) return false;

        Game.Instance.Logger.LogInfo($"Fragment shader '{relativePath}' loaded from file system.");
        loadedShaders[relativePath] = loadedShader;
        shader = loadedShader;
        return true;
    }
    /// <summary>
    /// Attempts to load a vertex shader from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the vertex shader to load.</param>
    /// <param name="shader">The loaded <see cref="Shader"/> if successful; otherwise, default.</param>
    /// <returns>True if the vertex shader was loaded successfully; otherwise, false.</returns>
    public bool TryLoadVertexShader(string filePath, out Shader shader)
    {
        shader = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedShaders.TryGetValue(relativePath, out shader))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadVertexShader(packFilePath, out var packShader))
            {
                Game.Instance.Logger.LogInfo($"Vertex shader '{relativePath}' loaded from content pack  {pack.SourceFilePath}.");
                shader = packShader;
                loadedShaders[relativePath] = packShader;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadVertexShader(relativePath, out var packShader))
            {
                Game.Instance.Logger.LogInfo($"Vertex shader '{relativePath}' loaded from root content pack  {rootPack.SourceFilePath}.");
                shader = packShader;
                loadedShaders[relativePath] = packShader;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadVertexShader(contentPath, out var loadedShader)) return false;

        loadedShaders[relativePath] = loadedShader;
        shader = loadedShader;
        return true;
    }
    /// <summary>
    /// Attempts to load an image from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the image to load.</param>
    /// <param name="image">The loaded <see cref="Image"/> if successful; otherwise, default.</param>
    /// <returns>True if the image was loaded successfully; otherwise, false.</returns>
    public bool TryLoadImage(string filePath, out Image image)
    {
        image = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedImages.TryGetValue(relativePath, out image))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadImage(packFilePath, out var packImage))
            {
                Game.Instance.Logger.LogInfo($"Image '{relativePath}' loaded from content pack  {pack.SourceFilePath}.");
                image = packImage;
                loadedImages[relativePath] = packImage;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadImage(relativePath, out var packImage))
            {
                Game.Instance.Logger.LogInfo($"Image '{relativePath}' loaded from root content pack  {rootPack.SourceFilePath}.");
                image = packImage;
                loadedImages[relativePath] = packImage;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadImage(contentPath, out var loadedImage)) return false;

        Game.Instance.Logger.LogInfo($"Image '{relativePath}' loaded from file system.");
        loadedImages[relativePath] = loadedImage;
        image = loadedImage;
        return true;
    }
    /// <summary>
    /// Attempts to load a font from the specified file path with the given font size.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the font to load.</param>
    /// <param name="font">The loaded <see cref="Font"/> if successful; otherwise, default.</param>
    /// <param name="fontSize">The size of the font to load. Defaults to 100.</param>
    /// <returns>True if the font was loaded successfully; otherwise, false.</returns>
    public bool TryLoadFont(string filePath, out Font font, int fontSize = 100)
    {
        font = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;
        // Check cache first
        if (loadedFonts.TryGetValue(relativePath, out var fontDict))
        {
            if (fontDict.TryGetValue(fontSize, out font))
            {
                Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
                return true;
            }
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadFont(packFilePath, out var packFont, fontSize))
            {
                Game.Instance.Logger.LogInfo($"Font '{relativePath}' loaded from content pack {pack.SourceFilePath} with font size: {fontSize}.");
                font = packFont;
                if (!loadedFonts.ContainsKey(relativePath)) loadedFonts[relativePath] = new Dictionary<int, Font>();
                loadedFonts[relativePath][fontSize] = packFont;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadFont(relativePath, out var packFont, fontSize))
            {
                Game.Instance.Logger.LogInfo($"Font '{relativePath}' loaded from root content pack  {rootPack.SourceFilePath} with font size: {fontSize}.");
                font = packFont;
                if (!loadedFonts.ContainsKey(relativePath)) loadedFonts[relativePath] = new Dictionary<int, Font>();
                loadedFonts[relativePath][fontSize] = packFont;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadFont(contentPath, out var loadedFont, fontSize))
        {
            return false;
        }

        Game.Instance.Logger.LogInfo($"Font '{relativePath}' loaded from file system with font size: {fontSize}.");
        if (!loadedFonts.ContainsKey(relativePath)) loadedFonts[relativePath] = new Dictionary<int, Font>();
        loadedFonts[relativePath][fontSize] = loadedFont;
        font = loadedFont;
        return true;
    }
    /// <summary>
    /// Attempts to load a sound from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the sound to load.</param>
    /// <param name="sound">The loaded <see cref="Sound"/> if successful; otherwise, default.</param>
    /// <returns>True if the sound was loaded successfully; otherwise, false.</returns>
    public bool TryLoadSound(string filePath, out Sound sound)
    {
        sound = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedSounds.TryGetValue(relativePath, out sound))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadSound(packFilePath, out var packSound))
            {
                Game.Instance.Logger.LogInfo($"Sound '{relativePath}' loaded from content pack {pack.SourceFilePath}.");
                sound = packSound;
                loadedSounds[relativePath] = packSound;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadSound(relativePath, out var packSound))
            {
                Game.Instance.Logger.LogInfo($"Sound '{relativePath}' loaded from root content pack {rootPack.SourceFilePath}.");
                sound = packSound;
                loadedSounds[relativePath] = packSound;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadSound(contentPath, out var loadedSound)) return false;

        Game.Instance.Logger.LogInfo($"Sound '{relativePath}' loaded from file system.");
        loadedSounds[relativePath] = loadedSound;
        sound = loadedSound;
        return true;
    }
    /// <summary>
    /// Attempts to load a music stream from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the music stream to load.</param>
    /// <param name="music">The loaded <see cref="Music"/> if successful; otherwise, default.</param>
    /// <returns>True if the music stream was loaded successfully; otherwise, false.</returns>
    public bool TryLoadMusic(string filePath, out Music music)
    {
        music = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedMusicStreams.TryGetValue(relativePath, out music))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadMusic(packFilePath, out var packMusic))
            {
                Game.Instance.Logger.LogInfo($"Music '{relativePath}' loaded from content pack {pack.SourceFilePath}.");
                music = packMusic;
                loadedMusicStreams[relativePath] = packMusic;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadMusic(relativePath, out var packMusic))
            {
                Game.Instance.Logger.LogInfo($"Music '{relativePath}' loaded from root content pack {rootPack.SourceFilePath}.");
                music = packMusic;
                loadedMusicStreams[relativePath] = packMusic;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadMusic(contentPath, out var loadedMusic)) return false;

        Game.Instance.Logger.LogInfo($"Music '{relativePath}' loaded from file system.");
        loadedMusicStreams[relativePath] = loadedMusic;
        music = loadedMusic;
        return true;
    }
    /// <summary>
    /// Attempts to load a wave from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the wave to load.</param>
    /// <param name="wave">The loaded <see cref="Wave"/> if successful; otherwise, default.</param>
    /// <returns>True if the wave was loaded successfully; otherwise, false.</returns>
    public bool TryLoadWave(string filePath, out Wave wave)
    {
        wave = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedWaves.TryGetValue(relativePath, out wave))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadWave(packFilePath, out var packWave))
            {
                Game.Instance.Logger.LogInfo($"Wave '{relativePath}' loaded from content pack  {pack.SourceFilePath}.");
                wave = packWave;
                loadedWaves[relativePath] = packWave;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadWave(relativePath, out var packWave))
            {
                Game.Instance.Logger.LogInfo($"Wave '{relativePath}' loaded from root content pack  {rootPack.SourceFilePath}.");
                wave = packWave;
                loadedWaves[relativePath] = packWave;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadWave(contentPath, out var loadedWave)) return false;

        Game.Instance.Logger.LogInfo($"Wave '{relativePath}' loaded from file system.");
        loadedWaves[relativePath] = loadedWave;
        wave = loadedWave;
        return true;
    }
    /// <summary>
    /// Attempts to load a text file from the specified file path.
    /// Checks the cache, content packs, and file system in order.
    /// </summary>
    /// <param name="filePath">The file path to the text file to load.</param>
    /// <param name="text">The loaded text if successful; otherwise, an empty string.</param>
    /// <returns>True if the text was loaded successfully; otherwise, false.</returns>
    public bool TryLoadText(string filePath, out string text)
    {
        text = string.Empty;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedTexts.TryGetValue(relativePath, out string? cachedText))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            text = cachedText;
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadText(packFilePath, out var packText))
            {
                Game.Instance.Logger.LogInfo($"Text '{relativePath}' loaded from content pack {pack.SourceFilePath}.");
                text = packText;
                loadedTexts[relativePath] = packText;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadText(relativePath, out var packText))
            {
                Game.Instance.Logger.LogInfo($"Text '{relativePath}' loaded from root content pack {rootPack.SourceFilePath}.");
                text = packText;
                loadedTexts[relativePath] = packText;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadText(contentPath, out var loadedText)) return false;

        Game.Instance.Logger.LogInfo($"Text '{relativePath}' loaded from file system.");
        loadedTexts[relativePath] = loadedText;
        text = loadedText;
        return true;
    }
    #endregion
    
    #region Unloading
    
    /// <summary>
    /// Unloads all content packs by calling <c>Clear()</c> on each loaded <see cref="ContentPack"/>.
    /// Does not remove the content packs from the manager; use <see cref="RemoveAllContentPacks"/> to remove them.
    /// </summary>
    public void UnloadAllContentPacks()
    {
        Game.Instance.Logger.LogInfo($"ContentManager unloading all {contentPacks.Count} content packs.");
        foreach (var pack in contentPacks.Values)
        {
            pack.Clear();
        }
        Game.Instance.Logger.LogInfo($"Unloading all content packs finished.");
        
    }
    
    /// <summary>
    /// Unloads all cached content (shaders, textures, images, fonts, waves, sounds, music streams, and texts)
    /// from the ContentManager, releasing their resources. Logs each unload operation.
    /// </summary>
    public void UnloadContent()
    {
        Game.Instance.Logger.LogInfo("ContentManager unloading content.");
        int unloadedContent = 0;
        foreach (var item in loadedShaders)
        {
            Raylib.UnloadShader(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Shader '{item.Key}' unloaded.");
        }
        foreach (var item in loadedTextures)
        {
            Raylib.UnloadTexture(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Texture '{item.Key}' unloaded.");
        }
        foreach (var item in loadedImages)
        {
            Raylib.UnloadImage(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Image '{item.Key}' unloaded.");
        }
        foreach (var item in loadedFonts)
        {
            foreach (var kvp in item.Value)
            {
                Raylib.UnloadFont(kvp.Value);
                unloadedContent++;
                Game.Instance.Logger.LogInfo($"Font '{item.Key}' size {kvp.Key} unloaded.");
            }
            
        }
        foreach (var item in loadedWaves)
        {
            Raylib.UnloadWave(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Wave '{item.Key}' unloaded.");
        }
        foreach (var item in loadedSounds)
        {
            Raylib.UnloadSound(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Sound '{item.Key}' unloaded.");
        }
        foreach (var item in loadedMusicStreams)
        {
            Raylib.UnloadMusicStream(item.Value);
            unloadedContent++;
            Game.Instance.Logger.LogInfo($"Music Stream '{item.Key}' unloaded.");
        }
        loadedShaders.Clear();
        loadedTextures.Clear();
        loadedImages.Clear();
        loadedFonts.Clear();
        loadedWaves.Clear();
        loadedSounds.Clear();
        loadedMusicStreams.Clear();
        loadedTexts.Clear();
        Game.Instance.Logger.LogInfo($"ContentManager unloading {unloadedContent} content items finished.");
    }
    
    /// <summary>
    /// Closes the ContentManager by unloading and removing all content packs and unloading all cached content.
    /// Logs the start and finish of the closing process.
    /// </summary>
    public void Close()
    {
        Game.Instance.Logger.LogInfo($"ContentManager closing started. Unloading and removing all content packs and unloading all cached content.");
        RemoveAllContentPacks(true);
        UnloadContent();
        Game.Instance.Logger.LogInfo($"ContentManager closing finished.");
    }
    
    #endregion
    
    #region Helpers
    /// <summary>
    /// Finds the most specific content pack that matches the given relative path.
    /// Searches for content packs whose root path is a parent directory of the provided relative path,
    /// starting from the most specific to the least specific.
    /// </summary>
    /// <param name="relativePath">The relative path to search for a content pack.</param>
    /// <param name="pack">The found content pack, or null if not found.</param>
    /// <param name="packFilePath">The file path relative to the found content pack's root.</param>
    /// <returns>True if a matching content pack is found; otherwise, false.</returns>
    private bool FindContentPack(string relativePath, out ContentPack? pack,  out string packFilePath)
    {
        // Generate all possible parent directory paths
        pathParts.Clear();
        pathParts.AddRange(relativePath.Split(separators, StringSplitOptions.RemoveEmptyEntries));
        
        possibleRoots.Clear();
        // Build paths from most specific to least specific
        // e.g., "Fonts/TitleFonts/HeaderFonts" -> "Fonts/TitleFonts" -> "Fonts"
        for (int i = pathParts.Count - 2; i >= 0; i--)
        {
            possibleRoots.Add(string.Join(Path.DirectorySeparatorChar, pathParts.Take(i + 1)));
        }
        
        // 1. Try all content packs with matching roots from most specific to least specific
        foreach (string possibleRoot in possibleRoots)
        {
            if (!contentPacks.TryGetValue(possibleRoot, out var contentPack)) continue;
            
            packFilePath = Path.GetRelativePath(possibleRoot, relativePath);
            pack = contentPack;
            return true;
        }
        
        packFilePath = string.Empty;
        pack = null;
        return false;
    }

    #endregion
    
}
