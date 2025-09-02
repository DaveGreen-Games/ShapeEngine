using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text;

public class BitmapFontAtlas
{
    #region Properties
    
    private const int Padding = 2; // Padding between glyphs in the atlas
    
    private readonly Dictionary<char, Rect> glyphUvRects = new(); // UV rects for each character
    private readonly int atlasWidth;
    private readonly int atlasHeight;
    private RenderTexture2D atlasTexture;
    private readonly BitmapFont font;
    private readonly int glyphWidth;
    private readonly int glyphHeight;
    private readonly List<char> supportedChars;
    private readonly int gridRows;
    private readonly int gridCols;
    
    /// <summary>
    /// Indicates whether the font atlas has been generated and is ready for use.
    /// </summary>
    public bool IsGenerated { get; private set; }
    
    #endregion

    #region Constructor
    
    /// <summary>
    /// Initializes a new instance of the <c>BitmapFontAtlas</c> class.
    /// </summary>
    /// <param name="font">The bitmap font containing supported characters.</param>
    /// <param name="glyphWidth">The width of each glyph in the atlas.</param>
    /// <param name="glyphHeight">The height of each glyph in the atlas.</param>
    /// <remarks>
    /// Filtering of supported characters is handled in the <c>BitmapFont</c> class.
    /// </remarks>
    public BitmapFontAtlas(BitmapFont font, int glyphWidth, int glyphHeight)
    {
        if (font.Count == 0) throw new ArgumentException("Font or font map is null or empty.");
        if (glyphWidth <= 0 || glyphHeight <= 0) throw new ArgumentException("Glyph size must be positive.");

        supportedChars = font.SupportedChars;

        if (supportedChars.Count == 0) throw new ArgumentException("No characters available for atlas generation.");
        
        this.font = font;
        int totalGlyphs = supportedChars.Count;

        // Calculate optimal grid size
        (gridRows, gridCols) = CalculateGrid(totalGlyphs);

        atlasWidth = Padding + gridCols * (glyphWidth + Padding);
        atlasHeight = Padding + gridRows * (glyphHeight + Padding);
        
        this.glyphWidth = glyphWidth;
        this.glyphHeight = glyphHeight;
        
        if (atlasWidth <= 0 || atlasHeight <= 0)
            throw new ArgumentException("Calculated atlas size is invalid. Check atlas size and character count.");
    }

    #endregion

    #region Generate Atlas
    
    /// <summary>
    /// Generates the font atlas texture by drawing each glyph with the specified color and background.
    /// Throws an exception if the atlas is already generated.
    /// </summary>
    /// <param name="glyphColor">The color to use for glyphs.</param>
    /// <param name="backgroundColor">The background color of the atlas.</param>
    /// <param name="writeDebugInfo">If true, writes debug information to the console during generation.</param>
    public void GenerateAtlas(ColorRgba glyphColor, ColorRgba backgroundColor, bool writeDebugInfo = false)
    {
        if (IsGenerated) throw new InvalidOperationException("Atlas already generated.");
        if(writeDebugInfo) {
            Console.WriteLine("Order of supportedChars during atlas generation:");
            for (int idx = 0; idx < supportedChars.Count; idx++)
                Console.WriteLine($"[{idx}] '{supportedChars[idx]}'");
        }
        atlasTexture = Raylib.LoadRenderTexture(atlasWidth, atlasHeight);
        if(writeDebugInfo) Console.WriteLine($"Atlas generation started with atlas size: {atlasWidth}x{atlasHeight}, grid: {gridRows}x{gridCols}");
        Raylib.BeginTextureMode(atlasTexture);
        Raylib.DrawRectangle(0, 0, atlasWidth, atlasHeight, backgroundColor.ToRayColor());
        int i = 0;
        
        // Generate atlas in normal top-to-bottom order
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (i >= supportedChars.Count) break;
                var c = supportedChars[i];
                var glyphRect = new Rect(
                    new Vector2(Padding + col * (glyphWidth + Padding), Padding + row * (glyphHeight + Padding)),
                    new Size(glyphWidth, glyphHeight),
                    AnchorPoint.TopLeft
                );
                glyphUvRects[c] = glyphRect;
                font.Draw(c, glyphRect, glyphColor);
                if(writeDebugInfo) Console.WriteLine($"Glyph '{c}' at row {row}, col {col} with {glyphRect}");
                i++;
            }
            if (i >= supportedChars.Count) break;
        }
        Raylib.EndTextureMode();
        IsGenerated = true;
        Raylib.SetTextureFilter(atlasTexture.Texture, TextureFilter.Point);
        if(writeDebugInfo) Console.WriteLine("Atlas generation completed.");
    }
    /// <summary>
    /// Generates the font atlas texture using a custom cell drawing action.
    /// Throws an exception if the atlas is already generated.
    /// </summary>
    /// <param name="backgroundColor">The background color of the atlas.</param>
    /// <param name="drawCell">
    /// An action to draw each cell, with parameters:
    /// <list type="bullet">
    /// <item><description>Rect: The rectangle for the glyph cell.</description></item>
    /// <item><description>bool: Indicates if the cell is active.</description></item>
    /// <item><description>char: The character for the glyph.</description></item>
    /// <item><description>int: The row index.</description></item>
    /// <item><description>int: The column index.</description></item>
    /// </list>
    /// </param>
    /// <param name="writeDebugInfo">If true, writes debug information to the console during generation.</param>
    public void GenerateAtlas(ColorRgba backgroundColor, Action<Rect, bool, char, int, int> drawCell, bool writeDebugInfo = false)
    {
        if (IsGenerated) throw new InvalidOperationException("Atlas already generated.");
        atlasTexture = Raylib.LoadRenderTexture(atlasWidth, atlasHeight);
        if(writeDebugInfo) Console.WriteLine($"Atlas generation started with atlas size: {atlasWidth}x{atlasHeight}, grid: {gridRows}x{gridCols}");
        Raylib.BeginTextureMode(atlasTexture);
        Raylib.DrawRectangle(0, 0, atlasWidth, atlasHeight, backgroundColor.ToRayColor());
        int i = 0;
        
        // Generate atlas in normal top-to-bottom order
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (i >= supportedChars.Count) break;
                var c = supportedChars[i];
                var glyphRect = new Rect(
                    new Vector2(Padding + col * (glyphWidth + Padding), Padding + row * (glyphHeight + Padding)),
                    new Size(glyphWidth, glyphHeight),
                    AnchorPoint.TopLeft
                );
                glyphUvRects[c] = glyphRect;
                font.Draw(c, glyphRect, drawCell);
                if(writeDebugInfo) Console.WriteLine($"Glyph '{c}' at row {row}, col {col} with custom {glyphRect}");
                i++;
            }
            if (i >= supportedChars.Count) break;
        }
        Raylib.EndTextureMode();
        IsGenerated = true;
        Raylib.SetTextureFilter(atlasTexture.Texture, TextureFilter.Point);
        if(writeDebugInfo) Console.WriteLine("Atlas generation completed.");
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
    /// The glyphs are drawn from top-left to bottom-right, with each glyph placed in order of supportedChars.
    /// </summary>
    /// <param name="rect">The rectangle area to fit the atlas into.</param>
    /// <param name="tint">The color tint to apply to the atlas texture.</param>
    /// <param name="padding">Optional padding around each glyph to prevent overlap.</param>
    public void DrawAtlas(Rect rect, ColorRgba tint, int padding = 8)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        // Calculate dynamic grid layout for the given rectangle (original formula)
        int glyphsPerRow = Math.Min(supportedChars.Count, Math.Max(1, (int)Math.Sqrt(rect.Size.Width * supportedChars.Count / rect.Size.Height)));
        int rows = (int)Math.Ceiling((double)supportedChars.Count / glyphsPerRow);
       
        float paddedGlyphWidth = glyphWidth + 2 * padding;
        float paddedGlyphHeight = glyphHeight + 2 * padding;
        float scaleX = rect.Size.Width / (glyphsPerRow * paddedGlyphWidth);
        float scaleY = rect.Size.Height / (rows * paddedGlyphHeight);
        float scale = Math.Min(scaleX, scaleY);
       
        // Calculate centering offsets to center the grid within the rectangle
        float actualGridWidth = glyphsPerRow * paddedGlyphWidth * scale;
        float actualGridHeight = rows * paddedGlyphHeight * scale;
        float offsetX = rect.TopLeft.X + (rect.Size.Width - actualGridWidth) / 2f;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - actualGridHeight) / 2f;
       
        // Draw characters in order from supportedChars, using their UV rectangles from atlas
        for (int charIndex = 0; charIndex < supportedChars.Count; charIndex++)
        {
            int row = charIndex / glyphsPerRow;
            int col = charIndex % glyphsPerRow;
           
            char ch = supportedChars[charIndex];
            if (!glyphUvRects.TryGetValue(ch, out var uvRect)) continue;
           
            var srcRect = GenerateSourceRectangle(uvRect);
            var destX = offsetX + col * paddedGlyphWidth * scale;
            var destY = offsetY + row * paddedGlyphHeight * scale;
            var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
            var origin = new Vector2(0, 0);
            Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, tint.ToRayColor());
        }
    }
    
    #endregion
    
    #region Draw
    
    /// <summary>
    /// Draws the specified text using the atlas, fitting each character into a grid within the given rectangle.
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <remarks>
    /// Glyphs may be distorted if the grid cell aspect ratio does not match the glyph aspect ratio.
    /// Use <see cref="Draw(string, Rect, ColorRgba, float, float, AnchorPoint)"/> for uniform scaling of glyphs.
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

           var srcRect = GenerateSourceRectangle(uvRect);
           var destRect = new Rectangle(charRects[i].TopLeft.X, charRects[i].TopLeft.Y, charRects[i].Size.Width, charRects[i].Size.Height);
           var origin = new Vector2(0, 0);
           float rotation = 0f;
           Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, rotation, color.ToRayColor());
       }
    }
    
    /// <summary>
    /// Draws the specified text using the atlas,
    /// fitting each character into a grid within the given rectangle,
    /// and applies custom horizontal and vertical padding to each glyph. 
    /// Only characters present in the atlas will be drawn.
    /// </summary>
    /// <param name="text">The text string to render.</param>
    /// <param name="rect">The rectangle area where the text will be drawn.</param>
    /// <param name="color">The color to apply to the rendered text.</param>
    /// <param name="paddingX">Horizontal padding to apply to each glyph.</param>
    /// <param name="paddingY">Vertical padding to apply to each glyph.</param>
    /// <param name="alignment">
    /// AnchorPoint value where X is between 0 and 1, determining horizontal alignment (0 = left, 1 = right, 0.5 = center),
    /// and Y is between 0 and 1 for vertical alignment (0 = top, 1 = bottom, 0.5 = center).
    /// </param>
    public void Draw(string text, Rect rect, ColorRgba color, float paddingX, float paddingY, AnchorPoint alignment)
    {
        if (!IsGenerated) throw new InvalidOperationException("Atlas not generated.");

        var chars = text.ToCharArray();
        float paddedGlyphWidth = glyphWidth + 2 * paddingX;
        float paddedGlyphHeight = glyphHeight + 2 * paddingY;

        // Calculate scale to fit all glyphs with padding into the rect
        float scaleX = rect.Size.Width / (chars.Length * paddedGlyphWidth);
        float scaleY = rect.Size.Height / paddedGlyphHeight;
        float scale = Math.Min(scaleX, scaleY);

        float totalWidth = chars.Length * paddedGlyphWidth * scale;
        float totalHeight = paddedGlyphHeight * scale;
        
        float offsetX = rect.TopLeft.X + (rect.Size.Width - totalWidth) * alignment.X;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalHeight) * alignment.Y;

        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!glyphUvRects.TryGetValue(c, out var uvRect)) continue;

            var srcRect = GenerateSourceRectangle(uvRect);
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
    /// <param name="paddingX">Absolute horizontal padding to apply to each glyph.</param>
    /// <param name="paddingY">Absolute vertical padding to apply to each glyph.</param>
    /// <param name="dontSplitWords">If true, words will not be split across lines when wrapping.</param>
    public void DrawWithLineWrap(string text, Rect rect, ColorRgba color, float paddingX = 0f, float paddingY = 0f, bool dontSplitWords = false)
    {
       if (dontSplitWords)
       {
           DrawWithLineWrapNoWordSplit(text, rect, color, paddingX, paddingY);
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

               var srcRect = GenerateSourceRectangle(uvRect);
               var destX = offsetX + col * paddedGlyphWidth * scale + paddingX * scale;
               var destY = offsetY + line * paddedGlyphHeight * scale + paddingY * scale;
               var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
               var origin = new Vector2(0, 0);
               Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
           }
       }
    }
    
    /// <summary>
    /// Draws text using the atlas with uniform scaling and line wrapping, ensuring words are not split across lines.
    /// Each line is built to fit within the available width, and words are wrapped to the next line if necessary.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area to fit the text into.</param>
    /// <param name="color">The color to apply to the glyphs.</param>
    /// <param name="paddingX">Horizontal padding for each glyph.</param>
    /// <param name="paddingY">Vertical padding for each glyph.</param>
    private void DrawWithLineWrapNoWordSplit(string text, Rect rect, ColorRgba color, float paddingX = 0f, float paddingY = 0f)
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

               var srcRect = GenerateSourceRectangle(uvRect);
               var destX = offsetX + col * paddedGlyphWidth * scale + paddingX * scale;
               var destY = offsetY + line * paddedGlyphHeight * scale + paddingY * scale;
               var destRect = new Rectangle(destX, destY, glyphWidth * scale, glyphHeight * scale);
               var origin = new Vector2(0, 0);
               Raylib.DrawTexturePro(atlasTexture.Texture, srcRect, destRect, origin, 0f, color.ToRayColor());
           }
       }
    }

    #endregion
    
    #region Helper
    
    /// <summary>
    /// Unloads the atlas texture from memory and marks the atlas as not generated.
    /// Safe to call multiple times; does nothing if the atlas is not generated.
    /// </summary>
    public void Unload()
    {
        if (!IsGenerated) return;
        Raylib.UnloadRenderTexture(atlasTexture);
        IsGenerated = false;
    }
    
    /// <summary>
    /// Calculates the optimal grid dimensions (rows and columns) for arranging a given number of glyphs.
    /// The goal is to make the grid as square as possible, minimizing dead space and the difference between rows and columns.
    /// </summary>
    /// <param name="count">The total number of glyphs to arrange in the grid.</param>
    /// <returns>A tuple containing the number of rows and columns for the grid.</returns>
    private static (int rows, int cols) CalculateGrid(int count)
    {
        // Try to make the grid as square as possible, minimizing dead space
        int bestRows = 1, bestCols = count;
        int minDeadSpace = int.MaxValue;
        int minDiff = int.MaxValue;
        for (int rows = 1; rows <= count; rows++)
        {
            int cols = (int)Math.Ceiling(count / (float)rows);
            int deadSpace = rows * cols - count;
            int diff = Math.Abs(rows - cols);
            if (deadSpace < minDeadSpace || (deadSpace == minDeadSpace && diff < minDiff))
            {
                minDeadSpace = deadSpace;
                minDiff = diff;
                bestRows = rows;
                bestCols = cols;
            }
        }
        return (bestRows, bestCols);
    }
    
    private Rectangle GenerateSourceRectangle(Rect uvRect)
    {
        return new Rectangle(uvRect.TopLeft.X, atlasHeight - uvRect.TopLeft.Y - uvRect.Size.Height, uvRect.Size.Width, -uvRect.Size.Height);
    }
    
    #endregion
}