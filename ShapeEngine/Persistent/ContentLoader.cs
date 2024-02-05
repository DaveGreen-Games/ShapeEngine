using Raylib_CsLo;
using System.Collections;

namespace ShapeEngine.Persistent
{
    /// <summary>
    /// Provides a simple wraper to load all types of Raylib resources and json string.
    /// </summary>
    public static class ContentLoader
    {
        public static int GLYPH_COUNT = 0;

        public static Font LoadFont(string filePath, int fontSize = 100)
        {
            unsafe
            {
                Font f = Raylib.LoadFontEx(filePath, fontSize, (int*)0, GLYPH_COUNT);
                Raylib.SetTextureFilter(f.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                return f;
            }
        }
        public static Shader LoadFragmentShader(string filePath)
        {
            Shader fs = Raylib.LoadShader(null, filePath);
            return fs;
        }
        public static Shader LoadVertexShader(string filePath)
        {
            Shader vs = Raylib.LoadShader(filePath, "");
            return vs;
        }
        public static Texture LoadTexture(string filePath)
        {
            Texture t = Raylib.LoadTextureFromImage(LoadImage(filePath));
            return t;
        }
        public static Image LoadImage(string filePath)
        {
            Image i = Raylib.LoadImage(filePath);
            return i;
        }
        public static Wave LoadWave(string filePath)
        {
            Wave w = Raylib.LoadWave(filePath);
            return w;
        }
        public static Sound LoadSound(string filePath)
        {
            Sound s = Raylib.LoadSound(filePath);
            return s;
        }
        public static Music LoadMusicStream(string filePath)
        {
            Music m = Raylib.LoadMusicStream(filePath);
            return m;
        }
        public static string LoadJson(string filePath)
        {
            return File.ReadAllText(filePath);
        }


        public static void UnloadFont(Font font) { Raylib.UnloadFont(font); }
        public static void UnloadFonts(IEnumerable<Font> fonts) { foreach (var font in fonts) UnloadFont(font); }

        public static void UnloadShader(Shader shader) { Raylib.UnloadShader(shader); }
        public static void UnloadShaders(IEnumerable<Shader> shaders) { foreach (var shader in shaders) UnloadShader(shader); }


        public static void UnloadTexture(Texture texture) { Raylib.UnloadTexture(texture); }
        public static void UnloadTextures(IEnumerable<Texture> textures) { foreach (var texture in textures) UnloadTexture(texture); }

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
