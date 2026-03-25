using System.Numerics;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
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
    //public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
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
    /// Gets all unique points from all triangles in the triangulation.
    /// </summary>
    /// <returns>A <see cref="Points"/> collection containing all unique vertices.</returns>
    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            uniqueVertices.Add(tri.A);
            uniqueVertices.Add(tri.B);
            uniqueVertices.Add(tri.C);
        }

        return new(uniqueVertices);
    }
    /// <summary>
    /// Gets all unique segments from all triangles in the triangulation.
    /// </summary>
    /// <returns>A <see cref="Segments"/> collection containing all unique segments.</returns>
    public Segments GetUniqueSegments()
    {
        var unique = new HashSet<Segment>();
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            unique.Add(tri.SegmentAToB);
            unique.Add(tri.SegmentBToC);
            unique.Add(tri.SegmentCToA);
        }

        return new(unique);
    }
    /// <summary>
    /// Gets all unique triangles in the triangulation.
    /// </summary>
    /// <returns>A <see cref="Triangulation"/> containing all unique triangles.</returns>
    public Triangulation GetUniqueTriangles()
    {
        var uniqueTriangles = new HashSet<Triangle>();
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            uniqueTriangles.Add(tri);
        }

        return new(uniqueTriangles);
    }
    /// <summary>
    /// Gets all triangles that contain the specified point.
    /// </summary>
    /// <param name="p">The point to test for containment.</param>
    /// <returns>A <see cref="Triangulation"/> containing all triangles that contain the point.</returns>
    public Triangulation GetContainingTriangles(Vector2 p)
    {
        Triangulation result = new();
        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.ContainsPoint(p)) result.Add(tri);
        }
        return result;
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
        var i = triangleIndex % Count;
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

    private static Triangulation queueBuffer = new();
    private static Triangulation subdivBuffer = new();
    
    //TODO: Docs
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
    
    //TODO: Docs
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
                tri.Triangulate(minArea, subdivBuffer);
                queueBuffer.AddRange(subdivBuffer);
            }
            queueBuffer.RemoveAt(endIndex);
        }
    }
    
    //TODO: Docs
    public void Subdivide(Triangulation result, float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
    {
        if (this.Count <= 0) return;
        queueBuffer.Clear();
        if (Count == 1)
        {
            subdivBuffer.Clear();
            this[0].Triangulate(minArea, subdivBuffer);
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
                tri.Triangulate(minArea, subdivBuffer);
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
                    tri.Triangulate(minArea, subdivBuffer);
                    queueBuffer.AddRange(subdivBuffer);
                }
            }
            queueBuffer.RemoveAt(endIndex);
        }
    }
    #endregion

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