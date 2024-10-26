using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public class Points : ShapeList<Vector2>, IEquatable<Points>
{
    #region Constructors
    public Points(){}
    public Points(int capacity) : base(capacity){}
    public Points(IEnumerable<Vector2> points) { AddRange(points); }
    #endregion

    #region Equals & HashCode
    public override int GetHashCode() { return Game.GetHashCode(this); }
    public bool Equals(Points? other)
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
    #endregion

    #region Points & Vertex

    /// <summary>
    /// Gets the value at the specified index wrapping around if index is smaller than 0 or bigger than count
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector2 GetPoint(int index)
    {
        return GetItem(index);
        //return Count <= 0 ? new() : this[index % Count];
    }
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
        
    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (var i = 0; i < Count; i++)
        {
            uniqueVertices.Add(this[i]);
        }
        return new(uniqueVertices);
    }

    public Vector2 GetRandomPoint() => GetRandomItem();
    public List<Vector2> GetRandomPoints(int amount) => GetRandomItems(amount);

    #endregion

    #region Math

    public void Floor() { Points.Floor(this); }
    public void Ceiling() { Points.Ceiling(this); }
    public void Truncate() { Points.Truncate(this); }
    public void Round() { Points.Round(this); }

    #endregion

    #region Shapes

    public override Points Copy() => new(this);

    public Polygon ToPolygon() => new(this);

    public Polyline ToPolyline() => new(this);
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


    public List<Vector2> GetRelativeVector2List(Vector2 origin)
    {
        var relative = new List<Vector2>(Count);
        foreach (var p in this)  relative.Add(p - origin);
        return relative;
    }
    public List<Vector2> GetRelativeVector2List(Transform2D transform)
    {
        var relative = new List<Vector2>(Count);
        foreach (var p in this)  relative.Add(transform.RevertPosition(p));
        return relative;
    }
    
    public Points GetRelativePoints(Vector2 origin)
    {
        var relative = new Points(Count);
        foreach (var p in this)  relative.Add(p - origin);
        return relative;
    }
    public Points GetRelativePoints(Transform2D transform)
    {
        var relative = new Points(Count);
        foreach (var p in this)  relative.Add(transform.RevertPosition(p));
        return relative;
    }

    #endregion
    
    #region Closest Distance
   
    public ClosestDistance GetClosestDistanceTo(Vector2 p)
    {
        if (Count <= 0) return new();

        if (Count == 1) return new(this[0], p);


        var closestPoint = this[0];
        var minDisSq = (closestPoint - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var disSq = (this[i] - p).LengthSquared();
            if (disSq >= minDisSq) continue;
            minDisSq = disSq;
            closestPoint = this[i];
        }

        return new(closestPoint, p);
    }
    
    #endregion

    #region Transform
    public void SetPosition(Vector2 newPosition, Vector2 origin)
    {
        var delta = newPosition - origin;
        ChangePosition(delta);
    }
    public void ChangePosition(Vector2 offset)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i] += offset;
        }
    }
    public void ChangeRotation(float rotRad, Vector2 origin)
    {
        if (Count < 2) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }
    public void SetRotation(float angleRad, Vector2 origin)
    {
        if (Count < 2) return;

        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }
    
    public void ScaleSize(float scale, Vector2 origin)
    {
        if (Count < 2) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }
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
    public void ChangeSize(float amount, Vector2 origin)
    {
        if (Count < 2) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
        
    }
    public void SetSize(float size, Vector2 origin)
    {
        if (Count < 2) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }

    }

    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        SetPosition(transform.Position, origin);
        SetRotation(transform.RotationRad, origin);
        SetSize(transform.ScaledSize.Length, origin);
    }
    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        ChangePosition(offset.Position);
        ChangeRotation(offset.RotationRad, origin);
        ChangeSize(offset.ScaledSize.Length, origin);
        
    }
    
    
    public Points? SetPositionCopy(Vector2 newPosition, Vector2 origin)
    {
        if (Count < 2) return null;
        var delta = newPosition - origin;
        return ChangePositionCopy(delta);
    }
    public Points? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }
    
        return newPolygon;
    }
    public Points? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }
    
        return newPolygon;
    }
    
    public Points? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    
    public Points? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add( origin + w * scale);
        }
    
        return newPolygon;
    }
    public Points? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }
    
        return newPolygon;
    }
    public Points? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.ChangeLength(amount));
        }
    
        return newPolygon;
    
    }
    public Points? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.SetLength(size));
        }
    
        return newPolygon;
    }
    
    public Points? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPoints = SetPositionCopy(transform.Position, origin);
        if (newPoints == null) return null;
        newPoints.SetRotation(transform.RotationRad, origin);
        newPoints.SetSize(transform.ScaledSize.Length, origin);
        return newPoints;
    }
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
    public static void Floor(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Floor();
        }
    }
    public static void Ceiling(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Ceiling();
        }
    }
    public static void Round(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Round();
        }
    }
    public static void Truncate(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Truncate();
        }
    }
    
    
    // public static Points Move(Points points, Vector2 translation)
    // {
    //     var result = new Points();
    //     for (int i = 0; i < points.Count; i++)
    //     {
    //         result.Add(points[i] + translation);
    //     }
    //     return result;
    // }
    
    
    
    #endregion
}