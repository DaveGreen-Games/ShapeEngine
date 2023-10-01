using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public class Points : ShapeList<Vector2>, IEquatable<Points>
{
    #region Constructors
    public Points(){}
    public Points(IEnumerable<Vector2> points) { AddRange(points); }
    #endregion

    #region Equals & HashCode
    public override int GetHashCode() { return ShapeUtils.GetHashCode(this); }
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

    #region Public
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
        
    public ClosestPoint GetClosest(Vector2 p)
    {
        if (Count <= 0) return new();

        float minDisSquared = float.PositiveInfinity;
        Vector2 closestPoint = new();

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            float disSquared = (point - p).LengthSquared();
            if (disSquared < minDisSquared)
            {
                minDisSquared = disSquared;
                closestPoint = point;
            }
        }
        return new(closestPoint, (p -closestPoint), MathF.Sqrt(minDisSquared));
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
    public Polygon ToPolygon()
    {
        return new Polygon(this);
    }
    public Polyline ToPolyline()
    {
        return new Polyline(this);
    }

    public void Floor() { Points.Floor(this); }
    public void Ceiling() { Points.Ceiling(this); }
    public void Truncate() { Points.Truncate(this); }
    public void Round() { Points.Round(this); }
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
    #endregion
}