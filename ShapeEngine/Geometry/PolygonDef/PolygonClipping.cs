using System.Numerics;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    #region Intersection
    /// <summary>
    /// Computes the geometric intersection between this polygon and <paramref name="other"/> and writes the first resulting polygon into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to intersect with this polygon.</param>
    /// <param name="result">The destination polygon that receives the first intersection result.</param>
    /// <returns><c>true</c> if at least one intersection polygon was produced; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If multiple intersection polygons are produced, only the first one is copied into <paramref name="result"/>.
    /// </remarks>
    public bool ClipIntersection(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
  
    /// <summary>
    /// Computes the geometric intersection between this polygon and <paramref name="other"/> and writes all resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to intersect with this polygon.</param>
    /// <param name="result">The destination collection that receives all intersection polygons.</param>
    /// <returns><c>true</c> if at least one intersection polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipIntersection(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    /// <summary>
    /// Computes the geometric intersection between this polygon and all polygons in <paramref name="others"/>, writing the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="others">The polygons to intersect with this polygon.</param>
    /// <param name="result">The destination collection that receives all resulting intersection polygons.</param>
    /// <returns><c>true</c> if at least one intersection polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipIntersectionMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Intersection, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    #endregion
    
    #region Difference
    /// <summary>
    /// Computes the geometric difference of this polygon minus <paramref name="other"/> and writes the first resulting polygon into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to subtract from this polygon.</param>
    /// <param name="result">The destination polygon that receives the first resulting difference polygon.</param>
    /// <returns><c>true</c> if at least one difference polygon was produced; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If multiple difference polygons are produced, only the first one is copied into <paramref name="result"/>.
    /// </remarks>
    public bool ClipDifference(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
  
    /// <summary>
    /// Computes the geometric difference of this polygon minus <paramref name="other"/> and writes all resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to subtract from this polygon.</param>
    /// <param name="result">The destination collection that receives the resulting difference polygons.</param>
    /// <returns><c>true</c> if at least one difference polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipDifference(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    /// <summary>
    /// Computes the geometric difference of this polygon minus all polygons in <paramref name="others"/> and writes the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="others">The polygons to subtract from this polygon.</param>
    /// <param name="result">The destination collection that receives the resulting difference polygons.</param>
    /// <returns><c>true</c> if at least one difference polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipDifferenceMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Difference, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    #endregion

    #region Union
    /// <summary>
    /// Computes the geometric union of this polygon and <paramref name="other"/> and writes the first resulting polygon into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to union with this polygon.</param>
    /// <param name="result">The destination polygon that receives the first union result.</param>
    /// <returns><c>true</c> if at least one union polygon was produced; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If multiple union polygons are produced, only the first one is copied into <paramref name="result"/>.
    /// </remarks>
    public bool ClipUnion(Polygon other, Polygon result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer[0].ToVector2List(result);
        return true;
    }
  
    /// <summary>
    /// Attempts to union this polygon with <paramref name="other"/> by first bridging nearby shapes when they are within <paramref name="distanceThreshold"/>.
    /// </summary>
    /// <param name="other">The polygon to union with this polygon.</param>
    /// <param name="distanceThreshold">The maximum allowed distance between the polygons before a temporary bridge polygon is generated.</param>
    /// <param name="result">The destination polygon that receives the union result.</param>
    /// <returns><c>true</c> if a bridge polygon was generated and the union succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This overload is intended for shapes that are close but not yet touching. It generates a temporary polygon at the closest point before performing the union.
    /// </remarks>
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
    
    /// <summary>
    /// Computes the geometric union of this polygon and <paramref name="other"/> and writes all resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to union with this polygon.</param>
    /// <param name="result">The destination collection that receives the union result polygons.</param>
    /// <returns><c>true</c> if at least one union polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipUnion(Polygon other, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
   
    /// <summary>
    /// Attempts to union this polygon with <paramref name="other"/> by first bridging nearby shapes when they are within <paramref name="distanceThreshold"/>, writing all resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="other">The polygon to union with this polygon.</param>
    /// <param name="distanceThreshold">The maximum allowed distance between the polygons before a temporary bridge polygon is generated.</param>
    /// <param name="result">The destination collection that receives the union result polygons.</param>
    /// <returns><c>true</c> if the bridge polygon was generated and the union produced a single intermediate shape that could be unioned with <paramref name="other"/>; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This overload requires the first intermediate union to collapse to exactly one polygon before continuing.
    /// </remarks>
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
    
    /// <summary>
    /// Computes both the overlap and the union between this polygon and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The polygon to union with this polygon.</param>
    /// <param name="newShapesResult">The destination collection that receives the union result polygons.</param>
    /// <param name="overlapsResult">The destination collection that receives the intersection polygons.</param>
    /// <returns><c>true</c>.</returns>
    /// <remarks>
    /// This method always returns <c>true</c> after updating both output collections, even if either collection ends up empty.
    /// </remarks>
    public bool ClipUnion(Polygon other, Polygons newShapesResult, Polygons overlapsResult)
    {
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(overlapsResult, true);
        
        ShapeClipper2D.ClipEngine.Execute(this, other, ShapeClipperClipType.Union, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        
        return true;
    }
    
    
    /// <summary>
    /// Computes the geometric union of this polygon with all polygons in <paramref name="others"/> and writes the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="others">The polygons to union with this polygon.</param>
    /// <param name="result">The destination collection that receives the union result polygons.</param>
    /// <returns><c>true</c> if at least one union polygon was produced; otherwise, <c>false</c>.</returns>
    public bool ClipUnionMany(Polygons others, Polygons result)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Union, clipResultBuffer);
        if (clipResultBuffer.Count <= 0) return false;
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    /// <summary>
    /// Computes both the overlap and the union between this polygon and all polygons in <paramref name="others"/>.
    /// </summary>
    /// <param name="others">The polygons to union with this polygon.</param>
    /// <param name="newShapesResult">The destination collection that receives the union result polygons.</param>
    /// <param name="overlapsResult">The destination collection that receives the intersection polygons.</param>
    /// <returns><c>true</c>.</returns>
    /// <remarks>
    /// This method always returns <c>true</c> after updating both output collections, even if either collection ends up empty.
    /// </remarks>
    public bool ClipUnionMany(Polygons others, Polygons newShapesResult, Polygons overlapsResult)
    {
        ShapeClipper2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(overlapsResult, true);
        
        ShapeClipper2D.ClipEngine.ExecuteMany(this, others, ShapeClipperClipType.Union, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        
        return true;
    }
    #endregion

    #region Cut
   
    /// <summary>
    /// Cuts this polygon in place using <paramref name="cutShape"/>, optionally collecting the removed intersections and any additional remaining pieces.
    /// </summary>
    /// <param name="cutShape">The polygon used to cut this polygon.</param>
    /// <param name="cutOutsResult">An optional destination collection that receives the intersecting cut-out polygons.</param>
    /// <param name="extraShapesResult">An optional destination collection that receives any remaining polygons beyond the first retained shape.</param>
    /// <returns><c>true</c> if at least one remaining difference polygon was produced and this polygon was updated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The first remaining polygon replaces the contents of this polygon. Any additional remaining polygons are written to <paramref name="extraShapesResult"/> when provided.
    /// </remarks>
    public bool CutSelf(Polygon cutShape, Polygons? cutOutsResult, Polygons? extraShapesResult)
    {
        if (cutOutsResult != null)
        {
            ShapeClipper2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Intersection, clipResultBuffer);
            clipResultBuffer.ToPolygons(cutOutsResult, true);
        }
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Difference, clipResultBuffer);
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
  
    /// <summary>
    /// Cuts this polygon with <paramref name="cutShape"/> and writes both the removed overlap polygons and the remaining polygons into the provided result collections.
    /// </summary>
    /// <param name="cutShape">The polygon used to cut this polygon.</param>
    /// <param name="cutOutsResult">The destination collection that receives the intersecting cut-out polygons.</param>
    /// <param name="newShapesResult">The destination collection that receives the remaining polygons after the cut.</param>
    /// <returns><c>true</c>.</returns>
    /// <remarks>
    /// This method always returns <c>true</c> after updating both output collections. The current polygon is not modified.
    /// </remarks>
    public bool Cut(Polygon cutShape, Polygons cutOutsResult, Polygons newShapesResult)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(cutOutsResult, true);
        
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.Execute(this, cutShape, ShapeClipperClipType.Difference, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        Console.WriteLine($"Shape Clockwise: {this.IsClockwise()}, Cut Shape Clockwise: {cutShape.IsClockwise()}");
        foreach (Polygon polygon in newShapesResult)
        {
            Console.WriteLine($"New Shape Clockwise: {polygon.IsClockwise()}");
        }
        return true;
    }
    
    /// <summary>
    /// Cuts this polygon with all polygons in <paramref name="cutShapes"/> and writes both the removed overlap polygons and the remaining polygons into the provided result collections.
    /// </summary>
    /// <param name="cutShapes">The polygons used to cut this polygon.</param>
    /// <param name="cutOutsResult">The destination collection that receives the intersecting cut-out polygons.</param>
    /// <param name="newShapesResult">The destination collection that receives the remaining polygons after the cut.</param>
    /// <returns><c>true</c>.</returns>
    /// <remarks>
    /// This method always returns <c>true</c> after updating both output collections. The current polygon is not modified.
    /// </remarks>
    public bool CutMany(Polygons cutShapes, Polygons cutOutsResult, Polygons newShapesResult)
    {
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.ExecuteMany(this, cutShapes, ShapeClipperClipType.Intersection, clipResultBuffer);
        clipResultBuffer.ToPolygons(cutOutsResult, true);
        
        clipResultBuffer.Clear();
        ShapeClipper2D.ClipEngine.ExecuteMany(this, cutShapes, ShapeClipperClipType.Difference, clipResultBuffer);
        clipResultBuffer.ToPolygons(newShapesResult, true);
        return true;
    }
    
    /// <summary>
    /// Generates a random polygonal cut shape around <paramref name="cutPos"/> and uses it to cut this polygon.
    /// </summary>
    /// <param name="cutPos">The center position used to generate the temporary cut polygon.</param>
    /// <param name="newShapesResult">The destination collection that receives the remaining polygons after the cut.</param>
    /// <param name="cutOutsResult">The destination collection that receives the intersecting cut-out polygons.</param>
    /// <param name="minCutRadius">The minimum radius of the generated cut polygon.</param>
    /// <param name="maxCutRadius">The maximum radius of the generated cut polygon.</param>
    /// <param name="pointCount">The number of vertices used to generate the temporary cut polygon.</param>
    /// <returns><c>true</c> if the temporary cut polygon was generated and the cut operation completed; otherwise, <c>false</c>.</returns>
    public bool CutSimple(Vector2 cutPos, Polygons newShapesResult, Polygons cutOutsResult, float minCutRadius, float maxCutRadius, int pointCount = 16)
    {
        if (!Generate(cutPos, pointCount, minCutRadius, maxCutRadius, clipPolygonBuffer)) return false;
        return Cut(clipPolygonBuffer, cutOutsResult, newShapesResult);
    }
    
    /// <summary>
    /// Generates a random polygonal cut shape along <paramref name="cutLine"/> and uses it to cut this polygon.
    /// </summary>
    /// <param name="cutLine">The guiding segment used to generate the temporary cut polygon.</param>
    /// <param name="newShapesResult">The destination collection that receives the remaining polygons after the cut.</param>
    /// <param name="cutOutsResult">The destination collection that receives the intersecting cut-out polygons.</param>
    /// <param name="minSectionLength">The minimum section length used when generating the cut polygon.</param>
    /// <param name="maxSectionLength">The maximum section length used when generating the cut polygon.</param>
    /// <param name="minMagnitude">The minimum magnitude used for the cut polygon offsets.</param>
    /// <param name="maxMagnitude">The maximum magnitude used for the cut polygon offsets.</param>
    /// <returns><c>true</c> if the temporary cut polygon was generated and the cut operation completed; otherwise, <c>false</c>.</returns>
    public bool CutSimple(Segment cutLine, Polygons newShapesResult, Polygons cutOutsResult, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
    {
        if(!Generate(cutLine, clipPolygonBuffer, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength)) return false;
        return Cut(clipPolygonBuffer, cutOutsResult, newShapesResult);
    }
    
    #endregion
    
    #region Split
    /// <summary>
    /// Splits this polygon along the infinite line defined by <paramref name="point"/> and <paramref name="direction"/>, writing the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="point">A point on the splitting line.</param>
    /// <param name="direction">The direction of the splitting line.</param>
    /// <param name="result">The destination collection that receives the split polygons.</param>
    /// <returns><c>true</c> if the split produced at least one resulting polygon; otherwise, <c>false</c>.</returns>
    public bool Split(Vector2 point, Vector2 direction, Polygons result)
    {
        var line = new Line(point, direction);
        return Split(line, result);
    }
    
    /// <summary>
    /// Splits this polygon along the specified line, writing the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="line">The line used to split the polygon.</param>
    /// <param name="result">The destination collection that receives the split polygons.</param>
    /// <returns><c>true</c> if the split produced at least one resulting polygon; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The line is converted to a sufficiently long segment based on the polygon center and diameter before clipping.
    /// </remarks>
    public bool Split(Line line, Polygons result)
    {
        var w = Center - line.Point;
        var l = w.Length();
        if (l < Diameter * 2f) l = Diameter * 2f;
        else l *= 2f;

        var segment = line.ToSegment(l);
        return Split(segment, result);
    }
    
    /// <summary>
    /// Splits this polygon using the specified segment as a cutting path, writing the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="segment">The segment used to split the polygon.</param>
    /// <param name="result">The destination collection that receives the split polygons.</param>
    /// <returns><c>true</c> if the split produced at least one resulting polygon; otherwise, <c>false</c>.</returns>
    public bool Split(Segment segment, Polygons result)
    {
        clipPolygonBuffer.Clear();
        clipPolygonBuffer.Add(segment.Start);
        clipPolygonBuffer.Add(segment.End);
        
        ShapeClipper2D.ClipEngine.Execute(this, clipPolygonBuffer, ShapeClipperClipType.Difference, clipResultBuffer);
        if(clipResultBuffer.Count <= 0) return false;
        
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    /// <summary>
    /// Splits this polygon using multiple segments as cutting paths, writing the resulting polygons into <paramref name="result"/>.
    /// </summary>
    /// <param name="segments">The segments used to split the polygon.</param>
    /// <param name="result">The destination collection that receives the split polygons.</param>
    /// <returns><c>true</c> if at least one segment was provided and the split produced at least one resulting polygon; otherwise, <c>false</c>.</returns>
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
        
        ShapeClipper2D.ClipEngine.ExecuteMany(this, clipPooledBuffer.Buffer, ShapeClipperClipType.Difference, clipResultBuffer);
        if(clipResultBuffer.Count <= 0) return false;
        
        clipResultBuffer.ToPolygons(result, true);
        return true;
    }
    
    /// <summary>
    /// Splits this polygon using a randomly perturbed fracture line derived from the specified direction.
    /// </summary>
    /// <param name="dir">The main fracture direction.</param>
    /// <param name="maxOffsetPercentage">The maximum lateral offset of fracture points, expressed as a fraction of each fracture segment length.</param>
    /// <param name="fractureLineComplexity">The number of intermediate fracture points used to build the fracture line.</param>
    /// <param name="result">The destination collection that receives the split polygons.</param>
    /// <returns><c>true</c> if a valid fracture line was generated and the split succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The fracture line is generated from the intersection of this polygon with a line through the polygon center in the given direction.
    /// </remarks>
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
    
    /// <summary>
    /// Generates a fractured polyline between <paramref name="start"/> and <paramref name="end"/>.
    /// </summary>
    /// <param name="start">The starting point of the fracture line.</param>
    /// <param name="end">The ending point of the fracture line.</param>
    /// <param name="maxOffsetPercentage">The maximum lateral offset of each intermediate fracture point, expressed as a fraction of the base segment length.</param>
    /// <param name="linePoints">The number of intermediate fracture points to generate.</param>
    /// <returns>A <see cref="Segments"/> collection representing the generated fracture line, or <c>null</c> if the inputs are invalid.</returns>
    /// <remarks>
    /// When <paramref name="maxOffsetPercentage"/> is negative, a single straight segment from <paramref name="start"/> to <paramref name="end"/> is returned.
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
    
    /// <summary>
    /// Generates a fractured polyline between <paramref name="start"/> and <paramref name="end"/> and writes the resulting segments into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that receives the generated fracture line segments.</param>
    /// <param name="start">The starting point of the fracture line.</param>
    /// <param name="end">The ending point of the fracture line.</param>
    /// <param name="maxOffsetPercentage">The maximum lateral offset of each intermediate fracture point, expressed as a fraction of the base segment length.</param>
    /// <param name="linePoints">The number of intermediate fracture points to generate.</param>
    /// <returns><c>true</c> if the fracture line was generated successfully; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// When <paramref name="maxOffsetPercentage"/> is negative, a single straight segment from <paramref name="start"/> to <paramref name="end"/> is returned.
    /// </remarks>
    public bool GenerateFractureLine(Segments result, Vector2 start, Vector2 end, float maxOffsetPercentage, int linePoints)
    {
        if (linePoints < 1) return false;
        
        result.Clear();
        result.EnsureCapacity(linePoints + 1);
        if (maxOffsetPercentage < 0f)
        {
            result.Add(new Segment(start, end));
            return true;
        }

        var w = end - start;
        var disSquared = w.LengthSquared();
        if (disSquared <= 0f) return false;

        var l = MathF.Sqrt(disSquared);
        var segmentLength = l / (linePoints + 1);
        var dir = w / l;
        var p = dir.GetPerpendicularLeft();

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

        return true;
    }
    #endregion

}