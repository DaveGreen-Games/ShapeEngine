using ShapeEngine.Color;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Text;

/// <summary>
/// Defines the interface for drawing emphasis effects (background and foreground) on text.
/// </summary>
public interface IEmphasisDrawer
{
    /// <summary>
    /// Draws the background for the emphasis effect.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the background.</param>
    public void DrawBackground(Rect rect, ColorRgba colorRgba);
    /// <summary>
    /// Draws the foreground for the emphasis effect.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the foreground.</param>
    public void DrawForeground(Rect rect, ColorRgba colorRgba);
}