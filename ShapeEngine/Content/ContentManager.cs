using Raylib_cs;

namespace ShapeEngine.Content;



//TODO: save content with filepath as well (can retrieve it later and prevents loading the same content multiple times)
//NOTE: ContentManager handles content packs
// -> load looks if content is available in a content pack first
// -> if not found, load from file system
// -> when closing, unloads all content that was loaded through it
// -> content pack can be unloaded separately if needed (unloads all content loaded from it)
// -> content pack can be added/removed from content manager
// -> content pack can be queried if it contains a specific file


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
        var m = ContentLoader.LoadMusicStream(filePath);
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

    #region Load Directories

    /// <summary>
    /// Loads all textures from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing texture files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>A list of loaded <see cref="Texture2D"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid texture extensions: .png, .bmp, .tga, .jpg, .jpeg, .gif, .psd, .pkm, .ktx, .pvr, .dds, .hdr
    /// </remarks>
    public List<Texture2D> LoadTexturesFromDirectory(string directoryPath, bool recursive = false)
    {
        var textures = ContentLoader.LoadTexturesFromDirectory(directoryPath, recursive);
        texturesToUnload.AddRange(textures);
        return textures;
    }
    /// <summary>
    /// Loads all images from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing image files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>A list of loaded <see cref="Image"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid image extensions: .png, .bmp, .tga, .jpg, .jpeg, .gif, .psd, .pkm, .ktx, .pvr, .dds, .hdr
    /// </remarks>
    public List<Image> LoadImagesFromDirectory(string directoryPath, bool recursive = false)
    {
        var images = ContentLoader.LoadImagesFromDirectory(directoryPath, recursive);
        imagesToUnload.AddRange(images);
        return images;
    }
    /// <summary>
    /// Loads all fonts from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing font files.</param>
    /// <param name="fontSize">The size of the fonts to load. Default is 100.</param>
    /// <param name="recursive">Whether to search subdirectories recursively. Default is false.</param>
    /// <returns>A list of loaded <see cref="Font"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid font extensions: .ttf, .otf
    /// </remarks>
    public List<Font> LoadFontsFromDirectory(string directoryPath, int fontSize = 100, bool recursive = false)
    {
        var fonts = ContentLoader.LoadFontsFromDirectory(directoryPath, fontSize, TextureFilter.Trilinear, recursive);
        fontsToUnload.AddRange(fonts);
        return fonts;
    }
    /// <summary>
    /// Loads all sounds from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing sound files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>A list of loaded <see cref="Sound"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid sound extensions: .wav, .mp3, .ogg, .flac
    /// </remarks>
    public List<Sound> LoadSoundsFromDirectory(string directoryPath, bool recursive = false)
    {
        var sounds = ContentLoader.LoadSoundsFromDirectory(directoryPath, recursive);
        soundsToUnload.AddRange(sounds);
        return sounds;
    }
    /// <summary>
    /// Loads all music streams from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing music files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively.</param>
    /// <returns>A list of loaded <see cref="Music"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid music extensions: .mp3, .ogg, .flac, .mod, .xm
    /// </remarks>
    public List<Music> LoadMusicStreamsFromDirectory(string directoryPath, bool recursive = false)
    {
        var music = ContentLoader.LoadMusicStreamsFromDirectory(directoryPath, recursive);
        musicToUnload.AddRange(music);
        return music;
    }
    /// <summary>
    /// Loads all wave files from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing wave files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively. Default is false.</param>
    /// <returns>A list of loaded <see cref="Wave"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid wave extensions: .wav, .mp3, .ogg, .flac
    /// </remarks>
    public List<Wave> LoadWavesFromDirectory(string directoryPath, bool recursive = false)
    {
        var waves = ContentLoader.LoadWavesFromDirectory(directoryPath, recursive);
        wavesToUnload.AddRange(waves);
        return waves;
    }
    /// <summary>
    /// Loads all fragment shaders from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing fragment shader files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively. Default is false.</param>
    /// <returns>A list of loaded <see cref="Shader"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid fragment shader extensions: .fs, .glsl, .frag
    /// </remarks>
    public List<Shader> LoadFragmentShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var shaders = ContentLoader.LoadFragmentShadersFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(shaders);
        return shaders;
    }
    /// <summary>
    /// Loads all vertex shaders from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing vertex shader files.</param>
    /// <param name="recursive">Whether to search subdirectories recursively. Default is false.</param>
    /// <returns>A list of loaded <see cref="Shader"/> objects.</returns>
    /// <remarks>
    /// Only loads files with valid vertex shader extensions: .vs, .glsl, .vert
    /// </remarks>
    public List<Shader> LoadVertexShadersFromDirectory(string directoryPath, bool recursive = false)
    {
        var shaders = ContentLoader.LoadVertexShadersFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(shaders);
        return shaders;
    }

    #endregion

    #region Load Directory with Filename
    
    /// <summary>
    /// Loads all textures from a directory and returns a dictionary mapping file names (without extension) to Texture2D objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing texture files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Texture2D objects.</returns>
    /// <remarks>
    /// Only loads files with valid texture extensions: .png, .bmp, .tga, .jpg, .jpeg, .gif, .psd, .pkm, .ktx, .pvr, .dds, .hdr
    /// </remarks>
    public Dictionary<string, Texture2D> LoadTexturesWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadTexturesWithFilenameFromDirectory(directoryPath, recursive);
        texturesToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all images from a directory and returns a dictionary mapping file names (without extension) to Image objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing image files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Image objects.</returns>
    /// <remarks>
    /// Only loads files with valid image extensions: .png, .bmp, .tga, .jpg, .jpeg, .gif, .psd, .pkm, .ktx, .pvr, .dds, .hdr
    /// </remarks>
    public Dictionary<string, Image> LoadImagesWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadImagesWithFilenameFromDirectory(directoryPath, recursive);
        imagesToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all fonts from a directory and returns a dictionary mapping file names (without extension) to Font objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing font files.</param>
    /// <param name="fontSize">The size of the fonts to load. Default is 100.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Font objects.</returns>
    /// <remarks>
    /// Only loads files with valid font extensions: .ttf, .otf
    /// </remarks>
    public Dictionary<string, Font> LoadFontsWithFilenameFromDirectory(string directoryPath, int fontSize = 100, bool recursive = false)
    {
        var dict = ContentLoader.LoadFontsWithFilenameFromDirectory(directoryPath, fontSize, TextureFilter.Trilinear, recursive);
        fontsToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all sound files from a directory and returns a dictionary mapping file names (without extension) to Sound objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Sound objects.</returns>
    /// <remarks>
    /// Only loads files with valid sound extensions: .wav, .mp3, .ogg, .flac
    /// </remarks>
    public Dictionary<string, Sound> LoadSoundsWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadSoundsWithFilenameFromDirectory(directoryPath, recursive);
        soundsToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all music streams from a directory and returns a dictionary mapping file names (without extension) to Music objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing music files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Music objects.</returns>
    /// <remarks>
    /// Only loads files with valid music extensions: .mp3, .ogg, .flac, .mod, .xm
    /// </remarks>
    public Dictionary<string, Music> LoadMusicStreamsWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadMusicStreamsWithFilenameFromDirectory(directoryPath, recursive);
        musicToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all wave sounds from a directory and returns a dictionary mapping file names (without extension) to Wave objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing wave sound files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Wave objects.</returns>
    /// <remarks>
    /// Only loads files with valid wave extensions: .wav, .mp3, .ogg, .flac
    /// </remarks>
    public Dictionary<string, Wave> LoadWavesWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadWavesWithFilenameFromDirectory(directoryPath, recursive);
        wavesToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all fragment shaders from a directory and returns a dictionary mapping file names (without extension) to Shader objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing fragment shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Shader objects.</returns>
    /// <remarks>
    /// Only loads files with valid fragment shader extensions: .fs, .glsl, .frag
    /// </remarks>
    public Dictionary<string, Shader> LoadFragmentShadersWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadFragmentShadersWithFilenameFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(dict.Values);
        return dict;
    }

    /// <summary>
    /// Loads all vertex shaders from a directory and returns a dictionary mapping file names (without extension) to Shader objects.
    /// </summary>
    /// <param name="directoryPath">The directory path containing vertex shader files.</param>
    /// <param name="recursive">Whether to search recursively in subdirectories. Default is false.</param>
    /// <returns>A dictionary mapping file names to loaded Shader objects.</returns>
    /// <remarks>
    /// Only loads files with valid vertex shader extensions: .vs, .glsl, .vert
    /// </remarks>
    public Dictionary<string, Shader> LoadVertexShadersWithFilenameFromDirectory(string directoryPath, bool recursive = false)
    {
        var dict = ContentLoader.LoadVertexShadersWithFilenameFromDirectory(directoryPath, recursive);
        shadersToUnload.AddRange(dict.Values);
        return dict;
    }
    
    #endregion
}