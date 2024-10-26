using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;
namespace ShapeEngine.Core.Shapes;

public class Triangulation : ShapeList<Triangle>
{
    #region Constructors
    public Triangulation() { }
    public Triangulation(int capacity) : base(capacity) { }
    //public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
    public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }
    #endregion
        
    #region Equals & HashCode
    public override int GetHashCode() => Game.GetHashCode(this);

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

    // public static bool operator ==(Triangulation left, Triangulation right) => left.Equals(right);
    //
    // public static bool operator !=(Triangulation left, Triangulation right) => !(left == right);

    #endregion

    #region Public
    public ClosestItem<Triangle> GetClosest(Vector2 p)
    {
        if (Count <= 0) return new();

        float minDisSquared = float.PositiveInfinity;
        Triangle closestTriangle = new();
        var contained = false;
        Vector2 closestTrianglePoint = new();

        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            bool containsPoint = tri.ContainsPoint(p);
            var closestPoint = tri.GetClosestCollisionPoint(p);
            float disSquared = (closestPoint.Point - p).LengthSquared();
            if (disSquared < minDisSquared)
            {
                if(containsPoint || !contained)
                {
                    minDisSquared = disSquared;
                    closestTriangle = tri;
                    closestTrianglePoint = closestPoint.Point;
                    if (containsPoint) contained = true;
                }
            }
            else
            {
                if (containsPoint && !contained)
                {
                    contained = true;
                    minDisSquared = disSquared;
                    closestTriangle = tri;
                    closestTrianglePoint = closestPoint.Point;
                }
            }
        }
        return new(closestTriangle, closestTrianglePoint, p);
    }
        
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
    /// Get the total area of all triangles in this triangulation.
    /// </summary>
    /// <returns></returns>
    public float GetArea()
    {
        var total = 0f;
        foreach (var t in this)
        {
            total += t.GetArea();
        }
        return total;
    }

    /// <summary>
    /// Remove all triangles with an area less than the threshold. If threshold is smaller or equal to 0, nothing happens.
    /// </summary>
    /// <param name="areaThreshold"></param>
    /// <returns></returns>
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

    #region Transform

    public void ChangeRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad);
        }
    }
    public void ChangeRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, origin);
        }
    }
    
    public void SetRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad);
        }
    }
    public void SetRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, origin);
        }
    }

    public void ScaleSize(float scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }
    public void ScaleSize(Size scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }
    public void ScaleSize(float scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }
    public void ScaleSize(Size scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }
    
    public void ChangeSize(float amount)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount);
        }
    }
    public void ChangeSize(float amount, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount, origin);
        }
    }
    
    public void SetSize(float size)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size);
        }
    }
    public void SetSize(float size, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size, origin);
        }
    }
    
    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }
    public void SetPosition(Vector2 position, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, origin);
        }
    }

    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, origin);
        }
    }
    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, origin);
        }
    }

    public Triangulation ChangeRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad));
        }

        return newTriangulation;
    }
    public Triangulation ChangeRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad, origin));
        }

        return newTriangulation;
    }
    
    public Triangulation SetRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad));
        }
        return newTriangulation;
    }
    public Triangulation SetRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad, origin));
        }

        return newTriangulation;
    }

    public Triangulation ScaleSizeCopy(float scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }
    public Triangulation ScaleSizeCopy(Size scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }
    public Triangulation ScaleSizeCopy(float scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }
    public Triangulation ScaleSizeCopy(Size scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }
    
    public Triangulation ChangeSizeCopy(float amount)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount));
        }

        return newTriangulation;
    }
    public Triangulation ChangeSizeCopy(float amount, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount, origin));
        }

        return newTriangulation;
    }
    
    public Triangulation SetSizeCopy(float size)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size));
        }

        return newTriangulation;
    }
    public Triangulation SetSizeCopy(float size, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size, origin));
        }

        return newTriangulation;
    }
    
    public Triangulation ChangePositionCopy(Vector2 offset)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangePosition(offset));
        }

        return newTriangulation;
    }
    public Triangulation SetPositionCopy(Vector2 position, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetPosition(position, origin));
        }

        return newTriangulation;
    }

    public Triangulation ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ApplyOffset(offset, origin));
        }
    
        return newTriangulation;
    }
    public Triangulation SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetTransform(transform, origin));
        }

        return newTriangulation;
    }

    #endregion
    
    #region Triangulation
    /// <summary>
    /// Get a new triangulation with triangles with an area >= areaThreshold.
    /// </summary>
    /// <param name="areaThreshold"></param>
    /// <returns></returns>
    public Triangulation Get(float areaThreshold)
    {
        Triangulation newTriangulation = new();
        if (areaThreshold <= 0f) return newTriangulation;

        for (int i = Count - 1; i >= 0; i--)
        {
            var t = this[i];
            if (t.GetArea() >= areaThreshold)
            {
                newTriangulation.Add(t);
            }
        }

        return newTriangulation;
    }

    /// <summary>
    /// Subdivide the triangulation until all triangles are smaller than min area.
    /// </summary>
    /// <param name="minArea">A triangle will always be subdivided if the area is bigger than min area.s</param>
    /// <returns></returns>
    public Triangulation Subdivide(float minArea)
    {
        Triangulation final = new();

        Triangulation queue = new();
        queue.AddRange(this);
        while (queue.Count > 0)
        {
            int endIndex = queue.Count - 1;
            var tri = queue[endIndex];

            var triArea = tri.GetArea();
            if (triArea < minArea) final.Add(tri);
            else queue.AddRange(tri.Triangulate(minArea));
            queue.RemoveAt(endIndex);
        }
        return final;
    }

    /// <summary>
    /// Subdivide the triangles further based on the parameters.
    /// </summary>
    /// <param name="minArea">Triangles with an area smaller than min area will never be subdivided.</param>
    /// <param name="maxArea">Triangles with an area bigger than maxArea will always be subdivided.</param>
    /// <param name="keepChance">The chance to keep a triangle and not subdivide it.</param>
    /// <param name="narrowValue">Triangles that are considered narrow will not be subdivided.</param>
    /// <returns></returns>
    public Triangulation Subdivide(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
    {
        if (this.Count <= 0) return this;

        Triangulation final = new();
        Triangulation queue = new();

        queue.AddRange(this.Count == 1 ? this[0].Triangulate(minArea) : this);


        while (queue.Count > 0)
        {
            int endIndex = queue.Count - 1;
            var tri = queue[endIndex];

            var triArea = tri.GetArea();
            if (triArea < minArea || tri.IsNarrow(narrowValue)) //too small or narrow
            {
                final.Add(tri);
            }
            else if (triArea > maxArea) //always subdivide because too big
            {
                queue.AddRange(tri.Triangulate(minArea));
            }
            else //subdivde or keep
            {
                float chance = keepChance;
                if (keepChance < 0 || keepChance > 1f)
                {
                    chance = (triArea - minArea) / (maxArea - minArea);
                }

                if (Rng.Instance.Chance(chance)) final.Add(tri);
                else queue.AddRange(tri.Triangulate(minArea));
            }
            queue.RemoveAt(endIndex);
        }
        return final;
    }
    #endregion
}