using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.Line;
using ShapeEngine.Geometry.Polygon;
using ShapeEngine.Geometry.Polyline;
using ShapeEngine.Geometry.Quad;
using ShapeEngine.Geometry.Ray;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Geometry.Segments;
using ShapeEngine.Geometry.Triangle;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Points;

/// <summary>
/// Represents a collection of 2D points and provides various geometric and transformation operations.
/// </summary>
/// <remarks>
/// This class is used for manipulating and analyzing sets of points in 2D space, including operations such as finding closest points, transforming, and interpolating between points.
/// </remarks>
public class Points : ShapeList<Vector2>, IEquatable<Points>
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Points"/> class.
    /// </summary>
    public Points(){}
    /// <summary>
    /// Initializes a new instance of the <see cref="Points"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the collection can contain.</param>
    public Points(int capacity) : base(capacity){}
    /// <summary>
    /// Initializes a new instance of the <see cref="Points"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="points">The collection whose elements are copied to the new list.</param>
    public Points(IEnumerable<Vector2> points) { AddRange(points); }
    #endregion

    #region Equals & HashCode
    /// <inheritdoc />
    public override int GetHashCode() { return Game.GetHashCode(this); }
    /// <summary>
    /// Determines whether the specified <see cref="Points"/> instance is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="Points"/> instance to compare with the current instance.</param>
    /// <returns>True if the specified <see cref="Points"/> is equal to the current instance; otherwise, false.</returns>
    /// <remarks>
    /// Two <see cref="Points"/> instances are considered equal if they have the same number of points and all corresponding points are the same.
    /// </remarks>
    public bool Equals(Points? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (!this[i].IsSimilar(other[i])) return false;
        }
        return true;
    }
    #endregion

    #region Points & Vertex

    /// <summary>
    /// Gets the value at the specified index, wrapping around if the index is less than 0 or greater than or equal to the count.
    /// </summary>
    /// <param name="index">The index of the point to retrieve. If out of range, wraps around using modulo.</param>
    /// <returns>The <see cref="Vector2"/> at the specified (wrapped) index.</returns>
    /// <remarks>
    /// This method allows for circular indexing, which is useful for closed shapes or polygons.
    /// </remarks>
    public Vector2 GetPoint(int index)
    {
        return GetItem(index);
        //return Count <= 0 ? new() : this[index % Count];
    }
    /// <summary>
    /// Finds the index of the point in the collection that is closest to the specified position.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <returns>The index of the closest point, or -1 if the collection is empty.</returns>
    public int GetClosestIndex(Vector2 p)
    {
        if (Count <= 0) return -1;
        else if (Count == 1) return 0;
            
        float minDistanceSquared = float.PositiveInfinity;
        int closestIndex = -1;

        for (var i = 0; i < Count; i++)
        {
            float disSquared = (this[i] - p).LengthSquared();
            if (disSquared < minDistanceSquared)
            {
                closestIndex = i;
                minDistanceSquared = disSquared;
            }
        }
        return closestIndex;
    }
    /// <summary>
    /// Finds the point in the collection that is closest to the specified position.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <returns>The closest <see cref="Vector2"/> in the collection, or a default value if the collection is empty.</returns>
    public Vector2 GetClosestVertex(Vector2 p)
    {
        if (Count <= 0) return new();
        else if (Count == 1) return this[0];
            
        float minDistanceSquared = float.PositiveInfinity;
        Vector2 closestPoint = new();

        for (var i = 0; i < Count; i++)
        {
            float disSquared = (this[i] - p).LengthSquared();
            if (disSquared < minDistanceSquared)
            {
                closestPoint = this[i];
                minDistanceSquared = disSquared;
            }
        }
        return closestPoint;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance containing only unique points from the collection.
    /// </summary>
    /// <returns>A <see cref="Points"/> object with duplicate points removed.</returns>
    /// <remarks>
    /// Uniqueness is determined by the default equality comparer for <see cref="Vector2"/>.
    /// </remarks>
    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (var i = 0; i < Count; i++)
        {
            uniqueVertices.Add(this[i]);
        }
        return new(uniqueVertices);
    }
    /// <summary>
    /// Returns a random point from the collection.
    /// </summary>
    /// <returns>A randomly selected <see cref="Vector2"/> from the collection.</returns>
    public Vector2 GetRandomPoint() => GetRandomItem();
    /// <summary>
    /// Returns a list of random points from the collection.
    /// </summary>
    /// <param name="amount">The number of random points to retrieve.</param>
    /// <returns>A list of randomly selected <see cref="Vector2"/> points.</returns>
    public List<Vector2> GetRandomPoints(int amount) => GetRandomItems(amount);

    #endregion

    #region Math

    /// <summary>
    /// Applies the floor operation to all points in this collection, modifying each coordinate to the largest integer less than or equal to it.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Floor() { Points.Floor(this); }
    /// <summary>
    /// Applies the ceiling operation to all points in this collection, modifying each coordinate to the smallest integer greater than or equal to it.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Ceiling() { Points.Ceiling(this); }
    /// <summary>
    /// Truncates all points in this collection, removing the fractional part of each coordinate.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Truncate() { Points.Truncate(this); }
    /// <summary>
    /// Rounds all points in this collection to the nearest integer values.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Round() { Points.Round(this); }

    #endregion

    #region Shapes

    /// <inheritdoc />
    public override Points Copy() => new(this);

    /// <summary>
    /// Converts this collection of points into a <see cref="Polygon"/> shape.
    /// </summary>
    /// <returns>A new <see cref="Polygon"/> instance containing the same points.</returns>
    public Polygon.Polygon ToPolygon() => new(this);

    /// <summary>
    /// Converts this collection of points into a <see cref="Polyline"/> shape.
    /// </summary>
    /// <returns>A new <see cref="Polyline"/> instance containing the same points.</returns>
    public Polyline.Polyline ToPolyline() => new(this);

    /// <summary>
    /// Returns a tuple containing a relative transform and a normalized polygon shape based on the specified center.
    /// </summary>
    /// <param name="center">The center point to use as the origin for normalization.</param>
    /// <returns>
    /// A tuple where:
    /// <list type="bullet">
    /// <item><description><see cref="Transform2D"/>: The transform representing the original center and scale.</description></item>
    /// <item><description><see cref="Polygon"/>: The normalized polygon shape with points in the range 0-1 relative to the center.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Useful for storing shapes in a normalized form and reconstructing them with a transform.
    /// </remarks>
    public (Transform2D transform, Polygon.Polygon shape) ToRelative(Vector2 center)
    {
        var maxLengthSq = 0f;
        for (int i = 0; i < this.Count; i++)
        {
            var lsq = (this[i] - center).LengthSquared();
            if (maxLengthSq < lsq) maxLengthSq = lsq;
        }

        var size = MathF.Sqrt(maxLengthSq);

        var relativeShape = new Polygon.Polygon();
        for (int i = 0; i < this.Count; i++)
        {
            var w = this[i] - center;
            relativeShape.Add(w / size); //transforms it to range 0 - 1
        }

        return (new Transform2D(center, 0f, new Size(size, 0f), 1f), relativeShape);
    }

    /// <summary>
    /// Returns a list of points relative to the specified origin.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <returns>A list of <see cref="Vector2"/> points relative to the origin.</returns>
    public List<Vector2> GetRelativeVector2List(Vector2 origin)
    {
        var relative = new List<Vector2>(Count);
        foreach (var p in this)  relative.Add(p - origin);
        return relative;
    }
    /// <summary>
    /// Returns a list of points relative to the specified transform.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <returns>A list of <see cref="Vector2"/> points relative to the transform.</returns>
    public List<Vector2> GetRelativeVector2List(Transform2D transform)
    {
        var relative = new List<Vector2>(Count);
        foreach (var p in this)  relative.Add(transform.RevertPosition(p));
        return relative;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> collection with all points relative to the specified origin.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <returns>A new <see cref="Points"/> instance with points relative to the origin.</returns>
    public Points GetRelativePoints(Vector2 origin)
    {
        var relative = new Points(Count);
        foreach (var p in this)  relative.Add(p - origin);
        return relative;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> collection with all points relative to the specified transform.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <returns>A new <see cref="Points"/> instance with points relative to the transform.</returns>
    public Points GetRelativePoints(Transform2D transform)
    {
        var relative = new Points(Count);
        foreach (var p in this)  relative.Add(transform.RevertPosition(p));
        return relative;
    }

    #endregion
    
    #region Closest Point
    /// <summary>
    /// Finds the point in this collection that is closest to the specified position.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <param name="disSquared">The squared distance from the closest point to <paramref name="p"/>.</param>
    /// <returns>The closest <see cref="Vector2"/> in the collection, or a default value if the collection is empty.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Vector2"/> and sets <paramref name="disSquared"/> to -1.
    /// </remarks>
    public Vector2 GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 0) return new();

        if (Count == 1)
        {
            disSquared = (this[0] - p).LengthSquared();
            return this[0];
        }


        var closestPoint = this[0];
        var minDisSq = (closestPoint - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var disSq = (this[i] - p).LengthSquared();
            if (disSq >= minDisSq) continue;
            minDisSq = disSq;
            closestPoint = this[i];
        }

        return closestPoint;
    }
    /// <summary>
    /// Finds the point in this collection that is closest to the specified position, and returns its index and squared distance.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <param name="disSquared">The squared distance from the closest point to <paramref name="p"/>.</param>
    /// <param name="index">The index of the closest point in the collection.</param>
    /// <returns>The closest <see cref="Vector2"/> in the collection, or a default value if the collection is empty.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Vector2"/>, sets <paramref name="disSquared"/> to -1, and <paramref name="index"/> to -1.
    /// </remarks>
    public Vector2 GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 0) return new();
        
        var closest = this[0];
        index = 0;
        disSquared = (closest - p).LengthSquared();
        
        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            var dis = (next - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = next;
                disSquared = dis;
            }
        
        }

        return closest;
    }
    
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Line"/>.
    /// </summary>
    /// <param name="other">The <see cref="Line"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the line.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Line.Line.GetClosestPointLinePoint(other.Point, other.Direction, closest, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            
            var result = Line.Line.GetClosestPointLinePoint(other.Point, other.Direction, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;
        
        return new(
            new(closest, -dir.Normalize()), 
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Ray"/>.
    /// </summary>
    /// <param name="other">The <see cref="Ray"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the ray.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Ray.Ray.GetClosestPointRayPoint(other.Point, other.Direction, closest, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            
            var result = Ray.Ray.GetClosestPointRayPoint(other.Point, other.Direction, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;
        
        return new(
            new(closest, -dir.Normalize()), 
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Segment"/>.
    /// </summary>
    /// <param name="other">The <see cref="Segment"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the segment.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Segment.Segment.GetClosestPointSegmentPoint(other.Start, other.End, closest, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            
            var result = Segment.Segment.GetClosestPointSegmentPoint(other.Start, other.End, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;
        
        return new(
            new(closest, -dir.Normalize()), 
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }
    
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Circle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Circle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the circle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle.Circle other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Circle.Circle.GetClosestPointCirclePoint(other.Center, other.Radius, closest, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            
            var result = Circle.Circle.GetClosestPointCirclePoint(other.Center, other.Radius, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        
        }

        var dir = closest - pointOnOther;
        var otherNormal = dir.Normalize();
        
        return new(
            new(closest, -otherNormal), 
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Triangle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Triangle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the triangle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Quad"/>.
    /// </summary>
    /// <param name="other">The <see cref="Quad"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the quad.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Quad.Quad other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Rect"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rect"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the rectangle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Rect.Rect other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Polygon"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polygon"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the polygon.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Polyline"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the polyline.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="other"/>.
    /// </summary>
    /// <param name="other">The <see cref="ClosestPointResult"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the segments.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Segments"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segments.Segments other)
    {
        if (Count <= 0) return new();

        var closestOther = new CollisionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            
            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()), 
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    #endregion
    
    #region Transform
    /// <summary>
    /// Sets the position of all points in the collection by translating them so that the specified <paramref name="origin"/> moves to <paramref name="newPosition"/>.
    /// </summary>
    /// <param name="newPosition">The new position to which the origin point will be moved.</param>
    /// <param name="origin">The reference origin point in the current collection to be moved to <paramref name="newPosition"/>.</param>
    /// <remarks>
    /// This method translates all points in the collection by the vector difference between <paramref name="newPosition"/> and <paramref name="origin"/>.
    /// Useful for repositioning shapes or point clouds relative to a specific anchor point.
    /// </remarks>
    public void SetPosition(Vector2 newPosition, Vector2 origin)
    {
        var delta = newPosition - origin;
        ChangePosition(delta);
    }
    /// <summary>
    /// Translates all points in the collection by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move every point in the collection.</param>
    /// <remarks>
    /// This method shifts all points by the same amount, effectively moving the entire shape or point cloud in 2D space.
    /// </remarks>
    public void ChangePosition(Vector2 offset)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i] += offset;
        }
    }
    /// <summary>
    /// Rotates all points in the collection around a specified origin by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <remarks>
    /// This method applies a uniform rotation to all points, preserving their relative distances from the origin.
    /// </remarks>
    public void ChangeRotation(float rotRad, Vector2 origin)
    {
        if (Count < 2) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }
    /// <summary>
    /// Sets the absolute rotation of the points around a specified origin to a given angle in radians.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <remarks>
    /// This method computes the shortest rotation needed to align the first point with the specified angle, then applies that rotation to all points relative to the origin.
    /// </remarks>
    public void SetRotation(float angleRad, Vector2 origin)
    {
        if (Count < 2) return;

        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }
    
    /// <summary>
    /// Scales the distance of all points from a specified origin by a uniform scalar value.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to all points relative to the origin. Values greater than 1 enlarge, less than 1 shrink.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <remarks>
    /// This method multiplies the distance of each point from the origin by the specified scale, preserving the shape's proportions.
    /// </remarks>
    public void ScaleSize(float scale, Vector2 origin)
    {
        if (Count < 2) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }
    /// <summary>
    /// Scales the distance of all points from a specified origin by a non-uniform (per-axis) scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the origin, per axis. For example, (2, 1) doubles the width but keeps the height unchanged.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <remarks>
    /// This method multiplies the distance of each point from the origin by the specified scale vector, allowing for stretching or compressing the shape along each axis independently.
    /// </remarks>
    public void ScaleSize(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return;// new();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
        //return path;
    }
    /// <summary>
    /// Changes the size of the shape or point cloud by modifying the length of each point's distance from the origin.
    /// </summary>
    /// <param name="amount">The amount by which to change the length of each point's distance. Positive values increase size, negative values decrease size.</param>
    /// <param name="origin">The point from which size is adjusted.</param>
    /// <remarks>
    /// This method effectively scales the shape or point cloud, altering its size while maintaining its overall form.
    /// </remarks>
    public void ChangeSize(float amount, Vector2 origin)
    {
        if (Count < 2) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
        
    }
    /// <summary>
    /// Sets the size of the shape or point cloud by adjusting the distance of each point from the origin to a specified value.
    /// </summary>
    /// <param name="size">The target distance from the origin for each point. All points will be set to this distance from the origin, standardizing the shape's size.</param>
    /// <param name="origin">The reference point from which distances are measured and set.</param>
    /// <remarks>
    /// This method standardizes the size of the shape or point cloud, making all points equidistant from the origin by the specified length.
    /// </remarks>
    public void SetSize(float size, Vector2 origin)
    {
        if (Count < 2) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }

    }

    /// <summary>
    /// Sets the position, rotation, and size of the shape or point cloud using the specified transform and origin.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> containing the target position, rotation (in radians), and scaled size.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <remarks>
    /// This method applies translation, rotation, and scaling in sequence to all points, aligning the shape with the given transform relative to the specified origin.
    /// </remarks>
    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        SetPosition(transform.Position, origin);
        SetRotation(transform.RotationRad, origin);
        SetSize(transform.ScaledSize.Length, origin);
    }
    /// <summary>
    /// Applies an offset transform to the shape or point cloud, modifying its position, rotation, and size relative to the specified origin.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> containing the position, rotation (in radians), and scaled size offsets to apply.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <remarks>
    /// This method applies translation, rotation, and scaling offsets in sequence to all points, modifying the shape relative to the given origin.
    /// </remarks>
    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        ChangePosition(offset.Position);
        ChangeRotation(offset.RotationRad, origin);
        ChangeSize(offset.ScaledSize.Length, origin);
        
    }
    
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points translated so that the specified origin moves to a new position.
    /// </summary>
    /// <param name="newPosition">The new position to which the origin point will be moved.</param>
    /// <param name="origin">The reference origin point in the current collection to be moved to <paramref name="newPosition"/>.</param>
    /// <returns>A new <see cref="Points"/> instance with translated points, or <c>null</c> if the collection has fewer than 2 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points translated by the vector difference between <paramref name="newPosition"/> and <paramref name="origin"/>.
    /// </remarks>
    public Points? SetPositionCopy(Vector2 newPosition, Vector2 origin)
    {
        if (Count < 2) return null;
        var delta = newPosition - origin;
        return ChangePositionCopy(delta);
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points translated by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move every point in the collection.</param>
    /// <returns>A new <see cref="Points"/> instance with translated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points shifted by the given offset.
    /// </remarks>
    public Points? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }
    
        return newPolygon;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points rotated around a specified origin by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <returns>A new <see cref="Points"/> instance with rotated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points rotated by the specified angle around the given origin.
    /// </remarks>
    public Points? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }
    
        return newPolygon;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points rotated around a specified origin to a given angle in radians.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <returns>A new <see cref="Points"/> instance with rotated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points rotated to the specified angle around the given origin.
    /// </remarks>
    public Points? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points scaled uniformly from a specified origin.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to all points relative to the origin. Values greater than 1 enlarge, less than 1 shrink.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <returns>A new <see cref="Points"/> instance with scaled points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points scaled by the given factor from the specified origin.
    /// </remarks>
    public Points? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add( origin + w * scale);
        }
    
        return newPolygon;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points scaled from the specified origin by a non-uniform (per-axis) scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the origin, per axis. For example, (2, 1) doubles the width but keeps the height unchanged.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <returns>A new <see cref="Points"/> instance with scaled points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points scaled by the given factor from the specified origin.
    /// </remarks>
    public Points? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }
    
        return newPolygon;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points' distances from the specified origin changed by the given amount.
    /// </summary>
    /// <param name="amount">The amount by which to change the length of each point's distance from the origin. Positive values increase size, negative values decrease size.</param>
    /// <param name="origin">The point from which size is adjusted.</param>
    /// <returns>A new <see cref="Points"/> instance with modified point distances, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points' distances from the origin changed by the specified amount.
    /// </remarks>
    public Points? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.ChangeLength(amount));
        }
    
        return newPolygon;
    
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points set to a specified distance from the origin.
    /// </summary>
    /// <param name="size">The target distance from the origin for each point.</param>
    /// <param name="origin">The reference point from which distances are measured and set.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points set to the specified distance from the origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points equidistant from the origin.
    /// </remarks>
    public Points? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon.Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.SetLength(size));
        }
    
        return newPolygon;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points transformed by the specified <see cref="Transform2D"/> and origin.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> containing the target position, rotation (in radians), and scaled size.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points transformed by the given transform and origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance. It applies translation, rotation, and scaling in sequence to all points,
    /// aligning the shape with the given transform relative to the specified origin.
    /// </remarks>
    public Points? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPoints = SetPositionCopy(transform.Position, origin);
        if (newPoints == null) return null;
        newPoints.SetRotation(transform.RotationRad, origin);
        newPoints.SetSize(transform.ScaledSize.Length, origin);
        return newPoints;
    }
    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points transformed by the specified offset <see cref="Transform2D"/> and origin.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> containing the position, rotation (in radians), and scaled size offsets to apply.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points transformed by the given offset and origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance.
    /// It applies translation, rotation, and scaling offsets in sequence to all points,
    /// modifying the shape relative to the given origin.
    /// </remarks>
    public Points? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;
        
        var newPoints = ChangePositionCopy(offset.Position);
        if (newPoints == null) return null;
        newPoints.ChangeRotation(offset.RotationRad, origin);
        newPoints.ChangeSize(offset.ScaledSize.Length, origin);
        return newPoints;
    }
    #endregion
    
    #region Static
    /// <summary>
    /// Applies the floor operation to each <see cref="Vector2"/> in the provided list,
    /// modifying each coordinate to the largest integer less than or equal to it.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be floored.</param>
    public static void Floor(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Floor();
        }
    }
    /// <summary>
    /// Applies the ceiling operation to each <see cref="Vector2"/> in the provided list,
    /// modifying each coordinate to the smallest integer greater than or equal to it.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be ceiled.</param>
    public static void Ceiling(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Ceiling();
        }
    }
    /// <summary>
    /// Rounds each <see cref="Vector2"/> in the provided list to the nearest integer values for both coordinates.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be rounded.</param>
    public static void Round(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Round();
        }
    }
    /// <summary>
    /// Truncates each <see cref="Vector2"/> in the provided list, removing the fractional part of each coordinate.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be truncated.</param>
    public static void Truncate(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Truncate();
        }
    }
    #endregion
    
    #region Interpolated Edge Points

    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// Interplates between last and first point as well (closed shape)
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <returns></returns>
    public Points? GetInterpolatedEdgePoints(float t)
    {
        if (Count < 2) return null;

        var result = new Points();
        for (int i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = this[(i + 1) % Count];
            var interpolated = cur.Lerp(next, t);// Vector2.Lerp(cur, next, t);
            result.Add(interpolated);
        }
        
        return result;
    }
    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// Interplates between last and first point as well (closed shape)
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <param name="steps">Recursive steps. The amount of times the result of InterpolatedEdgesPoints will be run through InterpolateEdgePoints.</param>
    /// <returns></returns>
    public Points? GetInterpolatedEdgePoints(float t, int steps)
    {
        if (Count < 2) return null;
        if (steps <= 1) return GetInterpolatedEdgePoints(t);

        int remainingSteps = steps;
        var result = new Points();
        var buffer = new Points();
        while (remainingSteps > 0)
        {
            var target = result.Count <= 0 ? this : result;
            for (int i = 0; i < target.Count; i++)
            {
                var cur = target[i];
                var next = target[(i + 1) % target.Count];
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
    
    #region Sort
    /// <summary>
    /// Sorts the points in the collection so that the closest points to the specified <paramref name="referencePoint"/> come first.
    /// </summary>
    /// <param name="referencePoint">The point to which distances are measured for sorting.</param>
    /// <returns>True if sorting was performed or not needed; false if the collection is empty.</returns>
    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if(Count == 1) return true;
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a).LengthSquared();
                float lb = (referencePoint - b).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    /// <summary>
    /// Sorts the points in the collection so that the furthest points from the specified <paramref name="referencePoint"/> come first.
    /// </summary>
    /// <param name="referencePoint">The point to which distances are measured for sorting.</param>
    /// <returns>True if sorting was performed or not needed; false if the collection is empty.</returns>
    public bool SortFurthestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if(Count == 1) return true;
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a).LengthSquared();
                float lb = (referencePoint - b).LengthSquared();

                if (la < lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    #endregion
}

