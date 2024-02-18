using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Transform2D
{
    public readonly Vector2 Position = new();
    public readonly float RotationRad = 0f;
    public readonly Vector2 Scale = new(1f);

    public Transform2D() { }
    public Transform2D(Vector2 pos) { this.Position = pos; }
    public Transform2D(Vector2 pos, float rotRad) { this.Position = pos; this.RotationRad = rotRad; }
    public Transform2D(Vector2 pos, float rotRad, Vector2 scale) { this.Position = pos; this.RotationRad = rotRad; this.Scale = scale; }

    public readonly Transform2D SetPosition(Vector2 newPosition) => new(newPosition, RotationRad, Scale);
    public readonly Transform2D SetRotationRad(float newRotationRad) => new(Position, newRotationRad, Scale);
    public readonly Transform2D SetScale(Vector2 newScale) => new(Position, RotationRad, newScale);
    public readonly Transform2D Difference(Transform2D other)
    {
        var scaleChange = Scale - other.Scale;
        var scaleDif = new Vector2
        (
            scaleChange.X >= 0 ? scaleChange.X : 1f / scaleChange.X,
            scaleChange.Y >= 0 ? scaleChange.Y : 1f / scaleChange.Y
        );
        return new
        (
            Position - other.Position,
            RotationRad - other.RotationRad,
            scaleDif
        );
    }
    public readonly Transform2D Subtract(Transform2D other)
    {
        return new
        (
            Position - other.Position,
            RotationRad - other.RotationRad,
            Scale - other.Scale
        );
    }
    public readonly Transform2D Add(Transform2D other)
    {
        return new
        (
            Position + other.Position,
            RotationRad + other.RotationRad,
            Scale + other.Scale
        );
    }
    public readonly Transform2D Multiply(Transform2D other)
    {
        return new
        (
            Position * other.Position,
            RotationRad * other.RotationRad,
            Scale * other.Scale
        );
    }

    public readonly Transform2D Multiply(float factor)
    {
        return new
        (
            Position * factor,
            RotationRad * factor,
            Scale * factor
        );
    }
    public readonly Transform2D Divide(float divisor)
    {
        return new
        (
            Position / divisor,
            RotationRad / divisor,
            Scale / divisor
        );
    }

    public readonly Vector2 Revert(Vector2 p)
    {
        
        var w = (p - Position).Rotate(-RotationRad);
        return new
        (
            Scale.X == 0f ? Position.X : Position.X + w.X,
            Scale.Y == 0f ? Position.Y : Position.Y + w.Y
        );
        
        
        // return Position + w.Rotate(-RotationRad) / Scale;
    }
    public readonly Vector2 Apply(Vector2 offset)
    {
        if (offset.LengthSquared() == 0f) return Position;
        return Position + offset.Rotate(RotationRad) * Scale;
    }
}