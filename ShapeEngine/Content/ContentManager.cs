using Raylib_cs;
using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Content;

//TODO: Docs & Cleanup (regions etc)

//TODO: test in GameloopExamples LoadContent with loading from disk, loading from 1 pack, or from multiple packs

public sealed class ContentManager
{
    #region Members
    public readonly string RootDirectory;
    
    private readonly Dictionary<string, Shader> loadedShaders = new();
    private readonly Dictionary<string, Texture2D> loadedTextures = new();
    private readonly Dictionary<string, Image> loadedImages = new();
    private readonly Dictionary<string, Font> loadedFonts = new();
    private readonly Dictionary<string, Sound> loadedSounds = new();
    private readonly Dictionary<string, Music> loadedMusicStreams = new();
    private readonly Dictionary<string, Wave> loadedWaves = new();
    private readonly Dictionary<string, string> loadedTexts = new();
    private readonly Dictionary<string, ContentPack> contentPacks = new();
    
    private readonly List<string> pathParts = [];
    private readonly List<string> possibleRoots = [];
    private static readonly char[] separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    #endregion
    
    #region Constructors
    public ContentManager(string rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    #endregion
    
    #region Content Packs
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
    public void RemoveAllContentPacks(bool clear = true)
    {
        if (clear) UnloadAllContentPacks();
        contentPacks.Clear();
    }
    
    public List<ContentPack> GetAllContentPacks()
    {
        return contentPacks.Values.ToList();
    }
    public List<ContentPack> GetAllLoadedContentPacks()
    {
        return contentPacks.Values.Where(x => x.IsLoaded).ToList();
    }
    public bool HasContentPack(string root)
    {
        return contentPacks.ContainsKey(root);
    }
    public bool TryGetContentPack(string root, out ContentPack? pack)
    {
        return contentPacks.TryGetValue(root, out pack);
    }   
    #endregion
    
    #region Try Load Content
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
                texture = packTexture;
                loadedTextures[relativePath] = packTexture;
                return true;
            }
        }
        
        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadTexture(contentPath, out var loadedTexture)) return false;
        
        loadedTextures[relativePath] = loadedTexture;
        texture = loadedTexture;
        return true;
    }
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
                shader = packShader;
                loadedShaders[relativePath] = packShader;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadFragmentShader(contentPath, out var loadedShader)) return false;

        loadedShaders[relativePath] = loadedShader;
        shader = loadedShader;
        return true;
    }
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
                image = packImage;
                loadedImages[relativePath] = packImage;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadImage(contentPath, out var loadedImage)) return false;

        loadedImages[relativePath] = loadedImage;
        image = loadedImage;
        return true;
    }
    public bool TryLoadFont(string filePath, out Font font)
    {
        font = default;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedFonts.TryGetValue(relativePath, out font))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadFont(packFilePath, out var packFont))
            {
                font = packFont;
                loadedFonts[relativePath] = packFont;
                return true;
            }
        }

        // 2. Try root pack if available
        if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
        {
            if (rootPack.TryLoadFont(relativePath, out var packFont))
            {
                font = packFont;
                loadedFonts[relativePath] = packFont;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadFont(contentPath, out var loadedFont)) return false;

        loadedFonts[relativePath] = loadedFont;
        font = loadedFont;
        return true;
    }
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
                sound = packSound;
                loadedSounds[relativePath] = packSound;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadSound(contentPath, out var loadedSound)) return false;

        loadedSounds[relativePath] = loadedSound;
        sound = loadedSound;
        return true;
    }
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
                music = packMusic;
                loadedMusicStreams[relativePath] = packMusic;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadMusic(contentPath, out var loadedMusic)) return false;

        loadedMusicStreams[relativePath] = loadedMusic;
        music = loadedMusic;
        return true;
    }
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
                wave = packWave;
                loadedWaves[relativePath] = packWave;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadWave(contentPath, out var loadedWave)) return false;

        loadedWaves[relativePath] = loadedWave;
        wave = loadedWave;
        return true;
    }
    public bool TryLoadText(string filePath, out string text)
    {
        text = string.Empty;

        // Normalize path to be relative to RootDirectory
        string relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;

        // Check cache first
        if (loadedTexts.TryGetValue(relativePath, out text))
        {
            Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
            return true;
        }

        if (FindContentPack(relativePath, out var pack, out string packFilePath) && pack != null)
        {
            if (pack.TryLoadText(packFilePath, out var packText))
            {
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
                text = packText;
                loadedTexts[relativePath] = packText;
                return true;
            }
        }

        // 3. Try loading from disk
        string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
        if (!ContentLoader.TryLoadText(contentPath, out var loadedText)) return false;

        loadedTexts[relativePath] = loadedText;
        text = loadedText;
        return true;
    }
    #endregion
    
    #region Unloading
    
    public void UnloadAllContentPacks()
    {
        foreach (var pack in contentPacks.Values)
        {
            pack.Clear();
        }
    }
    public void UnloadContentCache()
    {
        foreach (var item in loadedShaders)
        {
            Raylib.UnloadShader(item.Value);
        }
        foreach (var item in loadedTextures)
        {
            Raylib.UnloadTexture(item.Value);
        }
        foreach (var item in loadedImages)
        {
            Raylib.UnloadImage(item.Value);
        }
        foreach (var item in loadedFonts)
        {
            Raylib.UnloadFont(item.Value);
        }
        foreach (var item in loadedWaves)
        {
            Raylib.UnloadWave(item.Value);
        }
        foreach (var item in loadedSounds)
        {
            Raylib.UnloadSound(item.Value);
        }
        foreach (var item in loadedMusicStreams)
        {
            Raylib.UnloadMusicStream(item.Value);
        }
        loadedShaders.Clear();
        loadedTextures.Clear();
        loadedImages.Clear();
        loadedFonts.Clear();
        loadedWaves.Clear();
        loadedSounds.Clear();
        loadedMusicStreams.Clear();
        loadedTexts.Clear();
    }
    public void Close()
    {
        UnloadAllContentPacks();
        RemoveAllContentPacks();
        UnloadContentCache();
    }
    
    #endregion
    
    #region Helpers
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
            
            packFilePath = Path.GetRelativePath(relativePath, possibleRoot);
            pack = contentPack;
            return true;
        }
        
        packFilePath = string.Empty;
        pack = null;
        return false;
    }

    #endregion
    
}

/*
     // private string NormalizePath(string path)
   // {
   //     string fullPath = Path.GetFullPath(path);
   //     return Path.TrimEndingDirectorySeparator(fullPath).ToLowerInvariant();
   // }
     private bool TryGetContentPack2(string relativePath, out ContentPack? pack, out string newFilePath)
   {
       foreach (var kvp in contentPacks)
       {
           string packRoot = kvp.Key + Path.DirectorySeparatorChar;
           if (relativePath.StartsWith(packRoot))
           {
               newFilePath = Path.GetRelativePath(packRoot, relativePath);
               pack = kvp.Value;
               return true;
           }
       }
       
       pack = null;
       newFilePath = string.Empty;
       return false;
   }
   private bool TryGetContentPack(string relativePath, out ContentPack? pack, out string root)
   {
       foreach (var kvp in contentPacks)
       {
           string packRoot = kvp.Key;
           int index = relativePath.IndexOf(packRoot, StringComparison.Ordinal);
           if (index < 0) continue;
           
           // Get the full root path up to and including the matched packRoot
           int endIndex = index + packRoot.Length;
           root = relativePath.Substring(0, endIndex);
           pack = kvp.Value;
           return true;
       }
       
       pack = null;
       root = string.Empty;
       return false;
   }
   private ContentPack? GetContentPack (string relativePath, out string root)
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
           if (contentPacks.TryGetValue(possibleRoot, out var pack))
           {
               root = possibleRoot;
               return pack;
           }
       }
       root = string.Empty;
       return null;
   }
   
    public static string GetRelativePathRoot(string path)
   {
       if (string.IsNullOrEmpty(path)) return string.Empty;
       string[] parts = path.Split(separators, StringSplitOptions.RemoveEmptyEntries);
       return parts.Length > 0 ? parts[0] : string.Empty;
   }
    private bool TryLoadTexture_OLD1(string filePath, out Texture2D texture)
   {
       //strip root if filePath starts with it
       filePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;
       
       if (loadedTextures.TryGetValue(filePath, out texture))
       {
           Game.Instance.Logger.LogInfo($"Content '{filePath}' is already loaded, returning cached version.");
           return true;
       }

       if (contentPacks.Count > 0)
       {
           if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
           {
               if (rootPack.TryLoadTexture(filePath, out var packTexture))
               {
                   texture = packTexture;
                   loadedTextures[filePath] = packTexture;
                   return true;
               }
           }
           else
           {
               var packRoot = GetRelativePathRoot(filePath);
               if (string.IsNullOrEmpty(packRoot) || Path.HasExtension(packRoot))
               {
                   Game.Instance.Logger.LogWarning($"Pack Root invalid: {packRoot}. File path '{filePath}' does not contain a valid root directory. Trying to load from file system.");
               }
               else
               {
                   if (contentPacks.TryGetValue(packRoot, out var pack))
                   {
                       var relativePackPath = Path.GetRelativePath(packRoot, filePath);
                       if (pack.TryLoadTexture(relativePackPath, out var packTexture))
                       {
                           texture = packTexture;
                           loadedTextures[filePath] = packTexture;
                           return true;
                       }
                   }
           
                   Game.Instance.Logger.LogInfo($"No content pack found for root '{packRoot}'. Trying to load from file system.");
               }
           }
       }
       
       var contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, filePath) : filePath;  
       if (!ContentLoader.TryLoadTexture(contentPath, out var loadedTexture)) return false;
       
       loadedTextures[filePath] = loadedTexture;
       return true;

   }
   private bool TryLoadTexture_OLD2(string filePath, out Texture2D texture)
   {
       texture = default;
   
       // Normalize path to be relative to RootDirectory
       var relativePath = RootDirectory != string.Empty ? Path.GetRelativePath(RootDirectory, filePath) : filePath;
   
       if (loadedTextures.TryGetValue(relativePath, out texture))
       {
           Game.Instance.Logger.LogInfo($"Content '{relativePath}' is already loaded, returning cached version.");
           return true;
       }
   
       // Try all content packs whose root matches the start of the path
       foreach (var kvp in contentPacks)
       {
           string packRoot = kvp.Key;
           if (!relativePath.StartsWith(packRoot + Path.DirectorySeparatorChar)) continue;
           string packRelativePath = Path.GetRelativePath(packRoot, relativePath);
           if (!kvp.Value.TryLoadTexture(packRelativePath, out var packTexture)) continue;
           texture = packTexture;
           loadedTextures[relativePath] = packTexture;
           return true;
       }
   
       // Try root pack if it matches
       if (contentPacks.TryGetValue(RootDirectory, out var rootPack))
       {
           if (rootPack.TryLoadTexture(relativePath, out var packTexture))
           {
               texture = packTexture;
               loadedTextures[relativePath] = packTexture;
               return true;
           }
       }
   
       // Try loading from disk
       string contentPath = RootDirectory != string.Empty ? Path.Combine(RootDirectory, relativePath) : relativePath;
       if (!ContentLoader.TryLoadTexture(contentPath, out var loadedTexture)) return false;
   
       loadedTextures[relativePath] = loadedTexture;
       texture = loadedTexture;
       return true;
   }
   
   
 */