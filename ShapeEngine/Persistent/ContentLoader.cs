using System.Text;
using Raylib_cs;
using ShapeEngine.Core;


namespace ShapeEngine.Persistent
{
    /// <summary>
    /// Provides a simple wraper to load all types of Raylib resources and json string.
    /// </summary>
    public static class ContentLoader
    {
        public static int GLYPH_COUNT = 0;

        private static string GetMacOsAppBundleResourcePath(string relativePath)
        {
            if (!Game.OSXIsRunningInAppBundle()) return relativePath;
            
            // macOS .app bundle: executable is in Contents/MacOS/
            // Resources are in Contents/Resources/
            string exeDir = AppContext.BaseDirectory; // This is Contents/MacOS/
            string resourcesDir = Path.Combine(exeDir, "..", "Resources");//".." goes up one level to Contents
            string fullPath = Path.GetFullPath(Path.Combine(resourcesDir, relativePath));
            Console.WriteLine($"--- MacOS app bundle loading resource from path: {fullPath}");
            return fullPath;
        }
        
        public static Font LoadFont(string filePath, int fontSize = 100, TextureFilter textureFilter = TextureFilter.Trilinear)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            var f = Raylib.LoadFontEx(filePath, fontSize, Array.Empty<int>(), GLYPH_COUNT);
            Raylib.SetTextureFilter(f.Texture, textureFilter);
            return f;
        }
        public static Shader LoadFragmentShader(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadShader(null, filePath);;
        }
        public static Shader LoadVertexShader(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadShader(filePath, "");
        }
        public static Texture2D LoadTexture(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadTexture(filePath);
        }
        public static Image LoadImage(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadImage(filePath);
        }
        public static Wave LoadWave(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadWave(filePath);
        }
        public static Sound LoadSound(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadSound(filePath);
        }
        public static Music LoadMusicStream(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return Raylib.LoadMusicStream(filePath);
        }
        public static string LoadJson(string filePath)
        {
            if (Game.IsOSX())
            {
                filePath = GetMacOsAppBundleResourcePath(filePath);
            }
            return File.ReadAllText(filePath);
        }

        
        public static Texture2D LoadTextureFromContent(ContentInfo content)
        {
            return Raylib.LoadTextureFromImage(LoadImageFromContent(content));;
        }
        public static Image LoadImageFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            byte[] data = content.data;
            string extension = content.extension;
            return Raylib.LoadImageFromMemory(extension, data);
        }
        public static Font LoadFontFromContent(ContentInfo content, int fontSize = 100)
        {
            
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            byte[] data = content.data;
            string extension = content.extension;
            return Raylib.LoadFontFromMemory(extension, data, fontSize, Array.Empty<int>(), GLYPH_COUNT);
            
        }
        public static Wave LoadWaveFromContent(ContentInfo content)
        {
            // string filename = Path.GetFileNameWithoutExtension(filePath);
            byte[] data = content.data;
            string extension = content.extension;
            return Raylib.LoadWaveFromMemory(extension, data);
        }
        public static Sound LoadSoundFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            return Raylib.LoadSoundFromWave(LoadWaveFromContent(content));

        }
        public static Music LoadMusicFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            byte[] data = content.data;
            string extension = content.extension;
            return Raylib.LoadMusicStreamFromMemory(extension, data);
        }
        public static Shader LoadFragmentShaderFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            string file = Encoding.Default.GetString(content.data);
            return Raylib.LoadShaderFromMemory(null, file);
        }
        public static Shader LoadVertexShaderFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            string file = Encoding.Default.GetString(content.data);
            return Raylib.LoadShaderFromMemory(file, null);
        }
        public static string LoadJsonFromContent(ContentInfo content)
        {
            // string fileName = Path.GetFileNameWithoutExtension(filePath);
            return Encoding.Default.GetString(content.data);
        }

        

        public static void UnloadFont(Font font) { Raylib.UnloadFont(font); }
        public static void UnloadFonts(IEnumerable<Font> fonts) { foreach (var font in fonts) UnloadFont(font); }

        public static void UnloadShader(Shader shader) { Raylib.UnloadShader(shader); }
        public static void UnloadShaders(IEnumerable<Shader> shaders) { foreach (var shader in shaders) UnloadShader(shader); }


        public static void UnloadTexture(Texture2D texture) { Raylib.UnloadTexture(texture); }
        public static void UnloadTextures(IEnumerable<Texture2D> textures) { foreach (var texture in textures) UnloadTexture(texture); }

        public static void UnloadImage(Image image) { Raylib.UnloadImage(image); }
        public static void UnloadImages(IEnumerable<Image> images) { foreach (var image in images) UnloadImage(image); }

        public static void UnloadWave(Wave wave) { Raylib.UnloadWave(wave); }
        public static void UnloadWaves(IEnumerable<Wave> waves) { foreach (var wave in waves) UnloadWave(wave); }

        public static void UnloadSound(Sound sound) { Raylib.UnloadSound(sound); }
        public static void UnloadSounds(IEnumerable<Sound> sounds) { foreach (var sound in sounds) UnloadSound(sound); }

        public static void UnloadMusicStream(Music musicStream) { Raylib.UnloadMusicStream(musicStream); }
        public static void UnloadMusicStreams(IEnumerable<Music> musicStreams) { foreach (var musicStream in musicStreams) UnloadMusicStream(musicStream); }
    }

}
