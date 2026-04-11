using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangleDef;

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
    
    /// <summary>
    /// Draws the given <see cref="Segments"/> using two alternating <see cref="LineDrawingInfo"/> instances.
    /// The first segment uses <paramref name="striped"/>, the second uses <paramref name="alternatingStriped"/>,
    /// and the pattern repeats for the remaining segments.
    /// </summary>
    /// <param name="striped">Line drawing parameters applied to even-indexed segments (0-based).</param>
    /// <param name="alternatingStriped">Line drawing parameters applied to odd-indexed segments.</param>
    public void DrawAlternating(LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        int i = 0;
        foreach (var segment in this)
        {
            var info = i % 2 == 0 ? striped : alternatingStriped;
            segment.Draw(info);
            i++;
        }
    }
    
    /// <summary>
    /// Draws the given <see cref="Segments"/> using a repeating sequence of <see cref="LineDrawingInfo"/>
    /// instances supplied in <paramref name="alternatingInfo"/>. The segment at index <c>i</c> uses
    /// <c>alternatingInfo[i % alternatingInfo.Length]</c>.
    /// </summary>
    /// <param name="alternatingInfo">One or more <see cref="LineDrawingInfo"/> instances used in round-robin order.
    /// Must contain at least one element.</param>
    public void DrawAlternating(params LineDrawingInfo[] alternatingInfo)
    {
        if (alternatingInfo.Length == 0) return;
        
        var i = 0;
        foreach (var segment in this)
        {
            var infoIndex = i % alternatingInfo.Length;
            var info = alternatingInfo[infoIndex];
            segment.Draw(info);
            i++;
        }
    }
    
    
    //TODO: Change to Segments result parameter!
    
    /// <summary>
    /// Generates striped segments for a supported outer shape and optionally excludes the area covered by a supported inner shape.
    /// Dispatch is based on the runtime type of <paramref name="outsideShape"/>.
    /// </summary>
    /// <typeparam name="TO">
    /// The outer shape type. Supported runtime types are <see cref="Circle"/>, <see cref="Triangle"/>,
    /// <see cref="Rect"/>, <see cref="Quad"/>, and <see cref="Polygon"/>.
    /// </typeparam>
    /// <typeparam name="TI">
    /// The inner excluded shape type. Supported runtime types are <see cref="Circle"/>, <see cref="Triangle"/>,
    /// <see cref="Rect"/>, <see cref="Quad"/>, and <see cref="Polygon"/>.
    /// </typeparam>
    /// <param name="outsideShape">The outer shape to fill with stripe segments.</param>
    /// <param name="insideShape">The inner shape to exclude from the generated stripes.</param>
    /// <param name="spacing">The distance between adjacent stripe lines.</param>
    /// <param name="angleDeg">The stripe angle in degrees, measured from the x\-axis.</param>
    /// <param name="spacingOffset">An optional offset applied along the stripe spacing direction.</param>
    /// <returns>
    /// A <see cref="Segments"/> collection containing the generated stripe segments, or an empty collection if
    /// <paramref name="outsideShape"/> is not a supported runtime type.
    /// </returns>
    public static Segments GenerateStripedSegments<TO, TI>(TO outsideShape, TI insideShape, float spacing, float angleDeg, float spacingOffset = 0f) 
        where TO : IClosedShapeTypeProvider
        where TI : IClosedShapeTypeProvider
    {
        if(outsideShape is Circle circle) return circle.GenerateStripedSegments(insideShape, spacing, angleDeg, spacingOffset);
        if(outsideShape is Triangle triangle) return triangle.GenerateStripedSegments(insideShape, spacing, angleDeg, spacingOffset);
        if(outsideShape is Rect rect) return rect.GenerateStripedSegments(insideShape, spacing, angleDeg, spacingOffset);
        if(outsideShape is Quad quad) return quad.GenerateStripedSegments(insideShape, spacing, angleDeg, spacingOffset);
        if(outsideShape is Polygon polygon) return polygon.GenerateStripedSegments(insideShape, spacing, angleDeg, spacingOffset);

        return [];
    }
}


