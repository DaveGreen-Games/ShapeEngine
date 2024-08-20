using System.Globalization;
using System.Numerics;
using System.Text;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Dimensions : IEquatable<Dimensions>, IFormattable
{
    public readonly int Width;
    public readonly int Height;

    public Dimensions() { this.Width = 0; this.Height = 0; }
    public Dimensions(int value) { this.Width = value; this.Height = value; }
    public Dimensions(int width, int height) { this.Width = width; this.Height = height; }
    public Dimensions(float value) { this.Width = (int)value; this.Height = (int)value; }
    public Dimensions(float width, float height) { this.Width = (int)width; this.Height = (int)height; }
    public Dimensions(Vector2 v) { this.Width = (int)v.X; this.Height = (int)v.Y; }
    public static Dimensions GetInvalidDimension() => new(-1, -1);
    public bool IsValid() { return Width >= 0 && Height >= 0; }
    public float Area => Width * Height;

    public int MaxDimension => Width > Height ? Width : Height;
    public int MinDimension => Width < Height ? Width : Height;

    public float RatioW => Width / (float)Height;
    public float RatioH => Height / (float)Width;
    
    
    public Dimensions MatchAspectRatio(Dimensions targetDimensions)
    {
        if (Width == targetDimensions.Width && Height == targetDimensions.Height) return targetDimensions;
        
        float fWidth = (float)targetDimensions.Width / (float)targetDimensions.Height;
        float fHeight = (float)targetDimensions.Height / (float)targetDimensions.Width;

        int w = Width;
        int h = Height;

        float newWidth = ((h * fWidth) + w) * 0.5f;
        float newHeight = ((w * fHeight) + (h)) * 0.5f;

        Dimensions adjustedDimensions = new(newWidth, newHeight);
        return adjustedDimensions;
    }
    
    
    
    
    public Vector2 ScaleFactor(Dimensions to) => to.ToVector2().DivideSafe(ToVector2());
    public float ScaleFactorArea(Dimensions to)
    {
        var area = Area;
        if (area <= 0) return 1f;
        return to.Area / area;
    }
    public float ScaleFactorAreaSide(Dimensions to) => MathF.Sqrt(ScaleFactorArea(to));

    public DimensionConversionFactors GetConversionFactors(Dimensions to) => new(this, to);

    public Vector2 ToVector2() { return new Vector2(Width, Height); }
    public Size ToSize() => new(Width, Height);
        
    public bool Equals(Dimensions other)
    {
        return Width == other.Width && Height == other.Height;
    }
    public override bool Equals(object? obj)
    {
        if (obj is Dimensions d)
        {
            return Equals(d);
        }
        return false;
    }
    public readonly override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }
    public readonly string ToString(string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        StringBuilder sb = new StringBuilder();
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        sb.Append('<');
        sb.Append(Width.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(Height.ToString(format, formatProvider));
        sb.Append('>');
        return sb.ToString();
    }

    public static Dimensions operator +(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width + right.Width,
            left.Height + right.Height
        );
    }
    public static Dimensions operator /(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width / right.Width,
            left.Height / right.Height
        );
    }
    public static Dimensions operator /(Dimensions value1, int value2)
    {
        return value1 / new Dimensions(value2);
    }
    public static Dimensions operator /(Dimensions value1, float value2)
    {
        return new Dimensions(value1.Width / value2, value1.Height / value2);
    }
    public static bool operator ==(Dimensions left, Dimensions right)
    {
        return (left.Width == right.Width)
               && (left.Height == right.Height);
    }
    public static bool operator !=(Dimensions left, Dimensions right)
    {
        return !(left == right);
    }
    public static Dimensions operator *(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width * right.Width,
            left.Height * right.Height
        );
    }
    public static Dimensions operator *(Dimensions left, float right)
    {
        return new Dimensions(left.Width * right, left.Height * right);
    }
    public static Dimensions operator *(float left, Dimensions right)
    {
        return right * left;
    }
    public static Dimensions operator -(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width - right.Width,
            left.Height - right.Height
        );
    }
    public static Dimensions operator -(Dimensions value)
    {
        return new Dimensions(0) - value;
    }


    public static Dimensions Abs(Dimensions value)
    {
        return new Dimensions(
            (int)MathF.Abs(value.Width),
            (int)MathF.Abs(value.Height)
        );
    }
    public static Dimensions Clamp(Dimensions value1, Dimensions min, Dimensions max)
    {
        return Min(Max(value1, min), max);
    }
    public static Dimensions Lerp(Dimensions value1, Dimensions value2, float amount)
    {
        return (value1 * (1.0f - amount)) + (value2 * amount);
    }
    public static Dimensions Max(Dimensions value1, Dimensions value2)
    {
        return new Dimensions(
            (value1.Width > value2.Width) ? value1.Width : value2.Width,
            (value1.Height > value2.Height) ? value1.Height : value2.Height
        );
    }
    public static Dimensions Min(Dimensions value1, Dimensions value2)
    {
        return new Dimensions(
            (value1.Width < value2.Width) ? value1.Width : value2.Width,
            (value1.Height < value2.Height) ? value1.Height : value2.Height
        );
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }


}