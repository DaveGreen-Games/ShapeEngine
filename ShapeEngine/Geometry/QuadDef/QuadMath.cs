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

    #endregion

    #region Transform

    /// <summary>
    /// Rotates the quad by a specified angle in radians around a given anchor point.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns>A new <see cref="Quad"/> rotated by the specified angle.</returns>
    public Quad ChangeRotation(float rad, AnchorPoint alignment)
    {
        var pivotPoint = GetPoint(alignment);
        var a = pivotPoint + (A - pivotPoint).Rotate(rad);
        var b = pivotPoint + (B - pivotPoint).Rotate(rad);
        var c = pivotPoint + (C - pivotPoint).Rotate(rad);
        var d = pivotPoint + (D - pivotPoint).Rotate(rad);
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
        float amount = ShapeMath.GetShortestAngleRad(AngleRad, angleRad);
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

    /// <summary>
    /// Scales the size of the quad uniformly by a scalar value, relative to the origin.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the given factor.</returns>
    public Quad ScaleSize(float scale) => this * scale;

    /// <summary>
    /// Scales the size of the quad component-wise by a <see cref="Size"/>, relative to the origin.
    /// </summary>
    /// <param name="scale">The scale factors for width and height.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the given size.</returns>
    public Quad ScaleSize(Size scale) => new Quad(A * scale, B * scale, C * scale, D * scale);

    /// <summary>
    /// Scales the size of the quad uniformly by a scalar value, relative to a specified anchor point.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="alignment">The anchor point for scaling.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the given factor around the anchor point.</returns>
    public Quad ScaleSize(float scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );
    }

    /// <summary>
    /// Scales the size of the quad component-wise by a <see cref="Size"/>, relative to a specified anchor point.
    /// </summary>
    /// <param name="scale">The scale factors for width and height.</param>
    /// <param name="alignment">The anchor point for scaling.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the given size around the anchor point.</returns>
    public Quad ScaleSize(Size scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );
    }

    /// <summary>
    /// Changes the size of the quad by a specified amount, relative to its center.
    /// </summary>
    /// <param name="amount">The amount to change the size by.</param>
    /// <returns>A new <see cref="Quad"/> with the size changed by the specified amount.</returns>
    public Quad ChangeSize(float amount) => ChangeSize(amount, AnchorPoint.Center);

    /// <summary>
    /// Changes the size of the quad by a specified amount, relative to a specified anchor point.
    /// </summary>
    /// <param name="amount">The amount to change the size by.</param>
    /// <param name="alignment">The anchor point for resizing.</param>
    /// <returns>A new <see cref="Quad"/> with the size changed by the specified amount around the anchor point.</returns>
    public Quad ChangeSize(float amount, AnchorPoint alignment)
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

    /// <summary>
    /// Sets the size of the quad to a specific value, relative to its center.
    /// </summary>
    /// <param name="size">The new size for the quad.</param>
    /// <returns>A new <see cref="Quad"/> with the specified size.</returns>
    public Quad SetSize(float size) => SetSize(size, AnchorPoint.Center);

    /// <summary>
    /// Sets the size of the quad to a specific value, relative to a specified anchor point.
    /// </summary>
    /// <param name="size">The new size for the quad.</param>
    /// <param name="alignment">The anchor point for resizing.</param>
    /// <returns>A new <see cref="Quad"/> with the specified size around the anchor point.</returns>
    public Quad SetSize(float size, AnchorPoint alignment)
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
      /// Applies a transform to the quad by:
      /// <list type="bullet">
      /// <item>Moving it by <paramref name="offset"/>.Position</item>
      /// <item>Rotating the moved quad by <paramref name="offset"/>.RotationRad</item>
      /// <item>Changing the size of the rotated quad by <paramref name="offset"/>.ScaledSize.Length</item>
      /// </list>
      /// </summary>
      /// <param name="offset">The transform to apply to the quad.</param>
      /// <returns>A new <see cref="Quad"/> with the applied transform.</returns>
    public Quad ApplyOffset(Transform2D offset)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad);
        return newQuad.ChangeSize(offset.ScaledSize.Length);
    }

    /// <summary>
    /// Sets the transform of the quad to match the given <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="transform">The transform to set.</param>
    /// <returns>A new <see cref="Quad"/> with the specified transform.</returns>
    /// <remarks>Moves, rotates, and sets the size of the quad in sequence.</remarks>
    public Quad SetTransform(Transform2D transform)
    {
        var newQuad = SetPosition(transform.Position);
        newQuad = newQuad.SetRotation(transform.RotationRad);
        return newQuad.SetSize(transform.ScaledSize.Length);
    }

    /// <summary>
    /// Applies a transform to the quad by:
    /// <list type="bullet">
    /// <item>Moving it by <paramref name="offset"/>.Position</item>
    /// <item>Rotating the moved quad by <paramref name="offset"/>.RotationRad around the specified <paramref name="alignment"/></item>
    /// <item>Changing the size of the rotated quad by <paramref name="offset"/>.ScaledSize.Length, relative to <paramref name="alignment"/></item>
    /// </list>
    /// </summary>
    /// <param name="offset">The transform to apply to the quad.</param>
    /// <param name="alignment">The anchor point for rotation and scaling.</param>
    /// <returns>A new <see cref="Quad"/> with the applied transform.</returns>
    public Quad ApplyOffset(Transform2D offset, AnchorPoint alignment)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad, alignment);
        return newQuad.ChangeSize(offset.ScaledSize.Length, alignment);
    }

    /// <summary>
    /// Sets the transform of the quad by:
    /// <list type="bullet">
    /// <item>Moving it to <paramref name="transform"/>.Position, aligning <paramref name="alignment"/></item>
    /// <item>Rotating the moved quad to <paramref name="transform"/>.RotationRad around <paramref name="alignment"/></item>
    /// <item>Setting the size of the rotated quad to <paramref name="transform"/>.ScaledSize.Length, relative to <paramref name="alignment"/></item>
    /// </list>
    /// </summary>
    /// <param name="transform">The transform to set.</param>
    /// <param name="alignment">The anchor point for alignment, rotation, and scaling.</param>
    /// <returns>A new <see cref="Quad"/> with the specified transform.</returns>
    public Quad SetTransform(Transform2D transform, AnchorPoint alignment)
    {
        var newQuad = SetPosition(transform.Position, alignment);
        newQuad = newQuad.SetRotation(transform.RotationRad, alignment);
        return newQuad.SetSize(transform.ScaledSize.Length, alignment);
    }

    #endregion
}