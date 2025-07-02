using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    #region Transform

    /// <summary>
    /// Rotates the triangle around its centroid by the specified angle.
    /// </summary>
    /// <param name="rad">The rotation angle in radians.</param>
    /// <returns>A new triangle rotated by the specified angle around its centroid.</returns>
    /// <remarks>
    /// This method uses the triangle's centroid as the rotation origin for a balanced rotation.
    /// </remarks>
    public Triangle ChangeRotation(float rad)
    {
        return ChangeRotation(rad, GetCentroid());
    }

    /// <summary>
    /// Rotates the triangle around a specified origin point by the specified angle.
    /// </summary>
    /// <param name="rad">The rotation angle in radians.</param>
    /// <param name="origin">The point around which to rotate the triangle.</param>
    /// <returns>A new triangle rotated by the specified angle around the given origin.</returns>
    /// <remarks>
    /// This method rotates each vertex of the triangle around the specified origin point.
    /// </remarks>
    public Triangle ChangeRotation(float rad, Vector2 origin)
    {
        var newA = origin + (A - origin).Rotate(rad);
        var newB = origin + (B - origin).Rotate(rad);
        var newC = origin + (C - origin).Rotate(rad);
        return new(newA, newB, newC);
    }

    /// <summary>
    /// Sets the absolute rotation of the triangle to the specified angle, using the centroid as the rotation origin.
    /// </summary>
    /// <param name="rad">The target rotation angle in radians.</param>
    /// <returns>A new triangle with the specified absolute rotation.</returns>
    /// <remarks>
    /// This method calculates the shortest rotation needed to reach the target angle from the current orientation.
    /// The current orientation is determined by the direction from the centroid to vertex A.
    /// </remarks>
    public Triangle SetRotation(float rad)
    {
        var origin = GetCentroid();
        var w = A - origin;
        var currentAngleRad = w.AngleRad();
        var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
        return ChangeRotation(amount, origin);
    }

    /// <summary>
    /// Sets the absolute rotation of the triangle to the specified angle around a given origin point.
    /// </summary>
    /// <param name="rad">The target rotation angle in radians.</param>
    /// <param name="origin">The point around which to set the rotation.</param>
    /// <returns>A new triangle with the specified absolute rotation around the given origin.</returns>
    /// <remarks>
    /// This method determines the current orientation by finding the first non-zero vector from the origin
    /// to any vertex, then calculates the shortest rotation to reach the target angle.
    /// </remarks>
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

    /// <summary>
    /// Scales the triangle uniformly by the specified factor around the origin.
    /// </summary>
    /// <param name="scale">The scaling factor to apply to all vertices.</param>
    /// <returns>A new triangle scaled by the specified factor.</returns>
    /// <remarks>This method scales all vertices relative to the coordinate system origin (0,0).</remarks>
    public Triangle ScaleSize(float scale) => this * scale;
    
    /// <summary>
    /// Scales the triangle by different factors for X and Y dimensions around the origin.
    /// </summary>
    /// <param name="scale">The scaling factors for X and Y dimensions.</param>
    /// <returns>A new triangle scaled by the specified factors.</returns>
    /// <remarks>This method allows non-uniform scaling of the triangle relative to the coordinate system origin.</remarks>
    public Triangle ScaleSize(Size scale) => new Triangle(A * scale, B * scale, C * scale);

    /// <summary>
    /// Scales the triangle uniformly by the specified factor around a given origin point.
    /// </summary>
    /// <param name="scale">The scaling factor to apply to all vertices.</param>
    /// <param name="origin">The point around which to scale the triangle.</param>
    /// <returns>A new triangle scaled by the specified factor around the given origin.</returns>
    /// <remarks>This method scales the triangle relative to the specified origin point rather than the coordinate system origin.</remarks>
    public Triangle ScaleSize(float scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }

    /// <summary>
    /// Scales the triangle by different factors for X and Y dimensions around a given origin point.
    /// </summary>
    /// <param name="scale">The scaling factors for X and Y dimensions.</param>
    /// <param name="origin">The point around which to scale the triangle.</param>
    /// <returns>A new triangle scaled by the specified factors around the given origin.</returns>
    /// <remarks>This method allows non-uniform scaling relative to the specified origin point.</remarks>
    public Triangle ScaleSize(Size scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }

    /// <summary>
    /// Changes the size of the triangle by moving each vertex away from or toward the centroid by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the size. Positive values increase size, negative values decrease size.</param>
    /// <returns>A new triangle with vertices moved by the specified amount relative to the centroid.</returns>
    /// <remarks>This method changes the triangle's size while maintaining its shape and centroid position.</remarks>
    public Triangle ChangeSize(float amount) => ChangeSize(amount, GetCentroid());

    /// <summary>
    /// Changes the size of the triangle by moving each vertex away from or toward a specified origin by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the size. Positive values move vertices away from origin, negative values move them closer.</param>
    /// <param name="origin">The reference point from which to change the triangle size.</param>
    /// <returns>A new triangle with vertices moved by the specified amount relative to the origin.</returns>
    /// <remarks>
    /// This method moves each vertex along the line from the origin to the vertex by the specified amount.
    /// The triangle's shape is preserved but its overall size changes.
    /// </remarks>
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

    /// <summary>
    /// Sets the triangle to a specific size by scaling it uniformly around its centroid.
    /// </summary>
    /// <param name="size">The target size (average distance from centroid to vertices).</param>
    /// <returns>A new triangle scaled to the specified size around its centroid.</returns>
    /// <remarks>
    /// This method calculates the current average distance from centroid to vertices and scales
    /// the triangle to achieve the target size while maintaining its shape.
    /// </remarks>
    public Triangle SetSize(float size) => SetSize(size, GetCentroid());

    /// <summary>
    /// Sets the triangle to a specific size by scaling it uniformly around a specified origin.
    /// </summary>
    /// <param name="size">The target size (average distance from origin to vertices).</param>
    /// <param name="origin">The point around which to scale the triangle to the target size.</param>
    /// <returns>A new triangle scaled to the specified size around the given origin.</returns>
    /// <remarks>
    /// This method calculates the current average distance from origin to vertices and scales
    /// the triangle to achieve the target size while maintaining its shape.
    /// </remarks>
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

    /// <summary>
    /// Translates the triangle by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to translate all vertices of the triangle.</param>
    /// <returns>A new triangle translated by the specified offset.</returns>
    /// <remarks>This method moves the entire triangle without changing its size, shape, or orientation.</remarks>
    public Triangle ChangePosition(Vector2 offset)
    {
        return new(A + offset, B + offset, C + offset);
    }

    /// <summary>
    /// Sets the triangle's position so that its centroid is at the specified position.
    /// </summary>
    /// <param name="position">The target position for the triangle's centroid.</param>
    /// <returns>A new triangle with its centroid positioned at the specified location.</returns>
    /// <remarks>This method translates the triangle to position its centroid at the given coordinates.</remarks>
    public Triangle SetPosition(Vector2 position)
    {
        var centroid = GetCentroid();
        var delta = position - centroid;
        return ChangePosition(delta);
    }

    /// <summary>
    /// Sets the triangle's position so that the specified origin point within the triangle is at the target position.
    /// </summary>
    /// <param name="position">The target position for the origin point.</param>
    /// <param name="origin">The reference point within the triangle to position at the target location.</param>
    /// <returns>A new triangle positioned so the origin point is at the specified position.</returns>
    /// <remarks>
    /// This method allows positioning the triangle relative to any point, not just its centroid.
    /// The origin point acts as a handle for positioning the triangle.
    /// </remarks>
    public Triangle SetPosition(Vector2 position, Vector2 origin)
    {
        var delta = position - origin;
        return ChangePosition(delta);
    }

    /// <summary>
    /// Moves the triangle by offset.Position, rotates the result around its new centroid by offset.RotationRad,
    /// and changes the size of the rotated triangle by offset.ScaledSize.Length.
    /// </summary>
    /// <param name="offset">The transformation to apply.</param>
    /// <returns>A new triangle with the transformation applied.</returns>
    public Triangle ApplyOffset(Transform2D offset)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad);
        return newTriangle.ChangeSize(offset.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the triangle by offset.Position, rotates the result around the specified origin by offset.RotationRad,
    /// and changes the size of the rotated triangle by offset.ScaledSize.Length around the same origin.
    /// </summary>
    /// <param name="offset">The transformation to apply.</param>
    /// <param name="origin">The origin point for rotation and scaling.</param>
    /// <returns>A new triangle with the transformation applied.</returns>
    public Triangle ApplyOffset(Transform2D offset, Vector2 origin)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad, origin);
        return newTriangle.ChangeSize(offset.ScaledSize.Length, origin);
    }

    /// <summary>
    /// Moves the triangle to transform.Position, sets the rotation of the moved triangle to transform.RotationRad,
    /// and sets the size of the rotated triangle to transform.ScaledSize.Length. The centroid is used as the origin for rotation and scaling.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new triangle with the transform applied.</returns>
    public Triangle SetTransform(Transform2D transform)
    {
        var newTriangle = SetPosition(transform.Position);
        newTriangle = newTriangle.SetRotation(transform.RotationRad);
        return newTriangle.SetSize(transform.ScaledSize.Length);
    }

    /// <summary>
    /// Moves the triangle to transform.Position, sets the rotation of the moved triangle to transform.RotationRad,
    /// and sets the size of the rotated triangle to transform.ScaledSize.Length. All transformations are performed
    /// relative to the specified origin.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <param name="origin">The origin for all transformations.</param>
    /// <returns>A new triangle with the transform applied.</returns>
    public Triangle SetTransform(Transform2D transform, Vector2 origin)
    {
        var newTriangle = SetPosition(transform.Position, origin);
        newTriangle = newTriangle.SetRotation(transform.RotationRad, origin);
        return newTriangle.SetSize(transform.ScaledSize.Length, origin);
    }

    /// <summary>
    /// Applies a transformation to the triangle by scaling each vertex by <paramref name="transform"/>'s scaled size length,
    /// rotating by <paramref name="transform"/>'s rotation in radians, and translating by <paramref name="transform"/>'s position.
    /// </summary>
    /// <param name="transform">The transformation to apply, including position, rotation, and scale.</param>
    /// <returns>A new triangle with the transformation applied to all vertices.</returns>
    public Triangle ApplyTransform(Transform2D transform)
    {
        var aAbsolute = transform.Position + (A * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        var bAbsolute = transform.Position + (B * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        var cAbsolute = transform.Position + (C * transform.ScaledSize.Length).Rotate(transform.RotationRad);
        return new Triangle(aAbsolute, bAbsolute, cAbsolute);
    }

    #endregion

    #region Math

    /// <summary>
    /// Determines whether the triangle is geometrically valid.
    /// </summary>
    /// <returns>True if the triangle has a positive area; otherwise, false.</returns>
    /// <remarks>
    /// A triangle is considered valid if its vertices are not collinear and it has a measurable area.
    /// Invalid triangles may result from degenerate cases where all vertices lie on the same line.
    /// </remarks>
    public bool IsValid()
    {
        return GetArea() > 0f;
    }

    /// <summary>
    /// Calculates and returns the centroid (geometric center) of the triangle.
    /// </summary>
    /// <returns>The centroid point, which is the average of all three vertices.</returns>
    /// <remarks>
    /// The centroid is the point where the triangle would balance if it were a physical object of uniform density.
    /// It is always located inside the triangle, regardless of the triangle's shape.
    /// </remarks>
    public Vector2 GetCentroid() => (A + B + C) / 3;

    /// <summary>
    /// Gets the points that form the projected shape when this triangle is extruded along a vector.
    /// </summary>
    /// <param name="v">The vector along which to project the triangle.</param>
    /// <returns>A collection of points forming the projected shape, or null if the vector has zero length.</returns>
    /// <remarks>
    /// This method creates a 3D extrusion effect by projecting each vertex along the given vector,
    /// resulting in a hexagon-like shape when viewed from certain angles.
    /// </remarks>
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

    /// <summary>
    /// Projects the triangle along a vector to create a convex hull polygon.
    /// </summary>
    /// <param name="v">The vector along which to project the triangle.</param>
    /// <returns>A convex polygon representing the projected shape, or null if the vector has zero length.</returns>
    /// <remarks>
    /// This method creates a shadow-like projection by extruding the triangle along the vector
    /// and computing the convex hull of all resulting points.
    /// </remarks>
    public Polygon? ProjectShape(Vector2 v)
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
        return Polygon.FindConvexHull(points);
    }

    /// <summary>
    /// Returns a new triangle with all vertex coordinates rounded down to the nearest integer.
    /// </summary>
    /// <returns>A triangle with floor-rounded vertex coordinates.</returns>
    /// <remarks>This method is useful for pixel-perfect rendering or grid-aligned positioning.</remarks>
    public Triangle Floor()
    {
        return new(A.Floor(), B.Floor(), C.Floor());
    }

    /// <summary>
    /// Returns a new triangle with all vertex coordinates rounded up to the nearest integer.
    /// </summary>
    /// <returns>A triangle with ceiling-rounded vertex coordinates.</returns>
    /// <remarks>This method is useful for pixel-perfect rendering or grid-aligned positioning.</remarks>
    public Triangle Ceiling()
    {
        return new(A.Ceiling(), B.Ceiling(), C.Ceiling());
    }

    /// <summary>
    /// Returns a new triangle with all vertex coordinates rounded to the nearest integer.
    /// </summary>
    /// <returns>A triangle with rounded vertex coordinates.</returns>
    /// <remarks>This method is useful for pixel-perfect rendering or grid-aligned positioning.</remarks>
    public Triangle Round()
    {
        return new(A.Round(), B.Round(), C.Round());
    }

    /// <summary>
    /// Returns a new triangle with all vertex coordinates truncated to their integer parts.
    /// </summary>
    /// <returns>A triangle with truncated vertex coordinates.</returns>
    /// <remarks>This method removes the fractional parts of all vertex coordinates.</remarks>
    public Triangle Truncate()
    {
        return new(A.Truncate(), B.Truncate(), C.Truncate());
    }

    /// <summary>
    /// Calculates the perimeter of the triangle.
    /// </summary>
    /// <returns>The sum of the lengths of all three sides of the triangle.</returns>
    /// <remarks>The perimeter is useful for calculations involving the triangle's boundary length.</remarks>
    public float GetPerimeter() => SideA.Length() + SideB.Length() + SideC.Length();
    
    /// <summary>
    /// Calculates the squared perimeter of the triangle.
    /// </summary>
    /// <returns>The sum of the squared lengths of all three sides of the triangle.</returns>
    /// <remarks>
    /// This method avoids expensive square root calculations and is useful for performance-sensitive
    /// comparisons where only relative perimeter sizes matter.
    /// </remarks>
    public float GetPerimeterSquared() => SideA.LengthSquared() + SideB.LengthSquared() + SideC.LengthSquared();
    
    /// <summary>
    /// Calculates the area of the triangle using the cross product method.
    /// </summary>
    /// <returns>The area of the triangle in square units.</returns>
    /// <remarks>
    /// This method uses the cross product formula for efficiency and numerical stability.
    /// The result is always positive regardless of vertex ordering.
    /// </remarks>
    public float GetArea() => MathF.Abs((A.X - C.X) * (B.Y - C.Y) - (A.Y - C.Y) * (B.X - C.X)) / 2f;

    /// <summary>
    /// Determines whether the triangle is narrow (has very small angles) based on cross product analysis.
    /// </summary>
    /// <param name="narrowValue">The threshold value for determining narrowness (default: 0.2). Lower values are more restrictive.</param>
    /// <returns>True if any angle in the triangle is smaller than the threshold; otherwise, false.</returns>
    /// <remarks>
    /// Narrow triangles can cause numerical issues in various geometric algorithms and may indicate
    /// degenerate cases. This method checks each angle by examining the cross product of normalized edge vectors.
    /// </remarks>
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
