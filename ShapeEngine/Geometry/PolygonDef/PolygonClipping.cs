using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

//TODO: Docs
public partial class Polygon
{
    #region Intersection
    public bool ClipIntersection(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
    public bool ClipIntersection(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    public bool ClipIntersectionMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    #endregion
    
    #region Difference
    public bool ClipDifference(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
    public bool ClipDifference(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    public bool ClipDifferenceMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    #endregion

    #region Union
    public bool ClipUnion(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
    public bool ClipUnion(Polygon other, float distanceThreshold, Polygon result)
    {
        if (distanceThreshold <= 0f) return false;
        
        var cp = GetClosestPoint(other);
        if (cp.Valid && cp.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            if (!Generate(cp.Self.Point, 7, distanceThreshold, distanceThreshold * 2, clipPolygonBuffer)) return false;
            this.ClipUnion(clipPolygonBuffer, result);
            result.ClipUnion(other, result);
            return true;
        }

        return false;
    }
    
    public bool ClipUnion(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    public bool ClipUnion(Polygon other, float distanceThreshold, Polygons result)
    {
        if (distanceThreshold <= 0f) return false;
        
        var cp = GetClosestPoint(other);
        if (cp.Valid && cp.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            if (!Generate(cp.Self.Point, 7, distanceThreshold, distanceThreshold * 2, clipPolygonBuffer)) return false;
            this.ClipUnion(clipPolygonBuffer, result);
            if (result.Count > 1) return false;
            
            result[0].ClipUnion(other, result);
            return true;
        }

        return false;
    }
    
    public bool ClipUnion(Polygon other, Polygons newShapesResult, Polygons overlapsResult)
    {
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(overlapsResult, true);
        
        ClipperImmediate2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        
        return true;
    }
    
    
    public bool ClipUnionMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    public bool ClipUnionMany(Polygons others, Polygons newShapesResult, Polygons overlapsResult)
    {
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(overlapsResult, true);
        
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Union, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        
        return true;
    }
    #endregion

    #region Cut
   
    public bool CutSelf(Polygon cutShape, Polygons? cutOutsResult, Polygons? extraShapesResult)
    {
        if (cutOutsResult != null)
        {
            ClipperImmediate2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Intersection, clipResultBuffer);
            clipResultBuffer.ToPolygons(cutOutsResult, true);
        }
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Difference, clipResultBuffer);
        clipResultBuffer.RemoveAllHoles();
        if(clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(this);
        if (clipResultBuffer.Count > 1 && extraShapesResult != null)
        {
            clipResultBuffer.RemoveAt(0);
            clipResultBuffer.ToPolygons(extraShapesResult, true);
        }
        return true;
    }
  
    public bool Cut(Polygon cutShape, Polygons cutOutsResult, Polygons newShapesResult)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(cutOutsResult, true);
        
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Difference, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        Console.WriteLine($"Shape Clockwise: {this.IsClockwise()}, Cut Shape Clockwise: {cutShape.IsClockwise()}");
        foreach (Polygon polygon in newShapesResult)
        {
            Console.WriteLine($"New Shape Clockwise: {polygon.IsClockwise()}");
        }
        return true;
    }
    
    public bool CutMany(Polygons cutShapes, Polygons cutOutsResult, Polygons newShapesResult)
    {
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, cutShapes, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(cutOutsResult, true);
        
        clipResultBuffer.Clear();
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, cutShapes, ShapeClipperClipType.Difference, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        return true;
    }
    
    public bool CutSimple(Vector2 cutPos, Polygons newShapesResult, Polygons cutOutsResult, float minCutRadius, float maxCutRadius, int pointCount = 16)
    {
        if (!Generate(cutPos, pointCount, minCutRadius, maxCutRadius, clipPolygonBuffer)) return false;
        return Cut(clipPolygonBuffer, cutOutsResult, newShapesResult);
    }
    
    public bool CutSimple(Segment cutLine, Polygons newShapesResult, Polygons cutOutsResult, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
    {
        if(!Generate(cutLine, clipPolygonBuffer, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength)) return false;
        return Cut(clipPolygonBuffer, cutOutsResult, newShapesResult);
    }
    
    #endregion
    
    #region Split
    public bool Split(Vector2 point, Vector2 direction, Polygons result)
    {
        var line = new Line(point, direction);
        return Split(line, result);
    }
    
    public bool Split(Line line, Polygons result)
    {
        var w = Center - line.Point;
        var l = w.Length();
        if (l < Diameter * 2f) l = Diameter * 2f;
        else l *= 2f;

        var segment = line.ToSegment(l);
        return Split(segment, result);
    }
    
    public bool Split(Segment segment, Polygons result)
    {
        clipPolygonBuffer.Clear();
        clipPolygonBuffer.Add(segment.Start);
        clipPolygonBuffer.Add(segment.End);
        
        ClipperImmediate2D.ClipEngine.Execute(this, clipPolygonBuffer, ShapeClipperClipType.Difference, clipResultBuffer);
        if(clipResultBuffer.Count <= 0) return false;
        
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    public bool Split(Segments segments, Polygons result)
    {
        if(segments.Count <= 0) return false;
        clipPooledBuffer.PrepareBuffer(segments.Count);
        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var buffer = clipPooledBuffer.Buffer[i];
            buffer.Clear();
            buffer.Add(segment.Start.ToPoint64());
            buffer.Add(segment.End.ToPoint64());
        }
        
        ClipperImmediate2D.ClipEngine.ExecuteMany(this, clipPooledBuffer.Buffer, ShapeClipperClipType.Difference, clipResultBuffer);
        if(clipResultBuffer.Count <= 0) return false;
        
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    public bool FractureSplit(Vector2 dir, float maxOffsetPercentage, int fractureLineComplexity, Polygons result)
    {
        if (fractureLineComplexity < 1 || dir.LengthSquared() <= 0f) return false;

        var center = Center;
        var intersectionPoints = IntersectShape(new Line(center, dir));
        if (intersectionPoints == null || intersectionPoints.Count < 2) return false;

        var start = intersectionPoints[0].Point;
        var end = intersectionPoints[1].Point;
        if (intersectionPoints.Count > 2)
        {
            end = intersectionPoints.GetFurthestCollisionPoint(start).Point;
        }

        var fractureLine = GenerateFractureLine(start, end, maxOffsetPercentage, fractureLineComplexity);
        if (fractureLine == null) return false;
        Split(fractureLine, result);
        return true;
    }
    
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
    #endregion

}