using System.Reflection.Metadata.Ecma335;
using Raylib_cs;

namespace ShapeEngine.Content;



//TODO: save content with filepath as well (can retrieve it later and prevents loading the same content multiple times)?!
// -> instead of shaderToUnload, use Dictionary<string, Shader> loadedShaders -> can be used for unloading and to prevent loading the same content multiple times
//NOTE: ContentManager handles content packs
// -> load looks if content is available in a content pack first
// -> if not found, load from file system
// -> when closing, unloads all content that was loaded through it
// -> content pack can be unloaded separately if needed (unloads all content loaded from it)
// -> content pack can be added/removed from content manager
// -> content pack can be queried if it contains a specific file


public sealed class ContentManager2
{
    private readonly Dictionary<string, Shader> loadedShaders = new();
    private readonly Dictionary<string, Texture2D> loadedTextures = new();
    private readonly Dictionary<string, Image> loadedImages = new();
    private readonly Dictionary<string, Font> loadedFonts = new();
    private readonly Dictionary<string, Sound> loadedSounds = new();
    private readonly Dictionary<string, Music> loadedMusic = new();
    private readonly Dictionary<string, Wave> loadedWaves = new();
    
    private readonly Dictionary<string, ContentPack> loadedPacks = new();
    
    public ContentManager2()
    {
        
    }
    
    //TODO: implement the rest 
    private bool TryGetTexture(string filePath, out Texture2D texture)
    {
        if (loadedTextures.TryGetValue(filePath, out texture))
        {
            return true;
        }

        foreach (var pack in loadedPacks.Values)
        {
            if (!pack.IsLoaded) continue;
            if (!pack.HasFile(filePath)) continue;
            if (!pack.TryLoadTexture(filePath, out var packTexture)) continue;
            
            texture = packTexture;
            loadedTextures[filePath] = packTexture;
            return true;
        }

        if (!ContentLoader.TryLoadTexture(filePath, out var loadedTexture)) return false;
        
        loadedTextures[filePath] = loadedTexture;
        return true;

    }
    

    public void Close()
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
        foreach (var item in loadedMusic)
        {
            Raylib.UnloadMusicStream(item.Value);
        }
    }
}




/// <summary>
/// Provides a simple class to load content and automatically unload all loaded content when close is called.
/// </summary>
public sealed class ContentManager
{
    private readonly List<Shader> shadersToUnload = [];
    private readonly List<Texture2D> texturesToUnload = [];
    private readonly List<Image> imagesToUnload = [];
    private readonly List<Font> fontsToUnload = [];
    private readonly List<Sound> soundsToUnload = [];
    private readonly List<Music> musicToUnload = [];
    private readonly List<Wave> wavesToUnload = [];

    /// <summary>
    /// Creates a new content manager.
    /// </summary>
    public ContentManager() { }

    /// <summary>
    /// Unloads all loaded content.
    /// </summary>
    public void Close()
    {
        foreach (var item in shadersToUnload)
        {
            Raylib.UnloadShader(item);
        }

        foreach (var item in texturesToUnload)
        {
            Raylib.UnloadTexture(item);
        }
        foreach (var item in imagesToUnload)
        {
            Raylib.UnloadImage(item);
        }
        foreach (var item in fontsToUnload)
        {
            Raylib.UnloadFont(item);
        }
        foreach (var item in wavesToUnload)
        {
            Raylib.UnloadWave(item);
        }
        foreach (var item in soundsToUnload)
        {
            Raylib.UnloadSound(item);
        }
        foreach (var item in musicToUnload)
        {
            Raylib.UnloadMusicStream(item);
        }
    }
    
    #region Load
    
    /// <summary>
    /// Loads a texture from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the texture file.</param>
    /// <returns>The loaded texture.</returns>
    public Texture2D LoadTexture(string filePath)
    {
        var t = ContentLoader.LoadTexture(filePath);
        texturesToUnload.Add(t);
        return t;
    }
    /// <summary>
    /// Loads an image from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>The loaded image.</returns>
    public Image LoadImage(string filePath)
    {
        var i = ContentLoader.LoadImage(filePath);
        imagesToUnload.Add(i);
        return i;
    }
    /// <summary>
    /// Loads a font from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the font file.</param>
    /// <param name="fontSize">The size of the font.</param>
    /// <returns>The loaded font.</returns>
    public Font LoadFont(string filePath, int fontSize = 100)
    {
        var f = ContentLoader.LoadFont(filePath, fontSize);
        fontsToUnload.Add(f);
        return f;
    }
    /// <summary>
    /// Loads a wave from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the wave file.</param>
    /// <returns>The loaded wave.</returns>
    public Wave LoadWave(string filePath)
    {
        var w = ContentLoader.LoadWave(filePath);
        wavesToUnload.Add(w);
        return w;
    }
    /// <summary>
    /// Loads a sound from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <returns>The loaded sound.</returns>
    public Sound LoadSound(string filePath)
    {
        var s = ContentLoader.LoadSound(filePath);
        soundsToUnload.Add(s);
        return s;

    }
    /// <summary>
    /// Loads music from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the music file.</param>
    /// <returns>The loaded music.</returns>
    public Music LoadMusic(string filePath)
    {
        var m = ContentLoader.LoadMusic(filePath);
        musicToUnload.Add(m);
        return m;
    }
    /// <summary>
    /// Loads a fragment shader from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the fragment shader file.</param>
    /// <returns>The loaded shader.</returns>
    public Shader LoadFragmentShader(string filePath)
    {
        var fs = ContentLoader.LoadFragmentShader(filePath);
        shadersToUnload.Add(fs);
        return fs;
    }
    /// <summary>
    /// Loads a vertex shader from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the vertex shader file.</param>
    /// <returns>The loaded shader.</returns>
    public Shader LoadVertexShader(string filePath)
    {
        var vs = ContentLoader.LoadVertexShader(filePath);
        shadersToUnload.Add(vs);
        return vs;
    }
    /// <summary>
    /// Loads a text string from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the text file.</param>
    /// <returns>The loaded text string.</returns>
    public string LoadText(string filePath)
    {
        return ContentLoader.LoadText(filePath);
    }

    #endregion

    #region Load Directory

    /// <summary>
    /// Loads all textures from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing texture files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Texture2D"/> objects.
    /// </returns>
    public Dictionary<string, Texture2D> LoadTexturesFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadTexturesFromDirectory(directoryPath, recursive);
        texturesToUnload.AddRange(dict.Values);
        return dict;
    }
    /// <summary>
    /// Loads all images from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing image files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Image"/> objects.
    /// </returns>
    public Dictionary<string, Image> LoadImagesFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadImagesFromDirectory(directoryPath, recursive);
        imagesToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all fonts from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing font files.</param>
    /// <param name="fontSize">The size of the fonts to load.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Font"/> objects.
    /// </returns>
    public Dictionary<string, Font> LoadFontsFromDirectory(string directoryPath, int fontSize = 100, bool recursive = false)
    {
        var dict = ContentLoader.LoadFontsFromDirectory(directoryPath, fontSize, TextureFilter.Trilinear, recursive);
        fontsToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all sounds from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing sound files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Sound"/> objects.
    /// </returns>
    public Dictionary<string, Sound> LoadSoundsFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadSoundsFromDirectory(directoryPath, recursive);
        soundsToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all music files from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing music files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Music"/> objects.
    /// </returns>
    public Dictionary<string, Music> LoadMusicFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadMusicFromDirectory(directoryPath, recursive);
        musicToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all wave files from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing wave files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Wave"/> objects.
    /// </returns>
    public Dictionary<string, Wave> LoadWavesFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadWavesFromDirectory(directoryPath, recursive);
        wavesToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all fragment shaders from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing fragment shader files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Shader"/> objects.
    /// </returns>
    public Dictionary<string, Shader> LoadFragmentShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadFragmentShadersFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all vertex shaders from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing vertex shader files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>
    /// A dictionary mapping relative file paths to loaded <see cref="Shader"/> objects.
    /// </returns>
    public Dictionary<string, Shader> LoadVertexShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadVertexShadersFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(dict.Values);
        return dict;
    }
    
    #endregion
}