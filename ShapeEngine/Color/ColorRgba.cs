using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Raylib_cs;
using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public readonly struct ColorRgba : IEquatable<ColorRgba>
{
    #region Members
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;
    #endregion

    #region Constructors

    public ColorRgba()
    {
        this.R = 0;
        this.G = 0;
        this.B = 0;
        this.A = 0;
    }
    public ColorRgba(Raylib_cs.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }
    public ColorRgba(Raylib_cs.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }
    public ColorRgba(System.Drawing.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }
    public ColorRgba(System.Drawing.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }
    public ColorRgba(byte r, byte g, byte b, byte a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }
    public ColorRgba(byte r, byte g, byte b)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = byte.MaxValue;
    }
    public ColorRgba(int r, int g, int b, int a)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = (byte)ShapeMath.Clamp(a, 0, 255);
    }
    public ColorRgba(int r, int g, int b)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = byte.MaxValue;
    }
        
    #endregion

    #region Information
    /// <summary>
    /// Alpha is zero.
    /// </summary>
    public bool IsClear() => A == byte.MinValue;
    /// <summary>
    /// Is not clear and not opaque.
    /// </summary>
    public bool IsTransparent() => A is > byte.MinValue and < byte.MaxValue;
    /// <summary>
    /// Alpha is at max value.
    /// </summary>
    public bool IsOpaque() => A == byte.MaxValue;
    public bool IsNamed() => ToSysColor().IsNamedColor;
    public KnownColor ToKnownColor() => ToSysColor().ToKnownColor();
    public static ColorRgba FromName(string name) => new(System.Drawing.Color.FromName(name));
    public byte GetRelativeLuminance() => (byte)(0.2126f * R + 0.7152f * G + 0.0722f * B);
    public float GetRelativeLuminanceF() => 0.2126f * (R / 255f) + 0.7152f * (G / 255f) + 0.0722f * (B / 255f);
    #endregion

    #region Transformation
    public ColorRgba Lerp(ColorRgba to, float f)
    {
        return new ColorRgba(
            (byte)ShapeMath.LerpInt(R, to.R, f),
            (byte)ShapeMath.LerpInt(G, to.G, f),
            (byte)ShapeMath.LerpInt(B, to.B, f),
            (byte)ShapeMath.LerpInt(A, to.A, f));
    }

    public static ColorRgba operator +(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp((int)left.R + (int)right.R),
            Clamp((int)left.G + (int)right.G),
            Clamp((int)left.B + (int)right.B),
            Clamp((int)left.A + (int)right.A)
        );
    }
    public static ColorRgba operator -(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp((int)left.R - (int)right.R),
            Clamp((int)left.G - (int)right.G),
            Clamp((int)left.B - (int)right.B),
            Clamp((int)left.A - (int)right.A)
        );
    }
    public static ColorRgba operator *(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp((int)left.R * (int)right.R),
            Clamp((int)left.G * (int)right.G),
            Clamp((int)left.B * (int)right.B),
            Clamp((int)left.A * (int)right.A)
        );
    }

    /// <summary>
    /// Change the brightness of the color.
    /// </summary>
    /// <param name="correctionFactor">Range -1 to 1</param>
    public ColorRgba ChangeBrightness(float correctionFactor) => new(Raylib.ColorBrightness(ToRayColor(), correctionFactor));
    /// <summary>
    /// Change the contrast of the color.
    /// </summary>
    /// <param name="correctionFactor">Range -1 to 1</param>
    public ColorRgba ChangeContrast(float correctionFactor) => new(Raylib.ColorContrast(ToRayColor(), correctionFactor));

    
    public ColorRgba SetAlpha(byte a) => new(R, G, B, a);
    public ColorRgba ChangeAlpha(int amount) => new(R, G, B, Clamp((int)A + amount));
    public ColorRgba SetRed(byte r) => new(r, G, B, A);
    public ColorRgba ChangeRed(int amount) => new(Clamp((int)R + amount), G, B, A);
    public ColorRgba SetGreen(byte g) => new(R, g, B, A);
    public ColorRgba ChangeGreen(int amount) => new(R, Clamp((int)G + amount), B, A);
    public ColorRgba SetBlue(byte b) => new(R, G, b, A);
    public ColorRgba ChangeBlue(int amount) => new(R, G, Clamp((int)B + amount), A);
    #endregion

    #region Conversion
    public System.Drawing.Color ToSysColor() => System.Drawing.Color.FromArgb(R, G, B, A);
    public Raylib_cs.Color ToRayColor() => new (R, G, B, A);

    public (float r, float g, float b, float a) Normalize()
    {
        return 
        (
            R / 255f,
            G / 255f,
            B / 255f,
            A / 255f
        );
    }
    public static ColorRgba FromNormalize(float r, float g, float b, float a) => new((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f));

    public static ColorRgba FromNormalize((float r, float g, float b, float a) normalizedColor) => FromNormalize(normalizedColor.r, normalizedColor.g, normalizedColor.b, normalizedColor.a);

    public ColorHsl ToHSL()
    {
        float r = R/255.0f;
        float g = G/255.0f;
        float b = B/255.0f;
        float v;
        float m;
        float vm;
        float r2, g2, b2;
 
        float h = 0; // default to black
        float s = 0;
        float l = 0;
        v = Math.Max(r,g);
        v = Math.Max(v,b);
        m = Math.Min(r,g);
        m = Math.Min(m,b);
        l = (m + v) / 2.0f;
        if (l <= 0.0)
        {
            return new ColorHsl(h,s,l);
        }
        vm = v - m;
        s = vm;
        if (s > 0.0)
        {
            s /= (l <= 0.5f) ? (v + m ) : (2.0f - v - m) ;
        }
        else
        {
            return new ColorHsl(h,s,l);
        }
        r2 = (v - r) / vm;
        g2 = (v - g) / vm;
        b2 = (v - b) / vm;
        if (Math.Abs(r - v) < 0.0001f)
        {
            h = (Math.Abs(g - m) < 0.0001f ? 5.0f + b2 : 1.0f - g2);
        }
        else if (Math.Abs(g - v) < 0.0001f)
        {
            h = (Math.Abs(b - m) < 0.0001f ? 1.0f + r2 : 3.0f - b2);
        }
        else
        {
            h = (Math.Abs(r - m) < 0.0001f ? 3.0f + g2 : 5.0f - r2);
        }
        h /= 6.0f;
        
        return new ColorHsl(h,s,l);
    }

    
    public int ToHex() => Raylib.ColorToInt(ToRayColor());
    public static ColorRgba FromHex(int colorValue) => FromHex(colorValue, byte.MaxValue);
    public static ColorRgba FromHex(int colorValue, byte a)
    {
        byte[] rgb = BitConverter.GetBytes(colorValue);
        if (!BitConverter.IsLittleEndian) Array.Reverse(rgb);
        return new(rgb[2], rgb[1], rgb[0], a);
    }
    public static ColorRgba FromHex(string hexColor) => FromHex(hexColor, byte.MaxValue);
    public static ColorRgba FromHex(string hexColor, byte a)
    {
        //Remove # if present
        if (hexColor.IndexOf('#') != -1)
            hexColor = hexColor.Replace("#", "");

        int red = 0;
        int green = 0;
        int blue = 0;

        if (hexColor.Length == 6)
        {
            //#RRGGBB
            red = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            green = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        else if (hexColor.Length == 3)
        {
            //#RGB
            red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
            green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        return new((byte)red, (byte)green, (byte)blue, a);
    }
    public static ColorRgba[] ParseColors(params int[] colors)
    {
        ColorRgba[] palette = new ColorRgba[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            palette[i] = ColorRgba.FromHex(colors[i]);
        }
        return palette;
    }
    public static ColorRgba[] ParseColors(params string[] hexColors)
    {
        ColorRgba[] palette = new ColorRgba[hexColors.Length];
        for (int i = 0; i < hexColors.Length; i++)
        {
            palette[i] = ColorRgba.FromHex(hexColors[i]);
        }
        return palette;
    }

    #endregion

    #region Equatable & ToString
    public override string ToString() => ToSysColor().ToString();

    public static bool operator ==(ColorRgba left, ColorRgba right) =>
        left.A == right.A && left.R == right.R && left.G == right.G && left.B == right.B;

    public static bool operator !=(ColorRgba left, ColorRgba right) => !(left == right);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorRgba other && this.Equals(other);
    public bool Equals(ColorRgba other) => this == other;
    public override int GetHashCode() => ToSysColor().GetHashCode();
    #endregion

    public static ColorRgba White => new(255, 255, 255, 255);
    public static ColorRgba Black => new(0, 0, 0, 255);
    public static ColorRgba Clear => new(0, 0, 0, 0);
    private static byte Clamp(int value) => (byte)ShapeMath.Clamp(value, 0, 255);
}



//BROKEN!!!
// /// <summary>
    // /// Value range from 0 - 1;
    // /// </summary>
    // public float GetBrightness() => ToSysColor().GetBrightness();
    // /// <summary>
    // /// Value range from 0 - 360; (Degrees of the color wheel)
    // /// </summary>
    // public float GetHue() => ToSysColor().GetHue();
    // /// <summary>
    // /// Value range from 0 - 1;
    // /// </summary>
    // public float GetSaturation() => ToSysColor().GetSaturation();
    //
    // // public SColor ChangeBrightness(float correctionFactor)
    // // {
    // //     float red = R;
    // //     float green = G;
    // //     float blue = B;
    // //
    // //     if (correctionFactor < 0)
    // //     {
    // //         correctionFactor = 1 + correctionFactor;
    // //         red *= correctionFactor;
    // //         green *= correctionFactor;
    // //         blue *= correctionFactor;
    // //     }
    // //     else
    // //     {
    // //         red = (255 - red) * correctionFactor + red;
    // //         green = (255 - green) * correctionFactor + green;
    // //         blue = (255 - blue) * correctionFactor + blue;
    // //     }
    // //
    // //     return new((byte)red, (byte)green, (byte)blue, A);
    // // }
    // //
    // // public SColor ChangeHUE(int amount)
    // // {
    // //     var hvs = ColorToHSV(color);
    // //     return ColorFromHSV((hvs.hue + amount) % 360, hvs.saturation, hvs.value);
    // // }
    //
    // public ShapeColor ChangeBrightness(float amount)
    // {
    //     var hsv = ToHSV();
    //     return FromHSV(hsv.hue, hsv.saturation, hsv.value);
    //     
    //     float newValue = ShapeMath.Clamp(hsv.value + amount, 0f, 1f);
    //     // Console.WriteLine($"------------------Change Brigthness {hsv} | Old {hsv.value} | New {newValue} | Amount {amount}");
    //     return FromHSV(hsv.hue, hsv.saturation, newValue);
    // }
    // public ShapeColor ChangeHue(float amount)
    // {
    //     var hsv = ToHSV();
    //     float newValue = ShapeMath.Clamp(hsv.hue + amount, 0f, 360f);
    //     return FromHSV(newValue, hsv.saturation, hsv.value);
    // }
    // public ShapeColor ChangeSaturation(float amount)
    // {
    //     var hsv = ToHSV();
    //     float newValue = ShapeMath.Clamp(hsv.saturation + amount, 0f, 1f);
    //     return FromHSV(hsv.hue, newValue, hsv.value);
    // }
    //
    //
    //
    // public (float hue, float saturation, float value) ToHSV()
    // {
    //     double hue;
    //     double saturation;
    //     double value;
    //     ColorToHSV(this.ToSysColor(), out hue, out saturation, out value);
    //     return new((float)hue, (float)saturation, (float)value);
    //     // int max = Math.Max(R, Math.Max(G, B));
    //     // int min = Math.Min(R, Math.Min(G, B));
    //     //
    //     // var systemColor = ToSysColor(); // System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b);
    //     //
    //     // float hue = systemColor.GetHue();
    //     // float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
    //     // float value = max / 255f;
    //     // return new(hue, saturation, value);
    //     //
    //     // var sysColor = ToSysColor();
    //     // return new(sysColor.GetHue(), sysColor.GetSaturation(), sysColor.GetBrightness());
    // }
    // private static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
    // {
    //     int max = Math.Max(color.R, Math.Max(color.G, color.B));
    //     int min = Math.Min(color.R, Math.Min(color.G, color.B));
    //
    //     hue = (color.GetHue() + 180) % 360;
    //     saturation = (max == 0) ? 0 : 1d - (1d * min / max);
    //     value = max / 255d;
    // }
    //
    // public static ShapeColor FromHSV(float hue, float saturation, float value)
    // {
    //     return new(ColorFromHSV(hue, saturation, value));
    //     
    //     hue = ShapeMath.Clamp(hue, 0f, 1f);
    //     saturation = ShapeMath.Clamp(saturation, 0f, 1f);
    //     value = ShapeMath.Clamp(value, 0f, 1f);
    //         
    //     int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
    //     float f = hue / 60 - MathF.Floor(hue / 60);
    //
    //     value = value * 255;
    //     int v = Convert.ToInt32(value);
    //     int p = Convert.ToInt32(value * (1 - saturation));
    //     int q = Convert.ToInt32(value * (1 - f * saturation));
    //     int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
    //
    //     if (hi == 0) return new(v, t, p, 255);
    //     if (hi == 1) return new(q, v, p, 255);
    //     if (hi == 2) return new(p, v, t, 255);
    //     if (hi == 3) return new(p, q, v, 255);
    //     if (hi == 4) return new(t, p, v, 255);
    //     return new(v, p, q, 255);
    //
    //     
    // }
    // private static System.Drawing.Color ColorFromHSV(double hue, double saturation, double value)
    // {
    //     int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    //     double f = hue / 60 - Math.Floor(hue / 60);
    //
    //     value = value * 255;
    //     int v = Convert.ToInt32(value);
    //     int p = Convert.ToInt32(value * (1 - saturation));
    //     int q = Convert.ToInt32(value * (1 - f * saturation));
    //     int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
    //
    //     if (hi == 0)
    //         return System.Drawing.Color.FromArgb(255, v, t, p);
    //     else if (hi == 1)
    //         return System.Drawing.Color.FromArgb(255, q, v, p);
    //     else if (hi == 2)
    //         return System.Drawing.Color.FromArgb(255, p, v, t);
    //     else if (hi == 3)
    //         return System.Drawing.Color.FromArgb(255, p, q, v);
    //     else if (hi == 4)
    //         return System.Drawing.Color.FromArgb(255, t, p, v);
    //     else
    //         return System.Drawing.Color.FromArgb(255, v, p, q);
    // }
    //
    //----------------------------------------------
