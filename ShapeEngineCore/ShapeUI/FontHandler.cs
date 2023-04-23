using Raylib_CsLo;
using ShapePersistent;
using System.Numerics;

namespace ShapeUI
{
    public class FontHandler
    {
        private Dictionary<uint, Font> fonts = new();
        private Font defaultFont = GetFontDefault();
        /*
        //private Dictionary<string, float> fontSizes = new();

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
        */
        public void SetFontFilter(uint id, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
        {
            Font font = GetFont(id);
            SetTextureFilter(font.texture, textureFilter);
        }
        public void AddFont(uint id, string fileName, int fontSize = 100, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
        {
            if (fileName == "" || fonts.ContainsKey(id)) return;
            Font font = ResourceManager.LoadFontFromRaylib(fileName, fontSize);

            SetTextureFilter(font.texture, textureFilter);
            fonts.Add(id, font);
        }
        public void AddFont(uint id, Font font, int fontSize = 100, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
        {
            if (fonts.ContainsKey(id)) return;
            SetTextureFilter(font.texture, textureFilter);
            fonts.Add(id, font);
        }
        public Font GetFont(uint id)
        {
            if (!fonts.ContainsKey(id)) return defaultFont;
            return fonts[id];
        }
        public void SetDefaultFont(uint id)
        {
            if (!fonts.ContainsKey(id)) return;
            defaultFont = fonts[id];
        }
        public void Close()
        {
            foreach (Font font in fonts.Values)
            {
                UnloadFont(font);
            }
            fonts.Clear();
        }

        public float CalculateDynamicFontSize(string text, Vector2 size, uint fontID, float fontSpacing = 1f)
        {
            float baseSize = GetFont(fontID).baseSize;
            return GetFontScalingFactor(text, size, fontID, fontSpacing) * baseSize;
        }
        public float GetFontScalingFactor(string text, Vector2 size, uint fontID, float fontSpacing = 1)
        {
            float baseSize = GetFont(fontID).baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(fontID), text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public Vector2 GetTextSize(string text, float fontSize, float fontSpacing, uint fontID)
        {
            return MeasureTextEx(GetFont(fontID), text, fontSize, fontSpacing);
        }
        public static float CalculateDynamicFontSize(string text, Vector2 size, Font font, float fontSpacing = 1f)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(text, size, font, fontSpacing) * baseSize;
        }
        public static float GetFontScalingFactor(string text, Vector2 size, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(font, text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public static Vector2 GetTextSize(string text, float fontSize, float fontSpacing, Font font)
        {
            return MeasureTextEx(font, text, fontSize, fontSpacing);
        }
        
        /*
        public float CalculateDynamicFontSize(float height, uint fontID)
        {
            return CalculateDynamicFontSize(height, GetFont(fontID));
        }
        public float CalculateDynamicFontSize(string text, float width, uint fontID, float fontSpacing = 1f)
        {
            return CalculateDynamicFontSize(text, width, GetFont(fontID), fontSpacing);
        }
        public float GetFontScalingFactor(float height, uint fontID) { return GetFontScalingFactor(height, GetFont(fontID)); }
        public float GetFontScalingFactor(string text, float width, uint fontID, float fontSpacing = 1)
        {
            float baseSize = GetFont(fontID).baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(fontID), text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
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
        public static float GetFontScalingFactor(string text, float width, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            Vector2 textSize = MeasureTextEx(font, text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
        }
        */
    }

}
