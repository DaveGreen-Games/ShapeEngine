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
    /// Populates the provided <see cref="Points"/> collection with the quad's vertices and their projected positions along a given vector.
    /// </summary>
    /// <param name="result">
    /// The <see cref="Points"/> collection to clear and fill with the original and projected vertices.
    /// </param>
    /// <param name="v">The vector along which to project the quad's vertices.</param>
    /// <returns>
    /// <see langword="true"/> if the projection vector is non-zero and the points were added; otherwise, <see langword="false"/>.
    /// </returns>
    public bool GetProjectedShapePoints(Points result, Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return false;
        
        result.Clear();
        result.EnsureCapacity(8);
        
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
        result.Add(A + v);
        result.Add(B + v);
        result.Add(C + v);
        result.Add(D + v);

        return true;
    }

    
    /// <summary>
    /// Projects the quad along a given vector and returns the convex hull as a polygon.
    /// </summary>
    /// <param name="v">The vector along which to project the quad's vertices.</param>
    /// <param name="useBuffer"><c>true</c> to reuse the internal points buffer and avoid a temporary allocation; <c>false</c> to allocate a new temporary buffer.
    /// Set this to <c>false</c> when calling from parallel or multi\-threaded code, since the internal buffer is shared and not thread\-safe.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull of the projected shape,
    /// or null if the vector is zero.</returns>
    /// <remarks>Useful for shadow or extrusion effects.</remarks>
    public Polygon? ProjectShape(Vector2 v, bool useBuffer = false)
    {
        if (v.LengthSquared() <= 0f) return null;
        
        Points buffer;

        if (useBuffer)
        {
            pointsBuffer.Clear();
            pointsBuffer.EnsureCapacity(4);
            
            pointsBuffer.Add(A);
            pointsBuffer.Add(B);
            pointsBuffer.Add(C);
            pointsBuffer.Add(D);
            pointsBuffer.Add(A + v);
            pointsBuffer.Add(B + v);
            pointsBuffer.Add(C + v);
            pointsBuffer.Add(D + v);
            
            buffer = pointsBuffer;
        }
        else
        {
            buffer = new Points
            {
                A,
                B,
                C,
                D,
                A + v,
                B + v,
                C + v,
                D + v
            };
        }

        Polygon result = new Polygon(8);
        buffer.FindConvexHull(result);
        return result;
    }
    
    /// <summary>
    /// Projects the quad along a given vector and stores the convex hull in the provided <see cref="Polygon"/>.
    /// </summary>
    /// <param name="result">
    /// The <see cref="Polygon"/> instance to populate with the convex hull of the projected shape.
    /// </param>
    /// <param name="v">The vector along which to project the quad's vertices.</param>
    /// <param name="useBuffer"><c>true</c> to reuse the internal points buffer and avoid a temporary allocation; <c>false</c> to allocate a new temporary buffer.
    /// Set this to <c>false</c> when calling from parallel or multi\-threaded code, since the internal buffer is shared and not thread\-safe.</param>
    /// <returns>
    /// <see langword="true"/> if the projection vector is non\-zero and the polygon was populated; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ProjectShape(Polygon result, Vector2 v, bool useBuffer = false)
    {
        if (v.LengthSquared() <= 0f) return false;
        
        Points buffer;

        if (useBuffer)
        {
            pointsBuffer.Clear();
            pointsBuffer.EnsureCapacity(4);
            
            pointsBuffer.Add(A);
            pointsBuffer.Add(B);
            pointsBuffer.Add(C);
            pointsBuffer.Add(D);
            pointsBuffer.Add(A + v);
            pointsBuffer.Add(B + v);
            pointsBuffer.Add(C + v);
            pointsBuffer.Add(D + v);
            
            buffer = pointsBuffer;
        }
        else
        {
            buffer = new Points
            {
                A,
                B,
                C,
                D,
                A + v,
                B + v,
                C + v,
                D + v
            };
        }
        
        buffer.FindConvexHull(result);
        return true;
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
    /// <summary>
    /// Gets the current rotation of the quad in radians.
    /// </summary>
    /// <returns>
    /// The angle, in radians, derived from the vector between the quad center and the center of the CD edge.
    /// </returns>
    /// <remarks>
    /// A quad with zero rotation matches the orientation of an axis-aligned rectangle.
    /// The rotation is inferred from the direction from <c>Center</c> to <c>CDCenter</c>.
    /// </remarks>
    public float GetRotationRad()
    {
        //A quad with 0 rotation should equal a standard rect.
        //If the vector from the center to the right side center is 0 the quad is not rotated
        return ShapeVec.AngleRad(CDCenter - Center);
    }
    
    /// <summary>
    /// Gets the current rotation of the quad in degrees.
    /// </summary>
    /// <returns>The quad rotation converted from radians to degrees.</returns>
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
    /// <summary>
    /// Gets the current size of the quad.
    /// </summary>
    /// <returns>
    /// A <see cref="Size"/> whose width is the length of edge DA and whose height is the length of edge AB.
    /// </returns>
    /// <remarks>
    /// This assumes the quad is being treated as a rectangle-like shape where adjacent edge lengths
    /// represent width and height regardless of rotation.
    /// </remarks>
    public Size GetSize()
    {
        var e1 = B - A;
        var e2 = D - A;
        float height = e1.Length();
        float width = e2.Length();
        return new Size(width, height);
    }
    
    /// <summary>
    /// Scales the quad uniformly relative to its center.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to both width and height.</param>
    /// <returns>A new <see cref="Quad"/> with its size scaled around the center.</returns>
    public Quad ScaleSize(float scale)
    {
        return ScaleSize(scale, AnchorPoint.Center);
    }

    /// <summary>
    /// Scales the quad non-uniformly relative to its center.
    /// </summary>
    /// <param name="scale">
    /// A <see cref="Size"/> containing the width and height scale factors.
    /// </param>
    /// <returns>A new <see cref="Quad"/> with its size scaled around the center.</returns>
    public Quad ScaleSize(Size scale)
    {
        return ScaleSize(scale, AnchorPoint.Center);
    }
    
    /// <summary>
    /// Scales the quad uniformly around the specified anchor point.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to both width and height.</param>
    /// <param name="alignment">The anchor point that remains fixed during scaling.</param>
    /// <returns>A new <see cref="Quad"/> with its size scaled around the given anchor.</returns>
    public Quad ScaleSize(float scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() * scale, GetRotationRad(), alignment);
    }

    /// <summary>
    /// Scales the quad non-uniformly around the specified anchor point.
    /// </summary>
    /// <param name="scale">
    /// A <see cref="Size"/> containing the width and height scale factors.
    /// </param>
    /// <param name="alignment">The anchor point that remains fixed during scaling.</param>
    /// <returns>A new <see cref="Quad"/> with its size scaled around the given anchor.</returns>
    public Quad ScaleSize(Size scale, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() * scale, GetRotationRad(), alignment);
    }

    /// <summary>
    /// Changes the quad by moving each corner farther from or closer to the specified anchor point
    /// along the corner's current diagonal direction.
    /// </summary>
    /// <param name="amount">
    /// The distance to add to each corner's current distance from the anchor point.
    /// Positive values expand the quad; negative values contract it.
    /// </param>
    /// <param name="alignment">The anchor point used as the origin for the diagonal resize.</param>
    /// <returns>A new <see cref="Quad"/> with all corners adjusted radially from the anchor point.</returns>
    /// <remarks>
    /// Each vertex is processed independently. If a vertex already lies exactly on the anchor point,
    /// that vertex remains unchanged to avoid division by zero.
    /// </remarks>
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
    
    /// <summary>
    /// Changes the quad size uniformly by the same amount on both axes around its center.
    /// </summary>
    /// <param name="amount">
    /// The amount to add to both width and height.
    /// </param>
    /// <returns>A new <see cref="Quad"/> with the adjusted size.</returns>
    public Quad ChangeSize(float amount) => ChangeSize(amount, AnchorPoint.Center);

    /// <summary>
    /// Changes the quad size uniformly by the same amount on both axes around the specified anchor point.
    /// </summary>
    /// <param name="amount">The amount to add to both width and height.</param>
    /// <param name="alignment">The anchor point that remains fixed during the resize.</param>
    /// <returns>A new <see cref="Quad"/> with the adjusted size.</returns>
    public Quad ChangeSize(float amount, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() + amount, GetRotationRad(), alignment);
    }
    
    /// <summary>
    /// Changes the quad size by independent width and height amounts around its center.
    /// </summary>
    /// <param name="widthAmount">The amount to add to the current width.</param>
    /// <param name="heightAmount">The amount to add to the current height.</param>
    /// <returns>A new <see cref="Quad"/> with the adjusted width and height.</returns>
    public Quad ChangeSize(float widthAmount, float heightAmount)
    {
        return ChangeSize(new Size(widthAmount, heightAmount), AnchorPoint.Center);
    }
    
    /// <summary>
    /// Changes the quad size by independent width and height amounts around the specified anchor point.
    /// </summary>
    /// <param name="widthAmount">The amount to add to the current width.</param>
    /// <param name="heightAmount">The amount to add to the current height.</param>
    /// <param name="alignment">The anchor point that remains fixed during the resize.</param>
    /// <returns>A new <see cref="Quad"/> with the adjusted width and height.</returns>
    public Quad ChangeSize(float widthAmount, float heightAmount, AnchorPoint alignment)
    {
        return ChangeSize(new Size(widthAmount, heightAmount), alignment);
    }
    
    /// <summary>
    /// Changes the quad size by the specified width and height amounts around its center.
    /// </summary>
    /// <param name="amount">
    /// A <see cref="Size"/> whose components are added to the current width and height.
    /// </param>
    /// <returns>A new <see cref="Quad"/> with the adjusted size.</returns>
    public Quad ChangeSize(Size amount) => ChangeSize(amount, AnchorPoint.Center);
    
    /// <summary>
    /// Changes the quad size by the specified width and height amounts around the given anchor point.
    /// </summary>
    /// <param name="amount">
    /// A <see cref="Size"/> whose components are added to the current width and height.
    /// </param>
    /// <param name="alignment">The anchor point that remains fixed during the resize.</param>
    /// <returns>A new <see cref="Quad"/> with the adjusted size.</returns>
    public Quad ChangeSize(Size amount, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, GetSize() + amount, GetRotationRad(), alignment);
    }
    
    /// <summary>
    /// Sets the quad to a uniform square size around its center.
    /// </summary>
    /// <param name="size">The target width and height value.</param>
    /// <returns>A new <see cref="Quad"/> with both width and height set to <paramref name="size"/>.</returns>
    public Quad SetSize(float size) => SetSize(size, AnchorPoint.Center);
    
    /// <summary>
    /// Sets the quad to a uniform square size around the specified anchor point.
    /// </summary>
    /// <param name="size">The target width and height value.</param>
    /// <param name="alignment">The anchor point that remains fixed during the resize.</param>
    /// <returns>A new <see cref="Quad"/> with both width and height set to <paramref name="size"/>.</returns>
    public Quad SetSize(float size, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, new Size(size, size), GetRotationRad(), alignment);
    }

    /// <summary>
    /// Sets the quad size around its center.
    /// </summary>
    /// <param name="size">The target width and height.</param>
    /// <returns>A new <see cref="Quad"/> with the specified size.</returns>
    public Quad SetSize(Size size) => SetSize(size, AnchorPoint.Center);
   
    /// <summary>
    /// Sets the quad size around the specified anchor point.
    /// </summary>
    /// <param name="size">The target width and height.</param>
    /// <param name="alignment">The anchor point that remains fixed during the resize.</param>
    /// <returns>A new <see cref="Quad"/> with the specified size.</returns>
    public Quad SetSize(Size size, AnchorPoint alignment)
    {
        var p = GetPoint(alignment);
        return new(p, size, GetRotationRad(), alignment);
    }
    #endregion
    
    #region Transform Methods

    /// <summary>
    /// Applies a relative transform offset to the quad using its center as the anchor point.
    /// </summary>
    /// <param name="offset">
    /// The transform offset containing positional, rotational, and size adjustments.
    /// </param>
    /// <returns>
    /// A new <see cref="Quad"/> translated, rotated, and resized by the provided offset.
    /// </returns>
    /// <remarks>
    /// The operations are applied in the following order: position, rotation, then size.
    /// </remarks>
    public Quad ApplyOffset(Transform2D offset)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad);
        return newQuad.ChangeSize(offset.ScaledSize);
    }

    /// <summary>
    /// Replaces the quad transform using its center as the anchor point.
    /// </summary>
    /// <param name="transform">
    /// The target transform containing the desired position, rotation, and size.
    /// </param>
    /// <returns>
    /// A new <see cref="Quad"/> whose transform matches the provided values.
    /// </returns>
    /// <remarks>
    /// The operations are applied in the following order: position, rotation, then size.
    /// </remarks>
    public Quad SetTransform(Transform2D transform)
    {
        var newQuad = SetPosition(transform.Position);
        newQuad = newQuad.SetRotation(transform.RotationRad);
        return newQuad.SetSize(transform.ScaledSize);
    }
    
    /// <summary>
    /// Applies a relative transform offset to the quad using the specified anchor point.
    /// </summary>
    /// <param name="offset">
    /// The transform offset containing positional, rotational, and size adjustments.
    /// </param>
    /// <param name="alignment">The anchor point used for rotation and resizing.</param>
    /// <returns>
    /// A new <see cref="Quad"/> translated, rotated, and resized by the provided offset.
    /// </returns>
    /// <remarks>
    /// The translation is applied directly, while rotation and size changes are performed relative
    /// to <paramref name="alignment"/>.
    /// </remarks>
    public Quad ApplyOffset(Transform2D offset, AnchorPoint alignment)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad, alignment);
        return newQuad.ChangeSize(offset.ScaledSize, alignment);
    }

    /// <summary>
    /// Replaces the quad transform using the specified anchor point.
    /// </summary>
    /// <param name="transform">
    /// The target transform containing the desired position, rotation, and size.
    /// </param>
    /// <param name="alignment">The anchor point used for positioning, rotation, and resizing.</param>
    /// <returns>
    /// A new <see cref="Quad"/> whose transform matches the provided values.
    /// </returns>
    /// <remarks>
    /// The operations are applied in the following order: position, rotation, then size.
    /// </remarks>
    public Quad SetTransform(Transform2D transform, AnchorPoint alignment)
    {
        var newQuad = SetPosition(transform.Position, alignment);
        newQuad = newQuad.SetRotation(transform.RotationRad, alignment);
        return newQuad.SetSize(transform.ScaledSize, alignment);
    }

    #endregion
}