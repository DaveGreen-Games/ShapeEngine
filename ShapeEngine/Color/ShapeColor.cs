using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public readonly struct ColorHSL
{
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


    public ColorHSL(float hue, float saturation, float lightness)
    {
        this.Hue = hue;
        this.Saturation = saturation;
        this.Lightness = lightness;
    }

    public ShapeColor ToRGB()
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
    
    
    //change/set hue,saturation,lightness functions
}



public readonly struct ShapeColor : IEquatable<ShapeColor>
{
    #region Members
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;
    #endregion

    #region Constructors

    public ShapeColor()
    {
        this.R = 0;
        this.G = 0;
        this.B = 0;
        this.A = 0;
    }
    public ShapeColor(Raylib_CsLo.Color color)
    {
        this.R = color.r;
        this.G = color.g;
        this.B = color.b;
        this.A = color.a;
    }
    public ShapeColor(Raylib_CsLo.Color color, byte a)
    {
        this.R = color.r;
        this.G = color.g;
        this.B = color.b;
        this.A = a;
    }
    public ShapeColor(System.Drawing.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }
    public ShapeColor(System.Drawing.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }
    public ShapeColor(byte r, byte g, byte b, byte a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }
    public ShapeColor(byte r, byte g, byte b)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = byte.MaxValue;
    }
    public ShapeColor(int r, int g, int b, int a)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = (byte)ShapeMath.Clamp(a, 0, 255);
    }
    public ShapeColor(int r, int g, int b)
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
    public static ShapeColor FromName(string name) => new(System.Drawing.Color.FromName(name));
    #endregion

    #region Transformation
    public ShapeColor Lerp(ShapeColor to, float f)
    {
        return new ShapeColor(
            (byte)ShapeMath.LerpInt(R, to.R, f),
            (byte)ShapeMath.LerpInt(G, to.G, f),
            (byte)ShapeMath.LerpInt(B, to.B, f),
            (byte)ShapeMath.LerpInt(A, to.A, f));
    }

    public static ShapeColor operator +(ShapeColor left, ShapeColor right)
    {
        return new
        (
            Clamp((int)left.R + (int)right.R),
            Clamp((int)left.G + (int)right.G),
            Clamp((int)left.B + (int)right.B),
            Clamp((int)left.A + (int)right.A)
        );
    }
    public static ShapeColor operator -(ShapeColor left, ShapeColor right)
    {
        return new
        (
            Clamp((int)left.R - (int)right.R),
            Clamp((int)left.G - (int)right.G),
            Clamp((int)left.B - (int)right.B),
            Clamp((int)left.A - (int)right.A)
        );
    }
    public static ShapeColor operator *(ShapeColor left, ShapeColor right)
    {
        return new
        (
            Clamp((int)left.R * (int)right.R),
            Clamp((int)left.G * (int)right.G),
            Clamp((int)left.B * (int)right.B),
            Clamp((int)left.A * (int)right.A)
        );
    }
        
    public ShapeColor SetAlpha(byte a) => new(R, G, B, a);
    public ShapeColor ChangeAlpha(int amount) => new(R, G, B, Clamp((int)A + amount));
    public ShapeColor SetRed(byte r) => new(r, G, B, A);
    public ShapeColor ChangeRed(int amount) => new(Clamp((int)R + amount), G, B, A);
    public ShapeColor SetGreen(byte g) => new(R, g, B, A);
    public ShapeColor ChangeGreen(int amount) => new(R, Clamp((int)G + amount), B, A);
    public ShapeColor SetBlue(byte b) => new(R, G, b, A);
    public ShapeColor ChangeBlue(int amount) => new(R, G, Clamp((int)B + amount), A);
    #endregion

    #region Conversion
    public System.Drawing.Color ToSysColor() => System.Drawing.Color.FromArgb(R, G, B, A);
    public Raylib_CsLo.Color ToRayColor() => new Raylib_CsLo.Color(R, G, B, A);
    
    public ColorHSL ToHSL()
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
            return new ColorHSL(h,s,l);
        }
        vm = v - m;
        s = vm;
        if (s > 0.0)
        {
            s /= (l <= 0.5f) ? (v + m ) : (2.0f - v - m) ;
        }
        else
        {
            return new ColorHSL(h,s,l);
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
        
        return new ColorHSL(h,s,l);
    }

    
    
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
    // //TODO fix from/to hsv color conversion!!!!
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

    
    public static ShapeColor FromHex(int colorValue) => FromHex(colorValue, byte.MaxValue);
    public static ShapeColor FromHex(int colorValue, byte a)
    {
        byte[] rgb = BitConverter.GetBytes(colorValue);
        if (!BitConverter.IsLittleEndian) Array.Reverse(rgb);
        return new(rgb[2], rgb[1], rgb[0], a);
    }
    public static ShapeColor FromHex(string hexColor) => FromHex(hexColor, byte.MaxValue);
    public static ShapeColor FromHex(string hexColor, byte a)
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
    public static ShapeColor[] ParseColors(params int[] colors)
    {
        ShapeColor[] palette = new ShapeColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            palette[i] = ShapeColor.FromHex(colors[i]);
        }
        return palette;
    }
    public static ShapeColor[] ParseColors(params string[] hexColors)
    {
        ShapeColor[] palette = new ShapeColor[hexColors.Length];
        for (int i = 0; i < hexColors.Length; i++)
        {
            palette[i] = ShapeColor.FromHex(hexColors[i]);
        }
        return palette;
    }

    #endregion

    #region Equatable & ToString
    public override string ToString() => ToSysColor().ToString();

    public static bool operator ==(ShapeColor left, ShapeColor right) =>
        left.A == right.A && left.R == right.R && left.G == right.G && left.B == right.B;

    public static bool operator !=(ShapeColor left, ShapeColor right) => !(left == right);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ShapeColor other && this.Equals(other);
    public bool Equals(ShapeColor other) => this == other;
    public override int GetHashCode() => ToSysColor().GetHashCode();
    #endregion

    public static ShapeColor White => new(255, 255, 255, 255);
    public static ShapeColor Black => new(0, 0, 0, 255);
    public static ShapeColor Clear => new(0, 0, 0, 0);
    private static byte Clamp(int value) => (byte)ShapeMath.Clamp(value, 0, 255);
}