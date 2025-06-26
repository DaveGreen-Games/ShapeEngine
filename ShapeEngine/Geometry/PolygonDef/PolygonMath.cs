using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    #region Math

    public PointsDef.Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new PointsDef.Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return points;
    }

    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new PointsDef.Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return Polygon.FindConvexHull(points);
    }

    public Vector2 GetCentroid()
    {
        if (Count <= 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;

        var centroid = new Vector2();
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
            centroid += (a + b) * cross;
        }

        area *= 0.5f;
        return centroid / (area * 6);

        //return GetCentroidMean();
        // Vector2 result = new();

        // for (int i = 0; i < Count; i++)
        // {
        // var a = this[i];
        // var b = this[(i + 1) % Count];
        //// float factor = a.X * b.Y - b.X * a.Y; //clockwise 
        // float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
        // result.X += (a.X + b.X) * factor;
        // result.Y += (a.Y + b.Y) * factor;
        // }

        // return result * (1f / (GetArea() * 6f));
    }

    public float GetPerimeter()
    {
        if (this.Count < 3) return 0f;
        float length = 0f;
        for (int i = 0; i < Count; i++)
        {
            Vector2 w = this[(i + 1) % Count] - this[i];
            length += w.Length();
        }

        return length;
    }

    public float GetPerimeterSquared()
    {
        if (Count < 3) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count; i++)
        {
            var w = this[(i + 1) % Count] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }

    private float GetDiameterSquared()
    {
        if (Count <= 2) return 0;
        var center = GetCentroid();
        var maxDisSquared = -1f;
        for (int i = 0; i < Count; i++)
        {
            var p = this[0];
            var disSquared = (p - center).LengthSquared();
            if (maxDisSquared < 0 || disSquared > maxDisSquared)
            {
                maxDisSquared = disSquared;
            }
        }

        return maxDisSquared;
    }

    private float GetDiameter()
    {
        if (Count <= 2) return 0;
        return MathF.Sqrt(GetDiameterSquared());
    }

    public float GetArea()
    {
        if (Count < 3) return 0f;
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--) //backwards to be clockwise
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
        }

        return area / 2f;
    }

    public bool IsClockwise() => GetArea() < 0f;

    public bool IsConvex()
    {
        int num = this.Count;
        bool isPositive = false;

        for (int i = 0; i < num; i++)
        {
            int prevIndex = (i == 0) ? num - 1 : i - 1;
            int nextIndex = (i == num - 1) ? 0 : i + 1;
            var d0 = this[i] - this[prevIndex];
            var d1 = this[nextIndex] - this[i];
            var newIsP = d0.Cross(d1) > 0f;
            if (i == 0) isPositive = true;
            else if (isPositive != newIsP) return false;
        }

        return true;
    }

    public PointsDef.Points ToPoints()
    {
        return new(this);
    }

    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;
        var total = new Vector2(0f);
        foreach (var p in this)
        {
            total += p;
        }

        return total / Count;
    }

    /// <summary>
    /// Computes the length of this polygon's apothem. This will only be valid if
    /// the polygon is regular. More info: http://en.wikipedia.org/wiki/Apothem
    /// </summary>
    /// <returns>Return the length of the apothem.</returns>
    public float GetApothem() => (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();

    #endregion

    #region Transform

    public void SetPosition(Vector2 newPosition)
    {
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        ChangePosition(delta);
    }

    public void ChangeRotation(float rotRad)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    public void SetRotation(float angleRad)
    {
        if (Count < 3) return;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    public void ScaleSize(float scale)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    public void ChangeSize(float amount)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    public void SetSize(float size)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    public Polygon? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 3) return null;
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }

    public new Polygon? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }

    public new Polygon? ChangeRotationCopy(float rotRad, Vector2 origin)
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

    public Polygon? ChangeRotationCopy(float rotRad)
    {
        if (Count < 3) return null;
        return ChangeRotationCopy(rotRad, GetCentroid());
    }

    public new Polygon? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    public Polygon? SetRotationCopy(float angleRad)
    {
        if (Count < 3) return null;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    public Polygon? ScaleSizeCopy(float scale)
    {
        if (Count < 3) return null;
        return ScaleSizeCopy(scale, GetCentroid());
    }

    public new Polygon? ScaleSizeCopy(float scale, Vector2 origin)
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

    public new Polygon? ScaleSizeCopy(Vector2 scale, Vector2 origin)
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

    public new Polygon? ChangeSizeCopy(float amount, Vector2 origin)
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

    public Polygon? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroid());
    }

    public new Polygon? SetSizeCopy(float size, Vector2 origin)
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

    public Polygon? SetSizeCopy(float size)
    {
        if (Count < 3) return null;
        return SetSizeCopy(size, GetCentroid());
    }

    public new Polygon? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = SetPositionCopy(transform.Position);
        if (newPolygon == null) return null;
        newPolygon.SetRotation(transform.RotationRad, origin);
        newPolygon.SetSize(transform.ScaledSize.Length, origin);
        return newPolygon;
    }

    public new Polygon? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;

        var newPolygon = ChangePositionCopy(offset.Position);
        if (newPolygon == null) return null;
        newPolygon.ChangeRotation(offset.RotationRad, origin);
        newPolygon.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolygon;
    }

    #endregion
}