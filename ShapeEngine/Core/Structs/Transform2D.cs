using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Transform2D : IEquatable<Transform2D>
{
    public readonly Vector2 Position;
    public readonly float RotationRad;
    public readonly Vector2 Scale;

    public Transform2D()
    {
        this.Position = new(0f);
        this.RotationRad = 0f;
        this.Scale = new(1f);
    }
    public Transform2D(Vector2 pos)
    {
        this.Position = pos;
        this.RotationRad = 0f;
        this.Scale = new(1f);
    }
    public Transform2D(float rotRad)
    {
        this.Position = new(0f);
        this.RotationRad = rotRad;
        this.Scale = new(1f);
    }
    public Transform2D(Vector2 pos, float rotRad)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.Scale = new(1f);
    }
    public Transform2D(Vector2 pos, float rotRad, Vector2 scale)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.Scale = scale;
    }

    
    
    public readonly Transform2D MoveBy(Vector2 amount) => new(Position + amount, RotationRad, Scale);
    public readonly Transform2D MoveByX(float amount) => new(Position with { X = Position.X + amount }, RotationRad, Scale);
    public readonly Transform2D MoveByY(float amount) => new(Position with { Y = Position.Y + amount }, RotationRad, Scale);
    
    public readonly Transform2D ScaleBy(Vector2 amount) => new(Position, RotationRad, Scale + amount);
    public readonly Transform2D ScaleBy(float amount) => new(Position, RotationRad, Scale + new Vector2(amount));
    public readonly Transform2D ScaleByX(float amount) => new(Position, RotationRad, Scale with { X = Position.X + amount });
    public readonly Transform2D ScaleByY(float amount) => new(Position, RotationRad, Scale with { Y = Position.Y + amount });

    public readonly Transform2D RotateByRad(float amount) => new(Position, RotationRad + amount, Scale);
    public readonly Transform2D RotateByDeg(float amount) => new(Position, RotationRad + (amount * ShapeMath.DEGTORAD), Scale);
    
    public readonly Transform2D SetPosition(Vector2 newPosition) => new(newPosition, RotationRad, Scale);
    public readonly Transform2D SetRotationRad(float newRotationRad) => new(Position, newRotationRad, Scale);
    public readonly Transform2D SetScale(Vector2 newScale) => new(Position, RotationRad, newScale);
    public readonly Transform2D SetScale(float newScale) => new(Position, RotationRad, new(newScale));

    public readonly Transform2D Difference(Transform2D other)
    {
        var scaleDif = new Vector2
        (
            other.Scale.X <= 0 ? 1f : Scale.X / other.Scale.X,
            other.Scale.Y <= 0 ? 1f : Scale.Y / other.Scale.Y
        );
        // var scaleChange = Scale - other.Scale;
        // var scaleDif = new Vector2
        // (
            // scaleChange.X >= 0 ? scaleChange.X : 1f / scaleChange.X,
            // scaleChange.Y >= 0 ? scaleChange.Y : 1f / scaleChange.Y
        // );
        return new
        (
            Position - other.Position,
            RotationRad - other.RotationRad,
            scaleDif
        );
    }
    public readonly Transform2D Offset(Transform2D offset)
    {
        return new
        (
            Position + offset.Position,
            RotationRad + offset.RotationRad,
            Scale * offset.Scale
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

    
    // public readonly Transform2D Subtract(Transform2D other)
    // {
    //     return new
    //     (
    //         Position - other.Position,
    //         RotationRad - other.RotationRad,
    //         Scale - other.Scale
    //     );
    // }
    // public readonly Transform2D Add(Transform2D other)
    // {
    //     return new
    //     (
    //         Position + other.Position,
    //         RotationRad + other.RotationRad,
    //         Scale + other.Scale
    //     );
    // }
    // public readonly Transform2D Multiply(Transform2D other)
    // {
    //     return new
    //     (
    //         Position * other.Position,
    //         RotationRad * other.RotationRad,
    //         Scale * other.Scale
    //     );
    // }
    //
    // public readonly Transform2D Multiply(float factor)
    // {
    //     return new
    //     (
    //         Position * factor,
    //         RotationRad * factor,
    //         Scale * factor
    //     );
    // }
    // public readonly Transform2D Divide(float divisor)
    // {
    //     return new
    //     (
    //         Position / divisor,
    //         RotationRad / divisor,
    //         Scale / divisor
    //     );
    // }

   
    public static Transform2D operator +(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position + right.Position,
            left.RotationRad + right.RotationRad,
            left.Scale + right.Scale
        );
    }
    public static Transform2D operator -(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position - right.Position,
            left.RotationRad - right.RotationRad,
            left.Scale - right.Scale
        );
    }
    public static Transform2D operator /(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position / right.Position,
            left.RotationRad / right.RotationRad,
            left.Scale / right.Scale
        );
    }
    public static Transform2D operator *(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position * right.Position,
            left.RotationRad * right.RotationRad,
            left.Scale * right.Scale
        );
    }
    
    public static Transform2D operator +(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position + right,
            left.RotationRad,
            left.Scale
        );
    }
    public static Transform2D operator -(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position - right,
            left.RotationRad,
            left.Scale
        );
    }
    public static Transform2D operator *(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position * right,
            left.RotationRad,
            left.Scale
        );
    }
    public static Transform2D operator /(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position / right,
            left.RotationRad,
            left.Scale
        );
    }
    
    public static Transform2D operator +(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad + right,
            left.Scale
        );
    }
    public static Transform2D operator -(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad - right,
            left.Scale
        );
    }
    public static Transform2D operator *(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Scale * right
        );
    }
    public static Transform2D operator /(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Scale * right
        );
    }

    public static bool operator ==(Transform2D left, Transform2D right) => right.Equals(left);

    public static bool operator !=(Transform2D left, Transform2D right) => !(left == right);
    public bool Equals(Transform2D other) => Position.Equals(other.Position) && RotationRad.Equals(other.RotationRad) && Scale.Equals(other.Scale);

    public override bool Equals(object? obj) => obj is Transform2D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Position, RotationRad, Scale);
}