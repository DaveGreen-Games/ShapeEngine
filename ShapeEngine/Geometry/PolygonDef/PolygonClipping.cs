using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    
    /// <summary>
    /// Unions this polygon with another polygon and replaces the current shape with the result.
    /// </summary>
    /// <param name="b">The polygon to union with.</param>
    /// <param name="fillRule">The fill rule to use for the union operation.</param>
    /// <remarks>
    /// Uses the Clipper library for polygon union. The result replaces the current polygon if successful.
    /// </remarks>
    public void UnionShapeSelf(Polygon b, FillRule fillRule = FillRule.NonZero)
    {
        var result = Clipper.Union(this.ToClipperPaths(), b.ToClipperPaths(), fillRule);
        if (result.Count > 0)
        {
            this.Clear();
            foreach (var p in result[0])
            {
                this.Add(p.ToVec2());
            }
        }
    }

    /// <summary>
    /// Attempts to merge this polygon with another if their closest points are within a given distance threshold.
    /// </summary>
    /// <param name="other">The polygon to merge with.</param>
    /// <param name="distanceThreshold">The maximum allowed distance between closest points to perform a merge.</param>
    /// <returns>True if a merge was performed; otherwise, false.</returns>
    /// <remarks>
    /// Merges by generating a fill shape between the closest points and performing a union.
    /// </remarks>
    public bool MergeShapeSelf(Polygon other, float distanceThreshold)
    {
        if (distanceThreshold <= 0f) return false;
        
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            if (fillShape == null) return false;
            UnionShapeSelf(fillShape);
            UnionShapeSelf(other);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Attempts to merge this polygon with another if they overlap.
    /// </summary>
    /// <param name="other">The polygon to merge with.</param>
    /// <returns>True if a merge was performed; otherwise, false.</returns>
    public bool MergeShapeSelf(Polygon other)
    {
        var overlap = OverlapShape(other);
        if (overlap)
        {
            UnionShapeSelf(other);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Subtracts the specified polygon (`cutShape`) from this polygon.
    /// Optionally keeps only the intersected (cut-out) region if `keepCutout` is true.
    /// Returns true if the operation resulted in a valid polygon; otherwise, false.
    /// </summary>
    /// <param name="cutShape">The polygon to subtract from this polygon.</param>
    /// <param name="keepCutout">If true, keeps only the intersected region; otherwise, subtracts the cutShape.</param>
    /// <returns>True if the difference operation produced a valid polygon; otherwise, false.</returns>
    public bool CutShapeSelf(Polygon cutShape, bool keepCutout = false)
    {
        var newShapes = keepCutout ? this.Intersect(cutShape) : this.Difference(cutShape);
        
        if (newShapes.Count > 0)
        {
            var polygons = newShapes.ToPolygons(true);
            if (polygons.Count > 0)
            {
                foreach (var polygon in polygons)
                {
                    if(polygon.Count < 3) continue; // Skip invalid polygons
                    this.Clear();

                    if (polygon.IsClockwise())
                    {
                        for (int i = polygon.Count - 1; i >= 0; i--)//reverse order clockwise to counter-clockwise
                        {
                            Add(polygon[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < polygon.Count; i++)
                        {
                            Add(polygon[i]);
                        }
                    }
                    
                    return true;
                }
            }
        }

        return false;
    }
   
    
    /// <summary>
    /// Attempts to merge this polygon with another and returns the merged result as a new polygon.
    /// </summary>
    /// <param name="other">The polygon to merge with.</param>
    /// <param name="distanceThreshold">The maximum allowed distance between closest points to perform a merge.</param>
    /// <returns>The merged polygon if successful; otherwise, null.</returns>
    public Polygon? MergeShape(Polygon other, float distanceThreshold)
    {
        if(distanceThreshold <= 0f) return null;
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            if(fillShape == null) return null;
            var clip =  this.Union(fillShape);
            if (clip.Count > 0)
            {
                clip = clip[0].ToPolygon().Union(other);
                if (clip.Count > 0) return clip[0].ToPolygon();
            }
        }

        return null;
    }

    /// <summary>
    /// Cuts the current polygon with another polygon, returning the resulting shapes and the cut-out regions.
    /// </summary>
    /// <param name="cutShape">The polygon to cut with.</param>
    /// <returns>A tuple containing the new shapes and the cut-out regions.</returns>
    public (Polygons newShapes, Polygons cutOuts) CutShape(Polygon cutShape)
    {
        var cutOuts = this.Intersect(cutShape).ToPolygons(true);
        var newShapes = this.Difference(cutShape).ToPolygons(true);

        return (newShapes, cutOuts);
    }

    /// <summary>
    /// Cuts the current polygon with multiple polygons, returning the resulting shapes and the cut-out regions.
    /// </summary>
    /// <param name="cutShapes">The polygons to cut with.</param>
    /// <returns>A tuple containing the new shapes and the cut-out regions.</returns>
    public (Polygons newShapes, Polygons cutOuts) CutShapeMany(Polygons cutShapes)
    {
        var cutOuts = this.IntersectMany(cutShapes).ToPolygons(true);
        var newShapes = this.DifferenceMany(cutShapes).ToPolygons(true);
        return (newShapes, cutOuts);
    }

    /// <summary>
    /// Combines this polygon with another, returning the union and the overlapping regions.
    /// </summary>
    /// <param name="other">The polygon to combine with.</param>
    /// <returns>A tuple containing the new shapes and the overlapping regions.</returns>
    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygon other)
    {
        var overlaps = this.Intersect(other).ToPolygons(true);
        var newShapes = this.Union(other).ToPolygons(true);
        return (newShapes, overlaps);
    }

    /// <summary>
    /// Combines this polygon with multiple polygons, returning the union and the overlapping regions.
    /// </summary>
    /// <param name="others">The polygons to combine with.</param>
    /// <returns>A tuple containing the new shapes and the overlapping regions.</returns>
    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygons others)
    {
        var overlaps = this.IntersectMany(others).ToPolygons(true);
        var newShapes = this.UnionMany(others).ToPolygons(true);
        return (newShapes, overlaps);
    }

    /// <summary>
    /// Cuts the current polygon with a simple generated shape at a position, returning the resulting shapes and the cut-out regions.
    /// </summary>
    /// <param name="cutPos">The center position for the cut shape.</param>
    /// <param name="minCutRadius">The minimum radius for the cut shape.</param>
    /// <param name="maxCutRadius">The maximum radius for the cut shape.</param>
    /// <param name="pointCount">The number of points for the generated cut shape. Default is 16.</param>
    /// <returns>A tuple containing the new shapes and the cut-out regions.</returns>
    public (Polygons newShapes, Polygons cutOuts)? CutShapeSimple(Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
    {
        var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
        return cut == null ? null : CutShape(cut);
    }

    /// <summary>
    /// Cuts the current polygon with a simple generated shape along a segment, returning the resulting shapes and the cut-out regions.
    /// </summary>
    /// <param name="cutLine">The segment along which to generate the cut shape.</param>
    /// <param name="minSectionLength">Minimum section length for the generated cut. Default is 0.025.</param>
    /// <param name="maxSectionLength">Maximum section length for the generated cut. Default is 0.1.</param>
    /// <param name="minMagnitude">Minimum magnitude for the cut. Default is 0.05.</param>
    /// <param name="maxMagnitude">Maximum magnitude for the cut. Default is 0.25.</param>
    /// <returns>A tuple containing the new shapes and the cut-out regions.</returns>
    public (Polygons newShapes, Polygons cutOuts)? CutShapeSimple(Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f,
        float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
    {
        var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
        if (cut == null) return null;
        return CutShape(cut);
    }

    /// <summary>
    /// Splits the polygon using a line defined by a point and direction.
    /// </summary>
    /// <param name="point">A point on the splitting line.</param>
    /// <param name="direction">The direction of the splitting line.</param>
    /// <returns>The resulting polygons after the split, or null if no split occurred.</returns>
    public Polygons? Split(Vector2 point, Vector2 direction)
    {
        var line = new Line(point, direction);
        return Split(line);
    }

    /// <summary>
    /// Splits the polygon using a line.
    /// </summary>
    /// <param name="line">The line to split with.</param>
    /// <returns>The resulting polygons after the split, or null if no split occurred.</returns>
    public Polygons? Split(Line line)
    {
        var w = Center - line.Point;
        var l = w.Length();
        if (l < Diameter * 2f) l = Diameter * 2f;
        else l *= 2f;

        var segment = line.ToSegment(l);
        return Split(segment);
    }

    /// <summary>
    /// Splits the polygon using a segment.
    /// </summary>
    /// <param name="segment">The segment to split with.</param>
    /// <returns>The resulting polygons after the split, or null if no split occurred.</returns>
    public Polygons? Split(Segment segment)
    {
        var result = this.Difference(segment);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }

    /// <summary>
    /// Splits the polygon using multiple segments.
    /// </summary>
    /// <param name="segments">The segments to split with.</param>
    /// <returns>The resulting polygons after the split, or null if no split occurred.</returns>
    public Polygons? Split(Segments segments)
    {
        var result = this.DifferenceMany(segments);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }

    /// <summary>
    /// Generates a fracture line in the specified direction and splits the polygon with it.
    /// </summary>
    /// <param name="dir">The direction for the fracture.</param>
    /// <param name="maxOffsetPercentage">Maximum offset for each point along the fracture line,
    /// as a percentage of the segment length. Value Range <c>0-1</c>.</param>
    /// <param name="fractureLineComplexity">Number of points to generate for the fracture line.</param>
    /// <returns>The resulting polygons after the split, or null if no split occurred.</returns>
    /// <remarks>
    /// The fracture line is generated with random offsets to simulate a jagged break.
    /// </remarks>
    public Polygons? FractureSplit(Vector2 dir, float maxOffsetPercentage, int fractureLineComplexity)
    {
        if (fractureLineComplexity < 1 || dir.LengthSquared() <= 0f) return null;

        var center = Center;
        var result = IntersectShape(new Line(center, dir));
        if (result == null || result.Count < 2) return null;

        var start = result[0].Point;
        var end = result[1].Point;
        if (result.Count > 2)
        {
            end = result.GetFurthestCollisionPoint(start).Point;
        }

        var fractureLine = GenerateFractureLine(start, end, maxOffsetPercentage, fractureLineComplexity);
        if (fractureLine == null) return null;
        return Split(fractureLine);
    }

    /// <summary>
    /// Generates a fracture line from start to end with random offsets.
    /// </summary>
    /// <param name="start">Start of the fracture line.</param>
    /// <param name="end">End of the fracture line.</param>
    /// <param name="maxOffsetPercentage">Maximum offset for each point, as a percentage of the segment length. Value Range <c>0-1</c>.</param>
    /// <param name="linePoints">Number of points to generate along the line.</param>
    /// <returns>The generated fracture line as a set of segments, or null if invalid parameters.</returns>
    /// <remarks>
    /// The resulting fracture line can be used to split the polygon with a jagged effect.
    /// </remarks>
    public Segments? GenerateFractureLine(Vector2 start, Vector2 end, float maxOffsetPercentage, int linePoints)
    {
        if (linePoints < 1) return null;
        if (maxOffsetPercentage < 0f) return [new Segment(start, end)];

        var w = end - start;
        var disSquared = w.LengthSquared();
        if (disSquared <= 0f) return null;

        var l = MathF.Sqrt(disSquared);
        var segmentLength = l / (linePoints + 1);
        var dir = w / l;
        var p = dir.GetPerpendicularLeft();
        var result = new Segments();

        var curStart = start;
        var curLinePoint = start;
        for (int i = 0; i < linePoints; i++)
        {
            var point = curStart + dir * segmentLength;
            curStart = point;

            var offsetLength = Rng.Instance.RandF(0f, segmentLength * maxOffsetPercentage);
            var offset = p * offsetLength;
            if (Rng.Instance.Chance(0.5f))
            {
                offset = -p * offsetLength;
            }


            var nextLinePoint = point + offset;

            var segment = new Segment(curLinePoint, nextLinePoint);
            curLinePoint = nextLinePoint;
            result.Add(segment);
        }

        result.Add(new Segment(curLinePoint, end));

        return result;
    }

}