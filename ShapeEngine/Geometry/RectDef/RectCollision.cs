using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
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

    public BoundsCollisionInfo BoundsCollision(Circle boundingCircle)
    {
        var pos = boundingCircle.Center;
        var radius = boundingCircle.Radius;
        CollisionPoint horizontal;
        CollisionPoint vertical;
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

    public BoundsCollisionInfo BoundsCollision(Rect boundingBox)
    {
        var pos = boundingBox.Center;
        var halfSize = boundingBox.Size * 0.5f;

        var newPos = pos;
        CollisionPoint horizontal;
        CollisionPoint vertical;
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