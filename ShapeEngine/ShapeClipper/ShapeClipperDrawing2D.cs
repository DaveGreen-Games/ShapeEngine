using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.ShapeClipper;
/// <summary>
/// Provides immediate-style drawing helpers for polygons and polylines backed by ShapeClipper triangulation.
/// </summary>
/// <remarks>
/// These helpers generate temporary triangulated meshes through <see cref="ShapeClipperTriangulation2D"/>
/// and draw them immediately using a shared reusable <see cref="TriMesh"/> buffer.
/// The class is intended for sequential use and is not thread-safe.
/// </remarks>
public static class ShapeClipperDrawing2D
{
    #region Buffer
    
    private static readonly TriMesh triMeshBuffer = new();
    
    #endregion
    
    #region Draw Polygon

    /// <summary>
    /// Draws a polygon with holes by triangulating it and rendering the generated mesh.
    /// </summary>
    /// <param name="polygonWithHoles">The outer polygon and any hole polygons.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolygon(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, ColorRgba color, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolygonTriangulation(polygonWithHoles, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }

    /// <summary>
    /// Draws a single polygon by triangulating it and rendering the generated mesh.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolygon(IReadOnlyList<Vector2> polygonCCW, ColorRgba color, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolygonTriangulation(polygonCCW, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    #endregion
    
    #region Draw Outline
    
    /// <summary>
    /// Draws the stroked outline of a polygon by first triangulating the outline and then rendering the generated mesh.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolygonOutline(IReadOnlyList<Vector2> polygonCCW, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        ShapeClipperTriangulation2D.CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }

    /// <summary>
    /// Draws a stroked polyline by triangulating its outline and rendering the generated mesh.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used at the open ends of the polyline.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolyline(IReadOnlyList<Vector2> polyline, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool useDelaunay = false)
    {
        if (polyline.Count < 2 || thickness <= 0f) return;

        ShapeClipperTriangulation2D.CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    #endregion
    
    #region Draw Perimeter
    
    /// <summary>
    /// Draws the triangulated outline of a partial polygon perimeter measured by traveled distance.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="perimeter">The perimeter distance to trace before triangulating the outline section.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolygonOutlinePerimeter(IReadOnlyList<Vector2> polygonCCW, float perimeter, int startIndex, float thickness, ColorRgba color, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolygonOutlineTriangulationPerimeter(polygonCCW, perimeter, startIndex, thickness, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }

    /// <summary>
    /// Draws the triangulated outline of a partial polyline measured by traveled distance.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="perimeter">The distance to trace before triangulating the stroke section.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolylinePerimeter(IReadOnlyList<Vector2> polyline, float perimeter, float thickness, ColorRgba color, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolylineTriangulationPerimeter(polyline, perimeter, thickness, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    #endregion
    
    #region Draw Percentage
    
    /// <summary>
    /// Draws the triangulated outline of a partial polygon perimeter measured as a fraction of the total perimeter.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="f">The fraction of the total perimeter to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolygonOutlinePercentage(IReadOnlyList<Vector2> polygonCCW, float f, int startIndex, float thickness, ColorRgba color, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolygonOutlineTriangulationPercentage(polygonCCW, f, startIndex, thickness, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    /// <summary>
    /// Draws the triangulated outline of a partial polyline measured as a fraction of the total length.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="f">The fraction of the total polyline length to trace.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    public static void DrawPolylinePercentage(IReadOnlyList<Vector2> polyline, float f, float thickness, ColorRgba color, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolylineTriangulationPercentage(polyline, f, thickness, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }

    #endregion
    
    #region Draw Glow
    
    /// <summary>
    /// Draws a glowing polygon outline by rendering multiple outline passes with
    /// thickness and color interpolated across the supplied ranges.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="thicknessRange">The outline thickness range used from the first pass to the final pass.</param>
    /// <param name="colorRange">The color range used from the first pass to the final pass.</param>
    /// <param name="steps">The number of glow passes to render. A value of 1 draws a single pass using the maximum thickness and color.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <remarks>
    /// The first pass is triangulated at <see cref="ValueRange.Min"/> and subsequent passes
    /// scale the generated mesh about its centroid while interpolating colors.
    /// </remarks>
    public static void DrawPolygonOutlineGlow(IReadOnlyList<Vector2> polygonCCW, ValueRange thicknessRange, ValueRangeColor colorRange, int steps, 
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (polygonCCW.Count < 3 || steps <= 0) return;
        
        if (steps == 1)
        {
            DrawPolygonOutline(polygonCCW, thicknessRange.Max, colorRange.Max, miterLimit, beveled, useDelaunay);
            return;
        }

        if (thicknessRange.Min <= 0) return;
        if (!thicknessRange.HasPositiveRange()) return;
        if (!colorRange.HasRange()) return;

        var currentWidth = thicknessRange.Min;
        ShapeClipperTriangulation2D.CreatePolygonOutlineTriangulation(polygonCCW, currentWidth, miterLimit, beveled, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(colorRange.Min);
        var center = triMeshBuffer.GetCentroid();
        for (var s = 1; s < steps; s++)
        {
            var f = s / (float)(steps - 1);
            var nextWidth = thicknessRange.Lerp(f);
            var scale = nextWidth / currentWidth;
            var currentColor = colorRange.Lerp(f);
            triMeshBuffer.Scale(scale, center);
            triMeshBuffer.Draw(currentColor);
            currentWidth = nextWidth;
        }
    }

    /// <summary>
    /// Draws a glowing polyline by rendering multiple stroke passes with thickness
    /// and color interpolated across the supplied ranges.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="thicknessRange">The stroke thickness range used from the first pass to the final pass.</param>
    /// <param name="colorRange">The color range used from the first pass to the final pass.</param>
    /// <param name="steps">The number of glow passes to render. A value of 1 draws a single pass using the maximum thickness and color.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used at the open ends of the polyline.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <remarks>
    /// The first pass is triangulated at <see cref="ValueRange.Min"/> and subsequent passes
    /// scale the generated mesh about its centroid while interpolating colors.
    /// </remarks>
    public static void DrawPolylineGlow(IReadOnlyList<Vector2> polyline, ValueRange thicknessRange, ValueRangeColor colorRange, int steps, 
        float miterLimit = 2f, bool beveled = false, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool useDelaunay = false)
    {
        if (polyline.Count < 2 || steps <= 0) return;
        
        if (steps == 1)
        {
            DrawPolyline(polyline, thicknessRange.Max, colorRange.Max, miterLimit, beveled, endType, useDelaunay);
            return;
        }
        
        if (thicknessRange.Min <= 0) return;
        if (!thicknessRange.HasPositiveRange()) return;
        if (!colorRange.HasRange()) return;
        
        var currentWidth = thicknessRange.Min;
        ShapeClipperTriangulation2D.CreatePolylineTriangulation(polyline, currentWidth, miterLimit, beveled, endType, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(colorRange.Min);
        var center = triMeshBuffer.GetCentroid();
        for (var s = 1; s < steps; s++)
        {
            var f = s / (float)(steps - 1);
            var nextWidth = thicknessRange.Lerp(f);
            var scale = nextWidth / currentWidth;
            var currentColor = colorRange.Lerp(f);
            triMeshBuffer.Scale(scale, center);
            triMeshBuffer.Draw(currentColor);
            currentWidth = nextWidth;
        }
    }
    
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws a polygon after subtracting a mask polygon from it, triangulating the remaining filled area and rendering the generated mesh.
    /// </summary>
    /// <param name="polygonCCW">The source polygon vertices, expected in counterclockwise order.</param>
    /// <param name="polygonMask">The polygon vertices to subtract from the source polygon.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <remarks>
    /// This helper delegates to <see cref="ShapeClipperTriangulation2D.CreatePolygonTriangulationMasked(IReadOnlyList{Vector2}, IReadOnlyList{Vector2}, bool, TriMesh)"/>
    /// and immediately renders the shared temporary mesh buffer.
    /// </remarks>
    public static void DrawPolygonTriangulationMasked(IReadOnlyList<Vector2> polygonCCW, IReadOnlyList<Vector2> polygonMask, ColorRgba color, bool useDelaunay)
    {
        ShapeClipperTriangulation2D.CreatePolygonTriangulationMasked(polygonCCW, polygonMask, useDelaunay, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    /// <summary>
    /// Draws the outline of a polygon after subtracting a mask polygon from it, triangulating the remaining outline area and rendering the generated mesh.
    /// </summary>
    /// <param name="polygonCCW">The source polygon vertices, expected in counterclockwise order.</param>
    /// <param name="polygonMask">The polygon vertices to subtract from the source polygon.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated mesh.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <remarks>
    /// This overload subtracts <paramref name="polygonMask"/> from <paramref name="polygonCCW"/>, discards holes in the clipped result,
    /// triangulates the resulting outline through <see cref="ShapeClipperTriangulation2D"/>, and draws the shared temporary mesh buffer.
    /// </remarks>
    public static void DrawPolygonOutlineTriangulationMasked(IReadOnlyList<Vector2> polygonCCW, IReadOnlyList<Vector2> polygonMask, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        ShapeClipperTriangulation2D.CreatePolygonOutlineTriangulationMasked(polygonCCW, polygonMask, thickness, miterLimit, beveled, useDelaunay, false, triMeshBuffer);
        triMeshBuffer.Draw(color);
    }
    
    
    #endregion
}