using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text;

/// <summary>
/// Represents a bitmap font atlas that stores glyphs for efficient text rendering.
/// Handles atlas generation, drawing text using the atlas, and resource management.
/// </summary>
public class BitmapFontAtlas
{
    private const int Padding = 2; // Padding between glyphs in the atlas
    
    private readonly Dictionary<char, Rect> glyphUvRects = new(); // UV rects for each character
    private readonly int atlasWidth;
    private readonly int atlasHeight;
    private RenderTexture2D atlasTexture;
    private readonly BitmapFont font;
    private readonly int glyphWidth;
    private readonly int glyphHeight;
    private readonly List<char> supportedChars;
    public bool IsGenerated { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <c>BitmapFontAtlas</c> class.
    /// </summary>
    /// <param name="font">The bitmap font containing glyphs to be used in the atlas.</param>
    /// <param name="glyphWidth">The width of each glyph in the atlas.</param>
    /// <param name="glyphHeight">The height of each glyph in the atlas.</param>
    /// <param name="supportedChars">
    /// Optional. The list of characters to include in the atlas. If null, all characters from the font are used.
    /// </param>
    public BitmapFontAtlas(BitmapFont font, int glyphWidth, int glyphHeight, List<char>? supportedChars = null)
    {
        if (font.Count == 0) throw new ArgumentException("Font or font map is null or empty.");
        if (glyphWidth <= 0 || glyphHeight <= 0) throw new ArgumentException("Glyph size must be positive.");

        this.supportedChars = supportedChars ?? [..font.GetAllChars()];

        if (this.supportedChars.Count == 0) throw new ArgumentException("No characters available for atlas generation.");
        
        this.font = font;
        int totalGlyphs = this.supportedChars.Count;
        
        atlasWidth = Padding + totalGlyphs * (glyphWidth + Padding);
        atlasHeight = glyphHeight + 2 * Padding;
        
        this.glyphWidth = glyphWidth;
        this.glyphHeight = glyphHeight;
        
        if (atlasWidth <= 0 || atlasHeight <= 0)
            throw new ArgumentException("Calculated atlas size is invalid. Check atlas size and character count.");
    }

    /// <summary>
    /// Generates the bitmap font atlas by rendering all supported glyphs onto a texture.
    /// The atlas is filled with the specified background color. Optionally outputs debug info.
    /// </summary>
    /// <param name="glyphColor">The color used to render each glyph.</param>
    /// <param name="backgroundColor">The background color to fill the atlas texture.</param>
    /// <param name="writeDebugInfo">If true, writes debug information to the console during generation.</param>
    public void GenerateAtlas(ColorRgba glyphColor, ColorRgba backgroundColor, bool writeDebugInfo = false)
    {
        if (IsGenerated) throw new InvalidOperationException("Atlas already generated.");
        
        // Create a blank atlas texture
        atlasTexture = Raylib.LoadRenderTexture(atlasWidth, atlasHeight);

        if(writeDebugInfo) Console.WriteLine($"Atlas generation started with atlas size: {atlasWidth}x{atlasHeight}");
        Raylib.BeginTextureMode(atlasTexture);
        Raylib.DrawRectangle(0, 0, atlasWidth, atlasHeight, backgroundColor.ToRayColor());
        int i = 0;
        foreach (var c in supportedChars)
        {
            var glyphRect = new Rect(
                new Vector2(Padding + i * (glyphWidth + Padding), Padding),
                new Size(glyphWidth, glyphHeight),
                AnchorPoint.TopLeft
            );
            glyphUvRects[c] = glyphRect;
            font.Draw(c, glyphRect, glyphColor);
            if(writeDebugInfo) Console.WriteLine($"Glyph '{c}' with {glyphRect}");
            i++;
        }
        Raylib.EndTextureMode();
        IsGenerated = true;
        Raylib.SetTextureFilter(atlasTexture.Texture, TextureFilter.Point);
        if(writeDebugInfo) Console.WriteLine("Atlas generation completed.");
    }
    
    
    /// <summary>
    /// Draws the specified text using the atlas, fitting each character into a grid within the given rectangle.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <remarks>
    /// Glyphs may be distorted if the grid cell aspect ratio does not match the glyph aspect ratio.
    /// Use <see cref="DrawUniform(string, Rect, ColorRgba)"/> for uniform scaling of glyphs.
    /// </remarks>
    public void Draw(string text, Rect rect, ColorRgba color)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");
        
        var chars = text.ToCharArray();
        var charRects = rect.GetAlignedRectsGrid(new Grid(chars.Length, 1), new Size(0, 0));
        if (charRects == null) return;
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

            // Draw the section of the atlas texture for this character using Raylib.DrawTexturePro
            // Raylib render textures are vertically flipped, so flip the source rectangle's height
            var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
            var destRect = new Rectangle(charRects[i].TopLeft.X, charRects[i].TopLeft.Y, charRects[i].Size.Width, charRects[i].Size.Height);
            var origin = new Vector2(0, 0);
            float rotation = 0f;
            Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, rotation, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws the specified text using the atlas, fitting each character into a grid within the given rectangle,
    /// and applies custom horizontal and vertical padding to each glyph.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <param name="paddingX">Horizontal padding to apply to each glyph.</param>
    /// <param name="paddingY">Vertical padding to apply to each glyph.</param>
    public void Draw(string text, Rect rect, ColorRgba color, float paddingX, float paddingY)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        var chars = text.ToCharArray();
        float paddedGlyphWidth = glyphWidth + 2 * paddingX;
        float paddedGlyphHeight = glyphHeight + 2 * paddingY;
        var charRects = rect.GetAlignedRectsGrid(new Grid(chars.Length, 1), new Size(paddedGlyphWidth, paddedGlyphHeight));
        if (charRects == null) return;
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

            var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
            var destX = charRects[i].TopLeft.X + paddingX;
            var destY = charRects[i].TopLeft.Y + paddingY;
            var destRect = new Rectangle(destX, destY, glyphWidth, glyphHeight);
            var origin = new Vector2(0, 0);
            float rotation = 0f;
            Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, rotation, color.ToRayColor());
        }
    }
   
    /// <summary>
    /// Draws the specified text using the atlas, scaling each glyph uniformly to fit within the given rectangle.
    /// The text is centered and scaled to maintain the glyph aspect ratio.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    public void DrawUniform(string text, Rect rect, ColorRgba color)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");
    
        var chars = text.ToCharArray();
    
        // Calculate uniform scale based on glyph aspect ratio
        float scaleX = rect.Size.Width / (chars.Length * glyphWidth);
        float scaleY = rect.Size.Height / glyphHeight;
        float scale = Math.Min(scaleX, scaleY);
    
        float totalWidth = chars.Length * glyphWidth * scale;
        float totalHeight = glyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) / 2f;
    
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;
    
            var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
            var destX = offsetX + i * glyphWidth * scale;
            var destRect = new Rectangle(destX, offsetY, glyphWidth * scale, glyphHeight * scale);
            var origin = new Vector2(0, 0);
            Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
        }
    }
    
    /// <summary>
    /// Draws the specified text using the atlas, scaling each glyph uniformly to fit within the given rectangle,
    /// and applies custom horizontal and vertical padding to each glyph.
    /// The text is centered and scaled to maintain the glyph aspect ratio.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <param name="paddingX">Horizontal padding to apply to each glyph.</param>
    /// <param name="paddingY">Vertical padding to apply to each glyph.</param>
    public void DrawUniform(string text, Rect rect, ColorRgba color, float paddingX, float paddingY)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        var chars = text.ToCharArray();

        // Calculate uniform scale based on glyph aspect ratio and padding
        float paddedGlyphWidth = glyphWidth + 2 * paddingX;
        float paddedGlyphHeight = glyphHeight + 2 * paddingY;
        float scaleX = rect.Size.Width / (chars.Length * paddedGlyphWidth);
        float scaleY = rect.Size.Height / paddedGlyphHeight;
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = chars.Length * paddedGlyphWidth * scale;
        float totalHeight = paddedGlyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) / 2f;

        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

            var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
            var destX = offsetX + i * paddedGlyphWidth * scale + paddingX * scale;
            var destY = offsetY + paddingY * scale;
            var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
            var origin = new Vector2(0, 0);
            Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
        }
    }
    
    /// <summary>
    /// Draws the specified text using the atlas, scaling each glyph uniformly to fit within the given rectangle,
    /// and wraps text to new lines as needed. Optionally prevents splitting words across lines.
    /// The text is centered and scaled to maintain the glyph aspect ratio.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <param name="paddingX">Horizontal padding to apply to each glyph.</param>
    /// <param name="paddingY">Vertical padding to apply to each glyph.</param>
    /// <param name="dontSplitWords">If true, words will not be split across lines when wrapping.</param>
    public void DrawUniformWithLineWrap(string text, Rect rect, ColorRgba color, float paddingX = 0f, float paddingY = 0f, bool dontSplitWords = false)
    {
        if (dontSplitWords)
        {
            DrawUniformWithLineWrapNoWordSplit(text, rect, color, paddingX, paddingY);
            return;
        }
        
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        var chars = text.ToCharArray();
        float paddedGlyphWidth = glyphWidth + 2 * paddingX;
        float paddedGlyphHeight = glyphHeight + 2 * paddingY;

        int glyphsPerLine = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(chars.Length * rect.Size.Width / rect.Size.Height)));
        int lines = (int)Math.Ceiling((double)chars.Length / glyphsPerLine);

        float scaleX = rect.Size.Width / (glyphsPerLine * paddedGlyphWidth);
        float scaleY = rect.Size.Height / (lines * paddedGlyphHeight);
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = glyphsPerLine * paddedGlyphWidth * scale;
        float totalHeight = lines * paddedGlyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) / 2f;

        int i = 0;
        for (int line = 0; line < lines; line++)
        {
            for (int col = 0; col < glyphsPerLine && i < chars.Length; col++, i++)
            {
                var c = chars[i];
                if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

                var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
                var destX = offsetX + col * paddedGlyphWidth * scale + paddingX * scale;
                var destY = offsetY + line * paddedGlyphHeight * scale + paddingY * scale;
                var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
                var origin = new Vector2(0, 0);
                Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
            }
        }
    }
    
    /// <summary>
    /// Draws the entire atlas texture at the specified position and scale, applying the given tint color.
    /// The atlas is vertically flipped to match the expected orientation.
    /// </summary>
    /// <param name="position">The position on the screen where the atlas will be drawn.</param>
    /// <param name="scale">The scale factor to apply to the atlas texture.</param>
    /// <param name="tint">The color tint to apply to the atlas texture.</param>
    public void DrawAtlas(Vector2 position, Vector2 scale, ColorRgba tint)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");
        
        // Flip the atlas texture vertically when drawing
        var srcRect = new Rectangle(0, 0, atlasWidth, -atlasHeight);
        var destRect = new Rectangle(position.X, position.Y, atlasWidth * scale.X, atlasHeight * scale.Y);
        var origin = new Vector2(0, 0);
        const float rotation = 0f;
        Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, rotation, tint.ToRayColor());
    }
    
    /// <summary>
    /// Draws the entire atlas texture so it fits into the given rectangle, wrapping glyphs to new lines as needed.
    /// The glyphs are scaled and centered within the rectangle, and a tint color is applied.
    /// </summary>
    /// <param name="rect">The rectangle area to fit the atlas into.</param>
    /// <param name="tint">The color tint to apply to the atlas texture.</param>
    /// <param name="padding">Optional padding around each glyph to prevent overlap.</param>
    public void DrawAtlas(Rect rect, ColorRgba tint, int padding = 8)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        int glyphsPerRow = Math.Min(supportedChars.Count, Math.Max(1, (int)Math.Sqrt(rect.Size.Width * supportedChars.Count / rect.Size.Height)));
        int rows = (int)Math.Ceiling((double)supportedChars.Count / glyphsPerRow);

        float paddedGlyphWidth = glyphWidth + 2 * padding;
        float paddedGlyphHeight = glyphHeight + 2 * padding;

        float scaleX = rect.Size.Width / (glyphsPerRow * paddedGlyphWidth);
        float scaleY = rect.Size.Height / (rows * paddedGlyphHeight);
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = glyphsPerRow * paddedGlyphWidth * scale;
        float totalHeight = rows * paddedGlyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) / 2f;

        int i = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < glyphsPerRow && i < supportedChars.Count; c++, i++)
            {
                char ch = supportedChars[i];
                if (!glyphUvRects.TryGetValue(ch, out var uvRect)) continue;

                var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
                var destX = offsetX + c * paddedGlyphWidth * scale;
                var destY = offsetY + r * paddedGlyphHeight * scale;
                var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
                var origin = new Vector2(0, 0);
                Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, tint.ToRayColor());
            }
        }
    }
    
    
    /// <summary>
    /// Unloads the atlas texture from GPU memory and marks the atlas as not generated.
    /// Call this when the atlas is no longer needed to free resources.
    /// </summary>
    public void Unload()
    {
        if (!IsGenerated) return;
        Raylib.UnloadRenderTexture(atlasTexture);
        IsGenerated = false;
    }
    
    
    private void DrawUniformWithLineWrapNoWordSplit(string text, Rect rect, ColorRgba color, float paddingX = 0f, float paddingY = 0f)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        var words = text.Split(' ');
        float paddedGlyphWidth = glyphWidth + 2 * paddingX;
        float paddedGlyphHeight = glyphHeight + 2 * paddingY;

        // Estimate max glyphs per line
        int maxGlyphsPerLine = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(text.Length * rect.Size.Width / rect.Size.Height)));

        // Build lines without splitting words
        var linesList = new List<string>();
        string currentLine = "";
        int currentLineLen = 0;
        foreach (var word in words)
        {
            int wordLen = word.Length + (currentLineLen > 0 ? 1 : 0); // +1 for space
            if (currentLineLen + wordLen > maxGlyphsPerLine && currentLineLen > 0)
            {
                linesList.Add(currentLine);
                currentLine = word;
                currentLineLen = word.Length;
            }
            else
            {
                if (currentLineLen > 0)
                {
                    currentLine += " ";
                    currentLineLen++;
                }
                currentLine += word;
                currentLineLen += word.Length;
            }
        }
        if (currentLineLen > 0) linesList.Add(currentLine);

        int lines = linesList.Count;
        int glyphsPerLine = linesList.Max(l => l.Length);

        float scaleX = rect.Size.Width / (glyphsPerLine * paddedGlyphWidth);
        float scaleY = rect.Size.Height / (lines * paddedGlyphHeight);
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = glyphsPerLine * paddedGlyphWidth * scale;
        float totalHeight = lines * paddedGlyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) / 2f;

        for (int line = 0; line < lines; line++)
        {
            var chars = linesList[line].ToCharArray();
            for (int col = 0; col < chars.Length; col++)
            {
                var c = chars[col];
                if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

                var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
                var destX = offsetX + col * paddedGlyphWidth * scale + paddingX * scale;
                var destY = offsetY + line * paddedGlyphHeight * scale + paddingY * scale;
                var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
                var origin = new Vector2(0, 0);
                Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
            }
        }
    }
        
}
