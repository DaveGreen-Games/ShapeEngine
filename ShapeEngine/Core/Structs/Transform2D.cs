using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Transform2D : IEquatable<Transform2D>
{
    public readonly Vector2 Position;
    public readonly float RotationRad;
    public readonly Size Size;

    public Transform2D()
    {
        this.Position = new(0f);
        this.RotationRad = 0f;
        this.Size = new(1f);
    }
    public Transform2D(Vector2 pos)
    {
        this.Position = pos;
        this.RotationRad = 0f;
        this.Size = new(1f);
    }
    public Transform2D(float rotRad)
    {
        this.Position = new(0f);
        this.RotationRad = rotRad;
        this.Size = new(1f);
    }
    public Transform2D(Vector2 pos, float rotRad)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.Size = new(1f);
    }
    public Transform2D(Vector2 pos, float rotRad, Size size)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.Size = size;
    }

    
    
    public readonly Transform2D MoveBy(Vector2 amount) => new(Position + amount, RotationRad, Size);
    public readonly Transform2D MoveByX(float amount) => new(Position with { X = Position.X + amount }, RotationRad, Size);
    public readonly Transform2D MoveByY(float amount) => new(Position with { Y = Position.Y + amount }, RotationRad, Size);
    
    public readonly Transform2D ScaleBy(Size amount) => new(Position, RotationRad, Size + amount);
    public readonly Transform2D ScaleBy(float amount) => new(Position, RotationRad, Size + new Vector2(amount));
    public readonly Transform2D ScaleByX(float amount) => new(Position, RotationRad, new Size(Position.X + amount, Size.Height));
    public readonly Transform2D ScaleByY(float amount) => new(Position, RotationRad, new Size(Size.Width, Position.Y + amount));

    public readonly Transform2D RotateByRad(float amount) => new(Position, RotationRad + amount, Size);
    public readonly Transform2D RotateByDeg(float amount) => new(Position, RotationRad + (amount * ShapeMath.DEGTORAD), Size);
    
    public readonly Transform2D SetPosition(Vector2 newPosition) => new(newPosition, RotationRad, Size);
    public readonly Transform2D SetRotationRad(float newRotationRad) => new(Position, newRotationRad, Size);
    public Transform2D WrapRotationRad() => new(Position, ShapeMath.WrapAngleRad(RotationRad), Size);
    public readonly Transform2D SetSize(Size newSize) => new(Position, RotationRad, newSize);
    public readonly Transform2D SetSize(float newSize) => new(Position, RotationRad, new(newSize));

    public readonly Transform2D Difference(Transform2D other)
    {
        var scaleDif = new Size
        (
            other.Size.Width <= 0 ? 1f : Size.Width / other.Size.Width,
            other.Size.Width <= 0 ? 1f : Size.Height / other.Size.Height
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
            Size * offset.Size
        );
    }
    public readonly Vector2 Revert(Vector2 p)
    {
        
        var w = (p - Position).Rotate(-RotationRad);
        return new
        (
            Size.Width == 0f ? Position.X : Position.X + w.X,
            Size.Height == 0f ? Position.Y : Position.Y + w.Y
        );
        
        
        // return Position + w.Rotate(-RotationRad) / Scale;
    }
    public readonly Vector2 Apply(Vector2 offset)
    {
        if (offset.LengthSquared() == 0f) return Position;
        return Position + offset.Rotate(RotationRad) * Size;
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
            left.Size + right.Size
        );
    }
    public static Transform2D operator -(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position - right.Position,
            left.RotationRad - right.RotationRad,
            left.Size - right.Size
        );
    }
    public static Transform2D operator /(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position / right.Position,
            left.RotationRad / right.RotationRad,
            left.Size / right.Size
        );
    }
    public static Transform2D operator *(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position * right.Position,
            left.RotationRad * right.RotationRad,
            left.Size * right.Size
        );
    }
    
    public static Transform2D operator +(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position + right,
            left.RotationRad,
            left.Size
        );
    }
    public static Transform2D operator -(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position - right,
            left.RotationRad,
            left.Size
        );
    }
    public static Transform2D operator *(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position * right,
            left.RotationRad,
            left.Size
        );
    }
    public static Transform2D operator /(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position / right,
            left.RotationRad,
            left.Size
        );
    }
    
    public static Transform2D operator +(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad + right,
            left.Size
        );
    }
    public static Transform2D operator -(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad - right,
            left.Size
        );
    }
    public static Transform2D operator *(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad * right,
            left.Size
        );
    }
    public static Transform2D operator /(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad /right,
            left.Size 
        );
    }
    
    public static Transform2D operator +(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Size + right
        );
    }
    public static Transform2D operator -(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Size - right
        );
    }
    public static Transform2D operator *(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Size * right
        );
    }
    public static Transform2D operator /(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.Size / right
        );
    }

    

    public static bool operator ==(Transform2D left, Transform2D right) => right.Equals(left);

    public static bool operator !=(Transform2D left, Transform2D right) => !(left == right);
    public bool Equals(Transform2D other) => Position.Equals(other.Position) && RotationRad.Equals(other.RotationRad) && Size.Equals(other.Size);

    public override bool Equals(object? obj) => obj is Transform2D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Position, RotationRad, Size);
}