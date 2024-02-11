using System.Diagnostics.CodeAnalysis;
using Raylib_cs;
using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public readonly struct ColorHsl : IEquatable<ColorHsl>
{

    #region Members
    /// <summary>
    /// Value range from 0 - 360 (color wheel degrees)
    /// </summary>
    public readonly float Hue;
    /// <summary>
    /// Value range from 0 - 1 (gray to full color)
    /// </summary>
    public readonly float Saturation;
    /// <summary>
    /// Value range from 0 - 1 (black to white)
    /// </summary>
    public readonly float Lightness;
    #endregion

    #region Constructors
    public ColorHsl(float hue, float saturation, float lightness)
    {
        this.Hue = ShapeMath.WrapF(hue, 0f, 360f);
        this.Saturation = ShapeMath.Clamp(saturation, 0f, 1f);
        this.Lightness = ShapeMath.Clamp(lightness, 0f, 1f);
    }
    #endregion

    #region Conversion
    public static ColorHsl RaylibColorToHsl(Raylib_cs.Color rayColor)
    {
        var hsv = Raylib.ColorToHSV(rayColor);
        return new(hsv.X, hsv.Y, hsv.Z);
    }

    public static Raylib_cs.Color HslToRaylibColor(ColorHsl colorHsl)
    {
        return Raylib.ColorFromHSV(colorHsl.Hue, colorHsl.Saturation, colorHsl.Lightness);
    }
    public ColorRgba ToRGB()
    {
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
    public ColorHsl Lerp(ColorHsl to, float f)
    {
        return new(
            ShapeMath.LerpFloat(Hue, to.Hue, f),
            ShapeMath.LerpFloat(Saturation, to.Saturation, f),
            ShapeMath.LerpFloat(Lightness, to.Lightness, f)
        );
    }

    
    public ColorHsl SetHue(float amount)
    {
        return new
        (
            amount,
            Saturation,
            Lightness
        );
    }
    public ColorHsl SetSaturation(float amount)
    {
        return new
        (
            Hue,
            amount,
            Lightness
        );
    }
    public ColorHsl SetLightness(float amount)
    {
        return new
        (
            Hue,
            Saturation,
            amount
        );
    }

    public ColorHsl ChangeHue(float amount)
    {
        return new
        (
            Hue + amount,
            Saturation,
            Lightness
        );
    }
    public ColorHsl ChangeSaturation(float amount)
    {
        return new
        (
            Hue,
            Saturation + amount,
            Lightness
        );
    }
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
    public static ColorHsl operator +(ColorHsl left, ColorHsl right)
    {
        return new
        (
            left.Hue + right.Hue,
            left.Saturation + right.Saturation,
            left.Lightness + right.Lightness
        );
    }
    public static ColorHsl operator -(ColorHsl left, ColorHsl right)
    {
        return new
        (
            left.Hue - right.Hue,
            left.Saturation - right.Saturation,
            left.Lightness - right.Lightness
        );
    }
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
    public bool HasSameHue(ColorHsl other) => Math.Abs(Hue - other.Hue) < 0.0001f;
    public bool HasSameSaturation(ColorHsl other) => Math.Abs(Saturation - other.Saturation) < 0.0001f;
    public bool HasSameLightness(ColorHsl other) => Math.Abs(Lightness - other.Lightness) < 0.0001f;
    
    public static bool operator ==(ColorHsl left, ColorHsl right) =>
        left.HasSameHue(right) && left.HasSameSaturation(right) && left.HasSameLightness(right);

    public static bool operator !=(ColorHsl left, ColorHsl right) => !(left == right);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorHsl other && this.Equals(other);
    public bool Equals(ColorHsl other) => this == other;
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Hue);
        hashCode.Add(Saturation);
        hashCode.Add(Lightness);
        return hashCode.ToHashCode();
    }
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