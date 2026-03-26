using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
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
    #region Helper
    private static Points pointsBuffer = new();
    private static Segments segmentsBuffer1 = new();
    private static Segments segmentsBuffer2 = new();
    

    #endregion
    
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

    //TODO: Docs
    public bool ToPolygon(Polygon result)
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

    //TODO: Docs
    public bool ToPolyline(Polyline result)
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
    
    //TODO: Update Docs
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
    public Transform2D ToRelative(Vector2 center, Polygon result)
    {
        var maxLengthSq = 0f;
        for (int i = 0; i < this.Count; i++)
        {
            var lsq = (this[i] - center).LengthSquared();
            if (maxLengthSq < lsq) maxLengthSq = lsq;
        }

        var size = MathF.Sqrt(maxLengthSq);
        
        result.Clear();
        result.EnsureCapacity(this.Count);
        for (int i = 0; i < this.Count; i++)
        {
            var w = this[i] - center;
            result.Add(w / size); //transforms it to range 0 - 1
        }

        return (new Transform2D(center, 0f, new Size(size, 0f), 1f));
    }

    //TODO: Update Docs
    /// <summary>
    /// Returns a list of points relative to the specified origin.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <returns>A list of <see cref="Vector2"/> points relative to the origin.</returns>
    public void GetRelativeVector2List(Vector2 origin, List<Vector2> result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(p - origin);
    }
    
    //TODO: Update Docs
    /// <summary>
    /// Returns a list of points relative to the specified transform.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <returns>A list of <see cref="Vector2"/> points relative to the transform.</returns>
    public void GetRelativeVector2List(Transform2D transform, List<Vector2> result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(transform.RevertPosition(p));
    }
    
    //TODO: Update Docs
    /// <summary>
    /// Returns a new <see cref="Points"/> collection with all points relative to the specified origin.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <returns>A new <see cref="Points"/> instance with points relative to the origin.</returns>
    public void GetRelativePoints(Vector2 origin, Points result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(p - origin);
    }
    
    //TODO: Update Docs
    /// <summary>
    /// Returns a new <see cref="Points"/> collection with all points relative to the specified transform.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <returns>A new <see cref="Points"/> instance with points relative to the transform.</returns>
    public void GetRelativePoints(Transform2D transform, Points result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(transform.RevertPosition(p));
    }

    #endregion

    #region Interpolated Edge Points
    //TODO: Update Docs
    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// Interplates between last and first point as well (closed shape)
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <returns></returns>
    public void GetInterpolatedEdgePoints(float t, Points result)
    {
        if (Count < 2) return;

        result.Clear();
        result.EnsureCapacity(Count);
        for (int i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = this[(i + 1) % Count];
            var interpolated = cur.Lerp(next, t);
            result.Add(interpolated);
        }
    }

    
    //TODO: Update Docs
    /// <summary>
    /// Interpolate the edge(segment) between each pair of points using t and return the new interpolated points.
    /// Interplates between last and first point as well (closed shape)
    /// </summary>
    /// <param name="t">The value t for interpolation. Should be between 0 - 1.</param>
    /// <param name="steps">Recursive steps. The amount of times the result of InterpolatedEdgesPoints will be run through InterpolateEdgePoints.</param>
    /// <returns></returns>
    public void GetInterpolatedEdgePoints(float t, int steps, Points result)
    {
        if (Count < 2) return;
        if (steps <= 1)
        {
            GetInterpolatedEdgePoints(t, result);
            return;
        }

        int remainingSteps = steps;
        result.Clear();
        while (remainingSteps > 0)
        {
            var target = result.Count <= 0 ? this : result;
            target.GetInterpolatedEdgePoints(t, pointsBuffer);
            result.Clear();
            result.AddRange(pointsBuffer);
            pointsBuffer.Clear();
            remainingSteps--;
        }
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
    
    #region Triangulate Point Cloud

    //TODO: Docs
    public Rect GetPointCloudBoundingBox()
    {
        if (Count < 2) return new();
        var start = this[0];
        Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in this)
        {
            r = r.Enlarge(p);
        }

        return r;
    }

    //TODO: Docs
    public Triangle GetPointCloudBoundingTriangle(float marginFactor = 1f)
    {
        var bounds = GetPointCloudBoundingBox();
        float dMax = bounds.Size.Max() * marginFactor; 
        var center = bounds.Center;
        var a = new Vector2(center.X, bounds.BottomLeft.Y + dMax);
        var b = new Vector2(center.X - dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        var c = new Vector2(center.X + dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        
        return new Triangle(a, b, c);
    }

    //TODO: Docs
    public void TriangulatePointCloud(Triangulation result, float marginFactor = 2f)
    {
        var supraTriangle = GetPointCloudBoundingTriangle(marginFactor);
        TriangulatePointCloud(supraTriangle, result);
    }
    
    //TODO: Docs
    public void TriangulatePointCloud(Triangle supraTriangle, Triangulation result)
    {
        result.Clear();
        result.Add(supraTriangle);

        foreach (var p in this)
        {
            Triangulation badTriangles = new();

            //Identify 'bad triangles'
            for (int triIndex = result.Count - 1; triIndex >= 0; triIndex--)
            {
                Triangle triangle = result[triIndex];

                //A 'bad triangle' is defined as a triangle who's CircumCentre contains the current point
                var circumCircle = triangle.GetCircumCircle();
                float distSq = Vector2.DistanceSquared(p, circumCircle.Center);
                if (distSq < circumCircle.Radius * circumCircle.Radius)
                {
                    badTriangles.Add(triangle);
                    result.RemoveAt(triIndex);
                }
            }

            segmentsBuffer1.Clear();
            segmentsBuffer1.EnsureCapacity(badTriangles.Count * 3);
            foreach (var badTriangle in badTriangles)
            {
                segmentsBuffer1.AddRange(badTriangle.GetEdges());
            }

            segmentsBuffer1.GetUniqueSegments(segmentsBuffer2);
            //Create new triangles
            for (int i = 0; i < segmentsBuffer2.Count; i++)
            {
                var edge = segmentsBuffer2[i];
                result.Add(new(p, edge));
            }
        }

        //Remove all triangles that share a vertex with the supra triangle to recieve the final triangulation
        for (int i = result.Count - 1; i >= 0; i--)
        {
            var t = result[i];
            if (t.SharesVertex(supraTriangle)) result.RemoveAt(i);
        }
    }
    
    #endregion
    
    /// <summary>
    /// Draws a circle at each vertex in this collection using the provided radius, color and segment count.
    /// </summary>
    /// <param name="vertexRadius">Radius of the drawn circle for each vertex (in world units).</param>
    /// <param name="color">Color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public void DrawVertices(float vertexRadius, ColorRgba color, float smoothness)
    {
        foreach (var p in this)
        {
            var circle = new Circle(p, vertexRadius);
            circle.Draw(color, smoothness);
        }
    }
}

