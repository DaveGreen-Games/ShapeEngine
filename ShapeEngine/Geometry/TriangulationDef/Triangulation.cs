using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangulationDef;

/// <summary>
/// Represents a collection of triangles with utility methods for:
/// <list type="bullet">
/// <item>Performing triangulation operations</item>
/// <item>Finding the closest point or triangle to a given location</item>
/// <item>Checking if a point is contained within any triangle</item>
/// <item>Detecting intersections with other shapes or colliders</item>
/// </list>
/// </summary>
public partial class Triangulation : ShapeList<Triangle>, IEquatable<Triangulation>
{
    #region Helper

    private static Triangulation queueBuffer = new();
    private static Triangulation subdivBuffer = new();
    private static HashSet<Vector2> uniquePointsBuffer = new();
    private static HashSet<Segment> uniqueSegmentsBuffer = new();
    private static HashSet<Triangle> uniqueTrianglesBuffer = new();
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Triangulation"/> class.
    /// </summary>
    public Triangulation()
    {
        DecimalPlaces = ShapeClipper2D.DecimalPlaces;
    }
    
    /// <summary>
    /// Initializes a new empty instance of the <see cref="Triangulation"/> class with the specified capacity and optional hash/equality precision.
    /// </summary>
    /// <param name="capacity">The initial number of triangles the collection can store without resizing.</param>
    /// <param name="decimalPlaces">The decimal precision used for quantized hashing and equality comparisons. Pass a negative value to use <see cref="ShapeClipper2D.DecimalPlaces"/>.</param>
    public Triangulation(int capacity, int decimalPlaces = -1) : base(capacity)
    {
        DecimalPlaces = decimalPlaces <= 0 ? ShapeClipper2D.DecimalPlaces : decimalPlaces;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Triangulation"/> class with the specified triangles and optional hash/equality precision.
    /// </summary>
    /// <param name="triangles">The triangles to add to the collection in enumeration order.</param>
    /// <param name="decimalPlaces">The decimal precision used for quantized hashing and equality comparisons. Pass a negative value to use <see cref="ShapeClipper2D.DecimalPlaces"/>.</param>
    public Triangulation(IEnumerable<Triangle> triangles, int decimalPlaces = -1)
    {
        DecimalPlaces = decimalPlaces <= 0 ? ShapeClipper2D.DecimalPlaces : decimalPlaces;
        AddRange(triangles);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Triangulation"/> class from a flat vertex list and optional hash/equality precision.
    /// </summary>
    /// <param name="vertices">The vertex list whose values are grouped in consecutive triples to form triangles.</param>
    /// <param name="decimalPlaces">The decimal precision used for quantized hashing and equality comparisons. Pass a negative value to use <see cref="ShapeClipper2D.DecimalPlaces"/>.</param>
    /// <remarks>Any trailing vertices that do not form a complete triangle are ignored.</remarks>
    public Triangulation(IReadOnlyList<Vector2> vertices, int decimalPlaces = -1)
    {
        DecimalPlaces = decimalPlaces <= 0 ? ShapeClipper2D.DecimalPlaces : decimalPlaces;
        for (int i = 0; i < vertices.Count; i += 3)
        {
            if (i + 2 >= vertices.Count) break;
            Add(new Triangle(vertices[i], vertices[i + 1], vertices[i + 2]));
        }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the decimal precision used for quantized hashing and equality comparisons.
    /// </summary>
    public int DecimalPlaces { get; }
    #endregion
        
    #region Equals & HashCode
    /// <summary>
    /// Creates a stable 64-bit hash key for the current triangulation by hashing triangle vertices in order.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this triangulation's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = -1)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPlaces;

        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        ulong hash = hashQuantizer.StartHash(Count);
        for (int i = 0; i < Count; i++)
        {
            Triangle triangle = this[i];
            hash = hashQuantizer.Add(hash, triangle.A);
            hash = hashQuantizer.Add(hash, triangle.B);
            hash = hashQuantizer.Add(hash, triangle.C);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of the current triangulation hash key.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this triangulation's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = -1) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of the current triangulation hash key.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this triangulation's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = -1) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Returns a 32-bit hash code derived from the stable 64-bit triangulation hash key.
    /// </summary>
    /// <returns>A 32-bit hash code for the current triangulation.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }

    /// <summary>
    /// Determines whether the specified <see cref="Triangulation"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The triangulation to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified triangulation is equal to the current instance; otherwise, <c>false</c>.</returns>
    /// <remarks>The comparison uses the coarser precision of the two triangulations and compares triangles in order.</remarks>
    public bool Equals(Triangulation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Count != other.Count) return false;

        int decimalPlaces = Math.Max(DecimalPlaces, other.DecimalPlaces);
        for (var i = 0; i < Count; i++)
        {
            if (!this[i].Equals(other[i], decimalPlaces)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current triangulation.
    /// </summary>
    /// <param name="obj">The object to compare with the current triangulation.</param>
    /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Triangulation"/> equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Triangulation other && Equals(other);
    }

    /// <summary>
    /// Determines whether two triangulations are equal.
    /// </summary>
    /// <param name="left">The first triangulation to compare.</param>
    /// <param name="right">The second triangulation to compare.</param>
    /// <returns><c>true</c> if both triangulations are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Triangulation? left, Triangulation? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two triangulations are not equal.
    /// </summary>
    /// <param name="left">The first triangulation to compare.</param>
    /// <param name="right">The second triangulation to compare.</param>
    /// <returns><c>true</c> if the triangulations are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Triangulation? left, Triangulation? right)
    {
        return !(left == right);
    }
    #endregion

    #region Public
    /// <summary>
    /// Collects all unique triangle vertices in this triangulation and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique vertices.</param>
    /// <remarks>
    /// This method does not modify the current triangulation. Vertex uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void GetUniquePoints(Points result)
    {
        uniquePointsBuffer.Clear();
        uniquePointsBuffer.EnsureCapacity(Count * 3);
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            uniquePointsBuffer.Add(tri.A);
            uniquePointsBuffer.Add(tri.B);
            uniquePointsBuffer.Add(tri.C);
        }
    
        result.Clear();
        result.EnsureCapacity(uniquePointsBuffer.Count);
        result.AddRange(uniquePointsBuffer);
    }
    
    /// <summary>
    /// Collects all unique triangle edges in this triangulation and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique segments.</param>
    /// <remarks>
    /// This method does not modify the current triangulation. Segment uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void GetUniqueSegments(Segments result)
    {
        uniqueSegmentsBuffer.Clear();
        uniqueSegmentsBuffer.EnsureCapacity(Count * 3);
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            uniqueSegmentsBuffer.Add(tri.SegmentAToB);
            uniqueSegmentsBuffer.Add(tri.SegmentBToC);
            uniqueSegmentsBuffer.Add(tri.SegmentCToA);
        }

        result.Clear();
        result.EnsureCapacity(uniqueSegmentsBuffer.Count);
        result.AddRange(uniqueSegmentsBuffer);
    }
    
    /// <summary>
    /// Collects all unique triangles in this triangulation and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the unique triangles.</param>
    /// <remarks>
    /// This method does not modify the current triangulation. Triangle uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void  GetUniqueTriangles(Triangulation result)
    {
        uniqueTrianglesBuffer.Clear();
        uniqueTrianglesBuffer.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            uniqueTrianglesBuffer.Add(this[i]);
        }

        result.Clear();
        result.EnsureCapacity(uniqueTrianglesBuffer.Count);
        result.AddRange(uniqueTrianglesBuffer);
    }
    
    /// <summary>
    /// Finds all triangles in this triangulation that contain the specified point and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the containing triangles.</param>
    /// <param name="p">The point to test for containment.</param>
    public void GetContainingTriangles(Triangulation result, Vector2 p)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.ContainsPoint(p)) result.Add(tri);
        }
    }

    /// <summary>
    /// Gets the segment of a triangle at the specified indices.
    /// </summary>
    /// <param name="triangleIndex">The index of the triangle in the collection.</param>
    /// <param name="segmentIndex">The index of the segment in the triangle (0, 1, or 2).</param>
    /// <returns>The <see cref="Segment"/> at the specified indices.</returns>
    /// <remarks>Indices are wrapped using modulo operation.</remarks>
    public Segment GetSegment(int triangleIndex, int segmentIndex)
    {
        var i = ShapeMath.WrapIndex(Count, triangleIndex);
        // var i = triangleIndex % Count;
        return this[i].GetSegment(segmentIndex);
    }
    
    /// <summary>
    /// Remove all triangles with an area less than the threshold.
    /// </summary>
    /// <param name="areaThreshold">The area threshold. Triangles with an area less than this value will be removed. If the threshold is less than or equal to 0, no triangles are removed.</param>
    /// <returns>The number of triangles removed.</returns>
    /// <remarks>Triangles with area less than <paramref name="areaThreshold"/> are removed from the collection.</remarks>
    public int Remove(float areaThreshold)
    {
        if (areaThreshold <= 0f) return 0;

        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            if (this[i].GetArea() >= areaThreshold) continue;
            RemoveAt(i);
            count++;
        }

        return count;
    }
    #endregion

    #region Triangulation
    
    /// <summary>
    /// Filters this triangulation by triangle area and writes triangles whose area is greater than or equal to <paramref name="areaThreshold"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the filtered triangles.</param>
    /// <param name="areaThreshold">The minimum area a triangle must have to be copied into <paramref name="result"/>.</param>
    /// <remarks>
    /// If <paramref name="areaThreshold"/> is less than or equal to 0, the method returns immediately without modifying <paramref name="result"/>.
    /// </remarks>
    public void Get(Triangulation result, float areaThreshold)
    {
        if (areaThreshold <= 0f) return;

        result.Clear();
        for (int i = Count - 1; i >= 0; i--)
        {
            var t = this[i];
            if (t.GetArea() >= areaThreshold)
            {
                result.Add(t);
            }
        }
    }
    
    /// <summary>
    /// Recursively subdivides triangles in this triangulation until each resulting triangle has an area smaller than <paramref name="minArea"/>, then writes the final triangles into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the subdivided triangles.</param>
    /// <param name="minArea">The area threshold below which a triangle is kept instead of subdivided further.</param>
    /// <remarks>
    /// This method clears <paramref name="result"/> before processing. Triangles with area greater than or equal to <paramref name="minArea"/> are subdivided by calling <c>Triangle.Triangulate</c>.
    /// </remarks>
    public void Subdivide(Triangulation result, float minArea)
    {
        result.Clear();
        queueBuffer.Clear();
        queueBuffer.AddRange(this);
        while (queueBuffer.Count > 0)
        {
            int endIndex = queueBuffer.Count - 1;
            var tri = queueBuffer[endIndex];

            float triArea = tri.GetArea();
            if (triArea < minArea)
            {
                result.Add(tri);
            }
            else
            {
                subdivBuffer.Clear();
                tri.Triangulate(subdivBuffer, minArea);
                queueBuffer.AddRange(subdivBuffer);
            }
            queueBuffer.RemoveAt(endIndex);
        }
    }
    
    /// <summary>
    /// Recursively subdivides triangles in this triangulation using area limits, optional random retention, and narrow-triangle rejection, writing accepted triangles into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the accepted triangles.</param>
    /// <param name="minArea">Triangles with area smaller than this value are kept without further subdivision.</param>
    /// <param name="maxArea">Triangles with area greater than this value are always subdivided.</param>
    /// <param name="keepChance">The probability of keeping a triangle whose area lies between <paramref name="minArea"/> and <paramref name="maxArea"/>. Values outside the range [0, 1] cause a probability to be derived from the triangle area.</param>
    /// <param name="narrowValue">Triangles considered narrow by this threshold are kept without further subdivision.</param>
    /// <remarks>
    /// If the triangulation is empty, the method returns immediately without modifying <paramref name="result"/>. Unlike the other <c>Subdivide</c> overload, this method does not clear <paramref name="result"/> before appending accepted triangles.
    /// </remarks>
    public void Subdivide(Triangulation result, float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
    {
        if (this.Count <= 0) return;
        result.Clear();
        queueBuffer.Clear();
        if (Count == 1)
        {
            subdivBuffer.Clear();
            // this[0].Triangulate(subdivBuffer, minArea);
            this[0].Triangulate(subdivBuffer, this[0].GetRandomPointInside());
            queueBuffer.AddRange(subdivBuffer);
        }
        else
        {
            queueBuffer.AddRange(this);
        }
        
        while (queueBuffer.Count > 0)
        {
            int endIndex = queueBuffer.Count - 1;
            var tri = queueBuffer[endIndex];

            var triArea = tri.GetArea();
            if (triArea < minArea || tri.IsNarrow(narrowValue)) //too small or narrow
            {
                result.Add(tri);
            }
            else if (triArea > maxArea) //always subdivide because too big
            {
                subdivBuffer.Clear();
                // tri.Triangulate(subdivBuffer, minArea);
                tri.Triangulate(subdivBuffer, tri.GetRandomPointInside());
                queueBuffer.AddRange(subdivBuffer);
            }
            else //subdivde or keep
            {
                float chance = keepChance;
                if (keepChance < 0 || keepChance > 1f)
                {
                    chance = (triArea - minArea) / (maxArea - minArea);
                }

                if (Rng.Instance.Chance(chance))
                {
                    result.Add(tri);
                }
                else
                {
                    subdivBuffer.Clear();
                    // tri.Triangulate(subdivBuffer, minArea);
                    tri.Triangulate(subdivBuffer, tri.GetRandomPointInside());
                    queueBuffer.AddRange(subdivBuffer);
                }
            }
            queueBuffer.RemoveAt(endIndex);
        }
    }
    #endregion

    /// <summary>
    /// Converts this triangulation into a <see cref="TriMesh"/> by writing each triangle's vertices into <paramref name="dst"/>.
    /// </summary>
    /// <param name="dst">The destination mesh that will be cleared and populated with triangle vertex data.</param>
    /// <remarks>
    /// Each triangle contributes its vertices in A-B-C order to <c>dst.Triangles</c>.
    /// </remarks>
    public void ToTriMesh(TriMesh dst)
    {
        dst.Clear();
        foreach (var t in this)
        {
            dst.AddTriangle(t.A, t.B, t.C);
        }
    }
    
    /// <summary>
    /// Implicitly converts a <see cref="Triangulation"/> into a <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="triangulation">The triangulation to convert.</param>
    /// <returns>
    /// A new <see cref="TriMesh"/> containing the triangle vertices from <paramref name="triangulation"/>,
    /// or <c>null</c> if <paramref name="triangulation"/> is <c>null</c>.
    /// </returns>
    public static implicit operator TriMesh?(Triangulation? triangulation)
    {
        if (triangulation == null) return null;

        TriMesh mesh = new(triangulation.Count * 3);
        triangulation.ToTriMesh(mesh);
        return mesh;
    }

}