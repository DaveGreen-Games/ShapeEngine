using Raylib_CsLo;
using ShapePersistent;
using System.Numerics;

namespace ShapeUI
{
    public class FontHandler
    {
        private Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        private Font defaultFont = GetFontDefault();
        private Dictionary<string, float> fontSizes = new();

        public void AddFontSize(string name, float size)
        {
            if (fontSizes.ContainsKey(name)) fontSizes[name] = size;
            else fontSizes.Add(name, size);
        }
        public void RemoveFontSize(string name)
        {
            fontSizes.Remove(name);
        }

        public float GetFontSize(string name)
        {
            if (!fontSizes.ContainsKey(name)) return -1;
            else return fontSizes[name];
        }
        public void AddFont(string name, string fileName, int fontSize = 100)
        {
            if (fileName == "" || fonts.ContainsKey(name)) return;
            Font font = ResourceManager.LoadFontFromRaylib(fileName, fontSize);

            SetTextureFilter(font.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            fonts.Add(name, font);
        }
        public void AddFont(string name, Font font, int fontSize = 100)
        {
            if (fonts.ContainsKey(name)) return;
            SetTextureFilter(font.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            fonts.Add(name, font);
        }
        public Font GetFont(string name = "")
        {
            if (name == "" || !fonts.ContainsKey(name)) return defaultFont;
            return fonts[name];
        }
        public void SetDefaultFont(string name)
        {
            if (!fonts.ContainsKey(name)) return;
            defaultFont = fonts[name];
        }
        public void Close()
        {
            foreach (Font font in fonts.Values)
            {
                UnloadFont(font);
            }
            fonts.Clear();
        }

        public float CalculateDynamicFontSize(string text, Vector2 size, float fontSpacing = 1f, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            return GetFontScalingFactor(text, size, fontSpacing, fontName) * baseSize;
        }
        public float CalculateDynamicFontSize(float height, string fontName = "")
        {
            return CalculateDynamicFontSize(height, GetFont(fontName));
        }
        public float CalculateDynamicFontSize(string text, float width, float fontSpacing = 1f, string fontName = "")
        {
            return CalculateDynamicFontSize(text, width, GetFont(fontName), fontSpacing);
        }
        public float GetFontScalingFactor(float height, string fontName = "") { return GetFontScalingFactor(height, GetFont(fontName)); }
        public float GetFontScalingFactor(string text, float width, float fontSpacing = 1, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(fontName), text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
        }
        public float GetFontScalingFactor(string text, Vector2 size, float fontSpacing = 1, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(fontName), text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public Vector2 GetTextSize(string text, float fontSize, float fontSpacing, string fontName = "")
        {
            return MeasureTextEx(GetFont(fontName), text, fontSize, fontSpacing);
        }

        public static float CalculateDynamicFontSize(string text, Vector2 size, Font font, float fontSpacing = 1f)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(text, size, font, fontSpacing) * baseSize;
        }
        public static float CalculateDynamicFontSize(float height, Font font)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(height, font) * baseSize;
        }
        public static float CalculateDynamicFontSize(string text, float width, Font font, float fontSpacing = 1f)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(text, width, font, fontSpacing) * baseSize;
        }
        public static float GetFontScalingFactor(float height, Font font)
        {
            float baseSize = font.baseSize;
            return height / baseSize;
        }
        public static float GetFontScalingFactor(string text, Vector2 size, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(font, text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public static float GetFontScalingFactor(string text, float width, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            Vector2 textSize = MeasureTextEx(font, text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
        }
        public static Vector2 GetTextSize(string text, float fontSize, float fontSpacing, Font font)
        {
            return MeasureTextEx(font, text, fontSize, fontSpacing);
        }

    }

}
