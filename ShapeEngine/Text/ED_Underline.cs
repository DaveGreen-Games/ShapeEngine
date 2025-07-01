using ShapeEngine.Color;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Text;

/// <summary>
/// Emphasis drawer that draws an underline beneath the emphasized text.
/// </summary>
public class ED_Underline : IEmphasisDrawer
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
    /// Draws the underline as the emphasis foreground.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    /// <param name="colorRgba">The color to use for the underline.</param>
    public void DrawForeground(Rect rect, ColorRgba colorRgba)
    {
        float lineThickness = rect.Size.Min() * 0.1f;
        Segment s = new(rect.BottomLeft, rect.BottomRight);
        s.Draw(lineThickness, colorRgba, LineCapType.Extended);
    }
}