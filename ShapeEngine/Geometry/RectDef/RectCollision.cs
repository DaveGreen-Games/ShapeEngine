using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Checks if a bounding circle is out of the rectangle bounds and wraps it around if necessary.
    /// </summary>
    /// <param name="boundingCircle">The circle to check and wrap around the rectangle bounds.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the circle was out of bounds and the new position for the circle.
    /// </returns>
    /// <remarks>
    /// If the circle's center plus or minus its radius exceeds the rectangle's bounds, its position is wrapped to the opposite side.
    /// </remarks>
    public (bool outOfBounds, Vector2 newPos) BoundsWrapAround(Circle boundingCircle)
    {
        var pos = boundingCircle.Center;
        var radius = boundingCircle.Radius;
        var outOfBounds = false;
        var newPos = pos;
        if (pos.X + radius > X + Width)
        {
            newPos = new(X, pos.Y);
            outOfBounds = true;
        }
        else if (pos.X - radius < X)
        {
            newPos = new(X + Width, pos.Y);
            outOfBounds = true;
        }

        if (pos.Y + radius > Y + Height)
        {
            newPos = pos with { Y = Y };
            outOfBounds = true;
        }
        else if (pos.Y - radius < Y)
        {
            newPos = pos with { Y = Y + Height };
            outOfBounds = true;
        }

        return (outOfBounds, newPos);
    }

    /// <summary>
    /// Checks if a bounding rectangle is out of the rectangle bounds and wraps it around if necessary.
    /// </summary>
    /// <param name="boundingBox">The rectangle to check and wrap around the rectangle bounds.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the rectangle was out of bounds and the new position for the rectangle.
    /// </returns>
    /// <remarks>
    /// If the rectangle's center plus or minus half its size exceeds the rectangle's bounds, its position is wrapped to the opposite side.
    /// </remarks>
    public (bool outOfBounds, Vector2 newPos) BoundsWrapAround(Rect boundingBox)
    {
        var pos = boundingBox.Center;
        var halfSize = boundingBox.Size * 0.5f;
        var outOfBounds = false;
        var newPos = pos;
        if (pos.X + halfSize.Width > X + Width)
        {
            newPos = new(X, pos.Y);
            outOfBounds = true;
        }
        else if (pos.X - halfSize.Width < X)
        {
            newPos = new(X + Width, pos.Y);
            outOfBounds = true;
        }

        if (pos.Y + halfSize.Height > Y + Height)
        {
            newPos = pos with { Y = Y };
            outOfBounds = true;
        }
        else if (pos.Y - halfSize.Height < Y)
        {
            newPos = pos with { Y = Y + Height };
            outOfBounds = true;
        }

        return (outOfBounds, newPos);
    }

    /// <summary>
    /// Checks for collision between the rectangle and a bounding circle, returning collision information.
    /// </summary>
    /// <param name="boundingCircle">The circle to check for collision with the rectangle.</param>
    /// <returns>
    /// A <see cref="BoundsCollisionInfo"/> object containing details about the collision points and normals.
    /// </returns>
    /// <remarks>
    /// The method checks both horizontal and vertical bounds and provides the closest collision points and normals if a collision occurs.
    /// </remarks>
    public BoundsCollisionInfo BoundsCollision(Circle boundingCircle)
    {
        var pos = boundingCircle.Center;
        var radius = boundingCircle.Radius;
        IntersectionPoint horizontal;
        IntersectionPoint vertical;
        if (pos.X + radius > Right)
        {
            pos.X = Right - radius;
            Vector2 p = new(Right, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(-1, 0);
            horizontal = new(p, n);
        }
        else if (pos.X - radius < Left)
        {
            pos.X = Left + radius;
            Vector2 p = new(Left, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(1, 0);
            horizontal = new(p, n);
        }
        else horizontal = new();

        if (pos.Y + radius > Bottom)
        {
            pos.Y = Bottom - radius;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Bottom);
            Vector2 n = new(0, -1);
            vertical = new(p, n);
        }
        else if (pos.Y - radius < Top)
        {
            pos.Y = Top + radius;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Top);
            Vector2 n = new(0, 1);
            vertical = new(p, n);
        }
        else vertical = new();

        return new(pos, horizontal, vertical);
    }

    /// <summary>
    /// Checks for collision between the rectangle and a bounding rectangle, returning collision information.
    /// </summary>
    /// <param name="boundingBox">The rectangle to check for collision with the rectangle.</param>
    /// <returns>
    /// A <see cref="BoundsCollisionInfo"/> object containing details about the collision points and normals.
    /// </returns>
    /// <remarks>
    /// The method checks both horizontal and vertical bounds and provides the closest collision points and normals if a collision occurs.
    /// </remarks>
    public BoundsCollisionInfo BoundsCollision(Rect boundingBox)
    {
        var pos = boundingBox.Center;
        var halfSize = boundingBox.Size * 0.5f;

        var newPos = pos;
        IntersectionPoint horizontal;
        IntersectionPoint vertical;
        if (pos.X + halfSize.Width > Right)
        {
            newPos.X = Right - halfSize.Width;
            Vector2 p = new(Right, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(-1, 0);
            horizontal = new(p, n);
        }
        else if (pos.X - halfSize.Width < Left)
        {
            newPos.X = Left + halfSize.Width;
            Vector2 p = new(Left, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(1, 0);
            horizontal = new(p, n);
        }
        else horizontal = new();

        if (pos.Y + halfSize.Height > Bottom)
        {
            newPos.Y = Bottom - halfSize.Height;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Bottom);
            Vector2 n = new(0, -1);
            vertical = new(p, n);
        }
        else if (pos.Y - halfSize.Height < Top)
        {
            newPos.Y = Top + halfSize.Height;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Top);
            Vector2 n = new(0, 1);
            vertical = new(p, n);
        }
        else vertical = new();

        return new(newPos, horizontal, vertical);
    }

}