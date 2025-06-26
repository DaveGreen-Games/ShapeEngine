using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    #region Transform

    /// <summary>
    /// Scales the radius of the circle by a given factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to the radius.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public Circle ScaleRadius(float scale) => new(Center, Radius * scale);

    /// <summary>
    /// Changes the radius of the circle by a given amount.
    /// </summary>
    /// <param name="amount">The amount to add to the radius.</param>
    /// <returns>A new <see cref="Circle"/> with the modified radius.</returns>
    public Circle ChangeRadius(float amount) => new(Center, Radius + amount);

    /// <summary>
    /// Sets the radius of the circle to a specific value.
    /// </summary>
    /// <param name="radius">The new radius value.</param>
    /// <returns>A new <see cref="Circle"/> with the updated radius.</returns>
    public Circle SetRadius(float radius) => new(Center, radius);

    /// <summary>
    /// Rotates the circle's center around a given pivot point by a specified angle (in radians).
    /// The radius remains unchanged.
    /// </summary>
    /// <param name="rotationRad">The rotation angle in radians.</param>
    /// <param name="pivot">The pivot point to rotate around.</param>
    /// <returns>A new <see cref="Circle"/> with the rotated center.</returns>
    public Circle ChangeRotation(float rotationRad, Vector2 pivot)
    {
        var w = Center - pivot;
        var rotated = w.Rotate(rotationRad);
        return new(pivot + rotated, Radius);
    }

    /// <summary>
    /// Changes the position of the circle by a given offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the circle's position.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle ChangePosition(Vector2 offset) => this + offset;

    /// <summary>
    /// Changes the position of the circle by specific x and y offsets.
    /// </summary>
    /// <param name="x">The x-coordinate offset.</param>
    /// <param name="y">The y-coordinate offset.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle ChangePosition(float x, float y) => this + new Vector2(x, y);

    /// <summary>
    /// Sets the position of the circle to a specific value.
    /// </summary>
    /// <param name="position">The new position of the circle.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle SetPosition(Vector2 position) => new Circle(position, Radius);

    /// <summary>
    /// Moves the circle by the offset's <c>Position</c>
    /// and changes its radius by the offset's <c>ScaledSize.Radius</c>.
    /// </summary>
    /// <param name="offset">The transform offset to apply.</param>
    /// <returns>A new <see cref="Circle"/> with the applied offset.</returns>
    public Circle ApplyOffset(Transform2D offset)
    {
        var newCircle = ChangePosition(offset.Position);
        return newCircle.ChangeRadius(offset.ScaledSize.Radius);
    }

    /// <summary>
    /// Moves the circle to the transform's <c>Position</c>
    /// and sets its radius to the transform's <c>ScaledSize.Radius</c>.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new <see cref="Circle"/> with the applied transform.</returns>
    public Circle SetTransform(Transform2D transform)
    {
        var newCircle = SetPosition(transform.Position);
        return newCircle.SetRadius(transform.ScaledSize.Radius);
    }

    #endregion

    #region Math

    /// <summary>
    /// Projects the circle's shape points along a given vector.
    /// </summary>
    /// <param name="v">The vector to project the shape points along.</param>
    /// <param name="pointCount">The number of points to generate for the projection. Default is 8.</param>
    /// <returns>A <see cref="Points"/> collection representing the projected shape points,
    /// or null if invalid parameters are provided.</returns>
    public PointsDef.Points? GetProjectedShapePoints(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        PointsDef.Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }

        return points;
    }

    /// <summary>
    /// Projects the circle's shape into a polygon along a given vector.
    /// </summary>
    /// <param name="v">The vector to project the shape along.</param>
    /// <param name="pointCount">The number of points to generate for the polygon. Default is 8.</param>
    /// <returns>A <see cref="Polygon"/> representing the projected shape,
    /// or null if invalid parameters are provided.</returns>
    public Polygon? ProjectShape(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        PointsDef.Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }

        return Polygon.FindConvexHull(points);
    }

    /// <summary>
    /// Floors the circle's center and radius values to the nearest lower integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with floored values.</returns>
    public Circle Floor()
    {
        return new(Center.Floor(), MathF.Floor(Radius));
    }

    /// <summary>
    /// Ceils the circle's center and radius values to the nearest higher integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with ceiled values.</returns>
    public Circle Ceiling()
    {
        return new(Center.Ceiling(), MathF.Ceiling(Radius));
    }

    /// <summary>
    /// Rounds the circle's center and radius values to the nearest integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with rounded values.</returns>
    public Circle Round()
    {
        return new(Center.Round(), MathF.Round(Radius));
    }

    /// <summary>
    /// Truncates the circle's center and radius values to their integer parts.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with truncated values.</returns>
    public Circle Truncate()
    {
        return new(Center.Truncate(), MathF.Truncate(Radius));
    }

    /// <summary>
    /// Calculates the area of the circle.
    /// </summary>
    /// <returns>The area of the circle.</returns>
    public float GetArea()
    {
        return MathF.PI * Radius * Radius;
    }

    /// <summary>
    /// Calculates the circumference of the circle.
    /// </summary>
    /// <returns>The circumference of the circle.</returns>
    public float GetCircumference()
    {
        return MathF.PI * Radius * 2f;
    }

    /// <summary>
    /// Calculates the circumference of a circle given its radius.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>The circumference of the circle.</returns>
    public static float GetCircumference(float radius)
    {
        return MathF.PI * radius * 2f;
    }

    /// <summary>
    /// Calculates the square of the circle's circumference.
    /// </summary>
    /// <returns>The square of the circle's circumference.</returns>
    public float GetCircumferenceSquared()
    {
        return GetCircumference() * GetCircumference();
    }

    #endregion
}