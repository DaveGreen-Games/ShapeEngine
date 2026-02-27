using ShapeEngine.Color;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.TriangulationDef;

//TODO: Add overloads for all supported triangle drawing variants:
// - [] DrawScaled
// - [] DrawLinesScaled
// - [] DrawVertices
// - [Done] Draw
// - [Done] DrawLines
// - [Canceled] DrawLinesPercentage

/// <summary>
/// Collection of extension drawing helpers for <see cref="Triangulation"/>.
/// Contains methods to render filled triangles, outlines (with configurable thickness or style),
/// rounded corners and glow-like multi-pass drawing. All members are static extension methods
/// intended to be called on <see cref="Triangulation"/> instances.
/// </summary>
public static class TriangulationDrawing
{
    #region Draw
    /// <summary>
    /// Draws a collection of triangles filled with the specified color.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="color">The color to fill each triangle with.</param>
    public static void Draw(this Triangulation triangles, ColorRgba color)
    {
        foreach (var t in triangles) t.Draw(color);
    }
    // /// <summary>
    // /// Draws all triangles in the given <see cref="Triangulation"/> using rounded outer corners.
    // /// This is a convenience extension that iterates the collection and draws each triangle
    // /// using the same rounding parameters.
    // /// </summary>
    // /// <param name="triangles">The collection of triangles to draw.</param>
    // /// <param name="color">The fill color applied to each rounded triangle.</param>
    // /// <param name="cornerPoints">
    // /// Number of extra points used to approximate each rounded corner.
    // /// 0 = sharp corners (no rounding). Higher values produce smoother rounded corners. Default: 5.
    // /// </param>
    // /// <param name="cornerStrength">
    // /// Controls the strength (radius) of the corner rounding:
    // /// 0 = maximum roundness, 1 = no roundness (sharp corners).
    // /// Values between 0 and 1 interpolate between fully rounded and sharp corners. Default: 0.5.
    // /// </param>
    // public static void DrawRounded(this Triangulation triangles, ColorRgba color, int cornerPoints = 5, float cornerStrength = 0.5f)
    // {
    //     foreach (var t in triangles) t.DrawRounded(color, cornerPoints, cornerStrength);
    // }
    #endregion
    
    #region Draw Scaled
    //TODO: Implement
    #endregion
    
    #region Draw Lines
    //TODO: xml summary
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, miterLimit, beveled);
    }

    //TODO: xml summary
    public static void DrawLines(this Triangulation triangles, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        foreach (var t in triangles) t.DrawLines(lineInfo, miterLimit, beveled);
    }
    
    #endregion
    
    #region Draw Lines Scaled
    //TODO: Implement
    #endregion
    
    #region Draw Vertices
    //TODO: Implement
    #endregion
    
    #region Draw Glow
    /// <summary>
    /// Draws the triangulation using a glow-like effect by repeatedly drawing the triangles
    /// with interpolated colors between <paramref name="color"/> and <paramref name="endColorRgba"/>.
    /// </summary>
    /// <param name="triangulation">The triangulation to draw (extension method target).</param>
    /// <param name="color">The starting color for the glow (first pass).</param>
    /// <param name="endColorRgba">The ending color for the glow (last pass).</param>
    /// <param name="steps">Number of passes used to interpolate between colors. Must be &gt;= 1; 1 draws a single pass using <paramref name="color"/>.</param>
    public static void DrawGlow(this Triangulation triangulation, ColorRgba color, ColorRgba endColorRgba, int steps)
    {
        if (triangulation.Count < 3 || steps <= 0) return;

        if (steps == 1)
        {
            triangulation.Draw(color);
            return;
        }
    
        for (var s = 0; s < steps; s++)
        {
            float f = s / (float)(steps - 1);
            var currentColor = color.Lerp(endColorRgba, f);
            triangulation.Draw(currentColor);
        }
    }
    #endregion

}