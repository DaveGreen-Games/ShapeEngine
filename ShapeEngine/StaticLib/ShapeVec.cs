using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides extension methods and utilities for working with <see cref="Vector2"/> in 2D space.
/// Includes vector math, geometric, and utility functions for game  development.
/// </summary>
/// <remarks>
/// All methods are static and designed to extend the functionality of <see cref="Vector2"/>.
/// </remarks>
public static class ShapeVec
{
    /// <summary>
    /// Calculates the dot product between two normalized vectors and remaps it to the range 0-1.
    /// </summary>
    /// <param name="v1">The first normalized vector.</param>
    /// <param name="v2">The second normalized vector.</param>
    /// <returns>Returns 1 if both vectors point in the same direction, 0 if opposite.</returns>
    /// <remarks>Both vectors must be normalized for correct results.</remarks>
    public static float CalculateDotFactor(this Vector2 v1, Vector2 v2)
    {
        var dot =  v1.X * v2.X + v1.Y * v2.Y;
        return (dot + 1f) * 0.5f;
    }
    /// <summary>
    /// Calculates the dot product between two normalized vectors and remaps it to a custom range.
    /// </summary>
    /// <param name="v1">The first normalized vector.</param>
    /// <param name="v2">The second normalized vector.</param>
    /// <param name="min">The minimum value of the remapped range.</param>
    /// <param name="max">The maximum value of the remapped range.</param>
    /// <returns>Returns the dot product as a factor between <paramref name="min"/> and <paramref name="max"/>.</returns>
    /// <remarks>Both vectors must be normalized for correct results.</remarks>
    public static float CalculateDotFactor(this Vector2 v1, Vector2 v2, float min, float max)
    {
        var dot =  v1.X * v2.X + v1.Y * v2.Y;
        return ShapeMath.RemapFloat(dot, -1f, 1f, min, max);
    }
    /// <summary>
    /// Calculates the dot product between two normalized vectors and remaps it to the range 0-1, reversed.
    /// </summary>
    /// <param name="v1">The first normalized vector.</param>
    /// <param name="v2">The second normalized vector.</param>
    /// <returns>Returns 0 if both vectors point in the same direction, 1 if opposite.</returns>
    /// <remarks>Both vectors must be normalized for correct results.</remarks>
    public static float CalculateDotFactorReverse(this Vector2 v1, Vector2 v2)
    {
        var dot =  v1.X * v2.X + v1.Y * v2.Y;
        dot *= -1f;
        return (dot + 1f) * 0.5f;
    }
    /// <summary>
    /// Calculates the dot product between two normalized vectors and remaps it to a custom range, reversed.
    /// </summary>
    /// <param name="v1">The first normalized vector.</param>
    /// <param name="v2">The second normalized vector.</param>
    /// <param name="min">The minimum value of the remapped range.</param>
    /// <param name="max">The maximum value of the remapped range.</param>
    /// <returns>Returns the dot product as a factor between <paramref name="min"/> and <paramref name="max"/> (reversed).</returns>
    /// <remarks>Both vectors must be normalized for correct results.</remarks>
    public static float CalculateDotFactorReverse(this Vector2 v1, Vector2 v2, float min, float max)
    {
        var dot =  v1.X * v2.X + v1.Y * v2.Y;
        dot *= -1f;
        return ShapeMath.RemapFloat(dot, -1f, 1f, min, max);
    }
    /// <summary>
    /// Returns a formatted string representation of the vector with two decimal places.
    /// </summary>
    /// <param name="v">The vector to format.</param>
    /// <returns>String in the format x / y.</returns>
    public static string ToString(this Vector2 v)
    {
        return $"<{v.X:F2} / {v.Y:F2}>";
    }
    /// <summary>
    /// Converts a <see cref="Vector2"/> to polar coordinates.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <returns>A <see cref="PolarCoordinates"/> struct representing the vector.</returns>
    public static PolarCoordinates ToPolarCoordinates(this Vector2 v) => new(v);
    /// <summary>
    /// Checks if the vector is normalized (length squared is approximately 1).
    /// </summary>
    /// <param name="v">The vector to check.</param>
    /// <returns>True if normalized, otherwise false.</returns>
    public static bool IsNormalized(this Vector2 v) => Math.Abs(v.LengthSquared() - 1f) < 0.0000001f;
    /// <summary>
    /// Checks if both components of the vector are finite numbers.
    /// </summary>
    /// <param name="v">The vector to check.</param>
    /// <returns>True if both X and Y are finite, otherwise false.</returns>
    public static bool IsFinite(this Vector2 v) => float.IsFinite(v.X) && float.IsFinite(v.Y);
    /// <summary>
    /// Converts a <see cref="Vector2"/> to a <see cref="Size"/> struct.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <returns>A <see cref="Size"/> with X and Y as width and height.</returns>
    public static Size ToSize(this Vector2 v) => new(v.X, v.Y);
    /// <summary>
    /// Checks if two vectors are similar within a given tolerance.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <param name="tolerance">The allowed tolerance for each component (default 0.001).</param>
    /// <returns>True if both components differ by less than or equal to the tolerance.</returns>
    public static bool IsSimilar(this Vector2 a, Vector2 b, float tolerance = 0.001f)
    {
        return 
            MathF.Abs(a.X - b.X) <= tolerance &&
            MathF.Abs(a.Y - b.Y) <= tolerance;
    }
    /// <summary>
    /// Checks if both components of a vector are similar to a scalar value within a given tolerance.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <param name="tolerance">The allowed tolerance (default 0.001).</param>
    /// <returns>True if both X and Y differ from b by less than or equal to the tolerance.</returns>
    public static bool IsSimilar(this Vector2 a, float b, float tolerance = 0.001f)
    {
        return 
            MathF.Abs(a.X - b) <= tolerance &&
            MathF.Abs(a.Y - b) <= tolerance;
    }
    /// <summary>
    /// Returns the vector with both components negated.
    /// </summary>
    /// <param name="v">The vector to flip.</param>
    /// <returns>The flipped vector.</returns>
    public static Vector2 Flip(this Vector2 v) { return v * -1f; }
    /// <summary>
    /// Determines if two vectors are facing the same direction (dot product &gt; 0).
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>True if facing the same direction, otherwise false.</returns>
    public static bool IsFacingTheSameDirection(this Vector2 a,  Vector2 b) { return a.Dot(b) > 0; }
    /// <summary>
    /// Determines if two vectors are facing opposite directions (dot product &lt; 0).
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>True if facing opposite directions, otherwise false.</returns>
    public static bool IsFacingTheOppositeDirection(this Vector2 a, Vector2 b) { return a.Dot(b) < 0; }
    /// <summary>
    /// Checks if a normal vector is facing outward relative to a given outward direction.
    /// </summary>
    /// <remarks> Checks if normal is the facing the same direction as the outward direction.</remarks>
    /// <param name="normal">The normal vector.</param>
    /// <param name="outwardDirection">The reference outward direction.</param>
    /// <returns>True if the normal is facing outward, otherwise false.</returns>
    public static bool IsNormalFacingOutward(this Vector2 normal, Vector2 outwardDirection) { return normal.IsFacingTheSameDirection(outwardDirection); }
    /// <summary>
    /// Returns the normal vector, flipped if it is not facing the outward direction.
    /// </summary>
    /// <param name="normal">The normal vector.</param>
    /// <param name="outwardDirection">The reference outward direction.</param>
    /// <returns>The outward-facing normal vector.</returns>
    public static Vector2 GetOutwardFacingNormal(this Vector2 normal, Vector2 outwardDirection)
    {
        if(IsNormalFacingOutward(normal, outwardDirection)) return normal;
        else return -normal;
    }
    /// <summary>
    /// Determines if three points are colinear (lie on a straight line).
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point (vertex).</param>
    /// <param name="c">The third point.</param>
    /// <returns>True if the points are colinear, otherwise false.</returns>
    public static bool IsColinear(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 prevCur = a - b;
        Vector2 nextCur = c - b;

        return prevCur.Cross(nextCur) == 0f;
    }
    /// <summary>
    /// Returns the area represented by the vector (X * Y).
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The area (X * Y).</returns>
    public static float GetArea(this Vector2 v) { return v.X * v.Y; }
    /// <summary>
    /// Safely divides two vectors component-wise, returning 1 for any zero denominator.
    /// </summary>
    /// <param name="a">The numerator vector.</param>
    /// <param name="b">The denominator vector.</param>
    /// <returns>The result of the division.</returns>
    public static Vector2 DivideSafe(this Vector2 a, Vector2 b)
    {
        return new
        (
            b.X == 0f ? 1f : a.X / b.X,
            b.Y == 0f ? 1f : a.Y / b.Y
        );
    }
    /// <summary>
    /// Checks if either component of the vector is NaN. (Not a Number)
    /// </summary>
    /// <param name="v">The vector to check.</param>
    /// <returns>True if X or Y is NaN, otherwise false.</returns>
    public static bool IsNan(this Vector2 v) { return float.IsNaN(v.X) || float.IsNaN(v.Y); }
    /// <summary>
    /// Returns a unit vector pointing right (1, 0).
    /// </summary>
    /// <returns>A rightward unit vector.</returns>
    public static Vector2 Right() { return new(1.0f, 0.0f); }
    /// <summary>
    /// Returns a unit vector pointing left (-1, 0).
    /// </summary>
    /// <returns>A leftward unit vector.</returns>
    public static Vector2 Left() { return new(-1.0f, 0.0f); }
    /// <summary>
    /// Returns a unit vector pointing up (0, -1).
    /// </summary>
    /// <returns>An upward unit vector.</returns>
    public static Vector2 Up() { return new(0.0f, -1.0f); }
    /// <summary>
    /// Returns a unit vector pointing down (0, 1).
    /// </summary>
    /// <returns>A downward unit vector.</returns>
    public static Vector2 Down() { return new(0.0f, 1.0f); }
    /// <summary>
    /// Returns a vector with both components set to 1.
    /// </summary>
    /// <returns>A vector (1, 1).</returns>
    public static Vector2 One() { return new(1.0f, 1.0f); }
    /// <summary>
    /// Returns a vector with both components set to 0.
    /// </summary>
    /// <returns>A zero vector (0, 0).</returns>
    public static Vector2 Zero() { return new(0.0f, 0.0f); }
    /// <summary>
    /// Returns the right-hand perpendicular vector (-Y, X).
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The right-hand perpendicular vector.</returns>
    public static Vector2 GetPerpendicularRight(this Vector2 v) { return new(-v.Y, v.X); }
    /// <summary>
    /// Returns the left-hand perpendicular vector (Y, -X).
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The left-hand perpendicular vector.</returns>
    public static Vector2 GetPerpendicularLeft(this Vector2 v) { return new(v.Y, -v.X); }
    /// <summary>
    /// Rotates the vector 90 degrees counter-clockwise.
    /// </summary>
    /// <param name="v">The vector to rotate.</param>
    /// <returns>The rotated vector.</returns>
    /// <remarks>Same as <see cref="GetPerpendicularLeft"/>.</remarks>
    public static Vector2 Rotate90CCW(this Vector2 v) { return GetPerpendicularLeft(v); }
    /// <summary>
    /// Rotates the vector 90 degrees clockwise.
    /// </summary>
    /// <param name="v">The vector to rotate.</param>
    /// <returns>The rotated vector.</returns>
    /// <remarks>Same as <see cref="GetPerpendicularRight"/>.</remarks>
    public static Vector2 Rotate90CW(this Vector2 v) { return GetPerpendicularRight(v); }
    /// <summary>
    /// Creates a vector from an angle in radians.
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    /// <returns>The resulting unit vector.</returns>
    public static Vector2 VecFromAngleRad(float angleRad)
    {
        return new(MathF.Cos(angleRad), MathF.Sin(angleRad));
    }
    /// <summary>
    /// Creates a vector from an angle in degrees.
    /// </summary>
    /// <param name="angleDeg">The angle in degrees.</param>
    /// <returns>The resulting unit vector.</returns>
    public static Vector2 VecFromAngleDeg(float angleDeg)
    {
        return VecFromAngleRad(angleDeg * ShapeMath.DEGTORAD);
    }
    /// <summary>
    /// Finds the arithmetic mean (centroid) of a collection of vertices.
    /// </summary>
    /// <param name="vertices">The collection of vertices.</param>
    /// <returns>The arithmetic mean as a <see cref="Vector2"/>.</returns>
    /// <remarks>All vertices are added together component wise. The result is divided by the vertex count.</remarks>
    public static Vector2 FindArithmeticMean(IEnumerable<Vector2> vertices)
    {
        float sx = 0f;
        float sy = 0f;
        int count = 0;
        foreach (var v in vertices)
        {
            sx += v.X;
            sy += v.Y;
            count ++;
        }

        float invArrayLen = 1f / count;
        return new Vector2(sx * invArrayLen, sy * invArrayLen);
    }
    

    //Projection
    /// <summary>
    /// Calculates the projection time of the vector onto another vector.
    /// </summary>
    /// <param name="v">The vector to project.</param>
    /// <param name="onto">The vector to project onto.</param>
    /// <returns>The projection time.</returns>
    public static float ProjectionTime(this Vector2 v, Vector2 onto) { return (v.X * onto.X + v.Y * onto.Y) / onto.LengthSquared(); }
    /// <summary>
    /// Calculates the projection point of the vector onto another vector at a given time.
    /// </summary>
    /// <param name="point">The starting point of the projection.</param>
    /// <param name="v">The vector to project.</param>
    /// <param name="t">The time factor for the projection.</param>
    /// <returns>The projected point.</returns>
    public static Vector2 ProjectionPoint(this Vector2 point, Vector2 v, float t) { return point + v * t; }
    /// <summary>
    /// Projects the vector onto another vector, returning the projection.
    /// </summary>
    /// <param name="project">The vector to project.</param>
    /// <param name="onto">The vector to project onto.</param>
    /// <returns>The projection of the vector.</returns>
    public static Vector2 Project(this Vector2 project, Vector2 onto)
    {
        float d = Vector2.Dot(onto, onto);
        if (d > 0.0f)
        {
            float dp = Vector2.Dot(project, onto);
            return onto * (dp / d);
        }
        return onto;
    }
    /// <summary>
    /// Checks if two vectors are parallel (cross product is zero).
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>True if parallel, otherwise false.</returns>
    public static bool Parallel(this Vector2 a, Vector2 b)
    {
        var rotated = Rotate90CCW(a);
        return Vector2.Dot(rotated, b) == 0.0f;
    }
    /// <summary>
    /// Aligns a position by subtracting the size adjusted by the given anchor point.
    /// </summary>
    /// <param name="pos">The original position.</param>
    /// <param name="size">The size to align.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>The aligned position.</returns>
    public static Vector2 Align(this Vector2 pos, Size size, AnchorPoint alignment)
    {
        return pos - (size * alignment).ToVector2();
    }
    /// <summary>
    /// Wraps the vector components to be within the specified min and max bounds.
    /// </summary>
    /// <param name="v">The vector to wrap.</param>
    /// <param name="min">The minimum bounds.</param>
    /// <param name="max">The maximum bounds.</param>
    /// <returns>The wrapped vector.</returns>
    public static Vector2 Wrap(this Vector2 v, Vector2 min, Vector2 max)
    {
        return new
        (
            ShapeMath.WrapF(v.X, min.X, max.X),
            ShapeMath.WrapF(v.Y, min.Y, max.Y)
        );
    }
    /// <summary>
    /// Wraps the vector components to be within the specified min and max bounds.
    /// </summary>
    /// <param name="v">The vector to wrap.</param>
    /// <param name="min">The minimum bounds.</param>
    /// <param name="max">The maximum bounds.</param>
    /// <returns>The wrapped vector.</returns>
    public static Vector2 Wrap(this Vector2 v, float min, float max)
    {
        return new
        (
            ShapeMath.WrapF(v.X, min, max),
            ShapeMath.WrapF(v.Y, min, max)
        );
    }
    /// <summary>
    /// Returns the maximum component value of the vector.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The maximum component value.</returns>
    public static float Max(this Vector2 v) { return MathF.Max(v.X, v.Y); }
    /// <summary>
    /// Returns the minimum component value of the vector.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The minimum component value.</returns>
    public static float Min(this Vector2 v) { return MathF.Min(v.X, v.Y); }
    /// <summary>
    /// Linearly interpolates between two vectors based on a parameter t.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2 LerpDirection(this Vector2 from, Vector2 to, float t)
    {
        float angleA = ShapeVec.AngleRad(from);
        float angle = ShapeMath.GetShortestAngleRad(angleA, ShapeVec.AngleRad(to));
        return ShapeVec.Rotate(from, ShapeMath.LerpFloat(0, angle, t));
    }
    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2 Lerp(this Vector2 from, Vector2 to, float t) { return Vector2.Lerp(from, to, t); }

    /// <summary>
    /// Exponentially decays and linearly interpolates between two vectors.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="f">The decay factor.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The decayed and interpolated vector.</returns>
    public static Vector2 ExpDecayLerp(this Vector2 from, Vector2 to, float f, float dt)
    {
        var decay = ShapeMath.LerpFloat(1, 25, f);
        var scalar = MathF.Exp(-decay * dt);
        return from + (to - from) * scalar;
    }
    /// <summary>
    /// Exponentially decays and linearly interpolates between two vectors with a complex decay.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="decay">The decay rate.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The decayed and interpolated vector.</returns>
    public static Vector2 ExpDecayLerpComplex(this Vector2 from, Vector2 to, float decay, float dt)
    {
        var scalar = MathF.Exp(-decay * dt);
        return from + (to - from) * scalar;
    }
    /// <summary>
    /// Raises the remainder to the power of dt and lerps between two vectors.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="remainder">The remainder value.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2 PowLerp(this Vector2 from, Vector2 to, float remainder, float dt)
    {
        var scalar = MathF.Pow(remainder, dt);
        return from + (to - from) * scalar;
        
    }
    /// <summary>
    /// Moves the vector towards the target vector over a given time in seconds.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="seconds">The time in seconds to reach the target.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The moved vector.</returns>
    public static Vector2 LerpTowards(this Vector2 from, Vector2 to, float seconds, float dt)
    {
        var dir = to - from;
        var lsq = dir.LengthSquared();
        if (lsq <= 0f || seconds <= 0f) return to;

        var l = MathF.Sqrt(lsq);
        var step = (l / seconds) * dt;
        if (step > l) return to;
        return from + (dir / l) * step;
    }
    /// <summary>
    /// Moves the vector towards the target vector by a given speed.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The target vector.</param>
    /// <param name="speed">The speed of movement.</param>
    /// <returns>The moved vector.</returns>
    public static Vector2 MoveTowards(this Vector2 from, Vector2 to, float speed)
    {
        var dir = to - from;
        var lsq = dir.LengthSquared();
        if (lsq <= 0f || speed <= 0f) return from;
        if (speed * speed > lsq) return to;
        
        var l = MathF.Sqrt(lsq);
        return from + (dir / l) * speed;
    }
    /// <summary>
    /// Floors the components of the vector to the nearest lower integer.
    /// </summary>
    /// <param name="v">The vector to floor.</param>
    /// <returns>A new vector with floored components.</returns>
    public static Vector2 Floor(this Vector2 v) { return new(MathF.Floor(v.X), MathF.Floor(v.Y)); }
    /// <summary>
    /// Ceilings the components of the vector to the nearest higher integer.
    /// </summary>
    /// <param name="v">The vector to ceiling.</param>
    /// <returns>A new vector with ceilinged components.</returns>
    public static Vector2 Ceiling(this Vector2 v) { return new(MathF.Ceiling(v.X), MathF.Ceiling(v.Y)); }
    /// <summary>
    /// Rounds the components of the vector to the nearest integer.
    /// </summary>
    /// <param name="v">The vector to round.</param>
    /// <returns>A new vector with rounded components.</returns>
    public static Vector2 Round(this Vector2 v) { return new(MathF.Round(v.X), MathF.Round(v.Y)); }
    /// <summary>
    /// Truncates the components of the vector to remove any fractional part.
    /// </summary>
    /// <param name="v">The vector to truncate.</param>
    /// <returns>A new vector with truncated components.</returns>
    public static Vector2 Truncate(this Vector2 v) { return new(MathF.Truncate(v.X), MathF.Truncate(v.Y)); }
    /// <summary>
    /// Returns the absolute value of each component of the vector.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>A new vector with absolute component values.</returns>
    public static Vector2 Abs(this Vector2 v) { return Vector2.Abs(v); }
    /// <summary>
    /// Negates the components of the vector.
    /// </summary>
    /// <param name="v">The vector to negate.</param>
    /// <returns>A new vector with negated components.</returns>
    public static Vector2 Negate(this Vector2 v) { return Vector2.Negate(v); }
    /// <summary>
    /// Returns the component-wise minimum of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>A new vector with the minimum values.</returns>
    public static Vector2 Min(this Vector2 v1, Vector2 v2) { return Vector2.Min(v1, v2); }
    /// <summary>
    /// Returns the component-wise maximum of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>A new vector with the maximum values.</returns>
    public static Vector2 Max(this Vector2 v1, Vector2 v2) { return Vector2.Max(v1, v2); }
    /// <summary>
    /// Clamps the vector components to be within the specified min and max bounds.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="min">The minimum bounds.</param>
    /// <param name="max">The maximum bounds.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max) { return Vector2.Clamp(v, min, max); }
    /// <summary>
    /// Clamps the vector components to be within the specified min and max bounds.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="min">The minimum bounds.</param>
    /// <param name="max">The maximum bounds.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2 Clamp(this Vector2 v, float min, float max) { return Vector2.Clamp(v, new(min), new(max)); }
    /// <summary>
    /// Clamps the length of the vector to a maximum value.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2 ClampLength(this Vector2 v, float maxLength)
    {
        var lSq = v.LengthSquared();
        var maxLengthSq = maxLength * maxLength;
        if (lSq > maxLengthSq)
        {
            return v.Normalize() * maxLength;
        }
        return v;
    }
    /// <summary>
    /// Clamps the length of the vector to a specified range.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="minLength">The minimum length.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The clamped vector.</returns>
    /// <remarks>If v is longer than <c>maxLength</c>, v is clamped to maxLength.
    /// If v is shorter than <c>minLenght</c>, v is clamped to minLength.</remarks>
    public static Vector2 ClampLength(this Vector2 v, float minLength, float maxLength)
    {
        var lSq = v.LengthSquared();
        var maxLengthSq = maxLength * maxLength;
        var minLengthSq = minLength * minLength;
        if (lSq > maxLengthSq)
        {
            return v.Normalize() * maxLength;
        }
        
        if (lSq < minLengthSq)
        {
            return v.Normalize() * minLength;
        }
        return v;
    }
    /// <summary>
    /// Normalizes the given vector.
    /// </summary>
    /// <param name="v">The vector to normalize.</param>
    /// <returns>Returns the normalized vector.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Returns <paramref name="v"/> if the squared length of <paramref name="v"/> is one (normalized already).</item>
    /// <item>Returns a zero vector if the squared length of <paramref name="v"/> is zero.</item>
    /// </list>
    /// </remarks>
    public static Vector2 Normalize(this Vector2 v) 
    {
        float ls = v.LengthSquared();
        if (Math.Abs(ls - 1f) < 0.00001f) return v;
        return ls <= 0f ? new() : v / MathF.Sqrt(ls);
    } 
    /// <summary>
    /// Reflects the vector off a surface with the given normal.
    /// </summary>
    /// <param name="v">The vector to reflect.</param>
    /// <param name="n">The normal of the surface.</param>
    /// <returns>The reflected vector.</returns>
    public static Vector2 Reflect(this Vector2 v, Vector2 n) { return Vector2.Reflect(v, n); }
    
    /// <summary>
    /// Changes the length of the vector by the given amount.
    /// </summary>
    /// <param name="v">The vector to change.</param>
    /// <param name="amount">The amount to change the length by.</param>
    /// <returns>The vector with the changed length.</returns>
    public static Vector2 ChangeLength(this Vector2 v, float amount)
    {
        var lSq = v.LengthSquared();
        if (lSq <= 0f) return v;
        var l = MathF.Sqrt(lSq);
        var dir = v / l;
        return dir * (l + amount);
    }
    /// <summary>
    /// Sets the length of the vector to the specified value.
    /// </summary>
    /// <param name="v">The vector to modify.</param>
    /// <param name="length">The desired length.</param>
    /// <returns>The vector with the set length.</returns>
    public static Vector2 SetLength(this Vector2 v, float length)
    {
        var lSq = v.LengthSquared();
        if (lSq <= 0f) return v;
        var l = MathF.Sqrt(lSq);
        var dir = v / l;
        return dir * length;
    }
    /// <summary>
    /// Returns the product of v.Normalized() * v
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2 NormalizeScaled(this Vector2 v)
    {
        float l = v.Length();
        if (l <= 0f) return v;
        return (v / l) * v;
    }
    /// <summary>
    /// Returns the square root of each component of the vector.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>A new vector with square-rooted components.</returns>
    public static Vector2 SquareRoot(this Vector2 v) { return Vector2.SquareRoot(v); }
    /// <summary>
    /// Rotates the vector by the specified angle in radians.
    /// </summary>
    /// <param name="v">The vector to rotate.</param>
    /// <param name="angleRad">The angle in radians.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector2 Rotate(this Vector2 v, float angleRad) 
    {
        Vector2 result = new();
        float num = MathF.Cos(angleRad);
        float num2 = MathF.Sin(angleRad);
        result.X = v.X * num - v.Y * num2;
        result.Y = v.X * num2 + v.Y * num;
        return result;

        
    } //radians
    /// <summary>
    /// Rotates the vector by the specified angle in degrees.
    /// </summary>
    /// <param name="v">The vector to rotate.</param>
    /// <param name="angleDeg">The angle in degrees.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector2 RotateDeg(this Vector2 v, float angleDeg) { return Rotate(v, angleDeg * ShapeMath.DEGTORAD); }
    /// <summary>
    /// Calculates the angle in degrees between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The angle in degrees between the two vectors.</returns>
    public static float AngleDeg(this Vector2 v1, Vector2 v2) { return AngleRad(v1, v2) * ShapeMath.RADTODEG; }
    /// <summary>
    /// Calculates the angle in degrees between the vector and the positive X-axis.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The angle in degrees.</returns>
    public static float AngleDeg(this Vector2 v) { return AngleRad(v) * ShapeMath.RADTODEG; }
    /// <summary>
    /// Calculates the angle in radians between the vector and the positive X-axis.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The angle in radians.</returns>
    public static float AngleRad(this Vector2 v) { return AngleRad(Zero(), v); }
    /// <summary>
    /// Calculates the angle in radians between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The angle in radians between the two vectors.</returns>
    public static float AngleRad(this Vector2 v1, Vector2 v2) { return MathF.Atan2(v2.Y, v2.X) - MathF.Atan2(v1.Y, v1.X); }
    /// <summary>
    /// Calculates the distance between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The distance between the two vectors.</returns>
    public static float Distance(this Vector2 v1, Vector2 v2) { return Vector2.Distance(v1, v2); }
    /// <summary>
    /// Calculates the squared distance between two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The squared distance between the two vectors.</returns>
    public static float DistanceSquared(this Vector2 v1, Vector2 v2)
    {
        return (v1 - v2).LengthSquared();
        
    }
    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    public static float Dot(this Vector2 v1, Vector2 v2) { return v1.X * v2.X + v1.Y * v2.Y; }
    /// <summary>
    /// Calculates the cross product of two vectors (Z-component only).
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The cross product (Z-component).</returns>
    public static float Cross(this Vector2 value1, Vector2 value2) { return value1.X * value2.Y - value1.Y * value2.X; }


}

