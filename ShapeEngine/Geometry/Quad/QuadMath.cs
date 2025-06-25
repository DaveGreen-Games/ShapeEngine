using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Quad;

public readonly partial struct Quad
{
    #region Math

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return points;
    }

    public Polygon.Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return Polygon.Polygon.FindConvexHull(points);
    }

    public Quad Floor()
    {
        return new
        (
            A.Floor(),
            B.Floor(),
            C.Floor(),
            D.Floor()
        );
    }

    public Quad Ceiling()
    {
        return new
        (
            A.Ceiling(),
            B.Ceiling(),
            C.Ceiling(),
            D.Ceiling()
        );
    }

    public Quad Round()
    {
        return new
        (
            A.Round(),
            B.Round(),
            C.Round(),
            D.Round()
        );
    }

    public Quad Truncate()
    {
        return new
        (
            A.Truncate(),
            B.Truncate(),
            C.Truncate(),
            D.Truncate()
        );
    }

    public float GetPerimeter() => AB.Length() + BC.Length() + CD.Length() + DA.Length();
    public float GetPerimeterSquared() => AB.LengthSquared() + BC.LengthSquared() + CD.LengthSquared() + DA.LengthSquared();

    public float GetArea()
    {
        Triangle.Triangle abc = new(A, B, C);
        Triangle.Triangle cda = new(C, D, A);
        return abc.GetArea() + cda.GetArea();
    }

    #endregion

    #region Transform

    public Quad ChangeRotation(float rad, AnchorPoint alignement)
    {
        var pivotPoint = GetPoint(alignement);
        var a = pivotPoint + (A - pivotPoint).Rotate(rad);
        var b = pivotPoint + (B - pivotPoint).Rotate(rad);
        var c = pivotPoint + (C - pivotPoint).Rotate(rad);
        var d = pivotPoint + (D - pivotPoint).Rotate(rad);
        return new(a, b, c, d);
    }

    public Quad SetRotation(float angleRad, AnchorPoint alignement)
    {
        float amount = ShapeMath.GetShortestAngleRad(AngleRad, angleRad);
        return ChangeRotation(amount, alignement);
    }

    public Quad ChangeRotation(float rad) => ChangeRotation(rad, AnchorPoint.Center);
    public Quad SetRotation(float angleRad) => SetRotation(angleRad, AnchorPoint.Center);

    public Quad ChangePosition(Vector2 offset)
    {
        return new
        (
            A + offset,
            B + offset,
            C + offset,
            D + offset
        );
    }

    public Quad SetPosition(Vector2 newPosition, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        var translation = newPosition - p;
        return new
        (
            A + translation,
            B + translation,
            C + translation,
            D + translation
        );
    }

    public Quad SetPosition(Vector2 newPosition) => SetPosition(newPosition, AnchorPoint.Center);
    public Quad ScaleSize(float scale) => this * scale;
    public Quad ScaleSize(Size scale) => new Quad(A * scale, B * scale, C * scale, D * scale);

    public Quad ScaleSize(float scale, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );
    }

    public Quad ScaleSize(Size scale, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );
    }

    public Quad ChangeSize(float amount) => ChangeSize(amount, AnchorPoint.Center);

    public Quad ChangeSize(float amount, AnchorPoint alignement)
    {
        Vector2 newA, newB, newC, newD;

        var origin = GetPoint(alignement);

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

        var wD = (D - origin);
        var lSqD = wD.LengthSquared();
        if (lSqD <= 0f) newD = D;
        else
        {
            var l = MathF.Sqrt(lSqD);
            var dir = wD / l;
            newD = origin + dir * (l + amount);
        }

        return new(newA, newB, newC, newD);
    }

    public Quad SetSize(float size) => SetSize(size, AnchorPoint.Center);

    public Quad SetSize(float size, AnchorPoint alignement)
    {
        Vector2 newA, newB, newC, newD;

        var origin = GetPoint(alignement);

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

        var wD = (D - origin);
        var lSqD = wD.LengthSquared();
        if (lSqD <= 0f) newD = D;
        else
        {
            var l = MathF.Sqrt(lSqD);
            var dir = wD / l;
            newD = origin + dir * size;
        }

        return new(newA, newB, newC, newD);
    }

    /// <summary>
    /// Moves the quad by transform.Position
    /// Rotates the moved quad by transform.RotationRad
    /// Changes the size of the rotated quad by transform.Size!
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Quad ApplyOffset(Transform2D offset)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad);
        return newQuad.ChangeSize(offset.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the quad to transform.Position
    /// Rotates the moved quad to transform.RotationRad
    /// Sets the size of the rotated quad to transform.Size.Width
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Quad SetTransform(Transform2D transform)
    {
        var newQuad = SetPosition(transform.Position);
        newQuad = newQuad.SetRotation(transform.RotationRad);
        return newQuad.SetSize(transform.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the quad by transform.Position
    /// Rotates the moved quad by transform.RotationRad
    /// Changes the size of the rotated quad by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Quad ApplyOffset(Transform2D offset, AnchorPoint alignement)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad, alignement);
        return newQuad.ChangeSize(offset.ScaledSize.Length, alignement);
    }

    /// <summary>
    /// Moves the quad to transform.Position
    /// Rotates the moved quad to transform.RotationRad
    /// Sets the size of the rotated quad to transform.Size.Width
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Quad SetTransform(Transform2D transform, AnchorPoint alignement)
    {
        var newQuad = SetPosition(transform.Position, alignement);
        newQuad = newQuad.SetRotation(transform.RotationRad, alignement);
        return newQuad.SetSize(transform.ScaledSize.Length, alignement);
    }

    #endregion
}