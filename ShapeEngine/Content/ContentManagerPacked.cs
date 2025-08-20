using Raylib_cs;

namespace ShapeEngine.Content;

/// <summary>
/// Provides a simple class to load content that was packed with the ContentPacker.
/// </summary>
public sealed class ContentManagerPacked : IContentManager
{
    /// <summary>
    /// The number of glyphs to load for fonts.
    /// </summary>
    public int GLYPH_COUNT = 0;
    private Dictionary<string, ContentInfo> content = new();
    
    private readonly List<Shader> shadersToUnload = [];
    private readonly List<Texture2D> texturesToUnload = [];
    private readonly List<Image> imagesToUnload = [];
    private readonly List<Font> fontsToUnload = [];
    private readonly List<Sound> soundsToUnload = [];
    private readonly List<Music> musicToUnload = [];
    private readonly List<Wave> wavesToUnload = [];

    /// <summary>
    /// Creates a new packed content manager.
    /// </summary>
    /// <param name="path">The path to the packed content.</param>
    /// <param name="resourceFileName">The name of the resource file.</param>
    public ContentManagerPacked(string path, string resourceFileName = "resources.txt")
    {
        if (resourceFileName == "") return;
        content = ContentPacker.Unpack(path, resourceFileName);
    }

    /// <summary>
    /// Closes the content manager and unloads all loaded content.
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

    /// <summary>
    /// Loads a texture from the given file path.
    /// </summary>
    /// <param name="filePath">The path to the texture file.</param>
    /// <returns>The loaded texture.</returns>
    public Texture2D LoadTexture(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var t = ContentLoader.LoadTextureFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var i = ContentLoader.LoadImageFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var f = ContentLoader.LoadFontFromContent( content[fileName], fontSize);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var w =  ContentLoader.LoadWaveFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var s = ContentLoader.LoadSoundFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var m = ContentLoader.LoadMusicFromContent( content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var fs = ContentLoader.LoadFragmentShaderFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var vs = ContentLoader.LoadVertexShaderFromContent(content[fileName]);
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
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        return ContentLoader.LoadTextFromContent(content[fileName]);
    }

}