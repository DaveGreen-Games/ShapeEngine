using System.Numerics;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;
using Game = ShapeEngine.Core.GameDef.Game;

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
public partial class Triangulation : ShapeList<Triangle>
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
    public Triangulation() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Triangulation"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of triangles the collection can initially store.</param>
    public Triangulation(int capacity) : base(capacity) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Triangulation"/> class with the specified triangles.
    /// </summary>
    /// <param name="triangles">The collection of triangles to initialize with.</param>
    public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }
    #endregion
        
    #region Equals & HashCode
    /// <summary>
    /// Returns a hash code for this triangulation.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => Game.GetHashCode(this);

    /// <summary>
    /// Determines whether the specified <see cref="Triangulation"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The triangulation to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified triangulation is equal to the current instance; otherwise, <c>false</c>.</returns>
    /// <remarks>Equality is determined by comparing the count and each triangle in order.</remarks>
    public bool Equals(Triangulation? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (this[i] != other[i]) return false;
        }
        return true;
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
        queueBuffer.Clear();
        if (Count == 1)
        {
            subdivBuffer.Clear();
            this[0].Triangulate(subdivBuffer, minArea);
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
                tri.Triangulate(subdivBuffer, minArea);
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
                    tri.Triangulate(subdivBuffer, minArea);
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
            dst.Triangles.Add(t.A);
            dst.Triangles.Add(t.B);
            dst.Triangles.Add(t.C);
        }
    }
}