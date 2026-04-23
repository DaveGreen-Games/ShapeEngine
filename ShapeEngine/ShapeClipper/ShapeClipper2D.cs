using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Provides shared Clipper-backed 2D geometry utilities and configuration for path extraction, clipping, offsetting,
/// shape creation, simplification, winding analysis, and hole filtering.
/// </summary>
/// <remarks>
/// This type acts as the shared foundation for the higher-level <see cref="ShapeClipperConversion2D"/>,
/// <see cref="ShapeClipperTriangulation2D"/>, and <see cref="ShapeClipperDrawing2D"/> helpers.
/// It reuses static buffers and engine instances to reduce allocations during repeated geometry operations,
/// so it is intended for sequential use and is not thread-safe.
/// </remarks>
public static class ShapeClipper2D
{
    #region Public Settings
    
    /// <summary>
    /// Gets or sets the number of decimal places preserved when converting world-space coordinates to Clipper integer coordinates.
    /// </summary>
    /// <remarks>
    /// Updating this value also updates the precision used by <see cref="OffsetEngine"/> and affects the coordinate
    /// conversions performed by <see cref="ShapeClipperConversion2D"/>.
    /// </remarks>
    public static int DecimalPlaces
    {
        get => precision.DecimalPlaces;
        set
        {
            precision = new(value);
            OffsetEngine.Scale = precision;
        }
    }

    /// <summary>
    /// Gets the current scale factor used to convert world-space coordinates to Clipper integer coordinates.
    /// </summary>
    public static double Scale => precision.Scale;
   
    /// <summary>
    /// Gets the inverse of <see cref="Scale"/>, used to convert Clipper integer coordinates back to world space.
    /// </summary>
    public static double InvScale => precision.InvScale;
    #endregion
    
    #region Private Settings
    private static DecimalPrecision precision = new(4);
    #endregion
    
    #region Reused Clipper Engines
    
    /// <summary>
    /// Gets the shared offsetting engine used by this class and related ShapeClipper 2D helpers.
    /// </summary>
    public static ShapeClipperOffset OffsetEngine { get; private set; } = new();
 
    /// <summary>
    /// Gets the shared clipping engine used by this class and related ShapeClipper 2D helpers.
    /// </summary>
    public static ShapeClipper64 ClipEngine { get; private set; } = new();
    
    #endregion
    
    #region Buffers

    private static readonly Path64 path64Buffer = new();
    private static readonly Path64 path64Buffer2 = new();
    private static readonly Paths64 paths64Buffer = new();
    private static readonly Paths64PooledBuffer paths64ConversionBuffer = new(32);
   
    #endregion

    #region Perimeter & Percentage
    
    /// <summary>
    /// Extracts an open polyline that follows a portion of a polygon perimeter starting at the specified vertex index.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="perimeterToDraw">The perimeter distance to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="result">The destination list that receives the traced points.</param>
    public static void ToPolylinePerimeter(IReadOnlyList<Vector2> polygonCCW, float perimeterToDraw, int startIndex, List<Vector2> result)
    {
        if (polygonCCW.Count <= 1) return;
        
        result.Clear();
        
        bool ccw = perimeterToDraw > 0;
        float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        float accumulatedPerimeter = 0f;
        int currentIndex = ShapeMath.WrapIndex(polygonCCW.Count, startIndex);

        //create polyline based on perimeter & start index
        while (absPerimeterToDraw > accumulatedPerimeter)
        {
            int nextIndex = ShapeMath.WrapIndex(polygonCCW.Count, currentIndex + (ccw ? 1 : -1));
            var cur = polygonCCW[currentIndex];
            var next = polygonCCW[nextIndex];
            currentIndex = nextIndex;
            result.Add(cur);
            float segmentLength = (next - cur).Length();

            if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
            {
                float remainingPerimeter = absPerimeterToDraw - accumulatedPerimeter;
                float f = segmentLength <= 0f ? 0f : remainingPerimeter / segmentLength;
                var end = cur.Lerp(next, f);
                result.Add(end);
                break;
            }

            accumulatedPerimeter += segmentLength;
        }
    }
    
    /// <summary>
    /// Extracts an open polyline that follows a fraction of a polygon perimeter starting at the specified vertex index.
    /// </summary>
    /// <param name="polygonCCW">The polygon vertices, expected in counterclockwise order.</param>
    /// <param name="f">The fraction of the total perimeter to trace.</param>
    /// <param name="startIndex">The starting polygon vertex index.</param>
    /// <param name="result">The destination list that receives the traced points.</param>
    public static void ToPolylinePercentage(IReadOnlyList<Vector2> polygonCCW, float f, int startIndex, List<Vector2> result)
    {
        if (polygonCCW.Count <= 1) return;

        if (f >= 1f)
        {
            result.Clear();
            foreach (var p in polygonCCW)
            {
                result.Add(p);   
            }
            return;
        }
        
        float totalPerimeter = 0f;
        
        for (var i = 0; i < polygonCCW.Count; i++)
        {
            var start = polygonCCW[i];
            var end = polygonCCW[(i + 1) % polygonCCW.Count];
            float l = (end - start).Length();
            totalPerimeter += l;
        }
        
        ToPolylinePerimeter(polygonCCW, totalPerimeter * f, startIndex, result);
    }
    
    /// <summary>
    /// Extracts an open polyline that follows a portion of another open polyline by traveled distance.
    /// </summary>
    /// <param name="polyline">The source polyline.</param>
    /// <param name="perimeterToDraw">The distance to trace along the polyline.</param>
    /// <param name="result">The destination list that receives the traced points.</param>
    public static void ToPolylinePerimeter(IReadOnlyList<Vector2> polyline, float perimeterToDraw, List<Vector2> result)
    {
        if (polyline.Count <= 1) return;
        
        result.Clear();
        
        bool ccw = perimeterToDraw > 0;
        float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        float accumulatedPerimeter = 0f;
        int currentIndex = ccw ? 0 : polyline.Count - 1;
        
        
        // Create polyline based on perimeter.
        // // Positive walks forward from 0.
        // // Negative walks backward from Count - 1.
        // // No wrapping: polyline stays open.
        while (absPerimeterToDraw > accumulatedPerimeter)
        {
            int nextIndex = currentIndex + (ccw ? 1 : -1);
            if (nextIndex < 0 || nextIndex >= polyline.Count) break; //safety
            
            var cur = polyline[currentIndex];
            var next = polyline[nextIndex];
            result.Add(cur);
            
            float segmentLength = (next - cur).Length();

            if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
            {
                float remainingPerimeter = absPerimeterToDraw - accumulatedPerimeter;
                float f = segmentLength <= 0f ? 0f : remainingPerimeter / segmentLength;
                var end = cur.Lerp(next, f);
                result.Add(end);
                break;
            }

            accumulatedPerimeter += segmentLength;
            currentIndex = nextIndex;

            // Reached the open end of the polyline; add the endpoint and stop.
            if ((ccw && currentIndex == polyline.Count - 1) || (!ccw && currentIndex == 0))
            {
                result.Add(polyline[currentIndex]);
                break;
            }
        }
    }

    /// <summary>
    /// Extracts an open polyline that follows a fraction of another open polyline's total length.
    /// </summary>
    /// <param name="polyline">The source polyline.</param>
    /// <param name="f">The fraction of the total polyline length to trace.</param>
    /// <param name="result">The destination list that receives the traced points.</param>
    public static void ToPolylinePercentage(IReadOnlyList<Vector2> polyline, float f, List<Vector2> result)
    {
        if (polyline.Count <= 1) return;

        if (f >= 1f)
        {
            result.Clear();
            foreach (var p in polyline)
            {
                result.Add(p);
            }
            return;
        }
        
        float totalPerimeter = 0f;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            float l = (end - start).Length();
            totalPerimeter += l;
        }
        ToPolylinePerimeter(polyline, totalPerimeter * f, result);
    }
    
    #endregion
    
    #region Inflate

    /// <summary>
    /// Offsets a polyline into one or more polygons and writes the resulting shapes into <paramref name="result"/>.
    /// </summary>
    /// <param name="polyline">The polyline to inflate.</param>
    /// <param name="result">The destination polygon collection.</param>
    /// <param name="delta">The offset distance in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners.</param>
    /// <param name="endType">The end-cap style to use for the open polyline.</param>
    /// <param name="removeHoles">Whether to remove all polygons that are considered holes (wind clockwise) from the result.</param>
    public static void InflatePolyline(this IReadOnlyList<Vector2> polyline, Polygons result,  float delta, float miterLimit, bool beveled, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool removeHoles = true)
    {
        if (delta < 0f) delta *= -1f;
        OffsetEngine.OffsetPolyline(polyline, delta, miterLimit, beveled, endType, paths64Buffer);
        paths64Buffer.ToPolygons(result, removeHoles);
    }
  
    /// <summary>
    /// Offsets a closed polygon and writes the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="polygon">The polygon to inflate or deflate.</param>
    /// <param name="result">The destination polygon collection.</param>
    /// <param name="delta">The offset distance in world units.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins.</param>
    /// <param name="beveled">Whether non-miter joins should use beveled corners.</param>
    /// <param name="removeHoles">Whether to remove all polygons that are considered holes (wind clockwise) from the result.</param>
    public static void InflatePolygon(this IReadOnlyList<Vector2> polygon, Polygons result, float delta, float miterLimit, bool beveled, bool removeHoles = true)
    {
        OffsetEngine.OffsetPolygon(polygon, delta, miterLimit, beveled, paths64Buffer);
        paths64Buffer.ToPolygons(result, removeHoles);
    }
    
    #endregion
    
    #region Rect Clipping
    
    /// <summary>
    /// Clips closed paths against a rectangle and returns the resulting closed paths.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="poly">The paths to clip.</param>
    /// <returns>The clipped paths.</returns>
    public static Paths64 ClipRect(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClip(rect.ToRect64(), poly);
    }
  
    /// <summary>
    /// Clips line paths against a rectangle and returns the resulting open paths.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="poly">The paths to clip.</param>
    /// <returns>The clipped line paths.</returns>
    public static Paths64 ClipRectLines(this Rect rect, Paths64 poly)
    {
        return Clipper.RectClipLines(rect.ToRect64(), poly);
    }
    
    /// <summary>
    /// Clips a single polygon expressed as <see cref="Vector2"/> vertices against a rectangle.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="poly">The polygon to clip.</param>
    /// <returns>The clipped paths.</returns>
    public static Paths64 ClipRect(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    
    /// <summary>
    /// Clips a polyline expressed as <see cref="Vector2"/> vertices against a rectangle.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="poly">The polyline to clip.</param>
    /// <returns>The clipped line paths.</returns>
    public static Paths64 ClipRectLines(this Rect rect, List<Vector2> poly)
    {
        paths64ConversionBuffer.PrepareBuffer(poly.Count);
        poly.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    
    /// <summary>
    /// Clips a polygon with holes expressed as nested <see cref="Vector2"/> lists against a rectangle.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="polyWithHoles">The polygon and hole polygons to clip.</param>
    /// <returns>The clipped paths.</returns>
    public static Paths64 ClipRect(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClip(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    
    /// <summary>
    /// Clips line paths expressed as nested <see cref="Vector2"/> lists against a rectangle.
    /// </summary>
    /// <param name="rect">The clipping rectangle.</param>
    /// <param name="polyWithHoles">The line paths to clip.</param>
    /// <returns>The clipped line paths.</returns>
    public static Paths64 ClipRectLines(this Rect rect, List<List<Vector2>> polyWithHoles)
    {
        paths64ConversionBuffer.PrepareBuffer(polyWithHoles.Count);
        polyWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RectClipLines(rect.ToRect64(), paths64ConversionBuffer.Buffer);
    }
    
    #endregion
    
    #region Create Shapes
    
    /// <summary>
    /// Creates an elliptical path in Clipper coordinates.
    /// </summary>
    /// <param name="center">The ellipse center in world space.</param>
    /// <param name="radiusX">The horizontal radius.</param>
    /// <param name="radiusY">The vertical radius. If zero, Clipper's default behavior is used.</param>
    /// <param name="steps">The number of segments used to approximate the ellipse. A value of zero lets Clipper choose automatically.</param>
    /// <returns>The generated ellipse path.</returns>
    public static Path64 CreateEllipse(Vector2 center, double radiusX, double radiusY = 0f, int steps = 0)
    {
        return Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
    }
  
    /// <summary>
    /// Creates an ellipse and writes it as a single-path <see cref="Paths64"/> collection.
    /// </summary>
    /// <param name="center">The ellipse center in world space.</param>
    /// <param name="radiusX">The horizontal radius.</param>
    /// <param name="radiusY">The vertical radius.</param>
    /// <param name="steps">The number of segments used to approximate the ellipse.</param>
    /// <param name="result">The destination path collection.</param>
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, Paths64 result)
    {
        result.Clear();
        result.Add(Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps));
    }
    
    /// <summary>
    /// Creates an ellipse and writes its vertices as a flat <see cref="Vector2"/> list.
    /// </summary>
    /// <param name="center">The ellipse center in world space.</param>
    /// <param name="radiusX">The horizontal radius.</param>
    /// <param name="radiusY">The vertical radius.</param>
    /// <param name="steps">The number of segments used to approximate the ellipse.</param>
    /// <param name="result">The destination vertex list.</param>
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<Vector2> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2List(result);
    }
    
    /// <summary>
    /// Creates an ellipse and writes it as a single polygon inside a nested <see cref="Vector2"/> list.
    /// </summary>
    /// <param name="center">The ellipse center in world space.</param>
    /// <param name="radiusX">The horizontal radius.</param>
    /// <param name="radiusY">The vertical radius.</param>
    /// <param name="steps">The number of segments used to approximate the ellipse.</param>
    /// <param name="result">The destination nested vertex list.</param>
    public static void CreateEllipse(Vector2 center, double radiusX, double radiusY, int steps, List<List<Vector2>> result)
    {
        var ellipse = Clipper.Ellipse(center.ToPoint64(), radiusX, radiusY, steps);
        ellipse.ToVector2Lists(result);
    }
    #endregion
    
    #region Trim Collinear 
    /// <summary>
    /// Removes consecutive collinear vertices from a Clipper path.
    /// </summary>
    /// <param name="polygon">The path to trim.</param>
    /// <param name="isOpen">Whether the path should be treated as open instead of closed.</param>
    /// <returns>The trimmed path.</returns>
    public static Path64 TrimCollinear(this Path64 polygon, bool isOpen = false)
    {
        return Clipper.TrimCollinear(polygon, isOpen);
    }
   
    /// <summary>
    /// Removes consecutive collinear vertices from a polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to trim.</param>
    /// <param name="isOpen">Whether the path should be treated as open instead of closed.</param>
    /// <returns>The trimmed path in Clipper coordinates.</returns>
    public static Path64 TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.TrimCollinear(path64Buffer, isOpen);
    }
    
    /// <summary>
    /// Removes consecutive collinear vertices from a polygon and writes the result to <paramref name="result"/>.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to trim.</param>
    /// <param name="isOpen">Whether the path should be treated as open instead of closed.</param>
    /// <param name="result">The destination vertex list.</param>
    public static void TrimCollinear(this IReadOnlyList<Vector2> polygon, bool isOpen, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
        trimmedPath.ToVector2List(result);
    }
    
    /// <summary>
    /// Removes consecutive collinear vertices from each polygon in a collection and writes the results to <paramref name="result"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to trim.</param>
    /// <param name="isOpen">Whether each path should be treated as open instead of closed.</param>
    /// <param name="result">The destination Clipper path collection.</param>
    public static void TrimCollinear(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isOpen, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.TrimCollinear(path64Buffer, isOpen);
            result.Add(trimmedPath);
        }
    }
    
    /// <summary>
    /// Removes consecutive collinear vertices from each polygon in a collection and writes the results as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to trim.</param>
    /// <param name="isOpen">Whether each path should be treated as open instead of closed.</param>
    /// <param name="result">The destination nested vertex list.</param>
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
    
    /// <summary>
    /// Removes duplicate consecutive vertices from a Clipper path.
    /// </summary>
    /// <param name="polygon">The path to process.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <returns>The path with duplicates removed.</returns>
    public static Path64 StripDuplicates(this Path64 polygon, bool isClosedPath = false)
    {
        return Clipper.StripDuplicates(polygon, isClosedPath);
    }
  
    /// <summary>
    /// Removes duplicate consecutive vertices from a polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to process.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <returns>The cleaned path in Clipper coordinates.</returns>
    public static Path64 StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.StripDuplicates(path64Buffer, isClosedPath);
    }
    
    /// <summary>
    /// Removes duplicate consecutive vertices from a polygon and writes the result to <paramref name="result"/>.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to process.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <param name="result">The destination vertex list.</param>
    public static void StripDuplicates(this IReadOnlyList<Vector2> polygon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
        trimmedPath.ToVector2List(result);
    }
    
    /// <summary>
    /// Removes duplicate consecutive vertices from each polygon in a collection and writes the results to <paramref name="result"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to process.</param>
    /// <param name="isClosedPath">Whether each path should be treated as closed.</param>
    /// <param name="result">The destination Clipper path collection.</param>
    public static void StripDuplicates(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, bool isClosedPath, Paths64 result)
    {
        foreach (var p in polygonWithHoles)
        {
            p.ToPath64(path64Buffer);
            var trimmedPath = Clipper.StripDuplicates(path64Buffer, isClosedPath);
            result.Add(trimmedPath);
        }
    }
    
    /// <summary>
    /// Removes duplicate consecutive vertices from each polygon in a collection and writes the results as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to process.</param>
    /// <param name="isClosedPath">Whether each path should be treated as closed.</param>
    /// <param name="result">The destination nested vertex list.</param>
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
    
    /// <summary>
    /// Tests whether a point lies inside, outside, or on the boundary of a Clipper path.
    /// </summary>
    /// <param name="polygon">The polygon to test.</param>
    /// <param name="p">The point to test in world coordinates.</param>
    /// <returns>The point-in-polygon classification result.</returns>
    public static PointInPolygonResult PointInPolygon(this Path64 polygon, Vector2 p)
    {
        return Clipper.PointInPolygon(p.ToPoint64(), polygon);
    }
   
    /// <summary>
    /// Tests whether a point lies inside, outside, or on the boundary of a polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The polygon to test.</param>
    /// <param name="p">The point to test.</param>
    /// <returns>The point-in-polygon classification result.</returns>
    public static PointInPolygonResult PointInPolygon(this IReadOnlyList<Vector2> polygon, Vector2 p)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.PointInPolygon(p.ToPoint64(), path64Buffer);
    }
    
    /// <summary>
    /// Tests whether a point lies inside a polygon-with-holes representation.
    /// </summary>
    /// <param name="polygonWithHoles">The outer polygon followed by any hole polygons.</param>
    /// <param name="p">The point to test.</param>
    /// <returns>
    /// <see cref="PointInPolygonResult.IsInside"/> when the point is inside the outer polygon and outside all holes;
    /// otherwise the corresponding outside or boundary result.
    /// </returns>
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
    
    /// <summary>
    /// Computes the Minkowski difference of a Clipper polygon and pattern.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern path.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <returns>The generated Minkowski difference paths.</returns>
    public static Paths64 MinkowskiDiff(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Diff(pattern, polygon, isClosed);
    }
   
    /// <summary>
    /// Computes the Minkowski difference of a polygon and pattern expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern polygon.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <returns>The generated Minkowski difference paths.</returns>
    public static Paths64 MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
    }
    
    /// <summary>
    /// Computes the Minkowski difference of a polygon and pattern and writes the result as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern polygon.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <param name="result">The destination nested vertex list.</param>
    public static void MinkowskiDiff(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Diff(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    
    /// <summary>
    /// Computes the Minkowski sum of a Clipper polygon and pattern.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern path.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <returns>The generated Minkowski sum paths.</returns>
    public static Paths64 MinkowskiSum(this Path64 polygon, Path64 pattern, bool isClosed = false)
    {
        return Minkowski.Sum(pattern, polygon, isClosed);
    }
    
    /// <summary>
    /// Computes the Minkowski sum of a polygon and pattern expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern polygon.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <returns>The generated Minkowski sum paths.</returns>
    public static Paths64 MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed = false)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        return Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
    }
    
    /// <summary>
    /// Computes the Minkowski sum of a polygon and pattern and writes the result as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygon">The source polygon.</param>
    /// <param name="pattern">The pattern polygon.</param>
    /// <param name="isClosed">Whether the polygon should be treated as closed.</param>
    /// <param name="result">The destination nested vertex list.</param>
    public static void MinkowskiSum(this IReadOnlyList<Vector2> polygon, IReadOnlyList<Vector2> pattern, bool isClosed, List<List<Vector2>> result)
    {
        polygon.ToPath64(path64Buffer);
        pattern.ToPath64(path64Buffer2);
        
        var diff = Minkowski.Sum(path64Buffer2, path64Buffer, isClosed);
        diff.ToVector2Lists(result);
    }
    #endregion
    
    #region Simplify
    
    /// <summary>
    /// Simplifies a Clipper path using Clipper's path simplification algorithm.
    /// </summary>
    /// <param name="polygon">The path to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <returns>The simplified path.</returns>
    public static Path64 Simplify(this Path64 polygon, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPath(polygon, epsilon, isClosedPath);
    }
  
    /// <summary>
    /// Simplifies a collection of Clipper paths using Clipper's path simplification algorithm.
    /// </summary>
    /// <param name="polygonWithHoles">The paths to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the paths should be treated as closed.</param>
    /// <returns>The simplified paths.</returns>
    public static Paths64 Simplify(this Paths64 polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        return Clipper.SimplifyPaths(polygonWithHoles, epsilon, isClosedPath);
    }

    /// <summary>
    /// Simplifies a polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <returns>The simplified path in Clipper coordinates.</returns>
    public static Path64 Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath = false)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
    }
    
    /// <summary>
    /// Simplifies a polygon-with-holes representation expressed as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the paths should be treated as closed.</param>
    /// <returns>The simplified paths.</returns>
    public static Paths64 Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath = false)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
    }
    
    /// <summary>
    /// Simplifies a polygon and writes the result as a flat <see cref="Vector2"/> list.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the path should be treated as closed.</param>
    /// <param name="result">The destination vertex list.</param>
    public static void Simplify(this IReadOnlyList<Vector2> polygon, float epsilon, bool isClosedPath, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.SimplifyPath(path64Buffer, epsilon, isClosedPath);
        solution.ToVector2List(result);
    }
    
    /// <summary>
    /// Simplifies a polygon-with-holes representation and writes the result as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="isClosedPath">Whether the paths should be treated as closed.</param>
    /// <param name="result">The destination nested vertex list.</param>
    public static void Simplify(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, bool isClosedPath, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.SimplifyPaths(paths64ConversionBuffer.Buffer, epsilon, isClosedPath);
        solution.ToVector2Lists(result);
    }
    
    #endregion

    #region Simplify Ramer-Douglas-Peucker

    /// <summary>
    /// Simplifies a Clipper path using the Ramer-Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="polygon">The path to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <returns>The simplified path.</returns>
    public static Path64 SimplifyRamerDouglasPeucker(this Path64 polygon, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygon, epsilon);
    }
    /// <summary>
    /// Simplifies a collection of Clipper paths using the Ramer-Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="polygonWithHoles">The paths to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <returns>The simplified paths.</returns>
    public static Paths64 SimplifyRamerDouglasPeucker(this Paths64 polygonWithHoles, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(polygonWithHoles, epsilon);
    }

    /// <summary>
    /// Simplifies a polygon expressed as <see cref="Vector2"/> vertices using the Ramer-Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <returns>The simplified path in Clipper coordinates.</returns>
    public static Path64 SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
    }
    /// <summary>
    /// Simplifies a polygon-with-holes representation expressed as nested <see cref="Vector2"/> lists using the Ramer-Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <returns>The simplified paths.</returns>
    public static Paths64 SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        return Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
    }
    
    /// <summary>
    /// Simplifies a polygon using the Ramer-Douglas-Peucker algorithm and writes the result as a flat <see cref="Vector2"/> list.
    /// </summary>
    /// <param name="polygon">The polygon or polyline to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="result">The destination vertex list.</param>
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<Vector2> polygon, float epsilon, List<Vector2> result)
    {
        polygon.ToPath64(path64Buffer);
        var solution = Clipper.RamerDouglasPeucker(path64Buffer, epsilon);
        solution.ToVector2List(result);
    }
    /// <summary>
    /// Simplifies a polygon-with-holes representation using the Ramer-Douglas-Peucker algorithm and writes the result as nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to simplify.</param>
    /// <param name="epsilon">The simplification tolerance.</param>
    /// <param name="result">The destination nested vertex list.</param>
    public static void SimplifyRamerDouglasPeucker(this IReadOnlyList<IReadOnlyList<Vector2>> polygonWithHoles, float epsilon, List<List<Vector2>> result)
    {
        paths64ConversionBuffer.PrepareBuffer(polygonWithHoles.Count);
        polygonWithHoles.ToPaths64(paths64ConversionBuffer.Buffer);
        var solution =  Clipper.RamerDouglasPeucker(paths64ConversionBuffer.Buffer, epsilon);
        solution.ToVector2Lists(result);
    }

    #endregion

    #region Winding Order & Area
    /// <summary>
    /// Calculates the signed area of a Clipper path.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>The signed area of the path.</returns>
    public static double GetArea(Path64 path)
    {
        return Clipper.Area(path);
    }

    /// <summary>
    /// Determines whether a Clipper path has positive winding.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns><c>true</c> if the path has positive winding; otherwise, <c>false</c>.</returns>
    public static bool IsPositive(Path64 path)
    {
        return Clipper.IsPositive(path);
    }

    /// <summary>
    /// Determines whether a Clipper path is wound clockwise.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns><c>true</c> if the path is clockwise; otherwise, <c>false</c>.</returns>
    public static bool IsClockwise(Path64 path)
    {
        return !Clipper.IsPositive(path);
    }

    /// <summary>
    /// Determines whether a Clipper path is wound counterclockwise.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns><c>true</c> if the path is counterclockwise; otherwise, <c>false</c>.</returns>
    public static bool IsCounterClockwise(Path64 path)
    {
        return Clipper.IsPositive(path);
    }
    
    /// <summary>
    /// Calculates the signed area of a polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="polygon">The polygon to evaluate.</param>
    /// <returns>The signed area of the polygon.</returns>
    public static double GetArea(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.Area(path64Buffer);
    }

    /// <summary>
    /// Determines whether a polygon expressed as <see cref="Vector2"/> vertices has positive winding.
    /// </summary>
    /// <param name="polygon">The polygon to evaluate.</param>
    /// <returns><c>true</c> if the polygon has positive winding; otherwise, <c>false</c>.</returns>
    public static bool IsPositive(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.IsPositive(path64Buffer);
    }

    /// <summary>
    /// Determines whether a polygon expressed as <see cref="Vector2"/> vertices is wound clockwise.
    /// </summary>
    /// <param name="polygon">The polygon to evaluate.</param>
    /// <returns><c>true</c> if the polygon is clockwise; otherwise, <c>false</c>.</returns>
    public static bool IsClockwise(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return !Clipper.IsPositive(path64Buffer);
    }

    /// <summary>
    /// Determines whether a polygon expressed as <see cref="Vector2"/> vertices is wound counterclockwise.
    /// </summary>
    /// <param name="polygon">The polygon to evaluate.</param>
    /// <returns><c>true</c> if the polygon is counterclockwise; otherwise, <c>false</c>.</returns>
    public static bool IsCounterClockwise(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return Clipper.IsPositive(path64Buffer);
    }
    #endregion
    
    #region Holes

    /// <summary>
    /// Determines whether a Clipper path represents a hole based on its winding direction.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns><c>true</c> if the path is considered a hole; otherwise, <c>false</c>.</returns>
    public static bool IsHole(this Path64 path)
    {
        return !Clipper.IsPositive(path);
    }

    /// <summary>
    /// Determines whether a polygon expressed as <see cref="Vector2"/> vertices represents a hole based on its winding direction.
    /// </summary>
    /// <param name="polygon">The polygon to evaluate.</param>
    /// <returns><c>true</c> if the polygon is considered a hole; otherwise, <c>false</c>.</returns>
    public static bool IsHole(this IReadOnlyList<Vector2> polygon)
    {
        polygon.ToPath64(path64Buffer);
        return path64Buffer.IsHole();
    }

    /// <summary>
    /// Removes all hole paths from the specified <see cref="Paths64"/> collection in place.
    /// </summary>
    /// <param name="paths">The paths to filter.</param>
    /// <returns>The number of removed hole paths.</returns>
    public static int RemoveAllHoles(this Paths64 paths)
    {
        return paths.RemoveAll((p) => p.IsHole()); 
    }

    /// <summary>
    /// Removes all hole polygons from the specified nested <see cref="Vector2"/> list in place.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to filter.</param>
    /// <returns>The number of removed hole polygons.</returns>
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
   
    /// <summary>
    /// Removes all hole polygons from the specified <see cref="Polygons"/> collection in place.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to filter.</param>
    /// <returns>The number of removed hole polygons.</returns>
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
    
    /// <summary>
    /// Copies only non-hole paths from <paramref name="paths"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="paths">The source paths.</param>
    /// <param name="result">The destination collection for non-hole paths.</param>
    /// <returns>The number of skipped hole paths.</returns>
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

    /// <summary>
    /// Copies only non-hole polygons from <paramref name="polygonWithHoles"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The source polygons.</param>
    /// <param name="result">The destination collection for non-hole polygons.</param>
    /// <returns>The number of skipped hole polygons.</returns>
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
    
    /// <summary>
    /// Removes all non-hole paths from the specified <see cref="Paths64"/> collection in place, leaving only holes.
    /// </summary>
    /// <param name="paths">The paths to filter.</param>
    /// <returns>The number of removed non-hole paths.</returns>
    public static int GetAllHoles(this Paths64 paths)
    {
        return paths.RemoveAll((p) => !p.IsHole());
    }

    /// <summary>
    /// Removes all non-hole polygons from the specified nested <see cref="Vector2"/> list in place, leaving only holes.
    /// </summary>
    /// <param name="polygonWithHoles">The polygons to filter.</param>
    /// <returns>The number of removed non-hole polygons.</returns>
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
    
    /// <summary>
    /// Copies only hole paths from <paramref name="paths"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="paths">The source paths.</param>
    /// <param name="result">The destination collection for hole paths.</param>
    /// <returns>The number of skipped non-hole paths.</returns>
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

    /// <summary>
    /// Copies only hole polygons from <paramref name="polygonWithHoles"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="polygonWithHoles">The source polygons.</param>
    /// <param name="result">The destination collection for hole polygons.</param>
    /// <returns>The number of skipped non-hole polygons.</returns>
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
    
}