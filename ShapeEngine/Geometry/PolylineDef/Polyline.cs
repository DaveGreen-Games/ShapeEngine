using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline : Points, IEquatable<Polyline>
{
    #region Constructors
    public Polyline() { }
    public Polyline(int capacity) : base(capacity) { }
    
    /// <summary>
    /// Points should be in CCW order. Use Reverse if they are in CW order.
    /// </summary>
    /// <param name="points"></param>
    public Polyline(IEnumerable<Vector2> points) { AddRange(points); }
    public Polyline(Points points) : base(points.Count) { AddRange(points); }
    public Polyline(Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }
    public Polyline(Polygon poly) : base(poly.Count) { AddRange(poly); }
    #endregion

    #region Equals & HashCode
    public bool Equals(Polyline? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (!this[i].IsSimilar(other[i])) return false;
            //if (this[i] != other[i]) return false;
        }
        return true;
    }
    public override int GetHashCode() => Game.GetHashCode(this);

    #endregion

    #region Shapes

    public Circle GetBoundingCircle()
    {
        float maxD = 0f;
        int num = this.Count;
        Vector2 origin = new();
        for (int i = 0; i < num; i++) { origin += this[i]; }
        origin *= (1f / num);
        for (int i = 0; i < num; i++)
        {
            float d = (origin - this[i]).LengthSquared();
            if (d > maxD) maxD = d;
        }

        return new Circle(origin, MathF.Sqrt(maxD));
    }
    public Rect GetBoundingBox()
    {
        if (Count < 2) return new();
        Vector2 start = this[0];
        Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in this)
        {
            r = r.Enlarge(p); // ShapeRect.Enlarge(r, p);
        }
        return r;
    }

    /// <summary>
    /// Return the segments of the polyline. If points are in ccw order the normals face to the right of the direction of the segments.
    /// If InsideNormals = true the normals face to the left of the direction of the segments.
    /// </summary>
    /// <returns></returns>
    public Segments GetEdges()
    {
        if (Count <= 1) return new();
        if (Count == 2) return new() { new(this[0], this[1]) };

        Segments segments = new();
        for (int i = 0; i < Count - 1; i++)
        {
            segments.Add(new(this[i], this[(i + 1) % Count]));
        }
        return segments;
    }
    
    public Points ToPoints() { return new(this); }

    #endregion
    
    #region Points & Vertex
    public Segment GetSegment(int index)
    { 
        if (index < 0) return new Segment();
        if (Count < 2) return new Segment();
        var first = index % (Count - 1);
        var second = first + 1;
        return new Segment(this[first], this[second]);
    }
    
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    #endregion
    
    #region Interpolated Edge Points

    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <returns></returns>
    public Points? InterpolatedEdgePoints(float t)
    {
        if (Count < 2) return null;

        var result = new Points();
        for (int i = 0; i < Count - 1; i++)
        {
            var cur = this[i];
            var next = this[i + 1];
            var interpolated = cur.Lerp(next, t);// Vector2.Lerp(cur, next, t);
            result.Add(interpolated);
        }
        
        return result;
    }
    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <param name="steps">Recursive steps. The amount of times the result of InterpolatedEdgesPoints will be run through InterpolateEdgePoints.</param>
    /// <returns></returns>
    public Points? InterpolatedEdgePoints(float t, int steps)
    {
        if (Count < 2) return null;
        if (steps <= 1) return InterpolatedEdgePoints(t);

        int remainingSteps = steps;
        var result = new Points();
        var buffer = new Points();
        while (remainingSteps > 0)
        {
            var target = result.Count <= 0 ? this : result;
            for (int i = 0; i < target.Count; i++)
            {
                var cur = target[i];
                var next = target[i + 1];
                var interpolated = cur.Lerp(next, t);
                buffer.Add(interpolated);
            }

            (result, buffer) = (buffer, result);//switch buffer and result
            buffer.Clear();
            remainingSteps--;
        }

        
        return result;
    }
    #endregion
    
    #region Static
    public static Polyline GetShape(Points relative, Transform2D transform)
    {
        if (relative.Count < 3) return new();
        Polyline shape = new();
        for (int i = 0; i < relative.Count; i++)
        {
            shape.Add(transform.ApplyTransformTo(relative[i]));
            // shape.Add(transform.Position + relative[i].Rotate(transform.RotationRad) * transform.Scale);
        }
        return shape;
    }
    #endregion
}


