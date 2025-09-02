using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text;

/// <summary>
/// Simple bitmap font drawer using string arrays for each character.
/// </summary>
public partial class BitmapFont
{
    private readonly Dictionary<char, string[]> fontMap;
    private readonly int gridWidth;
    private readonly int gridHeight;

    /// <summary>
    /// Gets the number of characters defined in the font.
    /// </summary>
    public int Count => fontMap.Count;

    /// <summary>
    /// Returns all characters available in the font.
    /// </summary>
    public IEnumerable<char> GetAllChars() => fontMap.Keys;

    /// <summary>
    /// Gets the grid representation for the specified character.
    /// Returns null if the character is not defined in the font.
    /// </summary>
    /// <param name="c">The character to retrieve the grid for.</param>
    /// <returns>String array representing the character grid, or null if not found.</returns>
    public string[]? GetGrid(char c) => fontMap.GetValueOrDefault(c);

    /// <summary>
    /// Initializes a new instance of the BitmapFontRenderer.
    /// </summary>
    /// <param name="fontMap">
    /// Dictionary mapping characters to their grid representation as string arrays.
    /// Can not be empty and has to contain all characters that will be drawn.
    /// </param>
    /// <remarks>
    /// All characters need to have the same amount of rows (string array entries) and columns (string length).
    /// </remarks>
    public BitmapFont(Dictionary<char, string[]> fontMap)
    {
        if (fontMap == null || fontMap.Count == 0)
            throw new ArgumentException("Font map cannot be null or empty.");

        int expectedRows = fontMap.Values.First().Length;
        int expectedCols = fontMap.Values.First()[0].Length;

        foreach (var entry in fontMap)
        {
            var grid = entry.Value;
            if (grid.Length != expectedRows)
                throw new ArgumentException($"Character '{entry.Key}' does not have {expectedRows} rows.");

            foreach (var row in grid)
            {
                if (row.Length != expectedCols)
                    throw new ArgumentException($"Character '{entry.Key}' has a row with incorrect length (expected {expectedCols}).");
            }
        }

        this.fontMap = fontMap;
        gridHeight = expectedRows;
        gridWidth = expectedCols;
    }


    #region Draw Char
    
    /// <summary>
    /// Draws a single character into the specified rectangle using the provided color.
    /// </summary>
    /// <param name="c">The character to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the character.</param>
    /// <param name="cellColor">The color to use for each filled cell.</param>
    /// <remarks>
    /// Only cells marked as '1' in the character grid are drawn. If the rectangle is too small, nothing is drawn.
    /// </remarks>
    public void Draw(char c, Rect rect, ColorRgba cellColor)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (!fontMap.TryGetValue(c, out string[]? grid)) return;
        var cellRects = rect.Split(gridWidth, gridHeight);
        for (var row = 0; row < gridHeight; row++)
        {
            for (var col = 0; col < gridWidth; col++)
            {
                if (grid[row][col] != '1') continue;
                int cellIndex = row * gridWidth + col;
                cellRects[cellIndex].Draw(cellColor);
            }
        }
    }
    /// <summary>
    /// Draws a single character into the specified rectangle using a custom cell drawing action.
    /// </summary>
    /// <param name="c">The character to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the character.</param>
    /// <param name="drawCell">Action to invoke for each filled cell, providing the cell rectangle, character, row, and column.</param>
    /// <remarks>
    /// Only cells marked as '1' in the character grid are drawn. If the rectangle is too small, nothing is drawn.
    /// </remarks>
    public void Draw(char c, Rect rect, Action<Rect, char, int, int> drawCell)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (!fontMap.TryGetValue(c, out string[]? grid)) return;
        var cellRects = rect.Split(gridWidth, gridHeight);
        for (var row = 0; row < gridHeight; row++)
        {
            for (var col = 0; col < gridWidth; col++)
            {
                if (grid[row][col] != '1') continue;
                int cellIndex = row * gridWidth + col;
                drawCell(cellRects[cellIndex], c, row, col);
            }
        }
    }
    
    #endregion

    #region Draw
    /// <summary>
    /// Draws a string of text into the specified rectangle using the provided color.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="cellColor">The color to use for each filled cell.</param>
    /// <param name="spacing">Spacing between characters, relative to character width.</param>
    /// <remarks>
    /// Each character is drawn in sequence, spaced according to the spacing parameter. Only cells marked as '1' are drawn.
    /// </remarks>
    public void Draw(string text, Rect rect, ColorRgba cellColor, float spacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;

        float charWidth = rect.Size.Width / (charCount + (charCount - 1) * spacing);
        float charHeight = rect.Size.Height;
        float cellWidth = charWidth / gridWidth;
        float cellHeight = charHeight / gridHeight;

        float x = rect.TopLeft.X;
        float y = rect.TopLeft.Y;

        for (int i = 0; i < charCount; i++)
        {
            var c = chars[i];
            if (!fontMap.TryGetValue(c, out var grid)) continue;

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    if (grid[row][col] == '1')
                    {
                        float cellX = x + col * cellWidth;
                        float cellY = y + row * cellHeight;
                        var cellRect = new Rect(
                            new Vector2(cellX, cellY),
                            new Size(cellWidth, cellHeight),
                            AnchorPoint.TopLeft
                        );
                        cellRect.Draw(cellColor);
                    }
                }
            }
            x += charWidth + spacing * charWidth;
        }
    }
    
    /// <summary>
    /// Draws a string of text into the specified rectangle using a custom cell drawing action.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="drawCell">Action to invoke for each filled cell, providing the cell rectangle, character, row, and column.</param>
    /// <param name="spacing">Spacing between characters, relative to character width.</param>
    /// <remarks>
    /// Each character is drawn in sequence, spaced according to the spacing parameter. Only cells marked as '1' are drawn.
    /// </remarks>
    public void Draw(string text, Rect rect, Action<Rect, char, int, int> drawCell, float spacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;
        
        float charWidth = rect.Size.Width / (charCount + (charCount - 1) * spacing);
        float charHeight = rect.Size.Height;
        float cellWidth = charWidth / gridWidth;
        float cellHeight = charHeight / gridHeight;

        float x = rect.TopLeft.X;
        float y = rect.TopLeft.Y;

        for (int i = 0; i < charCount; i++)
        {
            var c = chars[i];
            if (!fontMap.TryGetValue(c, out var grid)) continue;

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    if (grid[row][col] == '1')
                    {
                        float cellX = x + col * cellWidth;
                        float cellY = y + row * cellHeight;
                        var cellRect = new Rect(
                            new Vector2(cellX, cellY),
                            new Size(cellWidth, cellHeight),
                            AnchorPoint.TopLeft
                        );
                        drawCell(cellRect, c, row, col);
                    }
                }
            }
            x += charWidth + spacing * charWidth;
        }
    }
    
    /// <summary>
    /// Draws a string of text into the specified rectangle with uniform scaling and alignment, using the provided color.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="cellColor">The color to use for each filled cell.</param>
    /// <param name="alignment">Anchor point for text alignment within the rectangle.</param>
    /// <param name="spacing">Spacing between characters, relative to character width.</param>
    /// <remarks>
    /// Uniform scaling ensures all characters are the same size and aligned according to the specified anchor point.
    /// </remarks>
    public void DrawUniform(string text, Rect rect, ColorRgba cellColor, AnchorPoint alignment, float spacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;

        // Uniform scaling using gridWidth/gridHeight ratio
        float totalGridWidth = charCount * gridWidth + (charCount - 1) * spacing * gridWidth;
        float scaleX = rect.Size.Width / totalGridWidth;
        float scaleY = rect.Size.Height / gridHeight;
        float scale = Math.Min(scaleX, scaleY);

        float charWidth = scale * gridWidth;
        float charHeight = scale * gridHeight;
        float cellWidth = charWidth / gridWidth;
        float cellHeight = charHeight / gridHeight;

        float textWidth = charCount * charWidth + (charCount - 1) * spacing * charWidth;
        float textHeight = charHeight;

        // Use AnchorPoint alignment for offset
        float offsetX = (rect.Size.Width - textWidth) * alignment.X;
        float offsetY = (rect.Size.Height - textHeight) * alignment.Y;
        float x = rect.TopLeft.X + offsetX;
        float y = rect.TopLeft.Y + offsetY;

        for (int i = 0; i < charCount; i++)
        {
            var c = chars[i];
            if (!fontMap.TryGetValue(c, out var grid)) continue;

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    if (grid[row][col] == '1')
                    {
                        float cellX = x + col * cellWidth;
                        float cellY = y + row * cellHeight;
                        var cellRect = new Rect(
                            new Vector2(cellX, cellY),
                            new Size(cellWidth, cellHeight),
                            AnchorPoint.TopLeft
                        );
                        cellRect.Draw(cellColor);
                    }
                }
            }
            x += charWidth + spacing * charWidth;
        }
    }
    
    /// <summary>
    /// Draws a string of text into the specified rectangle with uniform scaling and alignment, using a custom cell drawing action.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="drawCell">Action to invoke for each filled cell, providing the cell rectangle, character, character index, and row.</param>
    /// <param name="alignment">Anchor point for text alignment within the rectangle.</param>
    /// <param name="spacing">Spacing between characters, relative to character width.</param>
    /// <remarks>
    /// Uniform scaling ensures all characters are the same size and aligned according to the specified anchor point.
    /// </remarks>
    public void DrawUniform(string text, Rect rect, Action<Rect, char, int, int> drawCell, AnchorPoint alignment, float spacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;

        // Uniform scaling using gridWidth/gridHeight ratio
        float totalGridWidth = charCount * gridWidth + (charCount - 1) * spacing * gridWidth;
        float scaleX = rect.Size.Width / totalGridWidth;
        float scaleY = rect.Size.Height / gridHeight;
        float scale = Math.Min(scaleX, scaleY);

        float charWidth = scale * gridWidth;
        float charHeight = scale * gridHeight;
        float cellWidth = charWidth / gridWidth;
        float cellHeight = charHeight / gridHeight;

        float textWidth = charCount * charWidth + (charCount - 1) * spacing * charWidth;
        float textHeight = charHeight;

        // Use AnchorPoint alignment for offset
        float offsetX = (rect.Size.Width - textWidth) * alignment.X;
        float offsetY = (rect.Size.Height - textHeight) * alignment.Y;
        float x = rect.TopLeft.X + offsetX;
        float y = rect.TopLeft.Y + offsetY;

        for (int i = 0; i < charCount; i++)
        {
            var c = chars[i];
            if (!fontMap.TryGetValue(c, out var grid)) continue;

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    if (grid[row][col] == '1')
                    {
                        float cellX = x + col * cellWidth;
                        float cellY = y + row * cellHeight;
                        var cellRect = new Rect(
                            new Vector2(cellX, cellY),
                            new Size(cellWidth, cellHeight),
                            AnchorPoint.TopLeft
                        );
                        drawCell(cellRect, c, i, row);
                    }
                }
            }
            x += charWidth + spacing * charWidth;
        }
    }
    #endregion

    #region Draw With Wrap

    /// <summary>
    /// Draws a string of text into the specified rectangle, wrapping text to multiple lines as needed, using the provided color.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="cellColor">The color to use for each filled cell.</param>
    /// <param name="charSpacing">Spacing between characters, relative to character width.</param>
    /// <param name="lineSpacing">Spacing between lines, relative to character height.</param>
    /// <remarks>
    /// Automatically determines the optimal number of characters per line to maximize area usage. Only cells marked as '1' are drawn.
    /// </remarks>
    public void DrawWithWrap(string text, Rect rect, ColorRgba cellColor, float charSpacing = 0.05f, float lineSpacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;

        // Try all possible charsPerLine to maximize area usage
        int bestCharsPerLine = 1;
        float bestCharHeight = 0f;
        float bestCharWidth = 0f;
        int bestLines = charCount;
        for (int charsPerLine = 1; charsPerLine <= charCount; charsPerLine++)
        {
            int lines = (int)Math.Ceiling((float)charCount / charsPerLine);
            float totalSpacingX = (charsPerLine - 1) * charSpacing;
            float totalSpacingY = (lines - 1) * lineSpacing;
            float charWidth = rect.Size.Width / (charsPerLine + totalSpacingX);
            float charHeight = rect.Size.Height / (lines + totalSpacingY);
            // Maintain aspect ratio
            if (charHeight > 0 && charWidth > 0 && charHeight < charWidth * gridHeight / gridWidth)
            {
                charWidth = charHeight * gridWidth / gridHeight;
            }
            else
            {
                charHeight = charWidth * gridHeight / gridWidth;
            }
            if (charHeight > bestCharHeight)
            {
                bestCharHeight = charHeight;
                bestCharWidth = charWidth;
                bestCharsPerLine = charsPerLine;
                bestLines = lines;
            }
        }

        float cellWidth = bestCharWidth / gridWidth;
        float cellHeight = bestCharHeight / gridHeight;

        float totalTextHeight = bestLines * bestCharHeight + (bestLines - 1) * lineSpacing * bestCharHeight;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalTextHeight) * 0.5f;

        int charIndex = 0;
        for (int line = 0; line < bestLines; line++)
        {
            int charsThisLine = Math.Min(bestCharsPerLine, charCount - charIndex);
            float totalLineWidth = charsThisLine * bestCharWidth + (charsThisLine - 1) * charSpacing * bestCharWidth;
            float offsetX = rect.TopLeft.X + (rect.Size.Width - totalLineWidth) * 0.5f;
            float y = offsetY + line * (bestCharHeight + lineSpacing * bestCharHeight);
            float x = offsetX;
            for (int i = 0; i < charsThisLine; i++)
            {
                var c = chars[charIndex++];
                if (!fontMap.TryGetValue(c, out var grid)) continue;
                for (int row = 0; row < gridHeight; row++)
                {
                    for (int col = 0; col < gridWidth; col++)
                    {
                        if (grid[row][col] == '1')
                        {
                            float cellX = x + col * cellWidth;
                            float cellY = y + row * cellHeight;
                            var cellRect = new Rect(
                                new Vector2(cellX, cellY),
                                new Size(cellWidth, cellHeight),
                                AnchorPoint.TopLeft
                            );
                            cellRect.Draw(cellColor);
                        }
                    }
                }
                x += bestCharWidth + charSpacing * bestCharWidth;
            }
        }
    }

    /// <summary>
    /// Draws a string of text into the specified rectangle, wrapping text to multiple lines as needed, using a custom cell drawing action.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area in which to draw the text.</param>
    /// <param name="drawCell">Action to invoke for each filled cell, providing the cell rectangle, character, row, and column.</param>
    /// <param name="charSpacing">Spacing between characters, relative to character width.</param>
    /// <param name="lineSpacing">Spacing between lines, relative to character height.</param>
    /// <remarks>
    /// Automatically determines the optimal number of characters per line to maximize area usage. Only cells marked as '1' are drawn.
    /// </remarks>
    public void DrawWithWrap(string text, Rect rect, Action<Rect, char, int, int> drawCell, float charSpacing = 0.05f, float lineSpacing = 0.05f)
    {
        if(rect.Width <= 1 || rect.Height <= 1) return;
        if (string.IsNullOrEmpty(text)) return;
        var chars = text.ToCharArray();
        int charCount = chars.Length;
        if (charCount == 0) return;

        // Try all possible charsPerLine to maximize area usage
        int bestCharsPerLine = 1;
        float bestCharHeight = 0f;
        float bestCharWidth = 0f;
        int bestLines = charCount;
        for (int charsPerLine = 1; charsPerLine <= charCount; charsPerLine++)
        {
            int lines = (int)Math.Ceiling((float)charCount / charsPerLine);
            float totalSpacingX = (charsPerLine - 1) * charSpacing;
            float totalSpacingY = (lines - 1) * lineSpacing;
            float charWidth = rect.Size.Width / (charsPerLine + totalSpacingX);
            float charHeight = rect.Size.Height / (lines + totalSpacingY);
            // Maintain aspect ratio
            if (charHeight > 0 && charWidth > 0 && charHeight < charWidth * gridHeight / gridWidth)
            {
                charWidth = charHeight * gridWidth / gridHeight;
            }
            else
            {
                charHeight = charWidth * gridHeight / gridWidth;
            }
            if (charHeight > bestCharHeight)
            {
                bestCharHeight = charHeight;
                bestCharWidth = charWidth;
                bestCharsPerLine = charsPerLine;
                bestLines = lines;
            }
        }

        float cellWidth = bestCharWidth / gridWidth;
        float cellHeight = bestCharHeight / gridHeight;

        float totalTextHeight = bestLines * bestCharHeight + (bestLines - 1) * lineSpacing * bestCharHeight;
        float offsetY = rect.TopLeft.Y + (rect.Size.Height - totalTextHeight) * 0.5f;

        int charIndex = 0;
        for (int line = 0; line < bestLines; line++)
        {
            int charsThisLine = Math.Min(bestCharsPerLine, charCount - charIndex);
            float totalLineWidth = charsThisLine * bestCharWidth + (charsThisLine - 1) * charSpacing * bestCharWidth;
            float offsetX = rect.TopLeft.X + (rect.Size.Width - totalLineWidth) * 0.5f;
            float y = offsetY + line * (bestCharHeight + lineSpacing * bestCharHeight);
            float x = offsetX;
            for (int i = 0; i < charsThisLine; i++)
            {
                var c = chars[charIndex++];
                if (!fontMap.TryGetValue(c, out var grid)) continue;
                for (int row = 0; row < gridHeight; row++)
                {
                    for (int col = 0; col < gridWidth; col++)
                    {
                        if (grid[row][col] == '1')
                        {
                            float cellX = x + col * cellWidth;
                            float cellY = y + row * cellHeight;
                            var cellRect = new Rect(
                                new Vector2(cellX, cellY),
                                new Size(cellWidth, cellHeight),
                                AnchorPoint.TopLeft
                            );
                            drawCell(cellRect, c, row, col);
                        }
                    }
                }
                x += bestCharWidth + charSpacing * bestCharWidth;
            }
        }
    }
    
    #endregion

}
