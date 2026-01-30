using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Specifies how convex angled joins are handled when offsetting (inflating/shrinking) paths.
/// This enumeration is only required for offset operations (e.g. ClipperOffset) and is not used
/// for polygon clipping operations.
/// </summary>
public enum ShapeClipperJoinType
{
    /// <summary>
    /// Edges are offset a specified distance and extended to their intersection points.
    /// A miter limit is enforced to prevent very long spikes at acute angles; when the limit is
    /// exceeded the join is converted to a squared/bevel form.
    /// </summary>
    Miter,

    /// <summary>
    /// Convex joins are truncated with a squared edge. The midpoint of the squared edge is
    /// exactly the offset distance from the original vertex.
    /// </summary>
    Square,

    /// <summary>
    /// Bevel joins cut the corner with a straight edge between the offset edges. Beveling is
    /// typically simpler and faster than squared joins and is common in many graphics formats.
    /// </summary>
    Bevel,

    /// <summary>
    /// Convex joins are rounded using an arc with radius equal to the offset distance and the
    /// original join vertex as the arc center.
    /// </summary>
    Round
}

/// <summary>
/// The EndType enumerator controls how the ends of paths are handled when performing
/// offset (inflating/shrinking) operations. This enumeration is only required for
/// offset operations and is not used for polygon clipping.
/// </summary>
/// <remarks>
/// With both EndType.Polygon and EndType.Joined, path closure will occur regardless
/// of whether or not the first and last vertices in the path match.
/// </remarks>
public enum ShapeClipperEndType
{
    /// <summary>
    /// The path is treated as a closed polygon; offsets consider the path closed. (Filled)
    /// </summary>
    Polygon,

    /// <summary>
    /// The path is treated as a polyline and its ends are joined during offsetting. (Outline)
    /// </summary>
    Joined,

    /// <summary>
    /// Path ends are squared off without any extension (flat cutoff at ends).
    /// </summary>
    Butt,

    /// <summary>
    /// Path ends are extended by the offset amount and then squared off.
    /// </summary>
    Square,

    /// <summary>
    /// Path ends are extended by the offset amount and rounded (arc) off.
    /// </summary>
    Round
}

/// <summary>
/// Filling rules determine which sub-regions of complex polygons are considered "inside".
/// Complex polygons are defined by one or more closed contours; only portions of these contours
/// may contribute to filled regions, so a filling rule is required to decide which sub-regions
/// are treated as inside when performing clipping operations.
/// </summary>
/// <example>
/// Example algorithm (winding number):
/// From a point outside the polygon draw a ray through the polygon. Start with winding number 0.
/// For each contour crossed, increment the winding number if the crossing goes right-to-left
/// relative to the ray, otherwise decrement. Each sub-region gets the current winding number.
/// </example>
/// <remarks>
/// The supported fill rules are based on winding numbers derived from the orientation of each path:
/// <list type="bullet">
/// <item>Even-Odd: toggles inside/outside each time a contour is crossed.</item>
/// <item>Non-Zero: considers the sum of winding contributions; non-zero means inside.</item>
/// <item>Positive / Negative: depend on the sign of the winding number.</item>
/// </list>
/// Notes:
/// <list type="bullet">
/// <item>The most commonly used rules are Even-Odd and Non-Zero.</item>
/// <item>Reversing a path reverses its orientation (and the sign of winding numbers) but does not affect parity or whether a winding number is zero.</item>
/// <item>Filling rules are required only for clipping operations; they do not affect polygon offsetting.</item>
/// </list>
/// </remarks>
public enum ShapeClipperFillRule
{
    /// <summary>
    /// Even-Odd rule: only sub-regions with odd winding parity are filled.
    /// </summary>
    EvenOdd,

    /// <summary>
    /// Non-Zero rule: any sub-region with a non-zero winding number is filled.
    /// </summary>
    NonZero,

    /// <summary>
    /// Positive rule: only sub-regions with winding counts &gt; 0 are filled.
    /// </summary>
    Positive,

    /// <summary>
    /// Negative rule: only sub-regions with winding counts &lt; 0 are filled.
    /// </summary>
    Negative
}


/// <summary>
/// Provides static methods for performing geometric clipping and polygon operations using Clipper2.
/// </summary>
/// <remarks>
/// This class contains extension methods for polygons, polylines, and related shapes, enabling union, intersection, difference, inflation, simplification, and conversion operations.
/// </remarks>
public static class ShapeClipper
{
    #region Clip
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
    #endregion
    
    #region Union
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
    public static PathsD Union(this Polygon a, Polygon b, FillRule fillRule = FillRule.NonZero) { return Clipper.Union(a.ToClipperPaths(), b.ToClipperPaths(), fillRule); }
    #endregion
    
    #region Intersect
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
    #endregion
    
    #region Difference
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
    #endregion

    #region Holes
    /// <summary>
    /// Determines if a path is a hole (negative orientation).
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a hole; otherwise, false.</returns>
    public static bool IsHole(this PathD path)
    {
        return !Clipper.IsPositive(path);
    }

    /// <summary>
    /// Determines if a polygon is a hole (negative orientation).
    /// </summary>
    /// <param name="p">The polygon to check.</param>
    /// <returns>True if the polygon is a hole; otherwise, false.</returns>
    public static bool IsHole(this Polygon p)
    {
        return p.ToClipperPath().IsHole();
    }

    /// <summary>
    /// Removes all holes from the given paths in-place.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>The modified <see cref="PathsD"/> with holes removed.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static PathsD RemoveAllHoles(this PathsD paths)
    {
        paths.RemoveAll((p) => p.IsHole()); return paths;
    }

    /// <summary>
    /// Removes all holes from the given polygons in-place.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>The modified <see cref="Polygons"/> with holes removed.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static Polygons RemoveAllHoles(this Polygons polygons)
    {
        polygons.RemoveAll((p) => p.IsHole()); 
        return polygons;
    }

    /// <summary>
    /// Keeps only the holes in the given paths in-place.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <returns>The modified <see cref="PathsD"/> containing only holes.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static PathsD GetAllHoles(this PathsD paths)
    {
        paths.RemoveAll((p) => !p.IsHole()); 
        return paths;
    }

    /// <summary>
    /// Keeps only the holes in the given polygons in-place.
    /// </summary>
    /// <param name="polygons">The polygons to process.</param>
    /// <returns>The modified <see cref="Polygons"/> containing only holes.</returns>
    /// <remarks>This method modifies the input collection.</remarks>
    public static Polygons GetAllHoles(this Polygons polygons)
    {
        polygons.RemoveAll((p) => !p.IsHole());
        return polygons;
    }
    
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
    #endregion
    
    #region Inflate
    /// <summary>
    /// Inflates (offsets) a polyline by the specified delta.
    /// </summary>
    /// <param name="polyline">The polyline to offset.</param>
    /// <param name="delta">Offset distance. Positive values expand outward; negative values contract inward.</param>
    /// <param name="joinType">Type of corner joins to use.
    /// <list type="bullet">
    /// <item> JoinType.Miter: Edges are first offset a specified distance away from and parallel to their original (ie starting) edge positions.
    /// These offset edges are then extended to points where they intersect with adjacent edge offsets.
    /// However a limit must be imposed on how far mitered vertices can be from their original positions to avoid very convex joins producing unreasonably long and narrow spikes).
    /// To avoid unsightly spikes, joins will be 'squared' wherever distances between original vertices
    /// and their calculated offsets exceeds a specified value (expressed as a ratio relative to the offset distance).</item>
    /// <item> JoinType.Square: Convex joins will be truncated using a 'squaring' edge.
    /// And the mid-points of these squaring edges will be exactly the offset distance away from their original (or starting) vertices. </item>
    /// <item> JoinType.Bevel: Bevelled joins are similar to 'squared' joins except that squaring won't occur at a fixed distance.
    /// While bevelled joins may not be as pretty as squared joins, bevelling is much easier (ie faster) than squaring. </item>
    /// <item> JoinType.Round: Rounding is applied to all convex joins with the arc radius being the offset distance, and the original join vertex the arc center. </item>
    /// </list>
    /// </param>
    /// <param name="endType">How path ends are handled.
    /// <list type="bullet">
    /// <item> Polygon: the path is treated as a polygon </item>
    /// <item> Joined: ends are joined and the path treated as a polyline </item>
    /// <item> Square: ends extend the offset amount while being squared off </item>
    /// <item> Round: ends extend the offset amount while being rounded off </item>
    /// <item> Butt: ends are squared off without any extension </item>
    /// </list>
    /// </param>
    /// <param name="miterLimit">
    /// Miter limit applied when <see cref="JoinType.Miter"/> is used.
    /// This property sets the maximum distance in multiples of delta that vertices can be offset from their original positions before squaring is applied.
    /// (Squaring truncates a miter by 'cutting it off' at 1 × delta distance from the original vertex.)
    /// The default value for MiterLimit is 2 (ie twice delta). This is also the smallest MiterLimit that's allowed.
    /// If mitering was unrestricted (ie without any squaring), then offsets at very acute angles would generate unacceptably long 'spikes'.
    /// </param>
    /// <param name="precision">Decimal precision for the operation.</param>
    /// <param name="arcTolerance">
    /// Arc approximation tolerance used when rounding joins or ends.
    /// Ignored when neither <see cref="JoinType.Round"/> nor <see cref="EndType.Round"/> is used.
    /// ArcTolerance is only relevant when offsetting with JoinType.Round and / or EndType.Round (see ClipperOffset.AddPath and ClipperOffset.AddPaths).
    /// The Clipper2 library approximates arcs by using series of relatively short straight line segments (see Trigonometry).
    /// And logically, shorter line segments will produce better arc approximations. But very short segments can degrade performance, usually with little or no discernable improvement in curve quality.
    /// Very short segments can even detract from curve quality, due to the effects of integer rounding.
    /// Arc tolerance is user defined since there isn't an optimal number of line segments for any given arc radius (ie that perfectly balances curve approximation with performance).
    /// Nevertheless, when the user doesn't define an arc tolerance and uses the default value (0.0), a 'default' arc tolerance is calculated (see below) that generally produces visually smooth arc approximations,
    /// while avoiding excessively small segment lengths.
    /// The default ArcTolerance is: offset_radius / 500.
    /// </param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths.</returns>
    public static PathsD Inflate(this Polyline polyline, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Square, float miterLimit = 2f, int precision = 2, double arcTolerance = 0.0)
    {
        if (joinType != JoinType.Round && endType != EndType.Round) arcTolerance = 0.0f;
        return Clipper.InflatePaths(polyline.ToClipperPaths(), delta, joinType, endType, miterLimit, precision, arcTolerance);
    }

    /// <summary>
    /// Inflates (offsets) a polygon by the specified delta.
    /// </summary>
    /// <param name="poly">The polygon to offset.</param>
    /// <param name="delta">Offset distance. Positive values expand outward; negative values contract inward.</param>
    /// <param name="joinType">Type of corner joins to use.
    /// <list type="bullet">
    /// <item> JoinType.Miter: Edges are first offset a specified distance away from and parallel to their original (ie starting) edge positions.
    /// These offset edges are then extended to points where they intersect with adjacent edge offsets.
    /// However a limit must be imposed on how far mitered vertices can be from their original positions to avoid very convex joins producing unreasonably long and narrow spikes).
    /// To avoid unsightly spikes, joins will be 'squared' wherever distances between original vertices
    /// and their calculated offsets exceeds a specified value (expressed as a ratio relative to the offset distance).</item>
    /// <item> JoinType.Square: Convex joins will be truncated using a 'squaring' edge.
    /// And the mid-points of these squaring edges will be exactly the offset distance away from their original (or starting) vertices. </item>
    /// <item> JoinType.Bevel: Bevelled joins are similar to 'squared' joins except that squaring won't occur at a fixed distance.
    /// While bevelled joins may not be as pretty as squared joins, bevelling is much easier (ie faster) than squaring. </item>
    /// <item> JoinType.Round: Rounding is applied to all convex joins with the arc radius being the offset distance, and the original join vertex the arc center. </item>
    /// </list>
    /// </param>
    /// <param name="endType">How path ends are handled.
    /// <list type="bullet">
    /// <item> Polygon: the path is treated as a polygon </item>
    /// <item> Joined: ends are joined and the path treated as a polyline </item>
    /// <item> Square: ends extend the offset amount while being squared off </item>
    /// <item> Round: ends extend the offset amount while being rounded off </item>
    /// <item> Butt: ends are squared off without any extension </item>
    /// </list>
    /// </param>
    /// <param name="miterLimit">
    /// Miter limit applied when <see cref="JoinType.Miter"/> is used.
    /// This property sets the maximum distance in multiples of delta that vertices can be offset from their original positions before squaring is applied.
    /// (Squaring truncates a miter by 'cutting it off' at 1 × delta distance from the original vertex.)
    /// The default value for MiterLimit is 2 (ie twice delta). This is also the smallest MiterLimit that's allowed.
    /// If mitering was unrestricted (ie without any squaring), then offsets at very acute angles would generate unacceptably long 'spikes'.
    /// </param>
    /// <param name="precision">Decimal precision for the operation.</param>
    /// <param name="arcTolerance">
    /// Arc approximation tolerance used when rounding joins or ends.
    /// Ignored when neither <see cref="JoinType.Round"/> nor <see cref="EndType.Round"/> is used.
    /// ArcTolerance is only relevant when offsetting with JoinType.Round and / or EndType.Round (see ClipperOffset.AddPath and ClipperOffset.AddPaths).
    /// The Clipper2 library approximates arcs by using series of relatively short straight line segments (see Trigonometry).
    /// And logically, shorter line segments will produce better arc approximations. But very short segments can degrade performance, usually with little or no discernable improvement in curve quality.
    /// Very short segments can even detract from curve quality, due to the effects of integer rounding.
    /// Arc tolerance is user defined since there isn't an optimal number of line segments for any given arc radius (ie that perfectly balances curve approximation with performance).
    /// Nevertheless, when the user doesn't define an arc tolerance and uses the default value (0.0), a 'default' arc tolerance is calculated (see below) that generally produces visually smooth arc approximations,
    /// while avoiding excessively small segment lengths.
    /// The default ArcTolerance is: offset_radius / 500.
    /// </param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths.</returns>
    public static PathsD Inflate(this Polygon poly, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2, double arcTolerance = 0.0)
    {
        if (joinType != JoinType.Round && endType != EndType.Round) arcTolerance = 0.0f;
        return Clipper.InflatePaths(poly.ToClipperPaths(), delta, joinType, endType, miterLimit, precision, arcTolerance);
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
    public static PathsD InflateMany(this Polygons polygons, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polygons.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
    }
    /// <summary>
    /// Inflates (offsets) a collection of polylines by the specified delta.
    /// </summary>
    /// <param name="polylines">The collection of polylines to offset.</param>
    /// <param name="delta">Offset distance. Positive values expand outward; negative values contract inward.</param>
    /// <param name="joinType">Type of corner joins to use (defaults to <see cref="JoinType.Square"/>).</param>
    /// <param name="endType">How path ends are handled for the closed/open paths (defaults to <see cref="EndType.Polygon"/>).</param>
    /// <param name="miterLimit">Miter limit applied when <see cref="JoinType.Miter"/> is used (defaults to 2f).</param>
    /// <param name="precision">Decimal precision for the operation (defaults to 2).</param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths for all input polylines.</returns>
    public static PathsD InflateMany(this Polylines polylines, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polylines.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
    }
    #endregion
    
    #region Simplify
    /// <summary>
    /// Simplifies a polygon using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygon to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The simplified path as <see cref="PathD"/>.</returns>
    public static PathD Simplify(this Polygon poly, float epsilon, bool isOpen = false)
    {
        return Clipper.SimplifyPath(poly.ToClipperPath(), epsilon, isOpen);
    }

    /// <summary>
    /// Simplifies a collection of polygons using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygons to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <param name="isOpen">Whether the paths are open or closed.</param>
    /// <returns>The simplified paths as <see cref="PathsD"/>.</returns>
    public static PathsD Simplify(this Polygons poly, float epsilon, bool isOpen = false)
    {
        return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon, isOpen);
    }

    /// <summary>
    /// Simplifies a polygon using the Ramer-Douglas-Peucker algorithm.
    /// Only works on closed polygons.
    /// </summary>
    /// <param name="poly">The polygon to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <returns>The simplified path as <see cref="PathD"/>.</returns>
    public static PathD SimplifyRDP(this Polygon poly, float epsilon)
    {
        return Clipper.RamerDouglasPeucker(poly.ToClipperPath(), epsilon);
    }

    /// <summary>
    /// Simplifies a collection of polygons using the Ramer-Douglas-Peucker algorithm.
    /// Only works on closed polygons.
    /// </summary>
    /// <param name="poly">The polygons to simplify.</param>
    /// <param name="epsilon">The tolerance for simplification.</param>
    /// <returns>The simplified paths as <see cref="PathsD"/>.</returns>
    public static PathsD SimplifyRDP(this Polygons poly, float epsilon)
    {
        return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon);
    }
    #endregion
    
    #region Point Inside
    /// <summary>
    /// Determines if a point is inside a polygon using the Clipper algorithm.
    /// </summary>
    /// <param name="poly">The polygon to test.</param>
    /// <param name="p">The point to check.</param>
    /// <returns>The result as <see cref="PointInPolygonResult"/>.</returns>
    public static PointInPolygonResult IsPointInsideClipper(this Polygon poly, Vector2 p)
    {
        return Clipper.PointInPolygon(p.ToClipperPoint(), poly.ToClipperPath());
    }

    /// <summary>
    /// Determines if a point is inside a polygon.
    /// </summary>
    /// <param name="poly">The polygon to test.</param>
    /// <param name="p">The point to check.</param>
    /// <returns>True if the point is inside; otherwise, false.</returns>
    public static bool IsPointInside(this Polygon poly, Vector2 p)
    {
        return poly.IsPointInsideClipper(p) != PointInPolygonResult.IsOutside;
    }
    #endregion
    
    #region Trim Collinear & Strip Duplicates
    /// <summary>
    /// Removes collinear points from a polygon.
    /// </summary>
    /// <param name="poly">The polygon to process.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The trimmed path as <see cref="PathD"/>.</returns>
    public static PathD TrimCollinear(this Polygon poly, int precision, bool isOpen = false)
    {
        return Clipper.TrimCollinear(poly.ToClipperPath(), precision, isOpen);
    }

    /// <summary>
    /// Removes near-duplicate points from a polygon based on minimum edge length squared.
    /// </summary>
    /// <param name="poly">The polygon to process.</param>
    /// <param name="minEdgeLengthSquared">The minimum squared edge length to consider as unique.</param>
    /// <param name="isOpen">Whether the path is open or closed.</param>
    /// <returns>The processed path as <see cref="PathD"/>.</returns>
    public static PathD StripDuplicates(this Polygon poly, float minEdgeLengthSquared, bool isOpen = false)
    {
        return Clipper.StripNearDuplicates(poly.ToClipperPath(), minEdgeLengthSquared, isOpen);
    }
    #endregion
    
    #region Create Shapes
    /// <summary>
    /// Creates an ellipse polygon.
    /// </summary>
    /// <param name="center">The center of the ellipse.</param>
    /// <param name="radiusX">The X radius.</param>
    /// <param name="radiusY">The Y radius. If zero, uses <paramref name="radiusX"/>.</param>
    /// <param name="steps">The number of steps (vertices) for the ellipse. If zero, uses default.</param>
    /// <returns>A polygon representing the ellipse.</returns>
    public static Polygon CreateEllipse(Vector2 center, float radiusX, float radiusY = 0f, int steps = 0) { return Clipper.Ellipse(center.ToClipperPoint(), radiusX, radiusY, steps).ToPolygon(); }
    #endregion
    
    #region Minkowski
    /// <summary>
    /// Computes the Minkowski difference between two polygons.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon.</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiDiff(this Polygon poly, Polygon path, bool isClosed = false)
    {
        return Clipper.MinkowskiDiff(poly.ToClipperPath(), path.ToClipperPath(), isClosed);
    }

    /// <summary>
    /// Computes the Minkowski sum of two polygons.
    /// </summary>
    /// <param name="poly">The base polygon.</param>
    /// <param name="path">The path polygon.</param>
    /// <param name="isClosed">Whether the path is closed.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD MinkowskiSum(this Polygon poly, Polygon path, bool isClosed = false)
    {
        return Clipper.MinkowskiSum(poly.ToClipperPath(), path.ToClipperPath(), isClosed);
    }

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
    #endregion
    
    #region Struct Conversion
    /// <summary>
    /// Converts a <see cref="PointD"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="p">The point to convert.</param>
    /// <returns>The converted <see cref="Vector2"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static Vector2 ToVec2(this PointD p)
    {
        //flip of y necessary -> clipper up y is positve - raylib is negative
        return new((float)p.x, -(float)p.y);
    }

    /// <summary>
    /// Converts a <see cref="Vector2"/> to a <see cref="PointD"/> for Clipper.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <returns>The converted <see cref="PointD"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static PointD ToClipperPoint(this Vector2 v)
    {
        return new(v.X, -v.Y);
    }

    /// <summary>
    /// Converts a <see cref="Rect"/> to a <see cref="RectD"/> for Clipper.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted <see cref="RectD"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static RectD ToClipperRect(this Rect r)
    {
        return new RectD(r.X, -r.Y-r.Height, r.X + r.Width, -r.Y);
    }

    /// <summary>
    /// Converts a <see cref="RectD"/> to a <see cref="Rect"/>.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted <see cref="Rect"/>.</returns>
    /// <remarks>Y is flipped to match coordinate systems.</remarks>
    public static Rect ToRect(this RectD r)
    {
        return new Rect((float)r.left, (float)(-r.top-r.Height), (float)r.Width, (float)r.Height);
    }
    #endregion
    
    #region Enum Conversion
    /// <summary>
    /// Converts a <see cref="ShapeClipperFillRule"/> to the Clipper <see cref="FillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The ShapeClipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="FillRule"/> value.</returns>
    public static FillRule ToFillRule(this ShapeClipperFillRule fillRule)
    {
        return (FillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperJoinType"/> to the Clipper <see cref="JoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The ShapeClipper join type to convert.</param>
    /// <returns>The equivalent <see cref="JoinType"/> value.</returns>
    public static JoinType ToJoinType(this ShapeClipperJoinType joinType)
    {
        return (JoinType)joinType;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperEndType"/> to the Clipper <see cref="EndType"/> enum.
    /// </summary>
    /// <param name="endType">The ShapeClipper end type to convert.</param>
    /// <returns>The equivalent <see cref="EndType"/> value.</returns>
    public static EndType ToEndType(this ShapeClipperEndType endType)
    {
        return (EndType)endType;
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
    #endregion
    
    #region Class Conversion
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
    public static PathsD ToClipperPaths(this Segment segment){ return [segment.ToClipperPath()]; }

    /// <summary>
    /// Converts a <see cref="Polygon"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="poly">The polygon to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this Polygon poly) { return [poly.ToClipperPath()]; }

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
    public static PathsD ToClipperPaths(this Polyline polyline) { return [polyline.ToClipperPath()]; }

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
    
    #endregion
}
