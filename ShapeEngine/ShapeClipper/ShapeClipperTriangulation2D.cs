using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.TriangulationDef;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Provides Clipper-backed triangulation helpers for polygons, polygon outlines, and polyline strokes in 2D.
/// </summary>
/// <remarks>
/// These helpers build <see cref="TriMesh"/> or <see cref="Triangulation"/> results from world-space polygon data,
/// reusing shared intermediate buffers together with services exposed by <see cref="ShapeClipper2D"/>.
/// The class is intended for sequential use and is not thread-safe.
/// </remarks>
public static class ShapeClipperTriangulation2D
{
    #region Buffers

    private static readonly Paths64 _tmpOuter = new();
    private static readonly Paths64 _tmpInner = new();
    private static readonly Paths64 _tmpRing = new();
    private static readonly Paths64 _tmpStroke = new();
    private static readonly Paths64 _tmpResultClosed = new();
    private static readonly List<Vector2> polylineBuffer = new();
    private static readonly TriMesh _triMeshBuffer = new();
    
    private static readonly Paths64PooledBuffer paths64ConversionBuffer1 = new(8);
    private static readonly Paths64PooledBuffer paths64ConversionBuffer2 = new(8);
    #endregion
    
    #region Create Outline Triangulation Perimeter

    /// <summary>
    /// Triangulates the outline of a partial polygon perimeter measured by traveled distance into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="perimeter">The perimeter distance to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonOutlineTriangulationPerimeter(IReadOnlyList<Vector2> polygonCCW, float perimeter, int startIndex, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        ShapeClipper2D.ToPolylinePerimeter(polygonCCW, perimeter, startIndex, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polygon perimeter measured by traveled distance into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="perimeter">The perimeter distance to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonOutlineTriangulationPerimeter(IReadOnlyList<Vector2> polygonCCW, float perimeter, int startIndex, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        ShapeClipper2D.ToPolylinePerimeter(polygonCCW, perimeter, startIndex, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polyline measured by traveled distance into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="perimeter">The distance to trace along the polyline.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolylineTriangulationPerimeter(IReadOnlyList<Vector2> polyline, float perimeter, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        ShapeClipper2D.ToPolylinePerimeter(polyline, perimeter, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polyline measured by traveled distance into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="perimeter">The distance to trace along the polyline.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolylineTriangulationPerimeter(IReadOnlyList<Vector2> polyline, float perimeter, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        ShapeClipper2D.ToPolylinePerimeter(polyline, perimeter, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    #endregion
    
    #region Create Outline Triangulation Percentage
    
    /// <summary>
    /// Triangulates the outline of a partial polygon perimeter measured as a fraction of the total perimeter into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="f">The fraction of the total perimeter to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonOutlineTriangulationPercentage(IReadOnlyList<Vector2> polygonCCW, float f, int startIndex, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        ShapeClipper2D.ToPolylinePercentage(polygonCCW, f, startIndex, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polygon perimeter measured as a fraction of the total perimeter into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="f">The fraction of the total perimeter to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open outline section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonOutlineTriangulationPercentage(IReadOnlyList<Vector2> polygonCCW, float f, int startIndex, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        ShapeClipper2D.ToPolylinePercentage(polygonCCW, f, startIndex, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polyline measured as a fraction of the total length into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="f">The fraction of the total polyline length to trace.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolylineTriangulationPercentage(IReadOnlyList<Vector2> polyline, float f, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        ShapeClipper2D.ToPolylinePercentage(polyline, f, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the outline of a partial polyline measured as a fraction of the total length into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="f">The fraction of the total polyline length to trace.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used for the generated open stroke section.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolylineTriangulationPercentage(IReadOnlyList<Vector2> polyline, float f, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        ShapeClipper2D.ToPolylinePercentage(polyline, f, polylineBuffer);
        CreatePolylineTriangulation(polylineBuffer, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }

    #endregion
    
    #region Create Outline Triangulation

    //TODO: Add Docs
    public static void CreatePolygonOutlineTriangulation(Paths64 polygonWithHoles, float thickness, float miterLimit, bool beveled, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count <= 0 || thickness <= 0f) return;

        ShapeClipper2D.OffsetEngine.OffsetPaths(polygonWithHoles, +thickness, miterLimit, beveled, _tmpOuter);
        ShapeClipper2D.OffsetEngine.OffsetPaths(polygonWithHoles, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0) return;
        
        _tmpRing.Clear();
        ShapeClipper2D.ClipEngine.Execute(_tmpOuter, _tmpInner, ShapeClipperClipType.Difference, _tmpRing);
        
        if (_tmpRing.Count == 0) return;
    
        result.TriangulatePaths64ToMesh(_tmpRing, useDelaunay);
    }
    
    //TODO: Add Docs
    public static void CreatePolygonOutlineTriangulation(Paths64 polygonWithHoles, float thickness, float miterLimit, bool beveled, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonOutlineTriangulation(polygonWithHoles, thickness, miterLimit, beveled, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a polygon into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        ShapeClipper2D.OffsetEngine.OffsetPolygon(polygonCCW, +thickness, miterLimit, beveled, _tmpOuter);
        ShapeClipper2D.OffsetEngine.OffsetPolygon(polygonCCW, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0) return;
        
        _tmpRing.Clear();
        ShapeClipper2D.ClipEngine.Execute(_tmpOuter, _tmpInner, ShapeClipperClipType.Difference, _tmpRing);
        
        if (_tmpRing.Count == 0) return;
    
        result.TriangulatePaths64ToMesh(_tmpRing, useDelaunay);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a polygon into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="thickness">The outline thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a polyline into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used at the open ends of the polyline.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || thickness <= 0f) return;

        ShapeClipper2D.OffsetEngine.OffsetPolyline(polyline, thickness, miterLimit, beveled, endType, _tmpStroke);
        if (_tmpStroke.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(_tmpStroke, useDelaunay);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a polyline into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polyline">The polyline vertices.</param>
    /// <param name="thickness">The stroke thickness in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should use beveled corners.</param>
    /// <param name="endType">The cap style used at the open ends of the polyline.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }

    #endregion
    
    #region Create Triangulation
    
    /// <summary>
    /// Triangulates a Clipper polygon with holes into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The closed Clipper paths representing the outer polygon and any holes.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(polygonWithHoles, useDelaunay);
    }
    
    /// <summary>
    /// Triangulates a Clipper polygon with holes into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The closed Clipper paths representing the outer polygon and any holes.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    /// <summary>
    /// Triangulates a polygon with holes expressed as <see cref="Vector2"/> lists into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The outer polygon and any hole polygons.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonTriangulation(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        paths64ConversionBuffer1.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer1.Buffer);
        
        result.TriangulatePaths64ToMesh(paths64ConversionBuffer1.Buffer, useDelaunay);
    }
    
    /// <summary>
    /// Triangulates a polygon with holes expressed as <see cref="Vector2"/> lists into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The outer polygon and any hole polygons.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonTriangulation(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    /// <summary>
    /// Triangulates a single polygon into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination mesh that receives the generated geometry.</param>
    public static void CreatePolygonTriangulation(IReadOnlyList<Vector2> polygonCCW, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        paths64ConversionBuffer1.PrepareBuffer(1);
        polygonCCW.ToPaths64(paths64ConversionBuffer1.Buffer);
        
        result.TriangulatePaths64ToMesh(paths64ConversionBuffer1.Buffer, useDelaunay);
    }
    
    /// <summary>
    /// Triangulates a single polygon into a <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="useDelaunay">Whether Delaunay refinement should be applied during triangulation.</param>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    public static void CreatePolygonTriangulation(IReadOnlyList<Vector2> polygonCCW, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonCCW, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    #endregion
    
    #region Masked Triangulation
    
    //TODO: Add Docs
    public static void CreatePolygonTriangulationMasked(IReadOnlyList<Vector2> polygonCCW, IReadOnlyList<Vector2> polygonMask, bool useDelaunay, TriMesh result)
    {
        result.Clear();

        var buffer1 = paths64ConversionBuffer1.Buffer;
        paths64ConversionBuffer1.PrepareBuffer(1);
        polygonCCW.ToPaths64(buffer1);
        
        var buffer2 = paths64ConversionBuffer2.Buffer;
        paths64ConversionBuffer2.PrepareBuffer(1);
        polygonMask.ToPaths64(buffer2);
        
        _tmpResultClosed.Clear();
        
        ShapeClipper2D.ClipEngine.Execute(buffer1, buffer2, ShapeClipperClipType.Difference, _tmpResultClosed);
        // _tmpResultClosed.RemoveAllHoles(); //only needed for outline version
        
        result.TriangulatePaths64ToMesh(_tmpResultClosed, useDelaunay);
    }
    
    //TODO: Add Docs
    public static void CreatePolygonOutlineTriangulationMasked(IReadOnlyList<Vector2> polygonCCW, IReadOnlyList<Vector2> polygonMask, float thickness, float miterLimit, bool beveled, bool useDelaunay, bool keepHoles, TriMesh result)
    {
        if(thickness <= 0f) return;
        result.Clear();

        var buffer1 = paths64ConversionBuffer1.Buffer;
        paths64ConversionBuffer1.PrepareBuffer(1);
        polygonCCW.ToPaths64(buffer1);
        
        var buffer2 = paths64ConversionBuffer2.Buffer;
        paths64ConversionBuffer2.PrepareBuffer(1);
        polygonMask.ToPaths64(buffer2);
        
        _tmpResultClosed.Clear();
        
        ShapeClipper2D.ClipEngine.Execute(buffer1, buffer2, ShapeClipperClipType.Difference, _tmpResultClosed);
        if(!keepHoles) _tmpResultClosed.RemoveAllHoles();
        
        CreatePolygonOutlineTriangulation(_tmpResultClosed, thickness, miterLimit, beveled, useDelaunay, result);
    }
    #endregion
    
}