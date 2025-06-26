using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
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

    public bool MergeShapeSelf(Polygon other, float distanceThreshold)
    {
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Polygon.Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            UnionShapeSelf(fillShape);
            UnionShapeSelf(other);
        }

        return false;
    }

    public Polygon? MergeShape(Polygon other, float distanceThreshold)
    {
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Polygon.Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            var clip = ShapeClipper.Union(this, fillShape);
            if (clip.Count > 0)
            {
                clip = ShapeClipper.Union(clip[0].ToPolygon(), other);
                if (clip.Count > 0) return clip[0].ToPolygon();
            }
        }

        return null;
    }

    public (Polygons newShapes, Polygons cutOuts) CutShape(Polygon cutShape)
    {
        var cutOuts = ShapeClipper.Intersect(this, cutShape).ToPolygons(true);
        var newShapes = ShapeClipper.Difference(this, cutShape).ToPolygons(true);

        return (newShapes, cutOuts);
    }

    public (Polygons newShapes, Polygons cutOuts) CutShapeMany(Polygons cutShapes)
    {
        var cutOuts = ShapeClipper.IntersectMany(this, cutShapes).ToPolygons(true);
        var newShapes = ShapeClipper.DifferenceMany(this, cutShapes).ToPolygons(true);
        return (newShapes, cutOuts);
    }

    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygon other)
    {
        var overlaps = ShapeClipper.Intersect(this, other).ToPolygons(true);
        var newShapes = ShapeClipper.Union(this, other).ToPolygons(true);
        return (newShapes, overlaps);
    }

    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygons others)
    {
        var overlaps = ShapeClipper.IntersectMany(this, others).ToPolygons(true);
        var newShapes = ShapeClipper.UnionMany(this, others).ToPolygons(true);
        return (newShapes, overlaps);
    }

    public (Polygons newShapes, Polygons cutOuts) CutShapeSimple(Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
    {
        var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
        return this.CutShape(cut);
    }

    public (Polygons newShapes, Polygons cutOuts) CutShapeSimple(SegmentDef.Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f,
        float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
    {
        var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
        return this.CutShape(cut);
    }

    public Polygons? Split(Vector2 point, Vector2 direction)
    {
        var line = new Line(point, direction);
        return Split(line);
    }

    public Polygons? Split(Line line)
    {
        var w = Center - line.Point;
        var l = w.Length();
        if (l < Diameter * 2f) l = Diameter * 2f;
        else l *= 2f;

        var segment = line.ToSegment(l);
        return Split(segment);
    }

    public Polygons? Split(SegmentDef.Segment segment)
    {
        var result = this.Difference(segment);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }

    public Polygons? Split(Segments segments)
    {
        var result = this.DifferenceMany(segments);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }

    /// <summary>
    /// Generates a facture line in the direction of dir and splits the polygon with it.
    /// </summary>
    /// <param name="dir">The direction for the fracture.</param>
    /// <param name="maxOffsetPercentage">Max distance each point can be offset along the fracture line.
    /// Value Range 0-1.
    /// Relative to segment length of the fracture line.</param>
    /// <param name="fractureLineComplexity">How many points should be generated for the fracture line.</param>
    /// <returns></returns>
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
    /// Generate a fracture line from start to end.
    /// </summary>
    /// <param name="start">Start of the fracture line.</param>
    /// <param name="end">End of the fracture line.</param>
    /// <param name="maxOffsetPercentage">How far each point can be offset from the main line.
    /// Value Range 0-1.
    /// Relative to the segment length.</param>
    /// <param name="linePoints">How many points should be generated. Final segment count = segmentPoints + 1.</param>
    /// <returns></returns>
    public Segments? GenerateFractureLine(Vector2 start, Vector2 end, float maxOffsetPercentage, int linePoints)
    {
        if (linePoints < 1) return null;
        if (maxOffsetPercentage < 0f) return [new SegmentDef.Segment(start, end)];

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

            var segment = new SegmentDef.Segment(curLinePoint, nextLinePoint);
            curLinePoint = nextLinePoint;
            result.Add(segment);
        }

        result.Add(new SegmentDef.Segment(curLinePoint, end));

        return result;
    }

}