using Raylib_CsLo;

namespace ShapePersistent
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
        public static Music LoadMusic(string filePath)
        {
            Music m = Raylib.LoadMusicStream(filePath);
            return m;
        }
        public static string LoadJson(string filePath)
        {
            return File.ReadAllText(filePath);
        }

    }

}
