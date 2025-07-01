using System.Globalization;
using System.Numerics;
using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a two-dimensional size with integer width and height.
/// Provides various utility methods and operators for manipulation.
/// </summary>
public readonly struct Dimensions : IEquatable<Dimensions>, IFormattable
{
    #region Members
    /// <summary>
    /// The width component.
    /// </summary>
    public readonly int Width;
    /// <summary>
    /// The height component.
    /// </summary>
    public readonly int Height;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct with width and height set to 0.
    /// </summary>
    public Dimensions() { this.Width = 0; this.Height = 0; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct with both width and height set to the specified value.
    /// </summary>
    /// <param name="value">The value for both width and height.</param>
    public Dimensions(int value) { this.Width = value; this.Height = value; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct with the specified width and height.
    /// </summary>
    /// <param name="width">The width value.</param>
    /// <param name="height">The height value.</param>
    public Dimensions(int width, int height) { this.Width = width; this.Height = height; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct with both width and height set to the specified float value (cast to int).
    /// </summary>
    /// <param name="value">The value for both width and height.</param>
    public Dimensions(float value) { this.Width = (int)value; this.Height = (int)value; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct with the specified float width and height (cast to int).
    /// </summary>
    /// <param name="width">The width value.</param>
    /// <param name="height">The height value.</param>
    public Dimensions(float width, float height) { this.Width = (int)width; this.Height = (int)height; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Dimensions"/> struct from a <see cref="Vector2"/> (X as width, Y as height, cast to int).
    /// </summary>
    /// <param name="v">The vector to use.</param>
    public Dimensions(Vector2 v) { this.Width = (int)v.X; this.Height = (int)v.Y; }
    #endregion

    #region MyRegion

    /// <summary>
    /// Returns a <see cref="Dimensions"/> instance representing an invalid dimension (-1, -1).
    /// </summary>
    public static Dimensions GetInvalidDimension() => new(-1, -1);

    /// <summary>
    /// Determines whether the dimensions are valid (both width and height are non-negative).
    /// </summary>
    public bool IsValid() { return Width >= 0 && Height >= 0; }

    /// <summary>
    /// Gets the area (width * height).
    /// </summary>
    public float Area => Width * Height;

    /// <summary>
    /// Gets the larger of the width or height.
    /// </summary>
    public int MaxDimension => Width > Height ? Width : Height;

    /// <summary>
    /// Gets the smaller of the width or height.
    /// </summary>
    public int MinDimension => Width < Height ? Width : Height;

    /// <summary>
    /// Gets the width-to-height ratio.
    /// </summary>
    public float RatioW => Width / (float)Height;

    /// <summary>
    /// Gets the height-to-width ratio.
    /// </summary>
    public float RatioH => Height / (float)Width;

    #endregion

    #region Public Functions

    /// <summary>
    /// Returns a new <see cref="Dimensions"/> adjusted to match the aspect ratio of the target dimensions.
    /// </summary>
    /// <param name="targetDimensions">The target aspect ratio.</param>
    /// <returns>Adjusted <see cref="Dimensions"/>.</returns>
    public Dimensions MatchAspectRatio(Dimensions targetDimensions)
    {
        if (Width == targetDimensions.Width && Height == targetDimensions.Height) return targetDimensions;

        float fWidth = targetDimensions.Width / (float)targetDimensions.Height;
        float fHeight = targetDimensions.Height / (float)targetDimensions.Width;

        int w = Width;
        int h = Height;

        float newWidth = ((h * fWidth) + w) * 0.5f;
        float newHeight = ((w * fHeight) + (h)) * 0.5f;

        Dimensions adjustedDimensions = new(newWidth, newHeight);
        return adjustedDimensions;
    }

    /// <summary>
    /// Returns the scale factor as a <see cref="Vector2"/> to scale this dimension to another.
    /// </summary>
    /// <param name="to">The target dimensions.</param>
    public Vector2 ScaleFactor(Dimensions to) => to.ToVector2().DivideSafe(ToVector2());

    /// <summary>
    /// Returns the area scale factor to another dimension.
    /// </summary>
    /// <param name="to">The target dimensions.</param>
    public float ScaleFactorArea(Dimensions to)
    {
        var area = Area;
        if (area <= 0) return 1f;
        return to.Area / area;
    }

    /// <summary>
    /// Returns the scale factor for the side length (square root of area scale factor).
    /// </summary>
    /// <param name="to">The target dimensions.</param>
    public float ScaleFactorAreaSide(Dimensions to) => MathF.Sqrt(ScaleFactorArea(to));

    /// <summary>
    /// Gets the conversion factors between this and another dimension.
    /// </summary>
    /// <param name="to">The target dimensions.</param>
    public DimensionConversionFactors GetConversionFactors(Dimensions to) => new(this, to);

    /// <summary>
    /// Converts this dimension to a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 ToVector2() { return new Vector2(Width, Height); }

    /// <summary>
    /// Converts this dimension to a <see cref="Size"/>.
    /// </summary>
    public Size ToSize() => new(Width, Height);

    /// <summary>
    /// Determines whether this instance and another specified <see cref="Dimensions"/> object have the same value.
    /// </summary>
    public bool Equals(Dimensions other)
    {
        return Width == other.Width && Height == other.Height;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is Dimensions d)
        {
            return Equals(d);
        }
        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a string representation of the dimensions using the specified format.
    /// </summary>
    /// <param name="format">The format string.</param>
    public string ToString(string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a string representation of the dimensions using the specified format and format provider.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="formatProvider">The format provider.</param>
    public string ToString(string? format, IFormatProvider? formatProvider)
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

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    #endregion

    #region Operators

    /// <summary>
    /// Adds two <see cref="Dimensions"/> instances.
    /// </summary>
    public static Dimensions operator +(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width + right.Width,
            left.Height + right.Height
        );
    }

    /// <summary>
    /// Divides one <see cref="Dimensions"/> by another.
    /// </summary>
    public static Dimensions operator /(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width / right.Width,
            left.Height / right.Height
        );
    }

    /// <summary>
    /// Divides a <see cref="Dimensions"/> by an integer.
    /// </summary>
    public static Dimensions operator /(Dimensions value1, int value2)
    {
        return value1 / new Dimensions(value2);
    }

    /// <summary>
    /// Divides a <see cref="Dimensions"/> by a float.
    /// </summary>
    public static Dimensions operator /(Dimensions value1, float value2)
    {
        return new Dimensions(value1.Width / value2, value1.Height / value2);
    }

    /// <summary>
    /// Checks if two <see cref="Dimensions"/> instances are equal.
    /// </summary>
    public static bool operator ==(Dimensions left, Dimensions right)
    {
        return (left.Width == right.Width)
               && (left.Height == right.Height);
    }

    /// <summary>
    /// Checks if two <see cref="Dimensions"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Dimensions left, Dimensions right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Multiplies two <see cref="Dimensions"/> instances.
    /// </summary>
    public static Dimensions operator *(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width * right.Width,
            left.Height * right.Height
        );
    }

    /// <summary>
    /// Multiplies a <see cref="Dimensions"/> by a float.
    /// </summary>
    public static Dimensions operator *(Dimensions left, float right)
    {
        return new Dimensions(left.Width * right, left.Height * right);
    }

    /// <summary>
    /// Multiplies a float by a <see cref="Dimensions"/>.
    /// </summary>
    public static Dimensions operator *(float left, Dimensions right)
    {
        return right * left;
    }

    /// <summary>
    /// Subtracts one <see cref="Dimensions"/> from another.
    /// </summary>
    public static Dimensions operator -(Dimensions left, Dimensions right)
    {
        return new Dimensions(
            left.Width - right.Width,
            left.Height - right.Height
        );
    }

    /// <summary>
    /// Negates a <see cref="Dimensions"/> (returns 0 minus the value).
    /// </summary>
    public static Dimensions operator -(Dimensions value)
    {
        return new Dimensions(0) - value;
    }

    /// <summary>
    /// Adds a float to both width and height.
    /// </summary>
    public static Dimensions operator +(Dimensions left, float right)
    {
        return new Dimensions(
            left.Width + right,
            left.Height + right
        );
    }

    /// <summary>
    /// Subtracts a float from both width and height.
    /// </summary>
    public static Dimensions operator -(Dimensions left, float right)
    {
        return new Dimensions(
            left.Width - right,
            left.Height - right
        );
    }

    /// <summary>
    /// Adds an integer to both width and height.
    /// </summary>
    public static Dimensions operator +(Dimensions left, int right)
    {
        return new Dimensions(
            left.Width + right,
            left.Height + right
        );
    }

    /// <summary>
    /// Subtracts an integer from both width and height.
    /// </summary>
    public static Dimensions operator -(Dimensions left, int right)
    {
        return new Dimensions(
            left.Width - right,
            left.Height - right
        );
    }
    #endregion

    #region Static

    /// <summary>
    /// Returns a <see cref="Dimensions"/> with absolute values of width and height.
    /// </summary>
    public static Dimensions Abs(Dimensions value)
    {
        return new Dimensions(
            (int)MathF.Abs(value.Width),
            (int)MathF.Abs(value.Height)
        );
    }

    /// <summary>
    /// Clamps the given <see cref="Dimensions"/> between min and max.
    /// </summary>
    public static Dimensions Clamp(Dimensions value1, Dimensions min, Dimensions max)
    {
        return Min(Max(value1, min), max);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="Dimensions"/>.
    /// </summary>
    public static Dimensions Lerp(Dimensions value1, Dimensions value2, float amount)
    {
        return (value1 * (1.0f - amount)) + (value2 * amount);
    }

    /// <summary>
    /// Returns the maximum of two <see cref="Dimensions"/> (per component).
    /// </summary>
    public static Dimensions Max(Dimensions value1, Dimensions value2)
    {
        return new Dimensions(
            (value1.Width > value2.Width) ? value1.Width : value2.Width,
            (value1.Height > value2.Height) ? value1.Height : value2.Height
        );
    }

    /// <summary>
    /// Returns the minimum of two <see cref="Dimensions"/> (per component).
    /// </summary>
    public static Dimensions Min(Dimensions value1, Dimensions value2)
    {
        return new Dimensions(
            (value1.Width < value2.Width) ? value1.Width : value2.Width,
            (value1.Height < value2.Height) ? value1.Height : value2.Height
        );
    }
    #endregion
}