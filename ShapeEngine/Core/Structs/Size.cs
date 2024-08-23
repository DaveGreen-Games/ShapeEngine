using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Size : IEquatable<Size>
{
    public readonly float Width;
    public readonly float Height;
    
    /// <summary>
    /// Returns Width!
    /// </summary>
    public float Radius => Width;
    /// <summary>
    /// Returns Width!
    /// </summary>
    public float Length => Width;
    
    public bool Positive => Width >= 0 && Height >= 0;
    public Size()
    {
        Width = 0f;
        Height = 0f;
    }

    public Size(float size)
    {
        Width = size;
        Height = size;
    }
    public Size(float w, float h)
    {
        Width = w;
        Height = h;
    }

    public float Area => Width < 0 || Height < 0 ? 0 : Width * Height;
    public bool IsSquare => Width > 0 && Height > 0 && Math.Abs(Width - Height) < 0.0001f;

    public float Max() => MathF.Max(Width, Height);
    public float Min() => MathF.Min(Width, Height);
    public Size Max(Size other)
    {
        return new
            (
                MathF.Max(Width, other.Width),
                MathF.Max(Height, other.Height)
            );
    }
    public Size Min(Size other)
    {
        return new
        (
            MathF.Min(Width, other.Width),
            MathF.Min(Height, other.Height)
        );
    }
    public Size MaxArea(Size other) => Area >= other.Area ? this : other;
    public Size MinArea(Size other) => Area <= other.Area ? this : other;
    
    public Size Clamp(float min, float max)
    {
        return new
            (
                ShapeMath.Clamp(Width, min, max),
                ShapeMath.Clamp(Height, min,max)
            );
    }
    public Size Clamp(Size min, Size max)
    {
        return new
        (
            ShapeMath.Clamp(Width, min.Width, max.Width),
            ShapeMath.Clamp(Height, min.Height,max.Height)
        );
    }

    public Size Switch() => new(Height, Width);
    
    public Size ExpDecayLerp(Size to, float f, float dt)
    {
        var decay = ShapeMath.LerpFloat(1, 25, f);
        var scalar = MathF.Exp(-decay * dt);
        return this + (to - this) * scalar;
        
    }
    public Size ExpDecayLerpComplex(Size to, float decay, float dt)
    {
        var scalar = MathF.Exp(-decay * dt);
        return this + (to - this) * scalar;
    }
    public Size PowLerp(Size to, float remainder, float dt)
    {
        var scalar = MathF.Pow(remainder, dt);
        return this + (to - this) * scalar;
            
    }
    public Size Lerp(Size to, float f)
    {
        return new
            (
                ShapeMath.LerpFloat(Width, to.Width, f),
                ShapeMath.LerpFloat(Height, to.Height, f)
            );
    }
    public Size Lerp(float to, float f)
    {
        return new
        (
            ShapeMath.LerpFloat(Width, to, f),
            ShapeMath.LerpFloat(Height, to, f)
        );
    }
    
    public Size MoveTowards(Size to, float speed)
    {
        var fromVec = this.ToVector2();
        var toVec = to.ToVector2();
        var result = fromVec.MoveTowards(toVec, speed);
        return result.ToSize();
    }

    public Size Round()
    {
        return new
            (
                MathF.Round(Width),
                MathF.Round(Height)
            );
    }
    public Size Ceil()
    {
        return new
        (
            MathF.Ceiling(Width),
            MathF.Ceiling(Height)
        );
    }
    public Size Floor()
    {
        return new
        (
            MathF.Floor(Width),
            MathF.Floor(Height)
        );
    }
    public Size Truncate()
    {
        return new
        (
            MathF.Truncate(Width),
            MathF.Truncate(Height)
        );
    }
    public Size Round(int decimals)
    {
        return new
        (
            ShapeMath.RoundToDecimals(Width, decimals),
            ShapeMath.RoundToDecimals(Height, decimals)
        );
    }

    public override string ToString()
    {
        return $"(w: {Width}, h: {Height})";
    }

    public Vector2 ToVector2() => new(Width, Height);

    /// <summary>
    /// Changes width only!
    /// </summary>
    public Size SetRadius(float newRadius) => new(newRadius, Height);
    /// <summary>
    /// Changes width only!
    /// </summary>
    public Size ChangeRadius(float amount) => new(Width + amount, Height);
    /// <summary>
    /// Changes width only!
    /// </summary>
    public Size SetLength(float newLength) => new(newLength, Height);
    /// <summary>
    /// Changes width only!
    /// </summary>
    public Size ChangeLength(float amount) => new(Width + amount, Height);
    

    public static bool operator ==(Size left, Size right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Size left, Size right)
    {
        return !left.Equals(right);
    }
    public static bool operator ==(Size left, Vector2 right)
    {
        return Math.Abs(left.Width - right.X) < 0.0001f && Math.Abs(left.Height - right.Y) < 0.0001f;
    }

    public static bool operator !=(Size left, Vector2 right)
    {
        return !(left == right);
    }
    public static bool operator ==(Vector2 left, Size right)
    {
        return Math.Abs(left.X - right.Width) < 0.0001f && Math.Abs(left.Y - right.Height) < 0.0001f;
    }

    public static bool operator !=(Vector2 left, Size right)
    {
        return !(left == right);
    }
    public static Size operator +(Size left, Size right)
    {
        return new
        (
            left.Width + right.Width,
            left.Height + right.Height
        );
    }
    public static Size operator -(Size left, Size right)
    {
        return new
        (
            left.Width - right.Width,
            left.Height - right.Height
        );
    }
    public static Size operator *(Size left, Size right)
    {
        return new
        (
            left.Width * right.Width,
            left.Height * right.Height
        );
    }
    public static Size operator /(Size left, Size right)
    {
        return new
        (
             right.Width == 0 ? left.Width : left.Width / right.Width,
            right.Height == 0 ? left.Height : left.Height / right.Height
        );
    }
    public static Vector2 operator +(Vector2 left, Size right)
    {
        return new
        (
            left.X + right.Width,
            left.Y + right.Height   
        );
    }
    public static Vector2 operator -(Vector2 left, Size right)
    {
        return new
        (
            left.X - right.Width,
            left.Y - right.Height   
        );
    }
    public static Vector2 operator *(Vector2 left, Size right)
    {
        return new
        (
            left.X * right.Width,
            left.Y * right.Height   
        );
    }
    public static Vector2 operator /(Vector2 left, Size right)
    {
        return new
        (
            right.Width == 0 ? left.X : left.X / right.Width,
            right.Height == 0 ? left.Y : left.Y / right.Height   
        );
    }
    public static Size operator +(Size left, Vector2 right)
    {
        return new
        (
            left.Width + right.X,
            left.Height + right.Y   
        );
    }
    public static Size operator -(Size left, Vector2 right)
    {
        return new
        (
            left.Width - right.X,
            left.Height - right.Y   
        );
    }
    public static Size operator *(Size left, Vector2 right)
    {
        return new
        (
            left.Width * right.X,
            left.Height * right.Y   
        );
    }
    public static Size operator /(Size left, Vector2 right)
    {
        return new
        (
            right.X == 0 ? left.Width : left.Width / right.X,
            right.Y == 0 ? left.Height : left.Height / right.Y
        );
    }
    public static Size operator +(Size left, Direction right)
    {
        return new
        (
            left.Width + right.Horizontal,
            left.Height + right.Vertical   
        );
    }
    public static Size operator -(Size left, Direction right)
    {
        return new
        (
            left.Width - right.Horizontal,
            left.Height - right.Vertical   
        );
    }
    public static Size operator *(Size left, Direction right)
    {
        return new
        (
            left.Width * right.Horizontal,
            left.Height * right.Vertical   
        );
    }
    public static Size operator /(Size left, Direction right)
    {
        return new
        (
            right.Horizontal == 0 ? left.Width : left.Width / right.Horizontal,
            right.Vertical == 0 ? left.Height : left.Height / right.Vertical
        );
    }
    public static Size operator +(Size left, float right)
    {
        return new
        (
            left.Width + right,
            left.Height + right   
        );
    }
    public static Size operator -(Size left, float right)
    {
        return new
        (
            left.Width - right,
            left.Height - right   
        );
    }
    public static Size operator *(Size left, float right)
    {
        return new
        (
            left.Width * right,
            left.Height * right   
        );
    }
    public static Size operator /(Size left, float right)
    {
        if (right == 0) return left;
        return new
        (
            left.Width / right,
            left.Height / right   
        );
    }
    public bool Equals(Size other) => Width.Equals(other.Width) && Height.Equals(other.Height);

    public override bool Equals(object? obj) => obj is Size other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Width, Height);
}