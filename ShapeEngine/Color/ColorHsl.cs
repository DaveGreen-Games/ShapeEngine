using System.Diagnostics.CodeAnalysis;
using Raylib_cs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Color;

/// <summary>
/// Represents a color in HSL (Hue, Saturation, Lightness) color space.
/// </summary>
public readonly struct ColorHsl : IEquatable<ColorHsl>
{

    #region Members
    /// <summary>
    /// The hue component of the color. Value range from 0 - 360 (color wheel degrees).
    /// </summary>
    public readonly float Hue;
    /// <summary>
    /// The saturation component of the color. Value range from 0 - 1 (gray to full color).
    /// </summary>
    public readonly float Saturation;
    /// <summary>
    /// The lightness component of the color. Value range from 0 - 1 (black to white).
    /// </summary>
    public readonly float Lightness;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorHsl"/> struct with the specified hue, saturation, and lightness.
    /// </summary>
    /// <param name="hue">The hue value (0-360).</param>
    /// <param name="saturation">The saturation value (0-1).</param>
    /// <param name="lightness">The lightness value (0-1).</param>
    public ColorHsl(float hue, float saturation, float lightness)
    {
        this.Hue = ShapeMath.WrapF(hue, 0f, 360f);
        this.Saturation = ShapeMath.Clamp(saturation, 0f, 1f);
        this.Lightness = ShapeMath.Clamp(lightness, 0f, 1f);
    }
    #endregion

    #region Conversion
    /// <summary>
    /// Converts a Raylib_cs.Color to a <see cref="ColorHsl"/>.
    /// </summary>
    /// <param name="rayColor">The Raylib_cs.Color to convert.</param>
    /// <returns>A <see cref="ColorHsl"/> representation of the input color.</returns>
    public static ColorHsl RaylibColorToHsl(Raylib_cs.Color rayColor)
    {
        var hsv = Raylib.ColorToHSV(rayColor);
        return new(hsv.X, hsv.Y, hsv.Z);
    }

    /// <summary>
    /// Converts a <see cref="ColorHsl"/> to a Raylib_cs.Color.
    /// </summary>
    /// <param name="colorHsl">The <see cref="ColorHsl"/> to convert.</param>
    /// <returns>A Raylib_cs.Color representation of the input HSL color.</returns>
    public static Raylib_cs.Color HslToRaylibColor(ColorHsl colorHsl)
    {
        return Raylib.ColorFromHSV(colorHsl.Hue, colorHsl.Saturation, colorHsl.Lightness);
    }

    /// <summary>
    /// Converts this HSL color to an RGB color.
    /// </summary>
    /// <returns>A <see cref="ColorRgba"/> representation of this HSL color.</returns>
    public ColorRgba ToRGB()
    {
        //Todo Maybe just use raylib to/from hsv functions here as well?
        double v;
        double r,g,b;

        r = Lightness;   // default to gray
        g = Lightness;
        b = Lightness;
        v = (Lightness <= 0.5) ? (Lightness * (1.0 + Saturation)) : (Lightness + Saturation - Lightness * Saturation);
        if (v > 0)
        {
            double m;
            double sv;
            int sextant;
            double fract, vsf, mid1, mid2;

            m = Lightness + Lightness - v;
            sv = (v - m ) / v;
            var h = Hue * 6.0;
            sextant = (int)h;
            fract = h - sextant;
            vsf = v * sv * fract;
            mid1 = m + vsf;
            mid2 = v - vsf;
            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;
                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;
                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;
                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;
                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }

        return new
        (
            Convert.ToByte(r * 255.0),
            Convert.ToByte(g * 255.0),
            Convert.ToByte(b * 255.0)
        );
    }
    #endregion

    #region Math
    /// <summary>
    /// Linearly interpolates between this color and another HSL color.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The interpolation factor (0-1).</param>
    /// <returns>A new <see cref="ColorHsl"/> that is the interpolation between this and the target color.</returns>
    public ColorHsl Lerp(ColorHsl to, float f)
    {
        return new(
            ShapeMath.LerpFloat(Hue, to.Hue, f),
            ShapeMath.LerpFloat(Saturation, to.Saturation, f),
            ShapeMath.LerpFloat(Lightness, to.Lightness, f)
        );
    }

    /// <summary>
    /// Returns a new color with the specified hue.
    /// </summary>
    /// <param name="amount">The new hue value.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the specified hue.</returns>
    public ColorHsl SetHue(float amount)
    {
        return new
        (
            amount,
            Saturation,
            Lightness
        );
    }

    /// <summary>
    /// Returns a new color with the specified saturation.
    /// </summary>
    /// <param name="amount">The new saturation value.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the specified saturation.</returns>
    public ColorHsl SetSaturation(float amount)
    {
        return new
        (
            Hue,
            amount,
            Lightness
        );
    }

    /// <summary>
    /// Returns a new color with the specified lightness.
    /// </summary>
    /// <param name="amount">The new lightness value.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the specified lightness.</returns>
    public ColorHsl SetLightness(float amount)
    {
        return new
        (
            Hue,
            Saturation,
            amount
        );
    }

    /// <summary>
    /// Returns a new color with the hue changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the hue.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the hue changed.</returns>
    public ColorHsl ChangeHue(float amount)
    {
        return new
        (
            Hue + amount,
            Saturation,
            Lightness
        );
    }

    /// <summary>
    /// Returns a new color with the saturation changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the saturation.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the saturation changed.</returns>
    public ColorHsl ChangeSaturation(float amount)
    {
        return new
        (
            Hue,
            Saturation + amount,
            Lightness
        );
    }

    /// <summary>
    /// Returns a new color with the lightness changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the lightness.</param>
    /// <returns>A new <see cref="ColorHsl"/> with the lightness changed.</returns>
    public ColorHsl ChangeLightness(float amount)
    {
        return new
        (
            Hue,
            Saturation,
            Lightness + amount
        );
    }
    #endregion
   
    #region Operators
    /// <summary>
    /// Adds two HSL colors component-wise.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>The sum of the two colors.</returns>
    public static ColorHsl operator +(ColorHsl left, ColorHsl right)
    {
        return new
        (
            left.Hue + right.Hue,
            left.Saturation + right.Saturation,
            left.Lightness + right.Lightness
        );
    }

    /// <summary>
    /// Subtracts the second HSL color from the first component-wise.
    /// </summary>
    /// <param name="left">The color to subtract from.</param>
    /// <param name="right">The color to subtract.</param>
    /// <returns>The difference of the two colors.</returns>
    public static ColorHsl operator -(ColorHsl left, ColorHsl right)
    {
        return new
        (
            left.Hue - right.Hue,
            left.Saturation - right.Saturation,
            left.Lightness - right.Lightness
        );
    }

    /// <summary>
    /// Multiplies two HSL colors component-wise.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>The product of the two colors.</returns>
    public static ColorHsl operator *(ColorHsl left, ColorHsl right)
    {
        return new
        (
            left.Hue - right.Hue,
            left.Saturation - right.Saturation,
            left.Lightness - right.Lightness
        );
    }
    #endregion

    #region Equatable & ToString
    /// <summary>
    /// Determines whether this color has the same hue as another color.
    /// </summary>
    /// <param name="other">The other color to compare.</param>
    /// <returns>True if the hues are equal; otherwise, false.</returns>
    public bool HasSameHue(ColorHsl other) => Math.Abs(Hue - other.Hue) < 0.0001f;

    /// <summary>
    /// Determines whether this color has the same saturation as another color.
    /// </summary>
    /// <param name="other">The other color to compare.</param>
    /// <returns>True if the saturations are equal; otherwise, false.</returns>
    public bool HasSameSaturation(ColorHsl other) => Math.Abs(Saturation - other.Saturation) < 0.0001f;

    /// <summary>
    /// Determines whether this color has the same lightness as another color.
    /// </summary>
    /// <param name="other">The other color to compare.</param>
    /// <returns>True if the lightness values are equal; otherwise, false.</returns>
    public bool HasSameLightness(ColorHsl other) => Math.Abs(Lightness - other.Lightness) < 0.0001f;
    
    /// <summary>
    /// Determines whether two <see cref="ColorHsl"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color to compare.</param>
    /// <param name="right">The second color to compare.</param>
    /// <returns>True if the colors are equal; otherwise, false.</returns>
    public static bool operator ==(ColorHsl left, ColorHsl right) =>
        left.HasSameHue(right) && left.HasSameSaturation(right) && left.HasSameLightness(right);

    /// <summary>
    /// Determines whether two <see cref="ColorHsl"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first color to compare.</param>
    /// <param name="right">The second color to compare.</param>
    /// <returns>True if the colors are not equal; otherwise, false.</returns>
    public static bool operator !=(ColorHsl left, ColorHsl right) => !(left == right);

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="ColorHsl"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="ColorHsl"/>.</param>
    /// <returns>True if the specified object is a <see cref="ColorHsl"/> and is equal to this instance; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorHsl other && this.Equals(other);

    /// <summary>
    /// Determines whether the specified <see cref="ColorHsl"/> is equal to the current <see cref="ColorHsl"/>.
    /// </summary>
    /// <param name="other">The <see cref="ColorHsl"/> to compare with the current <see cref="ColorHsl"/>.</param>
    /// <returns>True if the specified <see cref="ColorHsl"/> is equal to this instance; otherwise, false.</returns>
    public bool Equals(ColorHsl other) => this == other;

    /// <summary>
    /// Returns a hash code for this <see cref="ColorHsl"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="ColorHsl"/>.</returns>
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Hue);
        hashCode.Add(Saturation);
        hashCode.Add(Lightness);
        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ColorHsl"/>.
    /// </summary>
    /// <returns>A string representation of the color.</returns>
    public override string ToString()
    {
        return $"Hue {ShapeMath.RoundToDecimals(Hue, 1)}, Saturation {ShapeMath.RoundToDecimals(Saturation, 1)}, Lightness {ShapeMath.RoundToDecimals(Lightness, 1)}";
        
        // DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 5);
        // interpolatedStringHandler.AppendFormatted(nameof (Color));
        // interpolatedStringHandler.AppendLiteral(" [A=");
        // interpolatedStringHandler.AppendFormatted<byte>(this.A);
        // interpolatedStringHandler.AppendLiteral(", R=");
        // interpolatedStringHandler.AppendFormatted<byte>(this.R);
        // interpolatedStringHandler.AppendLiteral(", G=");
        // interpolatedStringHandler.AppendFormatted<byte>(this.G);
        // interpolatedStringHandler.AppendLiteral(", B=");
        // interpolatedStringHandler.AppendFormatted<byte>(this.B);
        // interpolatedStringHandler.AppendLiteral("]");
        // return interpolatedStringHandler.ToStringAndClear();
    }

    #endregion
}