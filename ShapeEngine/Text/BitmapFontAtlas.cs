using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text
{
    /// <summary>
    /// BitmapFontAtlas: Renders all glyphs to a texture atlas for fast lookup and drawing.
    /// </summary>
    public class BitmapFontAtlas
    {
        private readonly Dictionary<char, Rect> glyphUvRects = new(); // UV rects for each character
        private readonly int atlasWidth;
        private readonly int atlasHeight;
        private RenderTexture2D atlasTexture;
        private bool isGenerated = false;
        private readonly BitmapFont font;
        private readonly int glyphWidth;
        private readonly int glyphHeight;
        private readonly List<char> supportedChars;

        public BitmapFontAtlas(BitmapFont font, int atlasWidth, int atlasHeight, List<char>? supportedChars = null)
        {
            if (font.Count == 0) throw new ArgumentException("Font or font map is null or empty.");
            if (atlasWidth <= 0 || atlasHeight <= 0) throw new ArgumentException("Atlas size must be positive.");

            this.font = font;
            this.atlasWidth = atlasWidth;
            this.atlasHeight = atlasHeight;
            // If supportedChars is null, use all characters from font
            this.supportedChars = supportedChars ?? new List<char>(font.GetAllChars());

            if (this.supportedChars.Count == 0) throw new ArgumentException("No characters available for atlas generation.");

            // Calculate glyph size based on atlas size and character count
            int totalGlyphs = this.supportedChars.Count;
            int cols = (int)Math.Ceiling(Math.Sqrt(totalGlyphs * atlasWidth / (float)atlasHeight));
            int rows = (int)Math.Ceiling(totalGlyphs / (float)cols);
            this.glyphWidth = atlasWidth / cols;
            this.glyphHeight = atlasHeight / rows;

            if (glyphWidth <= 0 || glyphHeight <= 0)
                throw new ArgumentException("Calculated glyph size is invalid. Check atlas size and character count.");

            atlasTexture = new();
        }

        /// <summary>
        /// Generates the atlas texture and stores UV rects for each character.
        /// </summary>
        public void GenerateAtlas()
        {
            // Create a blank atlas texture
            atlasTexture = Raylib.LoadRenderTexture(atlasWidth, atlasHeight);

            int cols = atlasWidth / glyphWidth;
            int rows = atlasHeight / glyphHeight;
            int i = 0;
            Raylib.BeginTextureMode((RenderTexture2D)atlasTexture);
            foreach (var c in supportedChars)
            {
                var grid = font.GetGrid(c);
                if (grid == null)
                {
                    Console.WriteLine($"BitmapFontAtlas - Generation - Character '{c}' not found in font map.");
                    continue;
                }

                int col = i % cols;
                int row = i / cols;
                var glyhpRect = new Rect(
                    new Vector2(col * glyphWidth, row * glyphHeight),
                    new Size(glyphWidth, glyphHeight),
                    AnchorPoint.TopLeft
                );
                glyphUvRects[c] = glyhpRect;
                font.Draw(c, glyhpRect, ColorRgba.White);
                i++;
            }
            Raylib.EndTextureMode();
            isGenerated = true;
        }

        /// <summary>
        /// Draws text using the atlas. Looks up each character's UV rect and draws that section.
        /// </summary>
        public void Draw(string text, Rect rect, ColorRgba color)
        {
            if (!isGenerated) throw new InvalidOperationException("Atlas not generated.");
            var chars = text.ToCharArray();
            var charRects = rect.GetAlignedRectsGrid(new Grid(chars.Length, 1), new Size(0, 0));
            if (charRects == null) return;
            for (int i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

                // Draw the section of the atlas texture for this character using Raylib.DrawTexturePro
                var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, uvRect.Size.Height);
                var destRect = new Rectangle(charRects[i].TopLeft.X, charRects[i].TopLeft.Y, charRects[i].Size.Width, charRects[i].Size.Height);
                var origin = new Vector2(0, 0);
                float rotation = 0f;
                Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, rotation, color.ToRayColor());
            }
        }
    }
}
