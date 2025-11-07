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
using Game = ShapeEngine.Core.GameDef.Game;

namespace ShapeEngine.Geometry.PolylineDef;

/// <summary>
/// Represents a polyline, which is a series of connected points in 2D space.
/// Provides methods for geometric operations and shape conversions.
/// </summary>
/// <remarks>
/// Points should be provided in counter-clockwise (CCW) order for correct geometric behavior.
/// Use the <see cref="ReverseOrder"/> method if your points are in clockwise (CW) order.
/// </remarks>
public partial class Polyline : Points, IEquatable<Polyline>, IShapeTypeProvider
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class with no points.
    /// </summary>
    public Polyline() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class with a specified capacity.
    /// </summary>
    /// <param name="capacity">The initial number of points the polyline can contain without resizing.</param>
    public Polyline(int capacity) : base(capacity) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class from a collection of points.
    /// </summary>
    /// <param name="points">The points to initialize the polyline with.
    /// Should be in CCW order for correct geometric operations.</param>
    /// <remarks>
    /// If the points are in CW order, use the <see cref="ReverseOrder"/> method to correct the order.
    /// </remarks>
    public Polyline(IEnumerable<Vector2> points) { AddRange(points); }

    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class from an existing <see cref="Points"/> collection.
    /// </summary>
    /// <param name="points">The <see cref="Points"/> collection to copy points from.</param>
    public Polyline(Points points) : base(points.Count) { AddRange(points); }

    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class by copying another <see cref="Polyline"/>.
    /// </summary>
    /// <param name="polyLine">The <see cref="Polyline"/> to copy points from.</param>
    public Polyline(Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }

    /// <summary>
    /// Initializes a new instance of the <see cref="Polyline"/> class from a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="poly">The <see cref="Polygon"/> whose points will be used to initialize the polyline.</param>
    public Polyline(Polygon poly) : base(poly.Count) { AddRange(poly); }
    #endregion

    #region Equals & HashCode
    /// <summary>
    /// Determines whether the specified <see cref="Polyline"/> is equal to the current <see cref="Polyline"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to compare with the current polyline.</param>
    /// <returns><c>true</c> if the polylines are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Equality is determined by comparing the number of points and the similarity of each corresponding point.
    /// </remarks>
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
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Polyline"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current polyline.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="Polyline"/> and is equal to the current polyline; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Polyline);
    }
    /// <summary>
    /// Returns a hash code for the current <see cref="Polyline"/>.
    /// </summary>
    /// <returns>A hash code for the current polyline.</returns>
    public override int GetHashCode() => Game.GetHashCode(this);

    public ShapeType GetShapeType() => ShapeType.PolyLine;

    #endregion

    #region Shapes

    /// <summary>
    /// Calculates the minimal bounding circle that contains all points of the polyline.
    /// </summary>
    /// <returns>A <see cref="Circle"/> that bounds the polyline.</returns>
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
    /// <summary>
    /// Calculates the axis-aligned bounding box that contains all points of the polyline.
    /// </summary>
    /// <returns>A <see cref="Rect"/> that bounds the polyline.</returns>
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
   /// Returns the segments (edges) of the polyline as a <see cref="Segments"/> collection.
   /// If the points are in counter-clockwise (CCW) order, the segment normals face to the right of the segment direction.
   /// If <c>InsideNormals</c> is true, the normals face to the left of the segment direction.
   /// </summary>
   /// <returns>A <see cref="Segments"/> collection representing the polyline's edges.</returns>
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
    /// <summary>
    /// Converts this <see cref="Polyline"/> to a <see cref="Points"/> collection containing the same points.
    /// </summary>
    /// <returns>A new <see cref="Points"/> instance with the points from this polyline.</returns>
    public Points ToPoints() { return new(this); }

    #endregion
    
    #region Points & Vertex
    /// <summary>
    /// Reverses the order of points in the polyline.
    /// After calling this method,
    /// the polyline will be in clockwise (CW) order if it was counter-clockwise (CCW) before and vice versa.
    /// </summary>
    public void ReverseOrder() => Reverse();
    /// <summary>
    /// Gets the segment (edge) between the two points at the specified index.
    /// </summary>
    /// <param name="index">The index of the segment to retrieve.</param>
    /// <returns>The <see cref="Segment"/> at the specified index, or an empty segment if the index is out of range.</returns>
    public Segment GetSegment(int index)
    { 
        if (index < 0) return new Segment();
        if (Count < 2) return new Segment();
        var first = index % (Count - 1);
        var second = first + 1;
        return new Segment(this[first], this[second]);
    }
    
    /// <summary>
    /// Gets a random vertex (point) from the polyline.
    /// </summary>
    /// <returns>A random <see cref="Vector2"/> vertex from the polyline.</returns>
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    /// <summary>
    /// Gets a random edge (segment) from the polyline.
    /// </summary>
    /// <returns>A random <see cref="Segment"/> edge from the polyline.</returns>
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    #endregion
    
    #region Interpolated Edge Points

    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between <c>0 - 1</c>.</param>
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
    /// <param name="t">The value t for interpolation. Should be between <c>0 - 1</c>.</param>
    /// <param name="steps">Recursive steps. The number of times the result of InterpolatedEdgesPoints will be run through InterpolateEdgePoints.</param>
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
    /// <summary>
    /// Creates a polyline shape from a set of relative points and a transformation.
    /// </summary>
    /// <param name="relative">The relative points that define the shape of the polyline.</param>
    /// <param name="transform">The transformation to apply to the relative points.</param>
    /// <returns>A new <see cref="Polyline"/> instance representing the transformed shape.</returns>
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
