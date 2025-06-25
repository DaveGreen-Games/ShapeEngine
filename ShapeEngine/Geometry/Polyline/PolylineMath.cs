using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Polyline;

public partial class Polyline
{
    #region Math

    public Points.Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points.Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return points;
    }

    public Polygon.Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new Points.Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return Polygon.Polygon.FindConvexHull(points);
    }

    public Vector2 GetCentroidOnLine()
    {
        return GetPoint(0.5f);
        // if (Count <= 0) return new(0f);
        // else if (Count == 1) return this[0];
        // float halfLengthSq = LengthSquared * 0.5f;
        // var segments = GetEdges();
        // float curLengthSq = 0f; 
        // foreach (var seg in segments)
        // {
        //     float segLengthSq = seg.LengthSquared;
        //     curLengthSq += segLengthSq;
        //     if (curLengthSq >= halfLengthSq)
        //     {
        //         float dif = curLengthSq - halfLengthSq;
        //         return seg.Center + seg.Dir * MathF.Sqrt(dif);
        //     }
        // }
        // return new Vector2();
    }

    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        else if (Count == 1) return this[0];
        Vector2 total = new(0f);
        foreach (Vector2 p in this)
        {
            total += p;
        }

        return total / Count;
    }

    public Vector2 GetPoint(float f)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return this[0].Lerp(this[1], f);
        if (f <= 0f) return this[0];
        if (f >= 1f) return this[^1];

        var totalLengthSq = GetLengthSquared();
        var targetLengthSq = totalLengthSq * f;
        var curLengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            var lSq = (start - end).LengthSquared();
            if (lSq <= 0) continue;

            if (curLengthSq + lSq >= targetLengthSq)
            {
                var aF = curLengthSq / totalLengthSq;
                var bF = (curLengthSq + lSq) / totalLengthSq;
                var curF = ShapeMath.LerpInverseFloat(aF, bF, f);
                return start.Lerp(end, curF);
            }

            curLengthSq += lSq;
        }

        return new();
    }

    public float GetLength()
    {
        if (this.Count < 2) return 0f;
        var length = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            length += w.Length();
        }

        return length;
    }

    public float GetLengthSquared()
    {
        if (this.Count < 2) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }

    #endregion

    #region Transform

    public void SetPosition(Vector2 newPosition)
    {
        var delta = newPosition - GetCentroidMean();
        ChangePosition(delta);
    }

    public void ChangeRotation(float rotRad)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    public void SetRotation(float angleRad)
    {
        if (Count < 2) return;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    public void ScaleSize(float scale)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    public void ChangeSize(float amount)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    public void SetSize(float size)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    public Polyline? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 2) return null;
        var centroid = GetCentroidMean();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }

    public new Polyline? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }

    public new Polyline? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }

    public Polyline? ChangeRotationCopy(float rotRad)
    {
        if (Count < 2) return null;
        return ChangeRotationCopy(rotRad, GetCentroidMean());
    }

    public new Polyline? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 2) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    public Polyline? SetRotationCopy(float angleRad)
    {
        if (Count < 2) return null;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    public Polyline? ScaleSizeCopy(float scale)
    {
        if (Count < 2) return null;
        return ScaleSizeCopy(scale, GetCentroidMean());
    }

    public new Polyline? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w * scale);
        }

        return newPolyline;
    }

    public new Polyline? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w * scale);
        }

        return newPolyline;
    }

    public new Polyline? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.ChangeLength(amount));
        }

        return newPolyline;
    }

    public Polyline? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroidMean());
    }

    public new Polyline? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.SetLength(size));
        }

        return newPolyline;
    }

    public Polyline? SetSizeCopy(float size)
    {
        if (Count < 2) return null;
        return SetSizeCopy(size, GetCentroidMean());
    }

    public new Polyline? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = SetPositionCopy(transform.Position);
        if (newPolyline == null) return null;
        newPolyline.SetRotation(transform.RotationRad, origin);
        newPolyline.SetSize(transform.ScaledSize.Length, origin);
        return newPolyline;
    }

    public new Polyline? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 2) return null;

        var newPolyline = ChangePositionCopy(offset.Position);
        if (newPolyline == null) return null;
        newPolyline.ChangeRotation(offset.RotationRad, origin);
        newPolyline.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolyline;
    }

    #endregion
}