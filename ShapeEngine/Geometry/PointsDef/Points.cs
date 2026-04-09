using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;

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
    
    protected static Points pointsBuffer = new();
    private static Segments segmentsBuffer1 = new();
    private static Segments segmentsBuffer2 = new();
    private static HashSet<Vector2> uniquePointsBuffer = new();
    
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
    /// <summary>
    /// Creates a stable 64-bit hash key for the current point collection by hashing points in order.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize point coordinates before hashing.</param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;

        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        ulong hash = hashQuantizer.StartHash(Count);
        for (int i = 0; i < Count; i++)
        {
            hash = hashQuantizer.Add(hash, this[i]);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of the current point collection hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize point coordinates before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of the current point collection hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize point coordinates before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Returns a 32-bit hash code derived from the stable 64-bit point collection hash key.
    /// </summary>
    /// <returns>A 32-bit hash code for the current point collection.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }
    
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
        return Equals(other, DecimalPrecision.DefaultDecimalPlaces);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Points"/> instance is equal to the current instance using quantized comparison.
    /// </summary>
    /// <param name="other">The <see cref="Points"/> instance to compare with the current instance.</param>
    /// <param name="decimalPlaces">The number of decimal places used to quantize point coordinates before comparison.</param>
    /// <returns>True if the specified <see cref="Points"/> is equal to the current instance after quantization; otherwise, false.</returns>
    public bool Equals(Points? other, int decimalPlaces)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;

        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;
        DecimalQuantizer quantizer = new(decimalPlaces);
        for (var i = 0; i < Count; i++)
        {
            if (!quantizer.QuantizedEquals(this[i], other[i])) return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current point collection.
    /// </summary>
    /// <param name="obj">The object to compare with the current point collection.</param>
    /// <returns>True if the specified object is a <see cref="Points"/> instance equal to the current one; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Points other && Equals(other);
    }

    /// <summary>
    /// Determines whether two point collections are equal.
    /// </summary>
    /// <param name="left">The first point collection to compare.</param>
    /// <param name="right">The second point collection to compare.</param>
    /// <returns>True if the point collections are equal; otherwise, false.</returns>
    public static bool operator ==(Points? left, Points? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two point collections are not equal.
    /// </summary>
    /// <param name="left">The first point collection to compare.</param>
    /// <param name="right">The second point collection to compare.</param>
    /// <returns>True if the point collections are not equal; otherwise, false.</returns>
    public static bool operator !=(Points? left, Points? right)
    {
        return !(left == right);
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
        uniquePointsBuffer.Clear();
        uniquePointsBuffer.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            uniquePointsBuffer.Add(this[i]);
        }
        
        var result = new Points(uniquePointsBuffer.Count);
        result.AddRange(uniquePointsBuffer);
        return result;
    }
    
    /// <summary>
    /// Collects all unique points from this collection and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique points.</param>
    /// <remarks>
    /// This method does not modify the current collection. Point uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void GetUniquePoints(Points result)
    {
        uniquePointsBuffer.Clear();
        uniquePointsBuffer.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            uniquePointsBuffer.Add(this[i]);
        }
        
        result.Clear();
        result.EnsureCapacity(uniquePointsBuffer.Count);
        result.AddRange(uniquePointsBuffer);
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
    public List<Vector2> GetRandomPoints(int amount)
    {
        return GetRandomItems(amount);
    }

    /// <summary>
    /// Selects random points from this collection and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination list that will receive the selected points.</param>
    /// <param name="amount">The number of random points to select.</param>
    /// <returns>The number of points written to <paramref name="result"/>.</returns>
    public int GetRandomPoints(List<Vector2> result, int amount)
    {
        return GetRandomItems(result, amount);
    }
    #endregion
    
    #region Shapes

    /// <inheritdoc />
    public override Points Copy() => new(this);

    /// <summary>
    /// Converts this collection of points into a <see cref="Polygon"/> shape.
    /// </summary>
    /// <returns>A new <see cref="Polygon"/> instance containing the same points.</returns>
    public Polygon ToPolygon() => new(this);

    /// <summary>
    /// Copies this point collection into the specified <see cref="Polygon"/> instance.
    /// </summary>
    /// <param name="result">The destination polygon that will receive the points from this collection.</param>
    /// <returns><c>true</c> if this collection contains at least three points and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. If the conversion succeeds, any existing points in <paramref name="result"/> are cleared before the new points are added.
    /// </remarks>
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

    /// <summary>
    /// Copies this point collection into the specified <see cref="Polyline"/> instance.
    /// </summary>
    /// <param name="result">The destination polyline that will receive the points from this collection.</param>
    /// <returns><c>true</c> if this collection contains at least three points and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. If the conversion succeeds, any existing points in <paramref name="result"/> are cleared before the new points are added.
    /// </remarks>
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
    
    /// <summary>
    /// Normalizes this point collection relative to the specified <paramref name="center"/> and writes the normalized points into <paramref name="result"/>.
    /// </summary>
    /// <param name="center">The center point used as the origin for normalization.</param>
    /// <param name="result">The destination polygon that will be cleared and populated with the normalized points.</param>
    /// <returns>A <see cref="Transform2D"/> whose position is <paramref name="center"/> and whose size stores the maximum point distance from that center.</returns>
    /// <remarks>
    /// This method does not modify the current instance. Each output point is computed by subtracting <paramref name="center"/> from the source point and dividing by the maximum distance from <paramref name="center"/> to any point in the collection.
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

    /// <summary>
    /// Writes all points offset relative to the specified <paramref name="origin"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <param name="result">The destination list that will be cleared and populated with the relative points.</param>
    public void GetRelativeVector2List(Vector2 origin, List<Vector2> result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(p - origin);
    }
    
    /// <summary>
    /// Writes all points transformed into the local space of the specified <paramref name="transform"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <param name="result">The destination list that will be cleared and populated with the transformed points.</param>
    public void GetRelativeVector2List(Transform2D transform, List<Vector2> result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(transform.RevertPosition(p));
    }
    
    /// <summary>
    /// Writes all points offset relative to the specified <paramref name="origin"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="origin">The origin to subtract from each point.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the relative points.</param>
    public void GetRelativePoints(Vector2 origin, Points result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(p - origin);
    }
    
    /// <summary>
    /// Writes all points transformed into the local space of the specified <paramref name="transform"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="transform">The transform to revert each point by.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed points.</param>
    public void GetRelativePoints(Transform2D transform, Points result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var p in this)  result.Add(transform.RevertPosition(p));
    }

    #endregion

    #region Interpolated Edge Points
    /// <summary>
    /// Computes one interpolated point along each edge of the closed point loop and writes the results into <paramref name="result"/>.
    /// </summary>
    /// <param name="t">The interpolation factor used for each edge. Values between 0 and 1 produce points between each vertex and the next vertex.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the interpolated points.</param>
    /// <remarks>
    /// The last point is interpolated toward the first point, so the input is treated as a closed shape. If the collection contains fewer than two points, the method returns without modifying <paramref name="result"/>.
    /// </remarks>
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
    
    /// <summary>
    /// Repeatedly computes interpolated edge points for the closed point loop and writes the final result into <paramref name="result"/>.
    /// </summary>
    /// <param name="t">The interpolation factor used for each edge on every pass. Values between 0 and 1 produce points between each vertex and the next vertex.</param>
    /// <param name="steps">The number of interpolation passes to perform. Values less than or equal to 1 perform a single pass.</param>
    /// <param name="result">The destination collection that will receive the interpolated points.</param>
    /// <remarks>
    /// The last point is interpolated toward the first point on each pass, so the input is treated as a closed shape. If the collection contains fewer than two points, the method returns without modifying <paramref name="result"/>.
    /// </remarks>
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

    /// <summary>
    /// Computes an axis-aligned bounding rectangle that encloses the points in this collection.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/> for the point cloud, or the default rectangle if the collection contains fewer than two points.</returns>
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

    /// <summary>
    /// Computes a triangle that encloses the current point cloud, optionally expanded by a margin factor.
    /// </summary>
    /// <param name="marginFactor">A multiplier applied to the bounding box extent when constructing the enclosing triangle.</param>
    /// <returns>A <see cref="Triangle"/> that surrounds the current point cloud.</returns>
    /// <remarks>
    /// This is typically used as a supra-triangle when triangulating the point cloud.
    /// </remarks>
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

    /// <summary>
    /// Triangulates this point cloud and writes the resulting triangles into <paramref name="result"/> using an automatically generated supra-triangle.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the generated triangles.</param>
    /// <param name="marginFactor">A multiplier applied when generating the enclosing supra-triangle.</param>
    public void TriangulatePointCloud(Triangulation result, float marginFactor = 2f)
    {
        var supraTriangle = GetPointCloudBoundingTriangle(marginFactor);
        TriangulatePointCloud(supraTriangle, result);
    }
    
    /// <summary>
    /// Triangulates this point cloud and writes the resulting triangles into <paramref name="result"/> using the specified supra-triangle.
    /// </summary>
    /// <param name="supraTriangle">An enclosing triangle large enough to contain all points in the cloud.</param>
    /// <param name="result">The destination triangulation that will be cleared and populated with the generated triangles.</param>
    /// <remarks>
    /// This method removes any triangles that still share a vertex with <paramref name="supraTriangle"/> before returning.
    /// </remarks>
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

    #region Convex Hull

    // Convex Hull Algorithms
    // This class implements the Jarvis March (Gift Wrapping) algorithm to find the convex hull of a set of points.
    // Reference: https://github.com/allfii/ConvexHull/tree/master
    
    // Alternative algorithms for convex hull computation:
    // - Graham scan: https://en.wikipedia.org/wiki/Graham_scan
    // - Chan's algorithm: https://en.wikipedia.org/wiki/Chan%27s_algorithm
    
    // Gift Wrapping algorithm resources:
    // - Coding Train video: https://www.youtube.com/watch?v=YNyULRrydVI
    // - Wikipedia: https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
    
    private static int Turn_JarvisMarch(Vector2 p, Vector2 q, Vector2 r)
    {
        return ((q.X - p.X) * (r.Y - p.Y) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0);
        // return ((q.getX() - p.getX()) * (r.getY() - p.getY()) - (r.getX() - p.getX()) * (q.getY() - p.getY())).CompareTo(0);
    }
    private static Vector2 NextHullPoint_JarvisMarch(IReadOnlyList<Vector2> points, Vector2 p)
    {
        // const int TurnLeft = 1;
        const int turnRight = -1;
        const int turnNone = 0;
        var q = p;
        int t;
        foreach (var r in points)
        {
            t = Turn_JarvisMarch(p, q, r);
            if (t == turnRight || t == turnNone && p.DistanceSquared(r) > p.DistanceSquared(q)) // dist(p, r) > dist(p, q))
                q = r;
        }

        return q;
    }

    /// <summary>
    /// Finds the convex hull of a set of points using the Jarvis March algorithm.
    /// </summary>
    /// <param name="points">The list of points to compute the convex hull for.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    public static Points ConvexHull_JarvisMarch(IReadOnlyList<Vector2> points)
    {
        var hull = new Points();
        if (points.Count <= 3)
        {
            foreach (var p in points)
            {
                hull.Add(p);
            }
            return hull;
        }
        
        foreach (var p in points)
        {
            if (hull.Count == 0)
                hull.Add(p);
            else
            {
                if (hull[0].X > p.X)
                    hull[0] = p;
                else if (ShapeMath.EqualsF(hull[0].X, p.X))
                    if (hull[0].Y > p.Y)
                        hull[0] = p;
            }
        }

        var counter = 0;
        while (counter < hull.Count)
        {
            var q = NextHullPoint_JarvisMarch(points, hull[counter]);
            if (q != hull[0])
            {
                hull.Add(q);
            }

            counter++;
        }

        return hull;
    }
    
    /// <summary>
    /// Computes the convex hull of the specified points using the Jarvis March algorithm and writes the hull vertices into <paramref name="result"/>.
    /// </summary>
    /// <param name="points">The points for which to compute the convex hull.</param>
    /// <param name="result">The destination list that will be cleared and populated with the hull vertices in traversal order.</param>
    /// <remarks>
    /// If <paramref name="points"/> contains three or fewer points, they are copied directly into <paramref name="result"/>.
    /// </remarks>
    public static void ConvexHull_JarvisMarch(IReadOnlyList<Vector2> points, List<Vector2> result)
    {
        result.Clear();
        result.EnsureCapacity(points.Count);
        if (points.Count <= 3)
        {
            foreach (var p in points)
            {
                result.Add(p);
            }
            return;
        }
        
        foreach (var p in points)
        {
            if (result.Count == 0)
                result.Add(p);
            else
            {
                if (result[0].X > p.X)
                    result[0] = p;
                else if (ShapeMath.EqualsF(result[0].X, p.X))
                    if (result[0].Y > p.Y)
                        result[0] = p;
            }
        }

        var counter = 0;
        while (counter < result.Count)
        {
            var q = NextHullPoint_JarvisMarch(points, result[counter]);
            if (q != result[0])
            {
                result.Add(q);
            }

            counter++;
        }
    }

    /// <summary>
    /// Computes the convex hull of this point collection.
    /// </summary>
    /// <returns>A new <see cref="Points"/> instance containing the convex hull vertices in traversal order.</returns>
    /// <remarks>
    /// This method uses the Jarvis March algorithm and does not modify the current collection.
    /// </remarks>
    public Points FindConvexHull()
    {
        var poly = new Polyline();
        poly.FindConvexHull(poly);
        
        return ConvexHull_JarvisMarch(this);
    }
    
    /// <summary>
    /// Computes the convex hull of this point collection and writes the hull vertices into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination list that will be cleared and populated with the convex hull vertices in traversal order.</param>
    /// <remarks>
    /// This method uses the Jarvis March algorithm and does not modify the current collection.
    /// </remarks>
    public void FindConvexHull(List<Vector2> result)
    {
        ConvexHull_JarvisMarch(this, result);
    }
    #endregion
    
    /// <summary>
    /// Draws a circle at each vertex in this collection using the provided radius, color and smoothness.
    /// </summary>
    /// <param name="vertexRadius">Radius of the drawn circle for each vertex (in world units).</param>
    /// <param name="color">Color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="Circle.CircleSideLengthRange"/>.
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

