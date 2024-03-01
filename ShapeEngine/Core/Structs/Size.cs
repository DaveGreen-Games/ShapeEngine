using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct Size : IEquatable<Size>
{
    public readonly float Width;
    public readonly float Height;

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

    public float Area => Width * Height;
    public bool IsSquare => Width > 0 && Height > 0 && Math.Abs(Width - Height) < 0.0001f;



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