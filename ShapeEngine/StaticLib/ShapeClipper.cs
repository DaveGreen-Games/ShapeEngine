using System.Numerics;
using ShapeEngine.Core.Shapes;
using Clipper2Lib;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Represents a collection of <see cref="Polygon"/> objects.
/// </summary>
public class Polygons : List<Polygon>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class.
    /// </summary>
    public Polygons() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public Polygons(int capacity) : base(capacity) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class containing the specified polygons.
    /// </summary>
    /// <param name="polygons">An array of polygons to add.</param>
    public Polygons(params Polygon[] polygons) { AddRange(polygons); }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class containing the specified polygons.
    /// </summary>
    /// <param name="polygons">An enumerable collection of polygons to add.</param>
    public Polygons(IEnumerable<Polygon> polygons) { AddRange(polygons); }
}

/// <summary>
/// Represents a collection of <see cref="Polyline"/> objects.
/// </summary>
public class Polylines : List<Polyline>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class.
    /// </summary>
    public Polylines() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class containing the specified polylines.
    /// </summary>
    /// <param name="polylines">An array of polylines to add.</param>
    public Polylines(params Polyline[] polylines) { AddRange(polylines); }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class containing the specified polylines.
    /// </summary>
    /// <param name="polylines">An enumerable collection of polylines to add.</param>
    public Polylines(IEnumerable<Polyline> polylines) { AddRange(polylines); }
}

/// <summary>
/// Provides static methods for performing geometric clipping and polygon operations using Clipper2.
/// </summary>
/// <remarks>
/// This class contains extension methods for polygons, polylines, and related shapes, enabling union, intersection, difference, inflation, simplification, and conversion operations.
/// </remarks>
public static class ShapeClipper
{
    /// <summary>
    /// Clips a polygon to a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to clip to.</param>
    /// <param name="poly">The polygon to be clipped.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The clipped paths as <see cref="PathsD"/>.</returns>
    public static PathsD ClipRect(this Rect rect, Polygon poly, int precision = 2)
    {
        return Clipper.RectClip(rect.ToClipperRect(), poly.ToClipperPath(), precision);
    }
    
    /// <summary>
    /// Computes the union of a polygon with multiple other polygons.
    /// </summary>
    /// <param name="a">The base polygon.</param>
    /// <param name="other">The collection of polygons to union with.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <returns>The unioned paths as <see cref="PathsD"/>.</returns>
    public static PathsD UnionMany(this Polygon a, Polygons other, FillRule fillRule = FillRule.NonZero)
    {
        return Clipper.Union(a.ToClipperPaths(), other.ToClipperPaths(), fillRule);
    }

    /// <summary>
    /// Computes the union of two polygons.
    /// </summary>
    /// <param name="a">The first polygon.</param>
    /// <param name="b">The second polygon.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <returns>The unioned paths as <see cref="PathsD"/>.</returns>
    public static PathsD Union(this Polygon a, Polygon b, FillRule fillRule = FillRule.NonZero) { return Clipper.Union(ToClipperPaths(a), ToClipperPaths(b), fillRule); }
    
    /// <summary>
    /// Computes the intersection of two polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The intersected paths as <see cref="PathsD"/>.</returns>
    public static PathsD Intersect(this Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        return Clipper.Intersect(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
    }

    /// <summary>
    /// Computes the intersection of a polygon with multiple subject polygons.
    /// </summary>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="subjects">The collection of subject polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The intersected paths as <see cref="PathsD"/>.</returns>
    public static PathsD Intersect(this Polygon clip, Polygons subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var result = new PathsD();
        foreach (var subject in subjects)
        {
            result.AddRange(Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule, precision));
        }
        return result;
    }

    /// <summary>
    /// Computes the intersection of a subject polygon with multiple clip polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clips">The collection of clip polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The intersected paths as <see cref="PathsD"/>.</returns>
    public static PathsD IntersectMany(this Polygon subject, Polygons clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        foreach (var clip in clips)
        {
            cur = Clipper.Intersect(cur, clip.ToClipperPaths(), fillRule, precision);
        }
        return cur;
    }

    /// <summary>
    /// Computes the difference between a subject polygon and a clip polygon.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
    }
    
    /// <summary>
    /// Computes the difference between multiple subject polygons and a clip polygon.
    /// </summary>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="subjects">The collection of subject polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon clip, Polygons subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var result = new PathsD();
        var clipPaths = clip.ToClipperPaths();
        foreach (var subject in subjects)
        {
            result.AddRange(Clipper.Difference(subject.ToClipperPaths(), clipPaths, fillRule, precision));
        }
        return result;
    }

    /// <summary>
    /// Computes the difference between a subject polygon and multiple clip polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clips">The collection of clip polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD DifferenceMany(this Polygon subject, Polygons clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        foreach (var clip in clips)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, precision);
        }
        return cur;
    }
    
    /// <summary>
    /// Computes the difference between a subject polygon and a polyline.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="polyline">The polyline to subtract.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon subject, Polyline polyline, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(ToClipperPaths(subject), ToClipperPaths(polyline), fillRule, precision);
    }
    
    /// <summary>
    /// Computes the difference between a subject polygon and multiple polylines.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="polylines">The collection of polylines to subtract.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD DifferenceMany(this Polygon subject, Polylines polylines, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        foreach (var clip in polylines)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, precision);
        }
        return cur;
    }

    /// <summary>
    /// Computes the difference between a subject polygon and a segment.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="segment">The segment to subtract.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon subject, Segment segment, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(ToClipperPaths(subject), ToClipperPaths(segment), fillRule, precision);
    }
    
    /// <summary>
    /// Computes the difference between a subject polygon and multiple segments.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="segments">The collection of segments to subtract.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD DifferenceMany(this Polygon subject, Segments segments, FillRule fillRule = FillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        foreach (var clip in segments)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, precision);
        }
        return cur;
    }

    /// <summary>
    /// Determines if a path is a hole (negative orientation).
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a hole; otherwise, false.</returns>
    public static bool IsHole(this PathD path) { return !Clipper.IsPositive(path); }

    /// <summary>
    /// Determines if a polygon is a hole (negative orientation).
    /// </summary>
    /// <param name="p">The polygon to check.</param>
    /// <returns>True if the polygon is a hole; otherwise, false.</returns>
    public static bool IsHole(this Polygon p) { return IsHole(p.ToClipperPath()); }
    
    /// <summary>
    /// Removes all holes from the given paths in-place.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>The modified <see cref="PathsD"/> with holes removed.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static PathsD RemoveAllHoles(this PathsD paths) { paths.RemoveAll((p) => { return IsHole(p); }); return paths; }

    /// <summary>
    /// Removes all holes from the given polygons in-place.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>The modified <see cref="Polygons"/> with holes removed.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static Polygons RemoveAllHoles(this Polygons polygons) { polygons.RemoveAll((p) => { return IsHole(p); }); return polygons; }
    
    /// <summary>
    /// Keeps only the holes in the given paths in-place.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>The modified <see cref="PathsD"/> containing only holes.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static PathsD GetAllHoles(this PathsD paths) { paths.RemoveAll((p) => { return !IsHole(p); }); return paths; }

    /// <summary>
    /// Keeps only the holes in the given polygons in-place.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>The modified <see cref="Polygons"/> containing only holes.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static Polygons GetAllHoles(this Polygons polygons) { polygons.RemoveAll((p) => { return !IsHole(p); }); return polygons; }
    
    /// <summary>
    /// Returns a copy of the given paths with all holes removed.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>A new <see cref="PathsD"/> with holes removed.</returns>
    public static PathsD RemoveAllHolesCopy(this PathsD paths)
    {
        var result = new PathsD();
        foreach (var p in paths)
        {
            if(!IsHole(p)) result.Add(p);
        }
        return result;
    }

    /// <summary>
    /// Returns a copy of the given polygons with all holes removed.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>A new <see cref="Polygons"/> with holes removed.</returns>
    public static Polygons RemoveAllHolesCopy(this Polygons polygons)
    {
        var result = new Polygons();
        foreach (var p in polygons)
        {
            if (!IsHole(p)) result.Add(p);
        }
        return result;
    }
   
    /// <summary>
    /// Returns a copy of the given paths containing only holes.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>A new <see cref="PathsD"/> containing only holes.</returns>
    public static PathsD GetAllHolesCopy(this PathsD paths)
    {
        var result = new PathsD();
        foreach (var p in paths)
        {
            if (IsHole(p)) result.Add(p);
        }
        return result;
    }

    /// <summary>
    /// Returns a copy of the given polygons containing only holes.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>A new <see cref="Polygons"/> containing only holes.</returns>
    public static Polygons GetAllHolesCopy(this Polygons polygons)
    {
        var result = new Polygons();
        foreach (var p in polygons)
        {
            if (IsHole(p)) result.Add(p);
        }
        return result;
    }

    /// <summary>
    /// Inflates (offsets) a polyline by a specified delta.
    /// </summary>
    /// <param name="polyline">The polyline to inflate.</param>
    /// <param name="delta">The offset distance.</param>
    /// <param name="joinType">The join type for corners.</param>
    /// <param name="endType">The end type for open paths.</param>
    /// <param name="miterLimit">The miter limit for miter joins.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The inflated paths as <see cref="PathsD"/>.</returns>
    public static PathsD Inflate(this Polyline polyline, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Square, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polyline.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
    }

    /// <summary>
    /// Inflates (offsets) a polygon by a specified delta.
    /// </summary>
    /// <param name="poly">The polygon to inflate.</param>
    /// <param name="delta">The offset distance.</param>
    /// <param name="joinType">The join type for corners.</param>
    /// <param name="endType">The end type for closed paths.</param>
    /// <param name="miterLimit">The miter limit for miter joins.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The inflated paths as <see cref="PathsD"/>.</returns>
    public static PathsD Inflate(this Polygon poly, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(poly.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
    }

    /// <summary>
    /// Inflates (offsets) a collection of polygons by a specified delta.
    /// </summary>
    /// <param name="polygons">The polygons to inflate.</param>
    /// <param name="delta">The offset distance.</param>
    /// <param name="joinType">The join type for corners.</param>
    /// <param name="endType">The end type for closed paths.</param>
    /// <param name="miterLimit">The miter limit for miter joins.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The inflated paths as <see cref="PathsD"/>.</returns>
    public static PathsD Inflate(this Polygons polygons, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polygons.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
    }
    
    /// <summary>
    /// Simplifies a polygon using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygon to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The simplified path as <see cref="PathD"/>.</returns>
    public static PathD Simplify(this Polygon poly, float epsilon, bool isOpen = false) { return Clipper.SimplifyPath(poly.ToClipperPath(), epsilon, isOpen); }

    /// <summary>
    /// Simplifies a collection of polygons using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygons to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <param name="isOpen">Whether the paths are open or closed.</param>
    /// <returns>The simplified paths as <see cref="PathsD"/>.</returns>
    public static PathsD Simplify(this Polygons poly, float epsilon, bool isOpen = false) { return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon, isOpen); }
    
    /// <summary>
    /// Simplifies a polygon using the Ramer-Douglas-Peucker algorithm.
    /// Only works on closed polygons.
    /// </summary>
    /// <param name="poly">The polygon to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <returns>The simplified path as <see cref="PathD"/>.</returns>
    public static PathD SimplifyRDP(this Polygon poly, float epsilon) { return Clipper.RamerDouglasPeucker(poly.ToClipperPath(), epsilon); }

    /// <summary>
    /// Simplifies a collection of polygons using the Ramer-Douglas-Peucker algorithm.
    /// Only works on closed polygons.
    /// </summary>
    /// <param name="poly">The polygons to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <returns>The simplified paths as <see cref="PathsD"/>.</returns>
    public static PathsD SimplifyRDP(this Polygons poly, float epsilon) { return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon); }

    /// <summary>
    /// Determines if a point is inside a polygon using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygon to test.</param>
    /// <param name="p">The point to check.</param>
    /// <returns>The result as <see cref="PointInPolygonResult"/>.</returns>
    public static PointInPolygonResult IsPointInsideClipper(this Polygon poly, Vector2 p) { return Clipper.PointInPolygon(p.ToClipperPoint(), poly.ToClipperPath()); }

    /// <summary>
    /// Determines if a point is inside a polygon.
    /// </summary>
    /// <param name="poly">The polygon to test.</param>
    /// <param name="p">The point to check.</param>
    /// <returns>True if the point is inside; otherwise, false.</returns>
    public static bool IsPointInside(this Polygon poly, Vector2 p) { return IsPointInsideClipper(poly, p) != PointInPolygonResult.IsOutside; }

    /// <summary>
    /// Removes collinear points from a polygon.
    /// </summary>
    /// <param name="poly">The polygon to process.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The trimmed path as <see cref="PathD"/>.</returns>
    public static PathD TrimCollinear(this Polygon poly, int precision, bool isOpen = false) { return Clipper.TrimCollinear(poly.ToClipperPath(), precision, isOpen); }

    /// <summary>
    /// Removes near-duplicate points from a polygon based on minimum edge length squared.
    /// </summary>
    /// <param name="poly">The polygon to process.</param>
    /// <param name="minEdgeLengthSquared">The minimum squared edge length to consider as unique.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The processed path as <see cref="PathD"/>.</returns>
    public static PathD StripDuplicates(this Polygon poly, float minEdgeLengthSquared, bool isOpen = false) { return Clipper.StripNearDuplicates(poly.ToClipperPath(), minEdgeLengthSquared, isOpen); }

    /// <summary>
    /// Computes the Minkowski difference between two polygons.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon.</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiDiff(this Polygon poly, Polygon path, bool isClosed = false) { return Clipper.MinkowskiDiff(poly.ToClipperPath(), path.ToClipperPath(), isClosed); }

    /// <summary>
    /// Computes the Minkowski sum of two polygons.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon.</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiSum(this Polygon poly, Polygon path, bool isClosed = false) { return Clipper.MinkowskiSum(poly.ToClipperPath(), path.ToClipperPath(), isClosed); }

    /// <summary>
    /// Computes the Minkowski difference between a polygon and a path, with the path positioned at the origin.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon (will be moved to origin).</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiDiffOrigin(this Polygon poly, Polygon path, bool isClosed = false)
    {
        var pathCopy = path.SetPositionCopy(new(0f));
        if (pathCopy == null) return new();
        return Clipper.MinkowskiDiff(poly.ToClipperPath(), pathCopy.ToClipperPath(), isClosed);
    }

    /// <summary>
    /// Computes the Minkowski sum of a polygon and a path, with the path positioned at the origin.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon (will be moved to origin).</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiSumOrigin(this Polygon poly, Polygon path, bool isClosed = false)
    {
        var pathCopy = path.SetPositionCopy(new(0f));
        if (pathCopy == null) return new();
        return Clipper.MinkowskiSum(poly.ToClipperPath(), pathCopy.ToClipperPath(), isClosed);
    }

    /// <summary>
    /// Creates an ellipse polygon.
    /// </summary>
    /// <param name="center">The center of the ellipse.</param>
    /// <param name="radiusX">The X radius.</param>
    /// <param name="radiusY">The Y radius. If zero, uses <paramref name="radiusX"/>.</param>
    /// <param name="steps">The number of steps (vertices) for the ellipse. If zero, uses default.</param>
    /// <returns>A polygon representing the ellipse.</returns>
    public static Polygon CreateEllipse(Vector2 center, float radiusX, float radiusY = 0f, int steps = 0) { return Clipper.Ellipse(center.ToClipperPoint(), radiusX, radiusY, steps).ToPolygon(); }

    /// <summary>
    /// Converts a <see cref="PointD"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="p">The point to convert.</param>
    /// <returns>The converted <see cref="Vector2"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static Vector2 ToVec2(this PointD p) { return new((float)p.x, -(float)p.y); }//flip of y necessary -> clipper up y is positve - raylib is negative

    /// <summary>
    /// Converts a <see cref="Vector2"/> to a <see cref="PointD"/> for Clipper.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <returns>The converted <see cref="PointD"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static PointD ToClipperPoint(this Vector2 v) { return new(v.X, -v.Y); }

    /// <summary>
    /// Converts a <see cref="Rect"/> to a <see cref="RectD"/> for Clipper.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectD"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static RectD ToClipperRect(this Rect r) { return new RectD(r.X, -r.Y-r.Height, r.X + r.Width, -r.Y); }

    /// <summary>
    /// Converts a <see cref="RectD"/> to a <see cref="Rect"/>.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted <see cref="Rect"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static Rect ToRect(this RectD r) { return new Rect((float)r.left, (float)(-r.top-r.Height), (float)r.Width, (float)r.Height); }

    /// <summary>
    /// Converts a <see cref="PathD"/> to a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The converted <see cref="Polygon"/>.</returns>
    public static Polygon ToPolygon(this PathD path)
    {
        var poly = new Polygon();
        foreach (var point in path)
        {
            poly.Add(point.ToVec2());
        }
        return poly;
    }

    /// <summary>
    /// Converts a <see cref="PathsD"/> to <see cref="Polygons"/>.
    /// </summary>
    /// <param name="paths">The paths to convert.</param>
    /// <param name="removeHoles">If true, removes holes from the result.</param>
    /// <returns>The converted <see cref="Polygons"/>.</returns>
    public static Polygons ToPolygons(this PathsD paths, bool removeHoles = false)
    {
        var polygons = new Polygons();
        foreach (var path in paths)
        {
            if (!removeHoles || !IsHole(path))
            {
                polygons.Add(path.ToPolygon());
            }
        }
        return polygons;
    }

    /// <summary>
    /// Converts a <see cref="Polygon"/> to a <see cref="PathD"/>.
    /// </summary>
    /// <param name="poly">The polygon to convert.</param>
    /// <returns>The converted <see cref="PathD"/>.</returns>
    public static PathD ToClipperPath(this Polygon poly)
    {
        var path = new PathD();
        foreach (var vertex in poly)
        {
            path.Add(vertex.ToClipperPoint());
        }
        return path;
    }

    /// <summary>
    /// Converts a <see cref="Segment"/> to a <see cref="PathD"/>.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <returns>The converted <see cref="PathD"/>.</returns>
    public static PathD ToClipperPath(this Segment segment)
    {
        var path = new PathD();
        path.Add(segment.Start.ToClipperPoint());
        path.Add(segment.End.ToClipperPoint());
        return path;
    }

    /// <summary>
    /// Converts a <see cref="Segment"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this Segment segment){ return new PathsD() { segment.ToClipperPath() }; }

    /// <summary>
    /// Converts a <see cref="Polygon"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="poly">The polygon to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this Polygon poly) { return new PathsD() { poly.ToClipperPath() }; }

    /// <summary>
    /// Converts an array of <see cref="Polygon"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="polygons">The polygons to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(params Polygon[] polygons) { return polygons.ToClipperPaths(); }

    /// <summary>
    /// Converts an enumerable of <see cref="Polygon"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="polygons">The polygons to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this IEnumerable<Polygon> polygons)
    {
        var result = new PathsD();
        foreach(var polygon in polygons)
        {
            result.Add(polygon.ToClipperPath());
        }
        return result;
    }

    /// <summary>
    /// Converts a <see cref="PathD"/> to a <see cref="Polyline"/>.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The converted <see cref="Polyline"/>.</returns>
    public static Polyline ToPolyline(this PathD path)
    {
        var polyline = new Polyline();
        foreach (var point in path)
        {
            polyline.Add(point.ToVec2());
        }
        return polyline;
    }

    /// <summary>
    /// Converts a <see cref="PathsD"/> to <see cref="Polylines"/>.
    /// </summary>
    /// <param name="paths">The paths to convert.</param>
    /// <param name="removeHoles">If true, removes holes from the result.</param>
    /// <returns>The converted <see cref="Polylines"/>.</returns>
    public static Polylines ToPolylines(this PathsD paths, bool removeHoles = false)
    {
        var polylines = new Polylines();
        foreach (var path in paths)
        {
            if (!removeHoles || !IsHole(path))
            {
                polylines.Add(path.ToPolyline());
            }
        }
        return polylines;
    }

    /// <summary>
    /// Converts a <see cref="Polyline"/> to a <see cref="PathD"/>.
    /// </summary>
    /// <param name="polyline">The polyline to convert.</param>
    /// <returns>The converted <see cref="PathD"/>.</returns>
    public static PathD ToClipperPath(this Polyline polyline)
    {
        var path = new PathD();
        foreach (var vertex in polyline)
        {
            path.Add(vertex.ToClipperPoint());
        }
        return path;
    }

    /// <summary>
    /// Converts a <see cref="Polyline"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="polyline">The polyline to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this Polyline polyline) { return new PathsD() { polyline.ToClipperPath() }; }

    /// <summary>
    /// Converts an array of <see cref="Polyline"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="polylines">The polylines to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(params Polyline[] polylines) { return polylines.ToClipperPaths(); }

    /// <summary>
    /// Converts an enumerable of <see cref="Polyline"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="polylines">The polylines to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this IEnumerable<Polyline> polylines)
    {
        var result = new PathsD();
        foreach (var polyline in polylines)
        {
            result.Add(polyline.ToClipperPath());
        }
        return result;
    }
}
