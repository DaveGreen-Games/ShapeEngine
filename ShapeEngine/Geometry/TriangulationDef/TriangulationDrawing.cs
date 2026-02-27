using ShapeEngine.Color;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.TriangulationDef;

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
  
    #endregion
    
    #region Draw Scaled
    
    /// <summary>
    /// Draws each triangle in the triangulation with scaled sides based on a specific draw type.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="color">The color of the drawn shapes.</param>
    /// <param name="sideScaleFactor">The scale factor of the sides (0 to 1). If >= 1, the full triangle is drawn. If &lt;= 0, nothing is drawn.</param>
    /// <param name="sideScaleOrigin">The origin point for scaling the sides (0 = start, 1 = end, 0.5 = center).</param>
    /// <param name="drawType">
    /// The style of drawing applied to each triangle:
    /// <list type="bullet">
    /// <item><description>0: [Filled] Drawn as 4 filled triangles, effectivly cutting of corners.</description></item>
    /// <item><description>1: [Sides] Each side is connected to the triangle's centroid.</description></item>
    /// <item><description>2: [Sides Inverse] The start of 1 side is connected to the end of the next side and is connected to the triangle's centroid.</description></item>
    /// </list>
    /// </param>
    public static void DrawScaled(this Triangulation triangles, ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        foreach (var t in triangles) t.DrawScaled(color, sideScaleFactor, sideScaleOrigin, drawType);
    }
    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the outline of each triangle in the triangulation with specified line thickness and color.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, miterLimit, beveled);
    }

    /// <summary>
    /// Draws the outline of each triangle in the triangulation using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineInfo">Contains line style information such as thickness and color.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLines(this Triangulation triangles, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        foreach (var t in triangles) t.DrawLines(lineInfo, miterLimit, beveled);
    }
    
    #endregion
    
    #region Draw Lines Scaled
    
    /// <summary>
    /// Draws the outlines of each triangle in the triangulation where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No Side, 1 = Full Side).</c></param>
    /// <param name="sideScaleOrigin">The point along each side to scale from in both directions <c>(0 = Start, 1 = End)</c>.</param>
    /// <remarks>
    /// Allows for dynamic scaling of triangle sides, useful for effects or partial outlines.
    /// </remarks>
    public static void DrawLinesScaled(this Triangulation triangles, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        foreach (var t in triangles) t.DrawLinesScaled(lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    
    #endregion
    
    #region Draw Vertices
    
    /// <summary>
    /// Draws circles at each vertex of every triangle in the triangulation.
    /// </summary>
    /// <param name="triangles">The collection of triangles whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDef.CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// </param>
    public static void DrawVertices(this Triangulation triangles, float vertexRadius, ColorRgba color, float smoothness)
    {
        foreach (var t in triangles) t.DrawVertices(vertexRadius, color, smoothness);
    } 
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