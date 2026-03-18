using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;

namespace ShapeEngine.StaticLib;

//TODO: Move to seperate namespace/folder
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

//TODO: Move to seperate namespace/folder
/// <summary>
/// The ShapeClipperEndType enumerator controls how the ends of paths are handled when performing
/// offset (inflating/shrinking) operations. This enumeration is only required for
/// offset operations and is not used for polygon clipping.
/// </summary>
/// <remarks>
/// With both ShapeClipperEndType.Polygon and ShapeClipperEndType.Joined, path closure will occur regardless
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

//TODO: Move to seperate namespace/folder
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

//TODO: Move to seperate namespace/folder
//TODO: See ClipperImmediate2D todos -> Reimplement everything here there

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
    public static PathsD UnionMany(this Polygon a, Polygons other, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero)
    {
        return Clipper.Union(a.ToClipperPaths(), other.ToClipperPaths(), fillRule.ToClipperFillRule());
    }

    /// <summary>
    /// Computes the union of two polygons.
    /// </summary>
    /// <param name="a">The first polygon.</param>
    /// <param name="b">The second polygon.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <returns>The unioned paths as <see cref="PathsD"/>.</returns>
    public static PathsD Union(this Polygon a, Polygon b, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero)
    {
        return Clipper.Union(a.ToClipperPaths(), b.ToClipperPaths(), fillRule.ToClipperFillRule());
    }
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
    public static PathsD Intersect(this Polygon subject, Polygon clip, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        return Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
    }

    /// <summary>
    /// Computes the intersection of a polygon with multiple subject polygons.
    /// </summary>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="subjects">The collection of subject polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The intersected paths as <see cref="PathsD"/>.</returns>
    public static PathsD Intersect(this Polygon clip, Polygons subjects, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var result = new PathsD();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var subject in subjects)
        {
            result.AddRange(Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), clipperFillRule, precision));
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
    public static PathsD IntersectMany(this Polygon subject, Polygons clips, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var clip in clips)
        {
            cur = Clipper.Intersect(cur, clip.ToClipperPaths(), clipperFillRule, precision);
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
    public static PathsD Difference(this Polygon subject, Polygon clip, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
    }
    
    /// <summary>
    /// Computes the difference between multiple subject polygons and a clip polygon.
    /// </summary>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="subjects">The collection of subject polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon clip, Polygons subjects, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var result = new PathsD();
        var clipPaths = clip.ToClipperPaths();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var subject in subjects)
        {
            result.AddRange(Clipper.Difference(subject.ToClipperPaths(), clipPaths, clipperFillRule, precision));
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
    public static PathsD Difference(this Polygon subject, Polyline polyline, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(subject.ToClipperPaths(), polyline.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
    }
    
    /// <summary>
    /// Computes the difference between a subject polygon and a segment.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="segment">The segment to subtract.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD Difference(this Polygon subject, Segment segment, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        return Clipper.Difference(subject.ToClipperPaths(), segment.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
    }
    
    /// <summary>
    /// Computes the difference between a subject polygon and multiple clip polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clips">The collection of clip polygons.</param>
    /// <param name="fillRule">The fill rule to use.</param>
    /// <param name="precision">The decimal precision for the operation.</param>
    /// <returns>The resulting paths as <see cref="PathsD"/>.</returns>
    public static PathsD DifferenceMany(this Polygon subject, Polygons clips, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var clip in clips)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), clipperFillRule, precision);
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
    public static PathsD DifferenceMany(this Polygon subject, Polylines polylines, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var clip in polylines)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
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
    public static PathsD DifferenceMany(this Polygon subject, Segments segments, ShapeClipperFillRule fillRule = ShapeClipperFillRule.NonZero, int precision = 2)
    {
        var cur = subject.ToClipperPaths();
        var clipperFillRule = fillRule.ToClipperFillRule();
        foreach (var clip in segments)
        {
            cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule.ToClipperFillRule(), precision);
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
        //!!!: because y is flipped PathD is in clipper orientation -> positive winding order = ccw and negative winding order = cw, where negative/cw is defined as a hole
        // if y is not flipped than the orientation flips, meaning that now a positive winding order is cw and considered a hole!
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
    /// <item> ShapeClipperJoinType.Miter: Edges are first offset a specified distance away from and parallel to their original (ie starting) edge positions.
    /// These offset edges are then extended to points where they intersect with adjacent edge offsets.
    /// However a limit must be imposed on how far mitered vertices can be from their original positions to avoid very convex joins producing unreasonably long and narrow spikes).
    /// To avoid unsightly spikes, joins will be 'squared' wherever distances between original vertices
    /// and their calculated offsets exceeds a specified value (expressed as a ratio relative to the offset distance).</item>
    /// <item> ShapeClipperJoinType.Square: Convex joins will be truncated using a 'squaring' edge.
    /// And the mid-points of these squaring edges will be exactly the offset distance away from their original (or starting) vertices. </item>
    /// <item> ShapeClipperJoinType.Bevel: Bevelled joins are similar to 'squared' joins except that squaring won't occur at a fixed distance.
    /// While bevelled joins may not be as pretty as squared joins, bevelling is much easier (ie faster) than squaring. </item>
    /// <item> ShapeClipperJoinType.Round: Rounding is applied to all convex joins with the arc radius being the offset distance, and the original join vertex the arc center. </item>
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
    /// Miter limit applied when <see cref="ShapeClipperJoinType.Miter"/> is used.
    /// This property sets the maximum distance in multiples of delta that vertices can be offset from their original positions before squaring is applied.
    /// (Squaring truncates a miter by 'cutting it off' at 1 × delta distance from the original vertex.)
    /// The default value for MiterLimit is 2 (ie twice delta). This is also the smallest MiterLimit that's allowed.
    /// If mitering was unrestricted (ie without any squaring), then offsets at very acute angles would generate unacceptably long 'spikes'.
    /// </param>
    /// <param name="precision">Decimal precision for the operation.</param>
    /// <param name="arcTolerance">
    /// Arc approximation tolerance used when rounding joins or ends.
    /// Ignored when neither <see cref="ShapeClipperJoinType.Round"/> nor <see cref="ShapeClipperEndType.Round"/> is used.
    /// ArcTolerance is only relevant when offsetting with ShapeClipperJoinType.Round and / or ShapeClipperEndType.Round (see ClipperOffset.AddPath and ClipperOffset.AddPaths).
    /// The Clipper2 library approximates arcs by using series of relatively short straight line segments (see Trigonometry).
    /// And logically, shorter line segments will produce better arc approximations. But very short segments can degrade performance, usually with little or no discernable improvement in curve quality.
    /// Very short segments can even detract from curve quality, due to the effects of integer rounding.
    /// Arc tolerance is user defined since there isn't an optimal number of line segments for any given arc radius (ie that perfectly balances curve approximation with performance).
    /// Nevertheless, when the user doesn't define an arc tolerance and uses the default value (0.0), a 'default' arc tolerance is calculated (see below) that generally produces visually smooth arc approximations,
    /// while avoiding excessively small segment lengths.
    /// The default ArcTolerance is: offset_radius / 500.
    /// </param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths.</returns>
    public static PathsD Inflate(this Polyline polyline, float delta, ShapeClipperJoinType joinType = ShapeClipperJoinType.Square, ShapeClipperEndType endType = ShapeClipperEndType.Square, float miterLimit = 2f, int precision = 2, double arcTolerance = 0.0)
    {
        if (joinType != ShapeClipperJoinType.Round && endType != ShapeClipperEndType.Round) arcTolerance = 0.0f;
        return Clipper.InflatePaths(polyline.ToClipperPaths(), delta, joinType.ToClipperJoinType(), endType.ToClipperEndType(), miterLimit, precision, arcTolerance);
    }

    /// <summary>
    /// Inflates (offsets) a polygon by the specified delta.
    /// </summary>
    /// <param name="poly">The polygon to offset.</param>
    /// <param name="delta">Offset distance. Positive values expand outward; negative values contract inward.</param>
    /// <param name="joinType">Type of corner joins to use.
    /// <list type="bullet">
    /// <item> ShapeClipperJoinType.Miter: Edges are first offset a specified distance away from and parallel to their original (ie starting) edge positions.
    /// These offset edges are then extended to points where they intersect with adjacent edge offsets.
    /// However a limit must be imposed on how far mitered vertices can be from their original positions to avoid very convex joins producing unreasonably long and narrow spikes).
    /// To avoid unsightly spikes, joins will be 'squared' wherever distances between original vertices
    /// and their calculated offsets exceeds a specified value (expressed as a ratio relative to the offset distance).</item>
    /// <item> ShapeClipperJoinType.Square: Convex joins will be truncated using a 'squaring' edge.
    /// And the mid-points of these squaring edges will be exactly the offset distance away from their original (or starting) vertices. </item>
    /// <item> ShapeClipperJoinType.Bevel: Bevelled joins are similar to 'squared' joins except that squaring won't occur at a fixed distance.
    /// While bevelled joins may not be as pretty as squared joins, bevelling is much easier (ie faster) than squaring. </item>
    /// <item> ShapeClipperJoinType.Round: Rounding is applied to all convex joins with the arc radius being the offset distance, and the original join vertex the arc center. </item>
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
    /// Miter limit applied when <see cref="ShapeClipperJoinType.Miter"/> is used.
    /// This property sets the maximum distance in multiples of delta that vertices can be offset from their original positions before squaring is applied.
    /// (Squaring truncates a miter by 'cutting it off' at 1 × delta distance from the original vertex.)
    /// The default value for MiterLimit is 2 (ie twice delta). This is also the smallest MiterLimit that's allowed.
    /// If mitering was unrestricted (ie without any squaring), then offsets at very acute angles would generate unacceptably long 'spikes'.
    /// </param>
    /// <param name="precision">Decimal precision for the operation.</param>
    /// <param name="arcTolerance">
    /// Arc approximation tolerance used when rounding joins or ends.
    /// Ignored when neither <see cref="ShapeClipperJoinType.Round"/> nor <see cref="ShapeClipperEndType.Round"/> is used.
    /// ArcTolerance is only relevant when offsetting with ShapeClipperJoinType.Round and / or ShapeClipperEndType.Round (see ClipperOffset.AddPath and ClipperOffset.AddPaths).
    /// The Clipper2 library approximates arcs by using series of relatively short straight line segments (see Trigonometry).
    /// And logically, shorter line segments will produce better arc approximations. But very short segments can degrade performance, usually with little or no discernable improvement in curve quality.
    /// Very short segments can even detract from curve quality, due to the effects of integer rounding.
    /// Arc tolerance is user defined since there isn't an optimal number of line segments for any given arc radius (ie that perfectly balances curve approximation with performance).
    /// Nevertheless, when the user doesn't define an arc tolerance and uses the default value (0.0), a 'default' arc tolerance is calculated (see below) that generally produces visually smooth arc approximations,
    /// while avoiding excessively small segment lengths.
    /// The default ArcTolerance is: offset_radius / 500.
    /// </param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths.</returns>
    public static PathsD Inflate(this Polygon poly, float delta, ShapeClipperJoinType joinType = ShapeClipperJoinType.Square, ShapeClipperEndType endType = ShapeClipperEndType.Polygon, float miterLimit = 2f, int precision = 2, double arcTolerance = 0.0)
    {
        if (joinType != ShapeClipperJoinType.Round && endType != ShapeClipperEndType.Round) arcTolerance = 0.0f;
        return Clipper.InflatePaths(poly.ToClipperPaths(), delta, joinType.ToClipperJoinType(), endType.ToClipperEndType(), miterLimit, precision, arcTolerance);
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
    public static PathsD InflateMany(this Polygons polygons, float delta, ShapeClipperJoinType joinType = ShapeClipperJoinType.Square, ShapeClipperEndType endType = ShapeClipperEndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polygons.ToClipperPaths(), delta, joinType.ToClipperJoinType(), endType.ToClipperEndType(), miterLimit, precision);
    }
    /// <summary>
    /// Inflates (offsets) a collection of polylines by the specified delta.
    /// </summary>
    /// <param name="polylines">The collection of polylines to offset.</param>
    /// <param name="delta">Offset distance. Positive values expand outward; negative values contract inward.</param>
    /// <param name="joinType">Type of corner joins to use (defaults to <see cref="ShapeClipperJoinType.Square"/>).</param>
    /// <param name="endType">How path ends are handled for the closed/open paths (defaults to <see cref="ShapeClipperEndType.Polygon"/>).</param>
    /// <param name="miterLimit">Miter limit applied when <see cref="ShapeClipperJoinType.Miter"/> is used (defaults to 2f).</param>
    /// <param name="precision">Decimal precision for the operation (defaults to 2).</param>
    /// <returns>A <see cref="PathsD"/> containing the offset paths for all input polylines.</returns>
    public static PathsD InflateMany(this Polylines polylines, float delta, ShapeClipperJoinType joinType = ShapeClipperJoinType.Square, ShapeClipperEndType endType = ShapeClipperEndType.Polygon, float miterLimit = 2f, int precision = 2)
    {
        return Clipper.InflatePaths(polylines.ToClipperPaths(), delta, joinType.ToClipperJoinType(), endType.ToClipperEndType(), miterLimit, precision);
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
    /// Converts a <see cref="Segment"/> to a <see cref="PathD"/>.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <returns>The converted <see cref="PathD"/>.</returns>
    public static PathD ToClipperPath(this Segment segment)
    {
        var path = new PathD
        {
            segment.Start.ToClipperPoint(),
            segment.End.ToClipperPoint()
        };
        return path;
    }

    /// <summary>
    /// Converts a <see cref="Segment"/> to <see cref="PathsD"/>.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <returns>The converted <see cref="PathsD"/>.</returns>
    public static PathsD ToClipperPaths(this Segment segment)
    {
        var path = new PathD
        {
            segment.Start.ToClipperPoint(),
            segment.End.ToClipperPoint()
        };
        var paths = new PathsD { path };
        return paths;
    }
    
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
            if (removeHoles && path.IsHole()) continue;
            polygons.Add(path.ToPolygon());
        }
        return polygons;
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
            if (removeHoles && path.IsHole()) continue;
            polylines.Add(path.ToPolyline());
        }
        return polylines;
    }
    
    
    /// <summary>
    /// Converts a <see cref="Points"/> collection to a Clipper <see cref="PathD"/>.
    /// </summary>
    /// <param name="points">The source collection of vertices to convert. Each vertex is transformed to a Clipper-compatible <see cref="PointD"/>.</param>
    /// <returns>A new <see cref="PathD"/> containing the converted points.</returns>
    public static PathD ToClipperPath(this Points points)
    {
        var path = new PathD();
        foreach (var vertex in points)
        {
            path.Add(vertex.ToClipperPoint());
        }
        return path;
    }

    /// <summary>
    /// Converts a <see cref="Points"/> collection to a Clipper <see cref="PathsD"/>.
    /// </summary>
    /// <param name="points">The source collection of vertices to convert.</param>
    /// <returns>
    /// A new <see cref="PathsD"/> containing a single <see cref="PathD"/> produced from <paramref name="points"/>.
    /// </returns>
    public static PathsD ToClipperPaths(this Points points)
    {
        var path = points.ToClipperPath();
        var paths = new PathsD { path };
        return paths;
    }

    /// <summary>
    /// Converts an array of <see cref="Points"/> to a Clipper <see cref="PathsD"/>.
    /// This is a convenience overload that forwards to the enumerable-based
    /// <see cref="ToClipperPaths(IEnumerable{Points})"/> extension.
    /// </summary>
    /// <param name="pointsArray">Array of point collections to convert.</param>
    /// <returns>A new <see cref="PathsD"/> containing a path for each input <see cref="Points"/>.</returns>
    public static PathsD ToClipperPaths(params Points[] pointsArray)
    {
        return pointsArray.ToClipperPaths();
    }
    
    /// <summary>
    /// Converts an enumerable of <see cref="Points"/> collections into a Clipper <see cref="PathsD"/>.
    /// Each <see cref="Points"/> instance is converted to a single <see cref="PathD"/> and added
    /// to the returned <see cref="PathsD"/> in the same order as the input enumeration.
    /// </summary>
    /// <param name="pointsEnumerable">An enumerable of <see cref="Points"/> to convert. Must not be null.</param>
    /// <returns>
    /// A new <see cref="PathsD"/> containing one <see cref="PathD"/> per input <see cref="Points"/>.
    /// </returns>
    /// <remarks>
    /// Enumerating a null <paramref name="pointsEnumerable"/> will result in an exception.
    /// This method does not remove holes or alter individual paths; use other helpers if post-processing is needed.
    /// </remarks>
    public static PathsD ToClipperPaths(this IEnumerable<Points> pointsEnumerable)
    {
        var result = new PathsD();
        foreach(var polygon in pointsEnumerable)
        {
            result.Add(polygon.ToClipperPath());
        }
        return result;
    }

    
    #endregion
    
    #region Class Conversion Ref
    
    /// <summary>
    /// Converts a <see cref="Segment"/> into a Clipper <see cref="PathD"/> by writing the segment's start and end points
    /// into the provided <paramref name="path"/>. The supplied <paramref name="path"/> is cleared before use so no new
    /// allocation is required when reusing an existing <see cref="PathD"/> instance.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <param name="path">Reference to the <see cref="PathD"/> that will be cleared and populated with the segment's points.</param>
    /// <returns>The number of points written to <paramref name="path"/> (expected to be 2 for a valid segment, or 0 if none were added).</returns>
    public static int ToClipperPath(this Segment segment, ref PathD path)
    {
        path.Clear();
        path.Add(segment.Start.ToClipperPoint());
        path.Add(segment.End.ToClipperPoint());
        return 2;
    }
    /// <summary>
    /// Converts a <see cref="Segment"/> into a Clipper <see cref="PathsD"/> by writing the segment's
    /// start and end points into the provided <paramref name="paths"/> collection.
    /// </summary>
    /// <param name="segment">The segment to convert.</param>
    /// <param name="paths">
    /// Reference to the <see cref="PathsD"/> that will be cleared and populated with a single <see cref="PathD"/>
    /// representing the segment. After the call, <paramref name="paths"/> will contain exactly one path.
    /// </param>
    /// <returns>
    /// The number of points written to the produced path (expected to be 2 for a valid segment).
    /// </returns>
    /// <remarks>
    /// This method attempts to reuse the first <see cref="PathD"/> in <paramref name="paths"/> when present:
    /// it clears that path, clears the collection, writes the segment's points into the reused path,
    /// and then adds it back to <paramref name="paths"/>. If <paramref name="paths"/> is initially empty,
    /// the method falls back to creating and adding a new path via the non-ref overload.
    /// </remarks>
    public static int ToClipperPaths(this Segment segment, ref PathsD paths)
    {
        if (paths.Count <= 0)
        {
            paths.Add(segment.ToClipperPath());
            return 2;
        }
        
        var path = paths[0];
        path.Clear();
        paths.Clear();
        int verticesAdded = segment.ToClipperPath(ref path);
        paths.Add(path);
        return verticesAdded;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="PathD"/> into the provided <see cref="Polygon"/> instance.
    /// The target <paramref name="polygon"/> is cleared and populated with converted points.
    /// </summary>
    /// <param name="path">Source Clipper path to convert.</param>
    /// <param name="polygon">Reference to the destination <see cref="Polygon"/> which will be cleared and filled.</param>
    /// <returns>The number of vertices written to <paramref name="polygon"/> (0 if <paramref name="path"/> is empty).</returns>
    public static int ToPolygon(this PathD path, ref Polygon polygon)
    {
        if(path.Count <= 0) return 0;
        
        var count = 0;
        polygon.Clear();
        
        foreach (var point in path)
        {
            polygon.Add(point.ToVec2());
            count++;
        }
        return count;
    }
    /// <summary>
    /// Converts a Clipper <see cref="PathsD"/> into the supplied <see cref="Polygons"/> instance.
    /// The destination <paramref name="polygons"/> is cleared and populated with one <see cref="Polygon"/>
    /// per path in <paramref name="paths"/>.
    /// </summary>
    /// <param name="paths">Source Clipper paths to convert.</param>
    /// <param name="polygons">Reference to the destination <see cref="Polygons"/> which will be cleared and filled.</param>
    /// <param name="removeHoles">If true, paths identified as holes (negative orientation) are skipped.</param>
    /// <returns>The number of polygons written to <paramref name="polygons"/>.</returns>
    /// <remarks>
    /// This method attempts to reuse existing <see cref="Polygon"/> instances in <paramref name="polygons"/>
    /// when possible, clearing and repopulating them with converted points from <paramref name="paths"/>.
    /// If there are more paths than existing polygons, new <see cref="Polygon"/> instances are created and added.
    /// After processing, any excess polygons in <paramref name="polygons"/> beyond the number of converted paths are removed.
    /// </remarks>
    public static int ToPolygons(this PathsD paths, ref Polygons polygons, bool removeHoles = false)
    {
        if (paths.Count <= 0) return 0;
        var outIndex = 0;

        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            if (path.Count <= 0 || (removeHoles && path.IsHole())) continue;

            if (polygons.Count > outIndex)
            {
                var poly = polygons[outIndex];
                poly.Clear();
                path.ToPolygon(ref poly);
            }
            else
            {
                var poly = path.ToPolygon();
                polygons.Add(poly);
            }

            outIndex++;
        }

        if (outIndex <= 0) return 0;
        if (outIndex >= polygons.Count) return outIndex;

        int remove = polygons.Count - outIndex;
        if (remove > 0) polygons.RemoveRange(outIndex, remove);

        return outIndex;
    }
    
    /// <summary>
    /// Converts the provided Clipper <see cref="PathD"/> into the given <see cref="Polyline"/> instance.
    /// The destination <paramref name="polyline"/> is cleared before being populated with converted points.
    /// </summary>
    /// <param name="path">Source Clipper path to convert.</param>
    /// <param name="polyline">Reference to the destination <see cref="Polyline"/> which will be cleared and filled.</param>
    /// <returns>The number of vertices written to <paramref name="polyline"/> (0 if <paramref name="path"/> is empty).</returns>
    public static int ToPolyline(this PathD path, ref Polyline polyline)
    {
        if(path.Count <= 0) return 0;
        
        var count = 0;
        polyline.Clear();
        
        foreach (var point in path)
        {
            polyline.Add(point.ToVec2());
            count++;
        }
        return count;
    }
    
    /// <summary>
    /// Converts the provided Clipper <see cref="PathsD"/> into the supplied <see cref="Polylines"/> instance.
    /// The destination <paramref name="polylines"/> is cleared and populated with one <see cref="Polyline"/> per path in <paramref name="paths"/>.
    /// This method attempts to reuse existing <see cref="Polyline"/> instances when possible.
    /// </summary>
    /// <param name="paths">Source Clipper paths to convert.</param>
    /// <param name="polylines">Reference to the destination <see cref="Polylines"/> which will be cleared and filled.</param>
    /// <param name="removeHoles">If true, paths identified as holes (negative orientation) are skipped.</param>
    /// <returns>The number of polylines written to <paramref name="polylines"/>.</returns>
    /// <remarks>
    /// This method attempts to reuse existing <see cref="Polyline"/> instances in <paramref name="polylines"/>
    /// when possible, clearing and repopulating them with converted points from <paramref name="paths"/>.
    /// If there are more paths than existing polylines, new <see cref="Polyline"/> instances are created and added.
    /// After processing, any excess polylines in <paramref name="polylines"/> beyond the number of converted paths are removed.
    /// </remarks>
    public static int ToPolylines(this PathsD paths, ref Polylines polylines, bool removeHoles = false)
    {
        if (paths.Count <= 0) return 0;
        var outIndex = 0;

        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            if (path.Count <= 0 || (removeHoles && path.IsHole())) continue;

            if (polylines.Count > outIndex)
            {
                var poly = polylines[outIndex];
                poly.Clear();
                path.ToPolyline(ref poly);
            }
            else
            {
                var poly = path.ToPolyline();
                polylines.Add(poly);
            }

            outIndex++;
        }

        if (outIndex <= 0) return 0;
        if (outIndex >= polylines.Count) return outIndex;

        int remove = polylines.Count - outIndex;
        if (remove > 0) polylines.RemoveRange(outIndex, remove);

        return outIndex;
    }
    
    /// <summary>
    /// Writes the vertices from a <see cref="Points"/> collection into the supplied Clipper <see cref="PathD"/>.
    /// The destination <paramref name="path"/> is cleared before use so the caller can reuse an existing
    /// <see cref="PathD"/> instance and avoid allocations.
    /// </summary>
    /// <param name="points">Source collection of vertices. Each vertex is converted using the project's <c>ToClipperPoint</c> conversion.</param>
    /// <param name="path">Reference to the destination <see cref="PathD"/> which will be cleared and populated.</param>
    /// <returns>The number of points written to <paramref name="path"/>.</returns>
    public static int ToClipperPath(this Points points, ref PathD path)
    {
        if(points.Count <= 0) return 0;
        path.Clear();
        var count = 0;
        foreach (var vertex in points)
        {
            path.Add(vertex.ToClipperPoint());
            count++;
        }
        return count;
    }
    /// <summary>
    /// Writes the vertices from a <see cref="Points"/> collection into the supplied Clipper <see cref="PathsD"/>.
    /// The destination <paramref name="paths"/> is cleared and populated with a single <see cref="PathD"/>
    /// representing the input <paramref name="points"/>. This overload is intended for reuse scenarios to avoid
    /// unnecessary allocations by reusing the provided <paramref name="paths"/> instance.
    /// </summary>
    /// <param name="points">Source collection of vertices to convert.</param>
    /// <param name="paths">Reference to the destination <see cref="PathsD"/> which will be cleared and filled.</param>
    /// <returns>The number of points written to the produced path (0 if <paramref name="points"/> is empty).</returns>
    public static int ToClipperPaths(this Points points, ref PathsD paths)
    {
        if(points.Count <= 0) return 0;
        
        if (paths.Count <= 0)
        {
            var newPath = new PathD();
            int count = points.ToClipperPath(ref newPath);
            paths.Add(newPath);
            return count;
        }
        
        var path = paths[0];
        path.Clear();
        paths.Clear();
        
        int verticesAdded = points.ToClipperPath(ref path);
        paths.Add(path);
        return verticesAdded;
    }
    /// <summary>
    /// Writes multiple <see cref="Points"/> collections into the supplied Clipper <see cref="PathsD"/> instance.
    /// The destination <paramref name="paths"/> is cleared and populated with one <see cref="PathD"/> per provided
    /// <see cref="Points"/>. This overload accepts a params array for convenience and forwards to the enumerable-based
    /// <see cref="ToClipperPaths(IEnumerable{Points})"/> implementation.
    /// </summary>
    /// <param name="paths">Reference to the destination <see cref="PathsD"/> which will be cleared and filled.</param>
    /// <param name="pointsArray">An array of <see cref="Points"/> collections to convert. May be empty but must not be null.</param>
    /// <returns>The total number of points written to the produced paths (sum of points across all input collections).</returns>
    public static int ToClipperPaths(ref PathsD paths, params Points[] pointsArray)
    {
        if (pointsArray.Length <= 0) return 0;
        var outIndex = 0;

        for (var i = 0; i < pointsArray.Length; i++)
        {
            var points = pointsArray[i];
            if (points.Count <= 0) continue;

            if (paths.Count > outIndex)
            {
                var path = paths[outIndex];
                path.Clear();
                points.ToClipperPath(ref path);
            }
            else
            {
                var path = points.ToClipperPath();
                paths.Add(path);
            }

            outIndex++;
        }

        if (outIndex <= 0) return 0;
        if (outIndex >= paths.Count) return outIndex;

        int remove = paths.Count - outIndex;
        if (remove > 0) paths.RemoveRange(outIndex, remove);

        return outIndex;
    }
    /// <summary>
    /// Writes multiple <see cref="Points"/> collections into the supplied Clipper <see cref="PathsD"/> instance.
    /// The destination <paramref name="paths"/> is cleared and populated with one <see cref="PathD"/> per provided
    /// <see cref="Points"/>. This method attempts to reuse existing <see cref="PathD"/> instances when possible
    /// to avoid unnecessary allocations.
    /// </summary>
    /// <param name="pointsEnumerable">An enumerable of <see cref="Points"/> to convert. Must not be null.</param>
    /// <param name="paths">Reference to the destination <see cref="PathsD"/> which will be cleared and filled.</param>
    /// <returns>
    /// The total number of points written to the produced paths (sum of points across all input collections).
    /// Returns 0 if no points were written.
    /// </returns>
    public static int ToClipperPaths(this IEnumerable<Points> pointsEnumerable, ref PathsD paths)
    {
        var outIndex = 0;
        foreach (var points in pointsEnumerable)
        {
            if (points.Count <= 0) continue;

            if (paths.Count > outIndex)
            {
                var path = paths[outIndex];
                path.Clear();
                points.ToClipperPath(ref path);
            }
            else
            {
                var path = points.ToClipperPath();
                paths.Add(path);
            }

            outIndex++;
        }

        if (outIndex <= 0) return 0;
        if (outIndex >= paths.Count) return outIndex;

        int remove = paths.Count - outIndex;
        if (remove > 0) paths.RemoveRange(outIndex, remove);

        return outIndex;
    }
    
    #endregion
}
