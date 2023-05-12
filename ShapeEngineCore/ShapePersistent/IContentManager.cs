using Raylib_CsLo;
using System.Text;

namespace ShapePersistent
{
    public interface IContentManager
    {
        public void Close();

        public Texture LoadTexture(string filePath);
        public Image LoadImage(string filePath);
        public Font LoadFont(string filePath, int fontSize = 100);
        public Wave LoadWave(string filePath);
        public Sound LoadSound(string filePath);
        public Music LoadMusic(string filePath);
        public Shader LoadFragmentShader(string filePath);
        public Shader LoadVertexShader(string filePath);
        public string LoadJson(string filePath);

    }
    
    /// <summary>
    /// Provides a simple class to load content and automatically unload all loaded content when close is called.
    /// </summary>
    public sealed class ContentManager : IContentManager
    {

        private List<Shader> shadersToUnload = new();
        private List<Texture> texturesToUnload = new();
        private List<Image> imagesToUnload = new();
        private List<Font> fontsToUnload = new();
        private List<Sound> soundsToUnload = new();
        private List<Music> musicToUnload = new();
        private List<Wave> wavesToUnload = new();

        public ContentManager() { }

        /// <summary>
        /// Unloads all loaded content.
        /// </summary>
        public void Close()
        {
            foreach (var item in shadersToUnload)
            {
                UnloadShader(item);
            }

            foreach (var item in texturesToUnload)
            {
                UnloadTexture(item);
            }
            foreach (var item in imagesToUnload)
            {
                UnloadImage(item);
            }
            foreach (var item in fontsToUnload)
            {
                UnloadFont(item);
            }
            foreach (var item in wavesToUnload)
            {
                UnloadWave(item);
            }
            foreach (var item in soundsToUnload)
            {
                UnloadSound(item);
            }
            foreach (var item in musicToUnload)
            {
                UnloadMusicStream(item);
            }
        }

        public Texture LoadTexture(string filePath)
        {
            Texture t = ContentLoader.LoadTexture(filePath);
            texturesToUnload.Add(t);
            return t;
        }
        public Image LoadImage(string filePath)
        {
            Image i = ContentLoader.LoadImage(filePath);
            imagesToUnload.Add(i);
            return i;
        }
        public Font LoadFont(string filePath, int fontSize = 100)
        {
            unsafe
            {
                Font f = ContentLoader.LoadFont(filePath, fontSize);
                fontsToUnload.Add(f);
                return f;
            }
        }
        public Wave LoadWave(string filePath)
        {
            Wave w = ContentLoader.LoadWave(filePath);
            wavesToUnload.Add(w);
            return w;
        }
        public Sound LoadSound(string filePath)
        {
            Sound s = ContentLoader.LoadSound(filePath);
            soundsToUnload.Add(s);
            return s;

        }
        public Music LoadMusic(string filePath)
        {
            Music m = ContentLoader.LoadMusic(filePath);
            musicToUnload.Add(m);
            return m;
        }
        public Shader LoadFragmentShader(string filePath)
        {
            Shader fs = ContentLoader.LoadFragmentShader(filePath);
            shadersToUnload.Add(fs);
            return fs;
        }
        public Shader LoadVertexShader(string filePath)
        {
            Shader vs = ContentLoader.LoadVertexShader(filePath);
            shadersToUnload.Add(vs);
            return vs;
        }
        public string LoadJson(string filePath)
        {
            return ContentLoader.LoadJson(filePath);
        }

    }
    
    /// <summary>
    /// Provides a simple class to load content that was packed with the ContentPacker.
    /// </summary>
    public sealed class ContentManagerPacked : IContentManager
    {
        public int GLYPH_COUNT = 0;
        private Dictionary<string, ContentInfo> content = new();
        private List<Texture> texturesToUnload = new();

        public ContentManagerPacked(string path, string resourceFileName = "resources.txt")
        {
            if (resourceFileName == "") return;
            content = ContentPacker.Unpack(path, resourceFileName);
        }

        public void Close()
        {
            foreach (var item in texturesToUnload)
            {
                UnloadTexture(item);
            }
        }

        public Texture LoadTexture(string filePath)
        {
            Texture t = Raylib.LoadTextureFromImage(LoadImage(filePath));
            texturesToUnload.Add(t);
            return t;
        }
        public Image LoadImage(string filePath)
        {
            unsafe
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var data = content[fileName].data;
                var extension = content[fileName].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadImageFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public Font LoadFont(string filePath, int fontSize = 100)
        {
            unsafe
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var data = content[fileName].data;
                var extension = content[fileName].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadFontFromMemory(extension, ptr, data.Length, fontSize, (int*)0, GLYPH_COUNT);
                }
            }
        }
        public Wave LoadWave(string filePath)
        {
            unsafe
            {
                string filename = Path.GetFileNameWithoutExtension(filePath);
                var data = content[filename].data;
                var extension = content[filename].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadWaveFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public Sound LoadSound(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return Raylib.LoadSoundFromWave(LoadWave(fileName));

        }
        public Music LoadMusic(string filePath)
        {
            unsafe
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var data = content[fileName].data;
                var extension = content[fileName].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadMusicStreamFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public Shader LoadFragmentShader(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string file = Encoding.Default.GetString(content[fileName].data);
            return Raylib.LoadShaderFromMemory(null, file);
        }
        public Shader LoadVertexShader(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string file = Encoding.Default.GetString(content[fileName].data);
            return Raylib.LoadShaderFromMemory(file, "");
        }
        public string LoadJson(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return Encoding.Default.GetString(content[fileName].data);
        }

    }

}

/*
    public class ResourceManager
    {
        private Dictionary<string, ContentInfo> resources = new();


        private List<Shader> shadersToUnload = new();
        private List<Texture> texturesToUnload = new();
        private List<Image> imagesToUnload = new();
        private List<Font> fontsToUnload = new();
        private List<Sound> soundsToUnload = new();
        private List<Music> musicToUnload = new();
        private List<Wave> wavesToUnload = new();

        private string path = "";
        private string resourceFileName = "";
        private bool editorMode = false;
        public static int GLYPH_COUNT = 0;
        public ResourceManager(string path, string resourceFileName = "resources.txt", bool editorMode = false)
        {
            if (resourceFileName == "") return;
            this.path = path;
            this.resourceFileName = resourceFileName;
            this.editorMode = editorMode;
            resources = LoadResources(path, resourceFileName, editorMode);
        }

        public void Close()
        {
            foreach (var item in shadersToUnload)
            {
                UnloadShader(item);
            }

            foreach (var item in texturesToUnload)
            {
                UnloadTexture(item);
            }
            foreach (var item in imagesToUnload)
            {
                UnloadImage(item);
            }
            foreach (var item in fontsToUnload)
            {
                UnloadFont(item);
            }
            foreach (var item in wavesToUnload)
            {
                UnloadWave(item);
            }
            foreach (var item in soundsToUnload)
            {
                UnloadSound(item);
            }
            foreach (var item in musicToUnload)
            {
                UnloadMusicStream(item);
            }
        }

        public Texture LoadTexture(string filePath, bool autoUnload = false)
        {
            Texture t = Raylib.LoadTextureFromImage(LoadImage(filePath));
            if (autoUnload) texturesToUnload.Add(t);
            return t;
        }
        public Image LoadImage(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Image i = Raylib.LoadImage(filePath);
                if (autoUnload) imagesToUnload.Add(i);
                return i;
            }
            else
            {
                unsafe
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    var data = resources[fileName].data;
                    var extension = resources[fileName].extension;
                    fixed (byte* ptr = data)
                    {
                        return Raylib.LoadImageFromMemory(extension, ptr, data.Length);
                    }
                }
            }
        }
        public Font LoadFont(string filePath, int fontSize = 100, bool autoUnload = false)
        {
            if (editorMode)
            {
                unsafe
                {
                    Font f = Raylib.LoadFontEx(filePath, fontSize, (int*)0, GLYPH_COUNT);
                    if(autoUnload) fontsToUnload.Add(f);
                    return f;
                }
            }
            else
            {
                unsafe
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    var data = resources[fileName].data;
                    var extension = resources[fileName].extension;
                    fixed (byte* ptr = data)
                    {
                        return Raylib.LoadFontFromMemory(extension, ptr, data.Length, fontSize, (int*)0, GLYPH_COUNT);
                    }
                }
            }
        }
        public Wave LoadWave(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Wave w = Raylib.LoadWave(filePath);
                if (autoUnload) wavesToUnload.Add(w);
                return w;
            }
            else
            {
                unsafe
                {
                    string filename = Path.GetFileNameWithoutExtension(filePath);
                    var data = resources[filename].data;
                    var extension = resources[filename].extension;
                    fixed (byte* ptr = data)
                    {
                        return Raylib.LoadWaveFromMemory(extension, ptr, data.Length);
                    }
                }
            }
        }
        public Sound LoadSound(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Sound s = Raylib.LoadSound(filePath);
                if (autoUnload) soundsToUnload.Add(s);
                return s;
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                return Raylib.LoadSoundFromWave(LoadWave(fileName));
            }
            
        }
        public Music LoadMusic(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Music m = Raylib.LoadMusicStream(filePath);
                if (autoUnload) musicToUnload.Add(m);
                return m;
            }
            else
            {
                unsafe
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    var data = resources[fileName].data;
                    var extension = resources[fileName].extension;
                    fixed (byte* ptr = data)
                    {
                        return Raylib.LoadMusicStreamFromMemory(extension, ptr, data.Length);
                    }
                }
            }
        }
        public Shader LoadFragmentShader(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Shader fs = Raylib.LoadShader(null, filePath);
                if (autoUnload) shadersToUnload.Add(fs);
                return fs;
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string file = Encoding.Default.GetString(resources[fileName].data);
                return Raylib.LoadShaderFromMemory(null, file);
            }
        }
        public Shader LoadVertexShader(string filePath, bool autoUnload = false)
        {
            if (editorMode)
            {
                Shader vs = Raylib.LoadShader(filePath, "");
                if(autoUnload) shadersToUnload.Add(vs);
                return vs;
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string file = Encoding.Default.GetString(resources[fileName].data);
                return Raylib.LoadShaderFromMemory(file, "");
            }
        }
        public string LoadJsonData(string filePath)
        {
            if (editorMode)
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                return Encoding.Default.GetString(resources[fileName].data);
            }
        }


        public Dictionary<string, T> LoadDataSheet<T>(string dataFileName, string sheetName) where T : DataObject
        {
            string dataString = LoadJsonData(dataFileName);
            var dataNode = JsonNode.Parse(dataString);
            if (dataNode == null) return new();
            var lines = DataContainerCDB.GetDataSheetLines(dataNode, sheetName);
            if (lines == null) return new();
            Dictionary<string, T> dict = new();
            foreach (var line in lines)
            {
                var result = line.Deserialize<T>();
                if (result == null) continue;
                dict.Add(result.name, result);
            }
            return dict;
        }
        public T? LoadDataLine<T>(string dataFileName, string sheetName, string lineName) where T : DataObject
        {
            var lines = LoadDataSheet<T>(dataFileName, sheetName);
            if (lines == null) return null;
            if (lines.ContainsKey(lineName)) return lines[lineName];
            else return null;
        }
        
        
        


        public static Font LoadFontFromRaylib(string filePath, int fontSize = 100)
        {
            unsafe
            {
                Font f = Raylib.LoadFontEx(filePath, fontSize, (int*)0, GLYPH_COUNT);
                return f;
            }
        }
        public static Shader LoadFragmentShaderFromRaylib(string filePath)
        {
            Shader fs = Raylib.LoadShader(null, filePath);
            return fs;
        }
        public static Shader LoadVertexShaderFromRaylib(string filePath)
        {
            Shader vs = Raylib.LoadShader(filePath, "");
            return vs;
        }
        public static Texture LoadTextureFromRaylib(string filePath)
        {
            Texture t = Raylib.LoadTextureFromImage(LoadImageFromRaylib(filePath));
            return t;
        }
        public static Image LoadImageFromRaylib(string filePath)
        {
            Image i = Raylib.LoadImage(filePath);
            return i;
        }
        public static Wave LoadWaveFromRaylib(string filePath)
        {
            Wave w = Raylib.LoadWave(filePath);
            return w;
        }
        public static Sound LoadSoundFromRaylib(string filePath)
        {
            Sound s = Raylib.LoadSound(filePath);
            return s;
        }
        public static Music LoadMusicFromRaylib(string filePath)
        {
            Music m = Raylib.LoadMusicStream(filePath);
            return m;
        }
        public static string LoadJsonDataFromRaylib(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        
        public static void Generate(string sourcePath, string outputPath, string outputFilename = "resources.txt")
        {
            string[] files = Directory.GetFiles(sourcePath, "", SearchOption.AllDirectories);
            List<string> lines = new List<string>();
            foreach (var file in files)
            {
                lines.Add(Path.GetFileName(file));
                var d = File.ReadAllBytes(file);
                lines.Add(Convert.ToBase64String(Compress(d)));
            }
            File.WriteAllLines(outputPath + outputFilename, lines);
        }
        private static Dictionary<string, ContentInfo> LoadResources(string path, string fileName = "resources.txt", bool editorMode = false)
        {
            if (editorMode) return new Dictionary<string, ContentInfo>() { };

            Dictionary<string, ContentInfo> result = new();
            var lines = File.ReadAllLines(path + fileName);
            for (int i = 0; i < lines.Length; i += 2)
            {
                string filenName = lines[i];
                string name = Path.GetFileNameWithoutExtension(filenName);
                string extension = Path.GetExtension(filenName);
                string dataText = lines[i + 1];
                var data = Convert.FromBase64String(dataText);
                result.Add(name, new(extension, Decompress(data)));
            }
            return result;
        }
        private static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        private static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

    }
    */