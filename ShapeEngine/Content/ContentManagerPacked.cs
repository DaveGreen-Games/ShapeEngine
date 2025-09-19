using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Resources;
using Raylib_cs;

namespace ShapeEngine.Content;

//TODO: Combine ContentManagerPacked and ContentManager into one class (called ContentManager) and
// add all static stuff from ContentLoader as well.

//NOTE: The new ContentManager has a nullable content dictionary.
// When loading resources, manager first checks if
// content is available and contains the requested resource. If so, it loads from there.
// If not, it falls back to loading from file system.

//NOTE: At any time content can be:
// - cleared (set to null) to free up memory
// - loaded from a packed file (text or binary) to fill the content dictionary
// - or additional packs can be loaded and added to the content dictionary


public class ContentManagerNew
{
    //NOTE:
    // -Can load from disk
    // -Handles ContentPacks
    
    //Old if ContentPack approach is used!
    // -Can load from packed content (text or binary) into memory
    // -Can load from packed content (text or binary) and add it to existing content in memory
    // -Can clear content in memory
    // -Good aproach is to load to memory, load all content from data and then clear memory
    // -If content is not in memory, it will load from disk, otherwise from memory
    
    
    //!!! This class should unify ContentManager, ContentManagerPacked, and ContentLoader into 1 class.
    
    //NOTE: The best option would be to have this class handle index loading as well, but how? 
    //NOTE: Maybe have a ContentPack class that handles the index and data loading, and ContentManagerNew can have a list of ContentPacks?
}

/// <summary>
/// Provides a simple class to load content that was packed with the ContentPacker.
/// </summary>
public sealed class ContentManagerPacked : IContentManager
{
    /// <summary>
    /// The number of glyphs to load for fonts.
    /// </summary>
    public int GLYPH_COUNT = 0;

    // private Dictionary<string, ContentInfo> content;
    private Dictionary<string, byte[]> content;
    
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
        var extension = Path.GetExtension(filePath);
        var t = ContentLoader.LoadTextureFromContent(extension, content[filePath]);
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
        var extension = Path.GetExtension(filePath);
        var i = ContentLoader.LoadImageFromContent(extension, content[filePath]);
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
        var extension = Path.GetExtension(filePath);
        var f = ContentLoader.LoadFontFromContent(extension, content[filePath], fontSize);
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
        var extension = Path.GetExtension(filePath);
        var w =  ContentLoader.LoadWaveFromContent(extension, content[filePath]);
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
        var extension = Path.GetExtension(filePath);
        var s = ContentLoader.LoadSoundFromContent(extension, content[filePath]);
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
        var extension = Path.GetExtension(filePath);
        var m = ContentLoader.LoadMusicFromContent(extension, content[filePath]);
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
        var fs = ContentLoader.LoadFragmentShaderFromContent(content[filePath]);
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
        var vs = ContentLoader.LoadVertexShaderFromContent(content[filePath]);
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
        var extension = Path.GetExtension(filePath);
        return ContentLoader.LoadTextFromContent(extension, content[filePath]);
    }
    
    /// <summary>
    /// Unpacks a text pack from the specified text file path.
    /// Each pair of lines in the file represents a relative path and its associated base64-encoded, compressed data.
    /// Returns a dictionary mapping relative paths to their decompressed byte arrays.
    /// </summary>
    /// <param name="textFilePath">The path to the packed text file.</param>
    /// <returns>A dictionary mapping relative paths to their unpacked byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackTextPack(string textFilePath)
    {
        Dictionary<string, byte[]> result = new();

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
        for (var i = 0; i < lines.Length; i += 2)
        {
            if (i + 1 >= lines.Length) break;
            string relativePath = lines[i];
            string dataText = lines[i + 1];
            byte[] data = Convert.FromBase64String(dataText);
            result.Add(relativePath, Decompress(data));
        }
        return result;
    }
    /// <summary>
    /// Unpacks a resource pack from the specified resource file path.
    /// Decompresses the file using GZip and reads its contents as key-value pairs,
    /// where each key is a relative path and each value is the corresponding byte array.
    /// </summary>
    /// <param name="resourceFilePath">The path to the resource pack file.</param>
    /// <returns>A dictionary mapping relative paths to their unpacked byte arrays.</returns>
    public static Dictionary<string, byte[]> UnpackResourcePack(string resourceFilePath)
    {
        var contentInfos = new Dictionary<string, byte[]>();
        if (!Directory.Exists(resourceFilePath))
        {
            Console.WriteLine($"Directory doesn't exist: {resourceFilePath}");
            return contentInfos;
        }

        if (!File.Exists(resourceFilePath))
        {
            Console.WriteLine($"File doesn't exist: {resourceFilePath}");
            return contentInfos;
        }
        
        
        using var decompressMemStream = DecompressGzip(resourceFilePath);
        using var reader = new ResourceReader(decompressMemStream);
        foreach (DictionaryEntry entry in reader)
        {
            var relativePath = entry.Key.ToString();
            if (relativePath == null) continue;
            if (entry.Value is byte[] bytes)
            {
                contentInfos.Add(relativePath, bytes);
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
    
    private static MemoryStream DecompressGzip(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        var memStream = new MemoryStream();
        gzipStream.CopyTo(memStream);
        memStream.Position = 0;
        return memStream;
    }
}