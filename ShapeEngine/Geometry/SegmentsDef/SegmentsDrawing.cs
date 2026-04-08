using ShapeEngine.Color;

namespace ShapeEngine.Geometry.SegmentsDef;

/// <summary>
/// Provides drawing methods for <see cref="Segments"/> collections.
/// </summary>
public partial class Segments
{
    /// <summary>
    /// Draws every segment in the collection using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="lineInfo">The drawing information applied to each segment.</param>
    public void Draw(LineDrawingInfo lineInfo)
    {
        if (Count <= 0) return;

        foreach (var seg in this)
        {
            seg.Draw(lineInfo);
        }
    }

    /// <summary>
    /// Draws a glow effect for every segment in the collection.
    /// </summary>
    /// <param name="width">The starting width of the glow.</param>
    /// <param name="endWidth">The ending width of the glow.</param>
    /// <param name="color">The starting color of the glow.</param>
    /// <param name="endColorRgba">The ending color of the glow.</param>
    /// <param name="steps">The number of glow interpolation steps.</param>
    /// <param name="capType">The line cap style to use.</param>
    /// <param name="capPoints">The number of points used to draw the cap.</param>
    public void DrawGlow(float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (Count <= 0) return;

        foreach (var seg in this)
        {
            seg.DrawGlow(width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }
}


