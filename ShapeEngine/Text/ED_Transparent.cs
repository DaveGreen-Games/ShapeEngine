using ShapeEngine.Color;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Text;

/// <summary>
/// Emphasis drawer that does not render any background or foreground, resulting in transparent emphasis.
/// </summary>
public class ED_Transparent : IEmphasisDrawer
{
    /// <summary>
    /// Draws the background for the emphasis. Not used in this implementation.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the background.</param>
    public void DrawBackground(Rect rect, ColorRgba colorRgba)
    {
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