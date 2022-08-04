using Raylib_CsLo;
using System.Text;
using System.IO.Compression;

namespace ShapeEngineCore.Globals.Persistent
{
    //shader loading does not work yet
    internal struct ResourceInfo
    {
        public string extension;
        public byte[] data;

        public ResourceInfo(string extension, byte[] data) { this.extension = extension; this.data = data; }
    }
    public static class ResourceManager
    {
        private static Dictionary<string, ResourceInfo> resources = new();

        
        public static void Initialize(string path)
        {
            resources = LoadResources(path);
        }

        public static void Generate(string sourcePath, string outputPath)
        {
            string filename = "resources.shp";
            string[] files = Directory.GetFiles(sourcePath, "", SearchOption.AllDirectories);
            List<string> lines = new List<string>();
            foreach (var file in files)
            {
                lines.Add(Path.GetFileName(file));
                var d = File.ReadAllBytes(file);
                lines.Add(Convert.ToBase64String(Compress(d)));
            }
            File.WriteAllLines(outputPath + filename, lines);
        }

        
        
        public static Texture LoadTexture(string name)
        {
            return Raylib.LoadTextureFromImage(LoadImage(name));
        }
        public static Image LoadImage(string name)
        {
            unsafe
            {
                var data = resources[name].data;
                var extension = resources[name].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadImageFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public static Font LoadFont(string name, int fontSize = 100)
        {
            unsafe
            {
                var data = resources[name].data;
                var extension = resources[name].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadFontFromMemory(extension, ptr, data.Length, fontSize, (int*)0, 300);
                }
            }
        }
        public static Wave LoadWave(string name)
        {
            unsafe
            {
                var data = resources[name].data;
                var extension = resources[name].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadWaveFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public static Sound LoadSound(string name)
        {
            return Raylib.LoadSoundFromWave(LoadWave(name));
        }
        public static Music LoadMusic(string name)
        {
            unsafe
            {
                var data = resources[name].data;
                var extension = resources[name].extension;
                fixed (byte* ptr = data)
                {
                    return Raylib.LoadMusicStreamFromMemory(extension, ptr, data.Length);
                }
            }
        }
        public static Shader LoadFragmentShader(string name)
        {
            if (Path.HasExtension(name)) return Raylib.LoadShader(null, name);
            else
            {
                string file = Encoding.Default.GetString(resources[name].data);
                return Raylib.LoadShaderFromMemory(null, file);
            }
        }
        public static Shader LoadVertexShader(string name)
        {
            if (Path.HasExtension(name)) return Raylib.LoadShader(name, "");
            else
            {
                string file = Encoding.Default.GetString(resources[name].data);
                return Raylib.LoadShaderFromMemory(file, "");
            }
        }
        public static string LoadJsonData(string name)
        {
            return Encoding.Default.GetString(resources[name].data);
        }


        private static Dictionary<string, ResourceInfo> LoadResources(string path)
        {
            Dictionary<string, ResourceInfo> result = new();
            var lines = File.ReadAllLines(path + "resources.shp");
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
