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
        atlasWidth = totalGlyphs * glyphWidth;
        atlasHeight = glyphWidth;
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
            // Place glyphs left-to-right in a single row
            var glyphRect = new Rect(
                new Vector2(i * glyphWidth, 0),
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
        if(writeDebugInfo) Console.WriteLine("Atlas generation completed.");
    }

    /// <summary>
    /// Draws the specified text using the atlas, fitting each character into a grid within the given rectangle.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
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
    // /// <summary>
    // /// Draws the entire atlas texture so it fits into the given rectangle, wrapping glyphs to new lines as needed.
    // /// </summary>
    // /// <param name="rect">The rectangle area to fit the atlas into.</param>
    // /// <param name="tint">The color tint to apply to the atlas texture.</param>
    // public void DrawAtlas(Rect rect, ColorRgba tint)
    // {
    //     if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");
    //
    //     int glyphsPerRow = (int)Math.Max(1, rect.Size.Width / glyphWidth);
    //     int rows = (int)Math.Ceiling((double)supportedChars.Count / glyphsPerRow);
    //
    //     float scaleX = (float)rect.Size.Width / (glyphsPerRow * glyphWidth);
    //     float scaleY = (float)rect.Size.Height / (rows * glyphHeight);
    //     float scale = Math.Min(scaleX, scaleY);
    //
    //     int i = 0;
    //     for (int r = 0; r < rows; r++)
    //     {
    //         for (int c = 0; c < glyphsPerRow && i < supportedChars.Count; c++, i++)
    //         {
    //             char ch = supportedChars[i];
    //             if (!glyphUvRects.TryGetValue(ch, out var uvRect)) continue;
    //
    //             var srcRect = new Rectangle(uvRect.TopLeft.X, uvRect.TopLeft.Y, uvRect.Size.Width, -uvRect.Size.Height);
    //             var destX = rect.TopLeft.X + c * glyphWidth * scale;
    //             var destY = rect.TopLeft.Y + r * glyphHeight * scale;
    //             var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
    //             var origin = new Vector2(0, 0);
    //             Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, tint.ToRayColor());
    //         }
    //     }
    // }
    
    /// <summary>
    /// Draws the entire atlas texture so it fits into the given rectangle, wrapping glyphs to new lines as needed.
    /// </summary>
    /// <param name="rect">The rectangle area to fit the atlas into.</param>
    /// <param name="tint">The color tint to apply to the atlas texture.</param>
    public void DrawAtlas(Rect rect, ColorRgba tint)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");
    
        int glyphsPerRow = (int)Math.Max(1, Math.Floor(rect.Size.Width / glyphWidth));
        int rows = (int)Math.Ceiling((double)supportedChars.Count / glyphsPerRow);

        float scaleX = (float)rect.Size.Width / (glyphsPerRow * glyphWidth);
        float scaleY = (float)rect.Size.Height / (rows * glyphHeight);
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = glyphsPerRow * glyphWidth * scale;
        float totalHeight = rows * glyphHeight * scale;
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
                var destX = offsetX + c * glyphWidth * scale;
                var destY = offsetY + r * glyphHeight * scale;
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
}
