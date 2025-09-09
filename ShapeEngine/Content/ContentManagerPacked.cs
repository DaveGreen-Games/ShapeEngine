using System.Collections;
using System.IO.Compression;
using System.Resources;
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

    private Dictionary<string, ContentInfo> content;
    
    private readonly List<Shader> shadersToUnload = [];
    private readonly List<Texture2D> texturesToUnload = [];
    private readonly List<Image> imagesToUnload = [];
    private readonly List<Font> fontsToUnload = [];
    private readonly List<Sound> soundsToUnload = [];
    private readonly List<Music> musicToUnload = [];
    private readonly List<Wave> wavesToUnload = [];



    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManagerPacked"/> class,
    /// loading packed content from the specified file path.
    /// </summary>
    /// <param name="path">The path to the packed content file (.txt or .resource for instance).</param>
    /// <remarks>
    /// If path ends in .txt <see cref="UnpackTextPack"/> is used to unpack the content. Otherwise <see cref="UnpackResourcePack"/> is used.
    /// Content files must have been packed using a compatible resource packing method.
    /// </remarks>
    public ContentManagerPacked(string path)
    {
        if (!Path.HasExtension(path))
        {
            Console.WriteLine($"ContentManagerPacked failed to load file: Path has no file extension: {path}");
            content = new();
        }
        
        var extension = Path.GetExtension(path);
        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        {
            content = UnpackTextPack(path);
        }
        else
        {
            content = UnpackResourcePack(path);
        }
        
        if (content.Count <= 0)
        {
            Console.WriteLine($"ContentManagerPacked no content loaded from file: {path}");
        }
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
    
    
    /// <summary>
    /// Loads and unpacks a text file generated with a compatible resource packing method.
    /// </summary>
    /// <param name="textFilePath">The path to the packed txt file.</param>
    /// <returns></returns>
    public static Dictionary<string, ContentInfo> UnpackTextPack(string textFilePath)
    {
        Dictionary<string, ContentInfo> result = new();

        if (!Directory.Exists(textFilePath))
        {
            Console.WriteLine($"Directory doesn't exist: {textFilePath}");
            return result;
        }
        
        var fileName = Path.GetFileName(textFilePath);
        if (!fileName.EndsWith(".txt"))
        {
            Console.WriteLine($"File name doesn't end with '.txt': {textFilePath}");
            return result;
        }

        if (!File.Exists(textFilePath))
        {
            Console.WriteLine($"File doesn't exist: {textFilePath}");
            return result;
        }
        
        var lines = File.ReadAllLines(textFilePath);
        for (int i = 0; i < lines.Length; i += 2)
        {
            string key = lines[i];
            string name = Path.GetFileNameWithoutExtension(key);
            string extension = Path.GetExtension(key);
            string dataText = lines[i + 1];
            var data = Convert.FromBase64String(dataText);
            result.Add(name, new(extension, Decompress(data)));
        }
        return result;
    }
    /// <summary>
    /// Extracts all resources from a file and returns them as a dictionary of <see cref="ContentInfo"/> objects.
    /// The key is the file name without extension, and the value contains the file name and its byte data.
    /// Only works with files that were packed using a compatible resource packing method.
    /// </summary>
    /// <param name="resxPath">The path to the .resx resource file.</param>
    /// <returns>
    /// A dictionary where each key is the file name without extension and the value is a <see cref="ContentInfo"/>
    /// containing the full file name and its byte data.
    /// </returns>
    public static Dictionary<string, ContentInfo> UnpackResourcePack(string resxPath)
    {
        var contentInfos = new Dictionary<string, ContentInfo>();
        if (!Directory.Exists(resxPath))
        {
            Console.WriteLine($"Directory doesn't exist: {resxPath}");
            return contentInfos;
        }

        if (!File.Exists(resxPath))
        {
            Console.WriteLine($"File doesn't exist: {resxPath}");
            return contentInfos;
        }
        
        using var reader = new ResourceReader(resxPath);
        foreach (DictionaryEntry entry in reader)
        {
            var fileName = entry.Key.ToString();
            if (fileName == null) continue;
            if (entry.Value is byte[] bytes)
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                contentInfos[name] = new ContentInfo(fileName, bytes);
            }
        }
        return contentInfos;
    }
    /// <summary>
    /// Decompresses binary data that was compressed using the Deflate algorithm using compression mode <see cref="CompressionMode.Decompress"/>.
    /// </summary>
    /// <seealso cref="DeflateStream"/>
    /// <seealso cref="MemoryStream"/>
    /// <param name="data">The compressed binary data to decompress.</param>
    /// <returns>The decompressed binary data.</returns>
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
        {
            deflateStream.CopyTo(output);
        }
        return output.ToArray();
    }
}