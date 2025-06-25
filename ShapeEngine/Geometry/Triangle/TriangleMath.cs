using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Triangle;

public readonly partial struct Triangle
{
    #region Transform

    public Triangle ChangeRotation(float rad)
    {
        return ChangeRotation(rad, GetCentroid());
    }

    public Triangle ChangeRotation(float rad, Vector2 origin)
    {
        var newA = origin + (A - origin).Rotate(rad);
        var newB = origin + (B - origin).Rotate(rad);
        var newC = origin + (C - origin).Rotate(rad);
        return new(newA, newB, newC);
    }

    public Triangle SetRotation(float rad)
    {
        var origin = GetCentroid();
        var w = A - origin;
        var currentAngleRad = w.AngleRad();
        var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
        return ChangeRotation(amount, origin);
    }

    public Triangle SetRotation(float rad, Vector2 origin)
    {
        var w = A - origin;
        if (w.LengthSquared() <= 0f) //origin is A
        {
            w = B - origin;
            if (w.LengthSquared() <= 0f) //origin is B
            {
                w = C - origin;
            }
        }

        var currentAngleRad = w.AngleRad();
        var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
        return ChangeRotation(amount, origin);
    }

    public Triangle ScaleSize(float scale) => this * scale;
    public Triangle ScaleSize(Size scale) => new Triangle(A * scale, B * scale, C * scale);

    public Triangle ScaleSize(float scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }

    public Triangle ScaleSize(Size scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }

    public Triangle ChangeSize(float amount) => ChangeSize(amount, GetCentroid());

    public Triangle ChangeSize(float amount, Vector2 origin)
    {
        Vector2 newA, newB, newC;

        var wA = (A - origin);
        var lSqA = wA.LengthSquared();
        if (lSqA <= 0f) newA = A;
        else
        {
            var l = MathF.Sqrt(lSqA);
            var dir = wA / l;
            newA = origin + dir * (l + amount);
        }

        var wB = (B - origin);
        var lSqB = wB.LengthSquared();
        if (lSqB <= 0f) newB = B;
        else
        {
            var l = MathF.Sqrt(lSqB);
            var dir = wB / l;
            newB = origin + dir * (l + amount);
        }

        var wC = (C - origin);
        var lSqC = wC.LengthSquared();
        if (lSqC <= 0f) newC = C;
        else
        {
            var l = MathF.Sqrt(lSqC);
            var dir = wC / l;
            newC = origin + dir * (l + amount);
        }

        return new(newA, newB, newC);
    }

    public Triangle SetSize(float size) => SetSize(size, GetCentroid());

    public Triangle SetSize(float size, Vector2 origin)
    {
        Vector2 newA, newB, newC;

        var wA = (A - origin);
        var lSqA = wA.LengthSquared();
        if (lSqA <= 0f) newA = A;
        else
        {
            var l = MathF.Sqrt(lSqA);
            var dir = wA / l;
            newA = origin + dir * size;
        }

        var wB = (B - origin);
        var lSqB = wB.LengthSquared();
        if (lSqB <= 0f) newB = B;
        else
        {
            var l = MathF.Sqrt(lSqB);
            var dir = wB / l;
            newB = origin + dir * size;
        }

        var wC = (C - origin);
        var lSqC = wC.LengthSquared();
        if (lSqC <= 0f) newC = C;
        else
        {
            var l = MathF.Sqrt(lSqC);
            var dir = wC / l;
            newC = origin + dir * size;
        }

        return new(newA, newB, newC);
    }

    public Triangle ChangePosition(Vector2 offset)
    {
        return new(A + offset, B + offset, C + offset);
    }

    public Triangle SetPosition(Vector2 position)
    {
        var centroid = GetCentroid();
        var delta = position - centroid;
        return ChangePosition(delta);
    }

    public Triangle SetPosition(Vector2 position, Vector2 origin)
    {
        var delta = position - origin;
        return ChangePosition(delta);
    }

    /// <summary>
    /// Moves the triangle by transform.Position
    /// Rotates the moved triangle by transform.RotationRad
    /// Changes the size of the rotated triangle by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Triangle ApplyOffset(Transform2D offset)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad);
        return newTriangle.ChangeSize(offset.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the triangle by transform.Position
    /// Rotates the moved triangle by transform.RotationRad
    /// Changes the size of the rotated triangle by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public Triangle ApplyOffset(Transform2D offset, Vector2 origin)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad, origin);
        return newTriangle.ChangeSize(offset.ScaledSize.Length, origin);
    }

    /// <summary>
    /// Moves the triangle to transform.Position
    /// Rotates the moved triangle to transform.RotationRad
    /// Sets the size of the rotated triangle to transform.ScaledSize.Length
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Triangle SetTransform(Transform2D transform)
    {
        var newTriangle = SetPosition(transform.Position);
        newTriangle = newTriangle.SetRotation(transform.RotationRad);
        return newTriangle.SetSize(transform.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the triangle to transform.Position
    /// Rotates the moved triangle to transform.RotationRad
    /// Sets the size of the rotated triangle to transform.ScaledSize.Length
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public Triangle SetTransform(Transform2D transform, Vector2 origin)
    {
        var newTriangle = SetPosition(transform.Position, origin);
        newTriangle = newTriangle.SetRotation(transform.RotationRad, origin);
        return newTriangle.SetSize(transform.ScaledSize.Length, origin);
    }

    public Triangle ApplyTransform(Transform2D transform)
    {
        var aAbsolute = transform.Position + (A * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        var bAbsolute = transform.Position + (B * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        var cAbsolute = transform.Position + (C * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        return new Triangle(aAbsolute, bAbsolute, cAbsolute);
    }

    #endregion

    #region Math

    public bool IsValid()
    {
        return GetArea() > 0f;
    }

    public Vector2 GetCentroid() => (A + B + C) / 3;

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A,
            B,
            C,
            A + v,
            B + v,
            C + v
        };
        return points;
    }

    public Polygon.Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A,
            B,
            C,
            A + v,
            B + v,
            C + v
        };
        return Polygon.Polygon.FindConvexHull(points);
    }

    public Triangle Floor()
    {
        return new(A.Floor(), B.Floor(), C.Floor());
    }

    public Triangle Ceiling()
    {
        return new(A.Ceiling(), B.Ceiling(), C.Ceiling());
    }

    public Triangle Round()
    {
        return new(A.Round(), B.Round(), C.Round());
    }

    public Triangle Truncate()
    {
        return new(A.Truncate(), B.Truncate(), C.Truncate());
    }

    public float GetPerimeter() => SideA.Length() + SideB.Length() + SideC.Length();
    public float GetPerimeterSquared() => SideA.LengthSquared() + SideB.LengthSquared() + SideC.LengthSquared();
    public float GetArea() => MathF.Abs((A.X - C.X) * (B.Y - C.Y) - (A.Y - C.Y) * (B.X - C.X)) / 2f;

    public bool IsNarrow(float narrowValue = 0.2f)
    {
        var prev = C;
        var cur = A;
        var next = B;

        var nextToCur = (next - cur).Normalize();
        var prevToCur = (prev - cur).Normalize();
        float cross = nextToCur.Cross(prevToCur);
        if (MathF.Abs(cross) < narrowValue) return true;

        prev = A;
        cur = B;
        next = C;

        nextToCur = (next - cur).Normalize();
        prevToCur = (prev - cur).Normalize();
        cross = nextToCur.Cross(prevToCur);
        if (MathF.Abs(cross) < narrowValue) return true;

        prev = B;
        cur = C;
        next = A;

        nextToCur = (next - cur).Normalize();
        prevToCur = (prev - cur).Normalize();
        cross = nextToCur.Cross(prevToCur);
        return MathF.Abs(cross) < narrowValue;
    }

    #endregion
}