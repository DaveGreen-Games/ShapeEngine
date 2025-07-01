using ShapeEngine.Color;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text;

/// <summary>
/// Emphasis drawer that draws a solid block background for emphasized text.
/// </summary>
public class ED_Block : IEmphasisDrawer
{
    /// <summary>
    /// Draws the background as a solid block.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the block.</param>
    public void DrawBackground(Rect rect, ColorRgba colorRgba)
    {
        rect.Draw(colorRgba);
    }

    /// <summary>
    /// Draws the foreground for the emphasis. Not used in this implementation.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the foreground.</param>
    public void DrawForeground(Rect rect, ColorRgba colorRgba)
    {
    }
}