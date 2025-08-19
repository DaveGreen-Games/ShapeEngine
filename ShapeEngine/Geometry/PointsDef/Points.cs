using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.StaticLib;
using Game = ShapeEngine.Core.GameDef.Game;

namespace ShapeEngine.Geometry.PointsDef;

/// <summary>
/// Represents a collection of 2D points and provides various geometric and transformation operations.
/// </summary>
/// <remarks>
/// This class is used for manipulating and analyzing sets of points in 2D space, including operations such as finding closest points, transforming, and interpolating between points.
/// </remarks>
public partial class Points : ShapeList<Vector2>, IEquatable<Points>
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
    
    #region Shapes

    /// <inheritdoc />
    public override Points Copy() => new(this);

    /// <summary>
    /// Converts this collection of points into a <see cref="Polygon"/> shape.
    /// </summary>
    /// <returns>A new <see cref="Polygon"/> instance containing the same points.</returns>
    public Polygon ToPolygon() => new(this);

    public bool ToPolygon(ref Polygon result)
    {
        if (Count < 3) return false;
        
        if(result.Count > 0) result.Clear();
        
        for (var i = 0; i < Count; i++)
        {
            var point = this[i];
            result.Add(point);
        }

        return true;
    }

    /// <summary>
    /// Converts this collection of points into a <see cref="Polyline"/> shape.
    /// </summary>
    /// <returns>A new <see cref="Polyline"/> instance containing the same points.</returns>
    public Polyline ToPolyline() => new(this);

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
    public (Transform2D transform, Polygon shape) ToRelative(Vector2 center)
    {
        var maxLengthSq = 0f;
        for (int i = 0; i < this.Count; i++)
        {
            var lsq = (this[i] - center).LengthSquared();
            if (maxLengthSq < lsq) maxLengthSq = lsq;
        }

        var size = MathF.Sqrt(maxLengthSq);

        var relativeShape = new Polygon();
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

