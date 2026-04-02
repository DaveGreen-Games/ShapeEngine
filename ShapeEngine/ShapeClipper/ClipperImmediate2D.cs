using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangulationDef;

namespace ShapeEngine.ShapeClipper;

//TODO: Rename
public static class ClipperImmediate2D
{
    #region Public Settings
    
    public static int DecimalPlaces
    {
        get => precision.DecimalPlaces;
        set
        {
            precision = new(value);
            OffsetEngine.Scale = precision;
        }
    }

    public static double Scale => precision.Scale;
    public static double InvScale => precision.InvScale;
    #endregion
    
    #region Private Settings
    private static DecimalPrecision precision = new(4);
    #endregion
    
    #region Reused Clipper Engines
    public static ShapeClipperOffset OffsetEngine { get; private set; } = new();
    public static ShapeClipper64 ClipEngine { get; private set; } = new();
    #endregion
    
    #region Buffers

    private static readonly Path64 path64Buffer = new();
    private static readonly Path64 path64Buffer2 = new();
    private static readonly Paths64 paths64Buffer = new();
    private static readonly Paths64 _tmpOuter = new();
    private static readonly Paths64 _tmpInner = new();
    private static readonly Paths64 _tmpRing = new();
    private static readonly Paths64 _tmpStroke = new();
    private static readonly TriMesh _triMeshBuffer = new();
    private static readonly Paths64PooledBuffer paths64ConversionBuffer = new();
    
    #endregion

    #region TriMesh Pool
    private static readonly TriMeshPool meshPool = new(32);
    #endregion
    
    #region Drawing
    
    public static void DrawPolygonOutline(IReadOnlyList<Vector2> polygonCCW, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        var mesh = meshPool.RentMesh();
        try
        {
            CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, mesh);
            mesh.Draw(color);
        }
        finally { meshPool.ReturnMesh(mesh); }
    }

    public static void DrawPolyline(IReadOnlyList<Vector2> polyline, float thickness, ColorRgba color, float miterLimit = 2f, bool beveled = false, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool useDelaunay = false)
    {
        if (polyline.Count < 2 || thickness <= 0f) return;

        var mesh = meshPool.RentMesh();
        try
        {
            CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, mesh);
            mesh.Draw(color);
        }
        finally { meshPool.ReturnMesh(mesh); }
    }

    
    //TODO: Add DrawPolygon function (draws filled polygon)
    
    //TODO: Add DrawPolygonWithHoles (Polygons)
    
    //TODO: Add DrawPolygonOutlinePercentage/Perimeter (Polygon.ToPolylinePercentage/Perimeter) -> use LineCapType parameter and ToShapeClipperEndType conversion
    
    //TODO: Add DrawPolylinePercentage/Perimeter (Polyline.ToPolylinePercentage/Perimeter) -> use LineCapType parameter and ToShapeClipperEndType conversion
    
    
    #endregion
    
    #region Create Outline Triangulation
    //TODO: Add CreatePolygonOutlineTriangulation for Polygons polygonWithHoles!
    
    //TODO: Add CreatePolygonOutlineTriangulation for perimeter/percentage
    
    //TODO: Add CreatePolylineTriangulation for perimeter/percentage
    
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonCCW.Count < 3 || thickness <= 0f) return;

        OffsetEngine.OffsetPolygon(polygonCCW, +thickness, miterLimit, beveled, _tmpOuter);
        OffsetEngine.OffsetPolygon(polygonCCW, -thickness, miterLimit, beveled, _tmpInner);
        if (_tmpOuter.Count == 0) return;
        
        _tmpRing.Clear();
        ClipEngine.Execute(_tmpOuter, _tmpInner, ShapeClipperClipType.Difference, _tmpRing);
        
        if (_tmpRing.Count == 0) return;
    
        result.TriangulatePaths64ToMesh(_tmpRing, useDelaunay);
    }
    
    public static void CreatePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }

    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polyline.Count < 2 || thickness <= 0f) return;

        OffsetEngine.OffsetPolyline(polyline, thickness, miterLimit, beveled, endType, _tmpStroke);
        if (_tmpStroke.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(_tmpStroke, useDelaunay);
    }
    
    public static void CreatePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }

    #endregion
    
    #region Triangulation
    
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        
        result.TriangulatePaths64ToMesh(polygonWithHoles, useDelaunay);
    }
    
    public static void CreatePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    public static void CreatePolygonTriangulation(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        if (polygonWithHoles.Count == 0) return;
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        
        result.TriangulatePaths64ToMesh(paths64ConversionBuffer.Buffer, useDelaunay);
    }
    
    public static void CreatePolygonTriangulation(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    public static void CreatePolygonTriangulation(IReadOnlyList<Vector2> polygon, bool useDelaunay, TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        result.Clear();

        paths64ConversionBuffer.PrepareBuffer(1);
        polygon.ToPaths64(paths64ConversionBuffer.Buffer);
        
        result.TriangulatePaths64ToMesh(paths64ConversionBuffer.Buffer, useDelaunay);
    }
    
    public static void CreatePolygonTriangulation(IReadOnlyList<Vector2> polygon, bool useDelaunay, Triangulation result)
    {
        _triMeshBuffer.Clear();
        CreatePolygonTriangulation(polygon, useDelaunay, _triMeshBuffer);
        _triMeshBuffer.ToTriangulation(result);
    }
    
    #endregion

    #region Inflate

    public static void InflatePolyline(this IReadOnlyList<Vector2> polyline, Polygons result,  float delta, float miterLimit, bool beveled, ShapeClipperEndType endType = ShapeClipperEndType.Butt)
    {
        if (delta < 0f) delta *= -1f;
        OffsetEngine.OffsetPolyline(polyline, delta, miterLimit, beveled, endType, paths64Buffer);
        paths64Buffer.ToPolygons(result, true);
    }
    public static void InflatePolygon(this IReadOnlyList<Vector2> polygon, Polygons result, float delta, float miterLimit, bool beveled)
    {
        OffsetEngine.OffsetPolygon(polygon, delta, miterLimit, beveled, paths64Buffer);
        paths64Buffer.ToPolygons(result, true);
    }
    #endregion
    
    #region Rect Clipping
    public static Paths64 ClipRect(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClip(rect.ToRect64(), poly);
    }
    public static Paths64 ClipRectLines(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClipLines(rect.ToRect64(), poly);
    }
    public static Paths64 ClipRect(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRectLines(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRect(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    public static Paths64 ClipRectLines(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    #endregion
    
    #region Create Shapes
    
    public static Path64 CreateEllipse(Vector2 center, double radiusX, double radiusY = 0f, int steps = 0)
    {
        return Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, Paths64 result)
    {
        result.Clear();
        result.Add(Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps));
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<Vector2> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2List(result);
    }
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<List<Vector2>> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2Lists(result);
    }
    #endregion
    
    #region Trim Collinear 
    public static Path64 TrimCollinear(this Path64 polygon, bool isOpen = false)
    {
        return Clipper.TrimCollinear(polygon, isOpen);
    }
    public static Path64 TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.TrimCollinear(path64Buffer, isOpen);
    }
    public static void TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
        trimmedPath.ToVector2List(result);
    }
    public static void TrimCollinear(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isOpen, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
            result.Add(trimmedPath);
        }
    }
    public static void TrimCollinear(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isOpen, List<List<Vector2>> result)
    {
        paths64Buffer.Clear();
        for (int i = 0; i < polygonWithHoles.Count; i++)
        {
            polygonWithHoles[i].ToPath64(path64Buffer);
            var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
            paths64Buffer.Add(trimmedPath);
        }
        paths64Buffer.ToVector2Lists(result);
    }
    #endregion
    
    #region Strip Duplicates
    public static Path64 StripDuplicates(this Path64 polygon, bool isClosedPath = false)
    {
        return Clipper.StripDuplicates(polygon, isClosedPath);
    }
    public static Path64 StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.StripDuplicates(path64Buffer, isClosedPath);
    }
    public static void StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
        trimmedPath.ToVector2List(result);
    }
    public static void StripDuplicates(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isClosedPath, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
            result.Add(trimmedPath);
        }
    }
    public static void StripDuplicates(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isClosedPath, List<List<Vector2>> result)
    {
        paths64Buffer.Clear();
        for (int i = 0; i < polygonWithHoles.Count; i++)
        {
            polygonWithHoles[i].ToPath64(path64Buffer);
            var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
            paths64Buffer.Add(trimmedPath);
        }
        paths64Buffer.ToVector2Lists(result);
    }
    #endregion

    #region Point In Polygon
    
    public static PointInPolygonResult PointInPolygon(this Path64 polygon, Vector2 p)
    {
        return Clipper.PointInPolygon(p.ToPoint64(), polygon);
    }
    public static PointInPolygonResult PointInPolygon(this IReadOnlyList<Vector2> polygon, Vector2 p)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.PointInPolygon(p.ToPoint64(), path64Buffer);
    }
    public static PointInPolygonResult PointInPolygons(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, Vector2 p)
    {
        if (polygonWithHoles.Count <= 0) return PointInPolygonResult.IsOutside;
        if(polygonWithHoles.Count == 1) return PointInPolygon(polygonWithHoles[0], p);
        var result = PointInPolygon(polygonWithHoles[0], p);
        
        //point is outside of the main polygon
        if (result == PointInPolygonResult.IsOutside) return result;

        var point = p.ToPoint64();
        for (int i = 1; i < polygonWithHoles.Count; i++)
        {
            var hole = polygonWithHoles[i];
            hole.ToPath64(path64Buffer);
            var holeResult = Clipper.PointInPolygon(point, path64Buffer);
            
            //point is inside the polygon but also inside a hole -> therefore it is considered outside
            if (holeResult == PointInPolygonResult.IsInside) return PointInPolygonResult.IsOutside;
        }

        //point is inside polygon but not inside any hole
        return PointInPolygonResult.IsInside;
    }
    #endregion
    
    #region Minkowski
    public static Paths64 MinkowskiDiff(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Diff(pattern, polygon, isClosed);
    }
    public static Paths64 MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
    }
    public static void MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    public static Paths64 MinkowskiSum(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Sum(pattern, polygon, isClosed);
    }
    public static Paths64 MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
    }
    public static void MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    #endregion
    
    #region Simplify
    public static Path64 Simplify(this Path64 polygon, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPath(polygon, epsilon, isClosedPath);
    }
    public static Paths64 Simplify(this Paths64 polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPaths(polygonWithHoles, epsilon, isClosedPath);
    }

    public static Path64 Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
    }
    public static Paths64 Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
    }
    
    public static void Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
        solution.ToVector2List(result);
    }
    public static void Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
        solution.ToVector2Lists(result);
    }
    #endregion

    #region Simplify Ramer-Douglas-Peucker

    public static Path64 SimplifyRamerDouglasPeucker(this Path64 polygon, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygon, epsilon);
    }
    public static Paths64 SimplifyRamerDouglasPeucker(this Paths64 polygonWithHoles, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygonWithHoles, epsilon);
    }

    public static Path64 SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
    }
    public static Paths64 SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
    }
    
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
        solution.ToVector2List(result);
    }
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
        solution.ToVector2Lists(result);
    }

    #endregion

    #region Winding Order & Area
    public static double GetArea(Path64 path)
    {
        return Clipper.Area(path);
    }

    public static bool IsPositive(Path64 path)
    {
        return Clipper.IsPositive(path);
    }

    public static bool IsClockwise(Path64 path)
    {
        return !Clipper.IsPositive(path);
    }

    public static bool IsCounterClockwise(Path64 path)
    {
        return Clipper.IsPositive(path);
    }
    
    public static double GetArea(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.Area(path64Buffer);
    }

    public static bool IsPositive(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.IsPositive(path64Buffer);
    }

    public static bool IsClockwise(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return !Clipper.IsPositive(path64Buffer);
    }

    public static bool IsCounterClockwise(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.IsPositive(path64Buffer);
    }
    #endregion
    
    #region Holes

    public static bool IsHole(this Path64 path)
    {
        return !Clipper.IsPositive(path);
    }

    public static bool IsHole(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return path64Buffer.IsHole();
    }

    public static int RemoveAllHoles(this Paths64 paths)
    {
        return paths.RemoveAll((p) => p.IsHole()); 
    }

    public static int RemoveAllHoles(this List<List<Vector2>> polygonWithHoles)
    {
        int count = 0;
        for (int i = polygonWithHoles.Count - 1; i >= 0; i--)
        {
            var p = polygonWithHoles[i];
            if (p.IsHole())
            {
                polygonWithHoles.RemoveAt(i);
                count++;
            }
        }
        return count;
    }
    public static int RemoveAllHoles(this Polygons polygonWithHoles)
    {
        int count = 0;
        for (int i = polygonWithHoles.Count - 1; i >= 0; i--)
        {
            var p = polygonWithHoles[i];
            if (p.IsHole())
            {
                polygonWithHoles.RemoveAt(i);
                count++;
            }
        }
        return count;
    }
    public static int RemoveAllHoles(this Paths64 paths, Paths64 result)
    {
        int count = 0;
        result.Clear();
        for (int i = paths.Count - 1; i >= 0; i--)
        {
            var p = paths[i];
            if (p.IsHole())
            {
                count++;
            }
            else
            {
                result.Add(p);
            }
        }
        return count;
    }

    public static int RemoveAllHoles(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, List<IReadOnlyList<Vector2>> result)
    {
        int count = 0;
        result.Clear();
        for (int i = polygonWithHoles.Count - 1; i >= 0; i--)
        {
            var p = polygonWithHoles[i];
            if (p.IsHole())
            {
                count++;
            }
            else
            {
                result.Add(p);
            }
        }
        return count;
    }
    
    public static int GetAllHoles(this Paths64 paths)
    {
        return paths.RemoveAll((p) => !p.IsHole());
    }

    public static int GetAllHoles(this List<List<Vector2>> polygonWithHoles)
    {
        int count = 0;
        for (int i = polygonWithHoles.Count - 1; i >= 0; i--)
        {
            var p = polygonWithHoles[i];
            if (!p.IsHole())
            {
                polygonWithHoles.RemoveAt(i);
                count++;
            }
        }
        return count;
    }
    
    public static int GetAllHoles(this Paths64 paths, Paths64 result)
    {
        int count = 0;
        result.Clear();
        for (int i = paths.Count - 1; i >= 0; i--)
        {
            var p = paths[i];
            if (!p.IsHole())
            {
                count++;
            }
            else
            {
                result.Add(p);
            }
        }
        return count;
    }

    public static int GetAllHoles(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, List<IReadOnlyList<Vector2>> result)
    {
        int count = 0;
        result.Clear();
        for (int i = polygonWithHoles.Count - 1; i >= 0; i--)
        {
            var p = polygonWithHoles[i];
            if (!p.IsHole())
            {
                count++;
            }
            else
            {
                result.Add(p);
            }
        }
        return count;
    }


    #endregion
    
    #region Conversion
    //Single to Single
    public static void ToPath64(this IReadOnlyList<Vector2> src, Path64 dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);

        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToPoint64());
        }
    }
    public static void ToVector2List(this Path64 src, List<Vector2> dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);
        
        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToVec2());
        }
    }
    //Multi to Multi
    public static void ToPaths64(this IReadOnlyList<IReadOnlyList<Vector2>> src, Paths64 dst) 
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new Path64();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToPath64(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToPath64(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    public static void ToVector2Lists(this Paths64 src, List<List<Vector2>> dst)
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new List<Vector2>();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    public static void ToPolygons(this Paths64 src, Polygons dst)
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new Polygon();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    public static void ToPolygons(this Paths64 src, Polygons dst, bool removeHoles)
    {
        for (int i = 0; i < src.Count; i++)
        {
            var srcItem = src[i];
            if(srcItem.Count <= 0) continue;
            if(removeHoles && srcItem.IsHole()) continue;
            if(dst.Count <= i)
            {
                var dstItem = new Polygon();
                dst.Add(dstItem);
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    //Single to Multi
    public static void ToPaths64(this IReadOnlyList<Vector2> src, Paths64 dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new Path64();
            src.ToPath64(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToPath64(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    public static void ToVector2Lists(this Path64 src, List<List<Vector2>> dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new List<Vector2>();
            src.ToVector2List(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToVector2List(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    public static void ToPolygons(this Path64 src, Polygons dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new Polygon();
            src.ToVector2List(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToVector2List(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    public static Rect64 ToRect64(this Rect r)
    {
        // long left   = (long)Math.Round(r.X * Scale);
        // long top    = (long)Math.Round(r.Y * Scale);
        // long right  = (long)Math.Round((r.X + r.Width) * Scale);
        // long bottom = (long)Math.Round((r.Y + r.Height) * Scale);
        // return new Rect64(left, top, right, bottom);
        
        long left   = (long)Math.Round(r.X * Scale);
        long bottom    = (long)Math.Round(-r.Y * Scale);
        long right  = (long)Math.Round((r.X + r.Width) * Scale);
        long top = (long)Math.Round((-r.Y - r.Height) * Scale);
        return new Rect64(left, top, right, bottom);
    }
    public static Rect ToRect(this Rect64 r)
    {
        // float x = (float)(r.left * InvScale);
        // float y = (float)(r.top * InvScale);
        // float w = (float)((r.right - r.left) * InvScale);
        // float h = (float)((r.bottom - r.top) * InvScale);
        //
        // return new Rect(x, y, w, h);
        
        float x = (float)(r.left * InvScale);
        float y = (float)((-r.top - r.Height) * InvScale);
        float w = (float)(r.Width * InvScale);
        float h = (float)(r.Height * InvScale);

        return new Rect(x, y, w, h);
    }
    
    public static Point64 ToPoint64(this Vector2 v)
    {
        long x = (long)Math.Round(v.X * Scale);
        long y = (long)Math.Round(-v.Y * Scale);
        return new Point64(x,y);
    }
    
    public static Vector2 ToVec2(this Point64 p)
    {
        return new Vector2((float)(p.X * InvScale), (float)(-p.Y * InvScale));
    }
    #endregion
    
    #region Enum Conversion
    /// <summary>
    /// Converts a <see cref="ShapeClipperFillRule"/> to the Clipper <see cref="FillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The ShapeClipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="FillRule"/> value.</returns>
    public static FillRule ToClipperFillRule(this ShapeClipperFillRule fillRule)
    {
        return (FillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperJoinType"/> to the Clipper <see cref="JoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The ShapeClipper join type to convert.</param>
    /// <returns>The equivalent <see cref="JoinType"/> value.</returns>
    public static JoinType ToClipperJoinType(this ShapeClipperJoinType joinType)
    {
        return (JoinType)joinType;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperEndType"/> to the Clipper <see cref="EndType"/> enum.
    /// </summary>
    /// <param name="endType">The ShapeClipper end type to convert.</param>
    /// <returns>The equivalent <see cref="EndType"/> value.</returns>
    public static EndType ToClipperEndType(this ShapeClipperEndType endType)
    {
        return (EndType)endType;
    }

    //TODO: Docs
    public static ClipType ToClipperClipType(this ShapeClipperClipType clipType)
    {
        return (ClipType)clipType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="FillRule"/> to the local <see cref="ShapeClipperFillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The Clipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperFillRule"/> value.</returns>
    public static ShapeClipperFillRule ToShapeClipperFillRule(this FillRule fillRule)
    {
        return (ShapeClipperFillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="JoinType"/> to the local <see cref="ShapeClipperJoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The Clipper join type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperJoinType"/> value.</returns>
    public static ShapeClipperJoinType ToShapeClipperJoinType(this JoinType joinType)
    {
        return (ShapeClipperJoinType)joinType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="EndType"/> to the local <see cref="ShapeClipperEndType"/> enum.
    /// </summary>
    /// <param name="endType">The Clipper end type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperEndType"/> value.</returns>
    public static ShapeClipperEndType ToShapeClipperEndType(this EndType endType)
    {
        return (ShapeClipperEndType)endType;
    }
    
    //TODO: Docs
    public static ShapeClipperClipType ToShapeClipperClipType(this ClipType clipType)
    {
        return (ShapeClipperClipType)clipType;
    }

    //TODO: Docs
    public static ShapeClipperEndType ToShapeClipperEndType(this LineCapType capType)
    {
        if (capType is LineCapType.None) return ShapeClipperEndType.Butt;
        if (capType is LineCapType.Extended) return ShapeClipperEndType.Square;
        if (capType is LineCapType.Capped) return ShapeClipperEndType.Round;
        if (capType is LineCapType.CappedExtended) return ShapeClipperEndType.Round;
        else return ShapeClipperEndType.Butt;
    }
    
    //TODO: Docs
    public static LineCapType ToLineCapType(this ShapeClipperEndType endType)
    {
        if (endType is ShapeClipperEndType.Polygon) return LineCapType.None;
        if (endType is ShapeClipperEndType.Joined) return LineCapType.None;
        if (endType is ShapeClipperEndType.Butt) return LineCapType.None;
        if (endType is ShapeClipperEndType.Square) return LineCapType.Extended;
        if (endType is ShapeClipperEndType.Round) return LineCapType.CappedExtended;
        else return LineCapType.None;
    }
    #endregion
    
}


/*
public static class TriMeshCacheOld
{
    /// <summary>Max cached triangulations (simple cap; cache clears when exceeded).</summary>
    public static int MaxTriangulationCacheEntries = 512;
    
    #region Internal Cache
    private static void EnsureCacheSpace()
    {
        if (_idToMesh.Count < MaxTriangulationCacheEntries) return;
        ClearTriangulationCache();
    }

    private readonly struct TriKey : IEquatable<TriKey>
    {
        public readonly ulong Hash;
        public readonly int DP;
        public readonly byte Kind;
        public readonly bool UseDelaunay;

        private TriKey(ulong hash, int dp, byte kind, bool useDelaunay)
        {
            Hash = hash;
            DP = dp;
            Kind = kind;
            UseDelaunay = useDelaunay;
        }

        public static TriKey FromPolygonOutline(IReadOnlyList<Vector2> polygon, float thickness, float miterLimit, bool beveled, bool useDelaunay, int dp)
        {
            ulong h = HashPoints(polygon, dp);
            h = HashFloat(h, thickness, dp);
            h = HashFloat(h, miterLimit, dp);
            h = HashBool(h, beveled);
            return new TriKey(h, dp, kind: 1, useDelaunay);
        }

        public static TriKey FromPolyline(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay, int dp)
        {
            ulong h = HashPoints(polyline, dp);
            h = HashFloat(h, thickness, dp);
            h = HashFloat(h, miterLimit, dp);
            h = HashBool(h, beveled);
            h = HashInt(h, (int)endType);
            return new TriKey(h, dp, kind: 2, useDelaunay);
        }

        public static TriKey FromPaths64(Paths64 paths, bool useDelaunay, int dp)
        {
            ulong h = HashPaths64(paths);
            return new TriKey(h, dp, kind: 3, useDelaunay);
        }

        public bool Equals(TriKey other) =>
            Hash == other.Hash && DP == other.DP && Kind == other.Kind && UseDelaunay == other.UseDelaunay;

        public override bool Equals(object? obj) => obj is TriKey k && Equals(k);

        public override int GetHashCode()
        {
            unchecked
            {
                int hc = (int)(Hash ^ (Hash >> 32));
                hc = (hc * 397) ^ DP;
                hc = (hc * 397) ^ Kind;
                hc = (hc * 397) ^ (UseDelaunay ? 1 : 0);
                return hc;
            }
        }

        private static ulong HashPoints(IReadOnlyList<Vector2> pts, int dp)
        {
            const ulong FNV_OFFSET = 14695981039346656037UL;
            const ulong FNV_PRIME = 1099511628211UL;

            ulong h = FNV_OFFSET;
            unchecked
            {
                h ^= (ulong)pts.Count; h *= FNV_PRIME;

                double scale = ToScale(dp);
                for (int i = 0; i < pts.Count; i++)
                {
                    long qx = (long)Math.Round(pts[i].X * scale);
                    long qy = (long)Math.Round(pts[i].Y * scale);
                    h ^= (ulong)qx; h *= FNV_PRIME;
                    h ^= (ulong)qy; h *= FNV_PRIME;
                }
            }
            return h;
        }

        private static ulong HashPaths64(Paths64 paths)
        {
            const ulong FNV_OFFSET = 14695981039346656037UL;
            const ulong FNV_PRIME = 1099511628211UL;

            ulong h = FNV_OFFSET;
            unchecked
            {
                h ^= (ulong)paths.Count; h *= FNV_PRIME;

                for (int i = 0; i < paths.Count; i++)
                {
                    var path = paths[i];
                    h ^= (ulong)path.Count; h *= FNV_PRIME;

                    for (int j = 0; j < path.Count; j++)
                    {
                        var p = path[j];
                        h ^= (ulong)p.X; h *= FNV_PRIME;
                        h ^= (ulong)p.Y; h *= FNV_PRIME;
                    }
                }
            }
            return h;
        }

        private static ulong HashFloat(ulong h, float v, int dp)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            double scale = ToScale(dp);
            long q = (long)Math.Round(v * scale);
            unchecked { h ^= (ulong)q; h *= FNV_PRIME; }
            return h;
        }

        private static ulong HashInt(ulong h, int v)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            unchecked { h ^= (ulong)v; h *= FNV_PRIME; }
            return h;
        }

        private static ulong HashBool(ulong h, bool v)
        {
            const ulong FNV_PRIME = 1099511628211UL;
            unchecked { h ^= (ulong)(v ? 1 : 0); h *= FNV_PRIME; }
            return h;
        }

        private static double ToScale(int dp)
        {
            if (dp <= 0) return 1.0;
            double s = 1.0;
            for (int i = 0; i < dp; i++) s *= 10.0;
            return s;
        }
    }
    #endregion
    
    #region Cache
    private static int _nextTriId = 1;
    private static readonly Dictionary<TriKey, int> _keyToId = new(256);
    private static readonly Dictionary<int, TriMesh> _idToMesh = new(256);
    private static readonly Stack<TriMesh> _meshPool = new();
    #endregion
    
    #region Cached Triangulation
    
    public static int CachePolygonOutlineTriangulation(IReadOnlyList<Vector2> polygonCCW, float thickness, float miterLimit, bool beveled, bool useDelaunay)
    {
        if (polygonCCW.Count < 3 || thickness <= 0f) return 0;

        var key = TriKey.FromPolygonOutline(polygonCCW, thickness, miterLimit, beveled, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonOutlineTriangulation(polygonCCW, thickness, miterLimit, beveled, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static int CachePolylineTriangulation(IReadOnlyList<Vector2> polyline, float thickness, float miterLimit, bool beveled, ShapeClipperEndType endType, bool useDelaunay)
    {
        if (polyline.Count < 2 || thickness <= 0f) return 0;

        var key = TriKey.FromPolyline(polyline, thickness, miterLimit, beveled, endType, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolylineTriangulation(polyline, thickness, miterLimit, beveled, endType, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static int CachePolygonTriangulation(Paths64 polygonWithHoles, bool useDelaunay)
    {
        if (polygonWithHoles.Count == 0) return 0;

        var key = TriKey.FromPaths64(polygonWithHoles, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonTriangulation(polygonWithHoles, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }

    public static int CachePolygonTriangulation(IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool useDelaunay)
    {
        if (polygonWithHoles.Count == 0) return 0;
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var key = TriKey.FromPaths64(paths64ConversionBuffer.Buffer, useDelaunay, DecimalPlaces);
        if (_keyToId.TryGetValue(key, out int id)) return id;

        EnsureCacheSpace();

        id = _nextTriId++;
        var mesh = RentMesh();
        CreatePolygonTriangulation(paths64ConversionBuffer.Buffer, useDelaunay, mesh);

        _keyToId[key] = id;
        _idToMesh[id] = mesh;
        return id;
    }
    
    public static bool GetCachedTriangulation(int triangulationId, out TriMesh mesh)
    {
        return _idToMesh.TryGetValue(triangulationId, out mesh!);
    }

    public static void ClearTriangulationCache()
    {
        _keyToId.Clear();

        foreach (var kv in _idToMesh)
        {
            kv.Value.Clear();
            _meshPool.Push(kv.Value);
        }
        _idToMesh.Clear();
    }
    
    #endregion
    
    #region Internal TriMesh Pooling
    private static TriMesh RentMesh()
    {
        if (_meshPool.Count > 0)
        {
            var m = _meshPool.Pop();
            m.Clear();
            return m;
        }
        return new TriMesh();
    }

    private static void ReturnMesh(TriMesh mesh)
    {
        if (_meshPool.Count < MaxTriangulationCacheEntries)
        {
            mesh.Clear();
            _meshPool.Push(mesh);
        }
    }
    #endregion
    
    public static void DrawCachedTriangulation(int triangulationId, ColorRgba color)
    {
        if (triangulationId == 0) return;
        if (!_idToMesh.TryGetValue(triangulationId, out var mesh)) return;
        mesh.Draw(color);
    }
}
*/
