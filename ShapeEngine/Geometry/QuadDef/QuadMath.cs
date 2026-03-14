using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    #region Math

    /// <summary>
    /// Gets the unit normal vector pointing to the left side of the quad.
    /// Computed by normalizing the vector from D to A.
    /// </summary>
    public Vector2 NormalLeft => DA.Normalize();

    /// <summary>
    /// Gets the unit normal vector pointing downward along the quad.
    /// Computed by normalizing the vector from A to B.
    /// </summary>
    public Vector2 NormalDown => AB.Normalize();

    /// <summary>
    /// Gets the unit normal vector pointing to the right side of the quad.
    /// Computed by normalizing the vector from B to C.
    /// </summary>
    public Vector2 NormalRight => BC.Normalize();

    /// <summary>
    /// Gets the unit normal vector pointing upward along the quad.
    /// Computed by normalizing the vector from C to D.
    /// </summary>
    public Vector2 NormalUp => CD.Normalize();
    
    /// <summary>
    /// Gets the projected points of the quad along a given vector.
    /// </summary>
    /// <param name="v">The vector along which to project the quad's vertices.</param>
    /// <returns>A <see cref="Points"/> collection containing the original and projected vertices,
    /// or null if the vector is zero.</returns>
    /// <remarks>Projects each vertex of the quad along the specified vector and returns all resulting points.</remarks>
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

    /// <summary>
    /// Projects the quad along a given vector and returns the convex hull as a polygon.
    /// </summary>
    /// <param name="v">The vector along which to project the quad's vertices.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull of the projected shape,
    /// or null if the vector is zero.</returns>
    /// <remarks>Useful for shadow or extrusion effects.</remarks>
    public Polygon? ProjectShape(Vector2 v)
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
        return Polygon.FindConvexHull(points);
    }

    /// <summary>
    /// Returns a new quad with all vertices floored to the nearest integer values.
    /// </summary>
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

    /// <summary>
    /// Returns a new quad with all vertices ceiled to the nearest integer values.
    /// </summary>
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

    /// <summary>
    /// Returns a new quad with all vertices rounded to the nearest integer values.
    /// </summary>
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

    /// <summary>
    /// Returns a new quad with all vertices truncated (rounded toward zero).
    /// </summary>
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

    /// <summary>
    /// Calculates the perimeter of the quad by summing the lengths of its edges.
    /// </summary>
    /// <returns>The total perimeter length.</returns>
    public float GetPerimeter() => AB.Length() + BC.Length() + CD.Length() + DA.Length();

    /// <summary>
    /// Calculates the sum of the squared lengths of the quad's edges.
    /// </summary>
    /// <returns>The sum of squared edge lengths.</returns>
    public float GetPerimeterSquared() => AB.LengthSquared() + BC.LengthSquared() + CD.LengthSquared() + DA.LengthSquared();

    /// <summary>
    /// Calculates the area of the quad by dividing it into two triangles.
    /// </summary>
    /// <returns>The total area of the quad.</returns>
    /// <remarks>Area is computed as the sum of the areas of triangles ABC and CDA.</remarks>
    public float GetArea()
    {
        Triangle abc = new(A, B, C);
        Triangle cda = new(C, D, A);
        return abc.GetArea() + cda.GetArea();
    }

    /// <summary>
    /// Gets the length of the diagonal connecting corner A and corner C.
    /// </summary>
    public float GetDiagonalLengt() => (A - C).Length();

    /// <summary>
    /// Gets the squared length of the diagonal connecting corner A and corner C.
    /// </summary>
    public float GetDiagonalLengthSquare() => (A - C).LengthSquared();

    #endregion
    
    #region Position Methods
    /// <summary>
    /// Moves the quad by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move the quad.</param>
    /// <returns>A new <see cref="Quad"/> translated by the offset.</returns>
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

    /// <summary>
    /// Moves the quad so that the specified anchor point aligns with a new position.
    /// </summary>
    /// <param name="newPosition">The new position for the anchor point.</param>
    /// <param name="alignment">The anchor point to align.</param>
    /// <returns>A new <see cref="Quad"/> with the anchor point at the new position.</returns>
    public Quad SetPosition(Vector2 newPosition, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        var translation = newPosition - p;
        return new
        (
            A + translation,
            B + translation,
            C + translation,
            D + translation
        );
    }

    /// <summary>
    /// Moves the quad so that its center aligns with a new position.
    /// </summary>
    /// <param name="newPosition">The new position for the center.</param>
    /// <returns>A new <see cref="Quad"/> with its center at the new position.</returns>
    public Quad SetPosition(Vector2 newPosition) => SetPosition(newPosition, AnchorPoint.Center);
    #endregion
    
    #region Rotation Methods
    
    public float GetRotationRad()
    {
        //A quad with 0 rotation should equal a standard rect.
        //If the vector from the center to the right side center is 0 the quad is not rotated
        return ShapeVec.AngleRad(CDCenter - Center);
    }
    
    public float GetRotationDeg() => GetRotationRad() * ShapeMath.RADTODEG;
    
    /// <summary>
    /// Rotates the quad by a specified angle in radians around a given anchor point.
    /// </summary>
    /// <param name="amountRad">The angle in radians to rotate.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns>A new <see cref="Quad"/> rotated by the specified angle.</returns>
    public Quad ChangeRotation(float amountRad, AnchorPoint alignment)
    {
        var pivotPoint = GetPoint(alignment);
        var a = pivotPoint + (A - pivotPoint).Rotate(amountRad);
        var b = pivotPoint + (B - pivotPoint).Rotate(amountRad);
        var c = pivotPoint + (C - pivotPoint).Rotate(amountRad);
        var d = pivotPoint + (D - pivotPoint).Rotate(amountRad);
        return new(a, b, c, d);
    }
    
    /// <summary>
    /// Rotates the quad to a specific angle in radians around a given anchor point.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns>A new <see cref="Quad"/> with the specified rotation.</returns>
    /// <remarks>Uses the shortest rotation path to reach the target angle.</remarks>
    public Quad SetRotation(float angleRad, AnchorPoint alignment)
    {
        float amount = ShapeMath.GetShortestAngleRad(GetRotationRad(), angleRad);
        return ChangeRotation(amount, alignment);
    }

    /// <summary>
    /// Rotates the quad by a specified angle in radians around its center.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate.</param>
    /// <returns>A new <see cref="Quad"/> rotated by the specified angle.</returns>
    public Quad ChangeRotation(float rad) => ChangeRotation(rad, AnchorPoint.Center);

    /// <summary>
    /// Rotates the quad to a specific angle in radians around its center.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <returns>A new <see cref="Quad"/> with the specified rotation.</returns>
    public Quad SetRotation(float angleRad) => SetRotation(angleRad, AnchorPoint.Center);
    #endregion
    
    #region Size Methods
    
    public Size GetSize()
    {
        var e1 = B - A;
        var e2 = D - A;
        float height = e1.Length();
        float width = e2.Length();
        return new Size(width, height);
    }
    
    public Quad ScaleSize(float scale)
    {
        return ScaleSize(scale, AnchorPoint.Center);
    }

    public Quad ScaleSize(Size scale)
    {
        return ScaleSize(scale, AnchorPoint.Center);
    }
    
    public Quad ScaleSize(float scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() * scale, GetRotationRad(), alignment);
        // return new
        // (
        //     p + (A - p) * scale,
        //     p + (B - p) * scale,
        //     p + (C - p) * scale,
        //     p + (D - p) * scale
        // );
    }

    public Quad ScaleSize(Size scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() * scale, GetRotationRad(), alignment);
        // return new
        // (
        //     p + (A - p) * scale,
        //     p + (B - p) * scale,
        //     p + (C - p) * scale,
        //     p + (D - p) * scale
        // );
    }

    
    public Quad ChangeDiagonalSize(float amount, AnchorPoint alignment)
    {
        Vector2 newA, newB, newC, newD;

        var origin = GetPoint(alignment);

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
    
    
    public Quad ChangeSize(float amount) => ChangeSize(amount, AnchorPoint.Center);

    public Quad ChangeSize(float amount, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() + amount, GetRotationRad(), alignment);
    }
    
    public Quad ChangeSize(float widthAmount, float heightAmount)
    {
        return ChangeSize(new Size(widthAmount, heightAmount), AnchorPoint.Center);
        // var up = NormalUp * heightAmount;
        // var right = NormalRight * widthAmount;
        // var down = NormalDown * heightAmount;
        // var left = NormalLeft * widthAmount;
        //
        // var newA = A + up + left ;
        // var newB = B + down + left;
        // var newC = C + down + right;
        // var newD = D + up + right;
        // return new(newA, newB, newC, newD);
    }
    
    public Quad ChangeSize(float widthAmount, float heightAmount, AnchorPoint alignment)
    {
        return ChangeSize(new Size(widthAmount, heightAmount), alignment);
    }
    
    public Quad ChangeSize(Size amount) => ChangeSize(amount, AnchorPoint.Center);
    
    public Quad ChangeSize(Size amount, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() + amount, GetRotationRad(), alignment);
    }
    
    public Quad SetSize(float size) => SetSize(size, AnchorPoint.Center);
    
    public Quad SetSize(float size, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, new Size(size, size), GetRotationRad(), alignment);
        // Vector2 newA, newB, newC, newD;
        //
        // var origin = GetPoint(alignment);
        //
        // var wA = (A - origin);
        // var lSqA = wA.LengthSquared();
        // if (lSqA <= 0f) newA = A;
        // else
        // {
        //     var l = MathF.Sqrt(lSqA);
        //     var dir = wA / l;
        //     newA = origin + dir * size;
        // }
        //
        // var wB = (B - origin);
        // var lSqB = wB.LengthSquared();
        // if (lSqB <= 0f) newB = B;
        // else
        // {
        //     var l = MathF.Sqrt(lSqB);
        //     var dir = wB / l;
        //     newB = origin + dir * size;
        // }
        //
        // var wC = (C - origin);
        // var lSqC = wC.LengthSquared();
        // if (lSqC <= 0f) newC = C;
        // else
        // {
        //     var l = MathF.Sqrt(lSqC);
        //     var dir = wC / l;
        //     newC = origin + dir * size;
        // }
        //
        // var wD = (D - origin);
        // var lSqD = wD.LengthSquared();
        // if (lSqD <= 0f) newD = D;
        // else
        // {
        //     var l = MathF.Sqrt(lSqD);
        //     var dir = wD / l;
        //     newD = origin + dir * size;
        // }
        //
        // return new(newA, newB, newC, newD);
    }

    public Quad SetSize(Size size) => SetSize(size, AnchorPoint.Center);
   
    public Quad SetSize(Size size, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, size, GetRotationRad(), alignment);
    }
    #endregion
    
    #region Transform Methods

    public Quad ApplyOffset(Transform2D offset)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad);
        return newQuad.ChangeSize(offset.ScaledSize);
    }

    public Quad SetTransform(Transform2D transform)
    {
        var newQuad = SetPosition(transform.Position);
        newQuad = newQuad.SetRotation(transform.RotationRad);
        return newQuad.SetSize(transform.ScaledSize);
    }
    
    public Quad ApplyOffset(Transform2D offset, AnchorPoint alignment)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad, alignment);
        return newQuad.ChangeSize(offset.ScaledSize, alignment);
    }

    public Quad SetTransform(Transform2D transform, AnchorPoint alignment)
    {
        var newQuad = SetPosition(transform.Position, alignment);
        newQuad = newQuad.SetRotation(transform.RotationRad, alignment);
        return newQuad.SetSize(transform.ScaledSize, alignment);
    }

    #endregion
}