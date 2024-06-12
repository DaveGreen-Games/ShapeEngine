
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;


//Polygon can use Transform2D with scale and just not use size

//Other things can use Transform2D with the size and use scale or not

//Transform2D can easily used for offset:
// -> Position + Offset.Position
// -> RotRad + Offset.RotRad
// -> Size + Offset.Size
// -> Scale * Offset.Scale!!!


public readonly struct Transform2D : IEquatable<Transform2D>
{
    #region Members
    public readonly Vector2 Position;
    public readonly float RotationRad;
    public readonly Size BaseSize;
    public readonly Size ScaledSize;
    public readonly float Scale;
    // public readonly Size ScaledSize => BaseSize * Scale;
    #endregion
    
    #region Constructors
    public Transform2D()
    {
        this.Position = new(0f);
        this.RotationRad = 0f;
        this.BaseSize = new(0f);
        this.Scale = 1f;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(Vector2 pos)
    {
        this.Position = pos;
        this.RotationRad = 0f;
        this.BaseSize = new(0f);
        this.Scale = 1f;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(float rotRad)
    {
        this.Position = new(0f);
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale = 1f;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(Vector2 pos, float rotRad)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale = 1f;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(Vector2 pos, float rotRad, float scale)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale = scale;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(Vector2 pos, float rotRad, Size baseSize)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    public Transform2D(Vector2 pos, float rotRad, Size baseSize, float scale)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.Scale = scale;
        this.ScaledSize = this.BaseSize * this.Scale;
    }
    #endregion

    #region Math
    public Vector2 RevertPosition(Vector2 position)
    {
        var w = (position - Position).Rotate(-RotationRad) / ScaledSize.Length;
        return Position + w;


        // var w = (position - Position).Rotate(-RotationRad);
        // return new
        // (
        //     BaseSize.Width == 0f ? Position.X : Position.X + w.X,
        //     BaseSize.Height == 0f ? Position.Y : Position.Y + w.Y
        // );
    }

    public Vector2 ApplyTransformTo(Vector2 relative) => Position + (relative * ScaledSize.Length).Rotate(RotationRad);

    public Vector2 ApplyTranslation(Vector2 translation)
    {
        if (translation.LengthSquared() == 0f) return Position;
        return Position + translation.Rotate(RotationRad) * ScaledSize;
    }
    // public Transform2D Difference(Transform2D other)
    // {
    //     var scaleDif = other.Scale <= 0 ? 1f : Scale / other.Scale;
    //     return new
    //     (
    //         Position - other.Position,
    //         RotationRad - other.RotationRad,
    //         BaseSize - other.BaseSize,
    //         scaleDif
    //     );
    // }

    public readonly Transform2D ChangePosition(Vector2 amount) => new(Position + amount, RotationRad, BaseSize, Scale);
    public readonly Transform2D ChangePositionX(float amount) => new(Position with { X = Position.X + amount }, RotationRad, BaseSize, Scale);
    public readonly Transform2D ChangePositionY(float amount) => new(Position with { Y = Position.Y + amount }, RotationRad, BaseSize, Scale);
    
    public readonly Transform2D ChangeSize(Size amount) => new(Position, RotationRad, BaseSize + amount, Scale);
    public readonly Transform2D ChangeSize(float amount) => new(Position, RotationRad, BaseSize + new Vector2(amount), Scale);
    public readonly Transform2D ChangeSizeX(float amount) => new(Position, RotationRad, new Size(Position.X + amount, BaseSize.Height), Scale);
    public readonly Transform2D ChangeSizeY(float amount) => new(Position, RotationRad, new Size(BaseSize.Width, Position.Y + amount), Scale);
    
    public readonly Transform2D MultiplyScale(float factor) => new(Position, RotationRad, BaseSize, Scale * factor);
    
    public readonly Transform2D ChangeScale(float amount) => new(Position, RotationRad, BaseSize, Scale + amount);
    
    public readonly Transform2D ChangeRotationRad(float amount) => new(Position, RotationRad + amount, BaseSize, Scale);
    public readonly Transform2D ChangeRotationDeg(float amount) => new(Position, RotationRad + (amount * ShapeMath.DEGTORAD), BaseSize, Scale);
    
    public readonly Transform2D SetPosition(Vector2 newPosition) => new(newPosition, RotationRad, BaseSize, Scale);
    public readonly Transform2D SetRotationRad(float newRotationRad) => new(Position, newRotationRad, BaseSize, Scale);
    public Transform2D WrapRotationRad() => new(Position, ShapeMath.WrapAngleRad(RotationRad), BaseSize, Scale);
    public readonly Transform2D SetSize(Size newSize) => new(Position, RotationRad, newSize, Scale);
    public readonly Transform2D SetSize(float newSize) => new(Position, RotationRad, new(newSize), Scale);
    
    public readonly Transform2D SetScale(float newScale) => new(Position, RotationRad, BaseSize, newScale);
    
    public Transform2D AddOffset(Transform2D offset)
    {
        return new
        (
            Position + offset.Position,
            RotationRad + offset.RotationRad,
            BaseSize + offset.BaseSize,
            Scale * offset.Scale
        );
    }
    public Transform2D RemoveOffset(Transform2D offset)
    {
        return new
        (
            Position - offset.Position,
            RotationRad - offset.RotationRad,
            BaseSize - offset.BaseSize,
            Scale / offset.Scale
        );
    }

    #endregion

    #region Operators

    public readonly Transform2D Multiply(float factor)
    {
        return new
        (
            Position * factor,
            RotationRad * factor,
            BaseSize * factor,
            Scale * factor
        );
    }
    public readonly Transform2D Divide(float divisor)
    {
        return new
        (
            Position / divisor,
            RotationRad / divisor,
            BaseSize / divisor,
            Scale / divisor
        );
    }

   
    public static Transform2D operator +(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position + right.Position,
            left.RotationRad + right.RotationRad,
            left.BaseSize + right.BaseSize,
            left.Scale + right.Scale
        );
    }
    public static Transform2D operator -(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position - right.Position,
            left.RotationRad - right.RotationRad,
            left.BaseSize - right.BaseSize,
            left.Scale - right.Scale
        );
    }
    public static Transform2D operator /(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position / right.Position,
            left.RotationRad / right.RotationRad,
            left.BaseSize / right.BaseSize,
            left.Scale / right.Scale
        );
    }
    public static Transform2D operator *(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position * right.Position,
            left.RotationRad * right.RotationRad,
            left.BaseSize * right.BaseSize,
            left.Scale * right.Scale
        );
    }
    
    /// <summary>
    /// Applies to Position only!
    /// </summary>
    public static Transform2D operator +(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position + right,
            left.RotationRad,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to Position only!
    /// </summary>
    public static Transform2D operator -(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position - right,
            left.RotationRad,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to Position only!
    /// </summary>
    public static Transform2D operator *(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position * right,
            left.RotationRad,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to Position only!
    /// </summary>
    public static Transform2D operator /(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position / right,
            left.RotationRad,
            left.BaseSize,
            left.Scale
        );
    }
    
    /// <summary>
    /// Applies to rotation only!
    /// </summary>
    public static Transform2D operator +(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad + right,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to rotation only!
    /// </summary>
    public static Transform2D operator -(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad - right,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to rotation only!
    /// </summary>
    public static Transform2D operator *(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad * right,
            left.BaseSize,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to rotation only!
    /// </summary>
    public static Transform2D operator /(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad /right,
            left.BaseSize ,
            left.Scale
        );
    }
    
    /// <summary>
    /// Applies to BaseSize only!
    /// </summary>
    public static Transform2D operator +(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize + right,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to BaseSize only!
    /// </summary>
    public static Transform2D operator -(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize - right,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to BaseSize only!
    /// </summary>
    public static Transform2D operator *(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize * right,
            left.Scale
        );
    }
    /// <summary>
    /// Applies to BaseSize only!
    /// </summary>
    public static Transform2D operator /(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize / right,
            left.Scale
        );
    }
    
    
    public static bool operator ==(Transform2D left, Transform2D right) => right.Equals(left);
    public static bool operator !=(Transform2D left, Transform2D right) => !(left == right);
    
    #endregion

    #region Equals & Hash Code
    public bool Equals(Transform2D other) => Position.Equals(other.Position) && RotationRad.Equals(other.RotationRad) && BaseSize.Equals(other.BaseSize) && Scale.Equals(other.Scale);
    public override bool Equals(object? obj) => obj is Transform2D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Position, RotationRad, BaseSize, Scale);
    #endregion
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
