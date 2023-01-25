using Raylib_CsLo;
using System.Text;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapePersistent
{
    //shader loading does not work yet
    internal struct ResourceInfo
    {
        public string extension;
        public byte[] data;

        public ResourceInfo(string extension, byte[] data) { this.extension = extension; this.data = data; }
    }
    public class ResourceManager
    {
        private Dictionary<string, ResourceInfo> resources = new();


        private List<Shader> shadersToUnload = new();
        private List<Texture> texturesToUnload = new();
        private List<Image> imagesToUnload = new();
        private List<Font> fontsToUnload = new();
        private List<Sound> soundsToUnload = new();
        private List<Music> musicToUnload = new();
        private List<Wave> wavesToUnload = new();

        private string path = "";
        private string resourceFileName = "";

        public ResourceManager(string path, string resourceFileName = "resources.txt")
        {
            if (path == "" || resourceFileName == "") return;
            this.path = path;
            this.resourceFileName = resourceFileName;
            resources = LoadResources(path, resourceFileName);
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
            if (EDITORMODE)
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
            if (EDITORMODE)
            {
                unsafe
                {
                    Font f = Raylib.LoadFontEx(filePath, fontSize, (int*)0, 300);
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
                        return Raylib.LoadFontFromMemory(extension, ptr, data.Length, fontSize, (int*)0, 300);
                    }
                }
            }
        }
        public Wave LoadWave(string filePath, bool autoUnload = false)
        {
            if (EDITORMODE)
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
            if (EDITORMODE)
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
            if (EDITORMODE)
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
            if (EDITORMODE)
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
            if (EDITORMODE)
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

        public string LoadJsonData(string filePath)
        {
            if (EDITORMODE)
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                return Encoding.Default.GetString(resources[fileName].data);
            }
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


        public static Font LoadFontFromRaylib(string filePath, int fontSize = 100)
        {
            unsafe
            {
                Font f = Raylib.LoadFontEx(filePath, fontSize, (int*)0, 300);
                return f;
            }
        }
        public static Shader LoadFragmentShaderFromRaylib(string filePath)
        {
            Shader fs = Raylib.LoadShader(null, filePath);
            return fs;
        }
        public Shader LoadVertexShaderFromRaylib(string filePath)
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

        private static Dictionary<string, ResourceInfo> LoadResources(string path, string fileName = "resources.txt")
        {
            if (EDITORMODE) return new Dictionary<string, ResourceInfo>() { };

            Dictionary<string, ResourceInfo> result = new();
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


}
