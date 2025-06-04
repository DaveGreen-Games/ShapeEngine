using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Raylib_cs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Color;

/// <summary>
/// Represents an RGBA color with 8-bit components for red, green, blue, and alpha channels.
/// </summary>
/// <remarks>
/// This immutable struct provides color manipulation, conversion between different color formats,
/// and various utility methods for color transformations.
/// It implements IEquatable for efficient comparison operations and integrates with both System.Drawing.Color and Raylib_cs.Color.
/// </remarks>
public readonly struct ColorRgba : IEquatable<ColorRgba>
{
    #region Members
    /// <summary>
    /// The red component of the color (0-255).
    /// </summary>
    public readonly byte R;
    
    /// <summary>
    /// The green component of the color (0-255).
    /// </summary>
    public readonly byte G;
    
    /// <summary>
    /// The blue component of the color (0-255).
    /// </summary>
    public readonly byte B;
    
    /// <summary>
    /// The alpha (transparency) component of the color (0-255).
    /// </summary>
    public readonly byte A;
    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with all components set to 0 (fully transparent black).
    /// </summary>
    public ColorRgba()
    {
        this.R = 0;
        this.G = 0;
        this.B = 0;
        this.A = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a Raylib color.
    /// </summary>
    /// <param name="color">The Raylib color to convert.</param>
    public ColorRgba(Raylib_cs.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a Raylib color with a custom alpha value.
    /// </summary>
    /// <param name="color">The Raylib color to convert.</param>
    /// <param name="a">The alpha value to use (0-255).</param>
    public ColorRgba(Raylib_cs.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a System.Drawing color.
    /// </summary>
    /// <param name="color">The System.Drawing color to convert.</param>
    public ColorRgba(System.Drawing.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a System.Drawing color with a custom alpha value.
    /// </summary>
    /// <param name="color">The System.Drawing color to convert.</param>
    /// <param name="a">The alpha value to use (0-255).</param>
    public ColorRgba(System.Drawing.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGBA components.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-255).</param>
    public ColorRgba(byte r, byte g, byte b, byte a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGB components and full opacity.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    public ColorRgba(byte r, byte g, byte b)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = byte.MaxValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGBA components as integers.
    /// Values are automatically clamped to the valid range (0-255).
    /// </summary>
    /// <param name="r">The red component as an integer (will be clamped to 0-255).</param>
    /// <param name="g">The green component as an integer (will be clamped to 0-255).</param>
    /// <param name="b">The blue component as an integer (will be clamped to 0-255).</param>
    /// <param name="a">The alpha component as an integer (will be clamped to 0-255).</param>
    public ColorRgba(int r, int g, int b, int a)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = (byte)ShapeMath.Clamp(a, 0, 255);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGB components as integers and full opacity.
    /// Values are automatically clamped to the valid range (0-255).
    /// </summary>
    /// <param name="r">The red component as an integer (will be clamped to 0-255).</param>
    /// <param name="g">The green component as an integer (will be clamped to 0-255).</param>
    /// <param name="b">The blue component as an integer (will be clamped to 0-255).</param>
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
    /// Determines whether the color is completely transparent (alpha is zero).
    /// </summary>
    /// <returns>True if the alpha component is zero; otherwise, false.</returns>
    public bool IsClear() => A == byte.MinValue;

    /// <summary>
    /// Determines whether the color is partially transparent (alpha is between 0 and 255).
    /// </summary>
    /// <returns>True if the alpha component is greater than zero and less than 255; otherwise, false.</returns>
    public bool IsTransparent() => A is > byte.MinValue and < byte.MaxValue;

    /// <summary>
    /// Determines whether the color is completely opaque (alpha is 255).
    /// </summary>
    /// <returns>True if the alpha component is 255; otherwise, false.</returns>
    public bool IsOpaque() => A == byte.MaxValue;

    /// <summary>
    /// Determines whether the color corresponds to a named color in the System.Drawing.Color enumeration.
    /// </summary>
    /// <returns>True if the color matches a named color; otherwise, false.</returns>
    public bool IsNamed() => ToSysColor().IsNamedColor;

    /// <summary>
    /// Converts the color to its corresponding KnownColor enumeration value if it matches a known color.
    /// </summary>
    /// <returns>The KnownColor enumeration value that corresponds to this color.</returns>
    public KnownColor ToKnownColor() => ToSysColor().ToKnownColor();

    /// <summary>
    /// Creates a new ColorRgba from a named color.
    /// </summary>
    /// <param name="name">The name of the color to create.</param>
    /// <returns>A new ColorRgba instance representing the named color.</returns>
    public static ColorRgba FromName(string name) => new(System.Drawing.Color.FromName(name));

    /// <summary>
    /// Calculates the relative luminance of the color as a byte value.
    /// Uses the standard formula: 0.2126*R + 0.7152*G + 0.0722*B
    /// </summary>
    /// <returns>The relative luminance as a byte value (0-255).</returns>
    public byte GetRelativeLuminance() => (byte)(0.2126f * R + 0.7152f * G + 0.0722f * B);

    /// <summary>
    /// Calculates the relative luminance of the color as a normalized float value.
    /// Uses the standard formula: 0.2126*R + 0.7152*G + 0.0722*B with normalized RGB components.
    /// </summary>
    /// <returns>The relative luminance as a float value (0.0-1.0).</returns>
    public float GetRelativeLuminanceF() => 0.2126f * (R / 255f) + 0.7152f * (G / 255f) + 0.0722f * (B / 255f);
    #endregion

    #region Transformation
    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The interpolation factor in the range [0.0, 1.0], where 0 returns the source color and 1 returns the target color.</param>
    /// <returns>A new color that is the linear interpolation between the source and target colors.</returns>
    public static ColorRgba Lerp(ColorRgba from, ColorRgba to, float f)
    {
        return new ColorRgba(
            (byte)ShapeMath.LerpInt(from.R, to.R, f),
            (byte)ShapeMath.LerpInt(from.G, to.G, f),
            (byte)ShapeMath.LerpInt(from.B, to.B, f),
            (byte)ShapeMath.LerpInt(from.A, to.A, f));
    }

    /// <summary>
    /// Performs an exponential decay interpolation between two colors.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The decay factor that controls the rate of interpolation. (0 - 1)</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the exponential decay interpolation between the source and target colors.</returns>
    public static ColorRgba ExpDecayLerp(ColorRgba from, ColorRgba to, float f, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpInt(from.R, to.R, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.G, to.G, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.B, to.B, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.A, to.A, f, dt));
    }

    /// <summary>
    /// Performs a power-based interpolation between two colors. (expensive!)
    /// Framerate independent lerp.
    /// <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/> should be used if possible.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="remainder">The remaining interpolation factor.
    /// How much fraction should remain after 1 second?</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the power-based interpolation between the source and target colors.</returns>
    public static ColorRgba PowLerp(ColorRgba from, ColorRgba to, float remainder, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.PowLerpInt(from.R, to.R, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.G, to.G, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.B, to.B, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.A, to.A, remainder, dt));
    }

    /// <summary>
    /// Performs a complex exponential decay interpolation between two colors.
    /// Framerate independent lerp.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// Base function for <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="decay">The decay rate that controls the interpolation curve.
    /// Best results between 1-25</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the complex exponential decay interpolation between the source and target colors.</returns>
    public static ColorRgba ExpDecayLerpComplex(ColorRgba from, ColorRgba to, float decay, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.R, to.R, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.G, to.G, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.B, to.B, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.A, to.A, decay, dt));
    }

    /// <summary>
    /// Linearly interpolates from this color to another color.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The interpolation factor in the range [0.0, 1.0], where 0 returns this color and 1 returns the target color.</param>
    /// <returns>A new color that is the linear interpolation between this color and the target color.</returns>
    public ColorRgba Lerp(ColorRgba to, float f)
    {
        return new ColorRgba(
            (byte)ShapeMath.LerpInt(R, to.R, f),
            (byte)ShapeMath.LerpInt(G, to.G, f),
            (byte)ShapeMath.LerpInt(B, to.B, f),
            (byte)ShapeMath.LerpInt(A, to.A, f));
    }

    /// <summary>
    /// Performs a frame rate independent exponential decay interpolation from this color to another color.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The decay factor that controls the rate of interpolation. (0 - 1)</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the exponential decay interpolation between this color and the target color.</returns>
    public ColorRgba ExpDecayLerp(ColorRgba to, float f, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpInt(R, to.R, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(G, to.G, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(B, to.B, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(A, to.A, f, dt));
    }

    /// <summary>
    /// Performs a frame rate independent power-based interpolation from this color to another color. (expensive!)
    /// <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/> should be used if possible.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="remainder">The remaining interpolation factor.</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the power-based interpolation between this color and the target color.</returns>
    public ColorRgba PowLerp(ColorRgba to, float remainder, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.PowLerpInt(R, to.R, remainder, dt),
            (byte)ShapeMath.PowLerpInt(G, to.G, remainder, dt),
            (byte)ShapeMath.PowLerpInt(B, to.B, remainder, dt),
            (byte)ShapeMath.PowLerpInt(A, to.A, remainder, dt));
    }

    /// <summary>
    /// Performs a framerate independent exponential decay interpolation from this color to another color.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>
    /// Base function for <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="decay">The decay rate that controls the interpolation curve. Best results between value 1-25.</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the complex exponential decay interpolation between this color and the target color.</returns>
    public ColorRgba ExpDecayLerpComplex(ColorRgba to, float decay, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpIntComplex(R, to.R, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(G, to.G, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(B, to.B, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(A, to.A, decay, dt));
    }

    /// <summary>
    /// Adds two colors together, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>A new color with each component being the sum of the corresponding components from the operands, clamped to the range [0, 255].</returns>
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

    /// <summary>
    /// Subtracts the second color from the first, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The color to subtract from.</param>
    /// <param name="right">The color to subtract.</param>
    /// <returns>A new color with each component being the difference of the corresponding components from the operands, clamped to the range [0, 255].</returns>
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

    /// <summary>
    /// Multiplies two colors together, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>A new color with each component being the product of the corresponding components from the operands, clamped to the range [0, 255].</returns>
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

    /// <summary>
    /// Creates a new color with the specified alpha value while preserving the RGB components.
    /// </summary>
    /// <param name="a">The new alpha value (0-255).</param>
    /// <returns>A new color with the specified alpha value and the same RGB components as this color.</returns>
    public ColorRgba SetAlpha(byte a) => new(R, G, B, a);

    /// <summary>
    /// Creates a new color with the alpha value adjusted by the specified amount while preserving the RGB components.
    /// </summary>
    /// <param name="amount">The amount to adjust the alpha value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted alpha value and the same RGB components as this color.</returns>
    public ColorRgba ChangeAlpha(int amount) => new(R, G, B, Clamp((int)A + amount));

    /// <summary>
    /// Creates a new color with the specified red value while preserving the other components.
    /// </summary>
    /// <param name="r">The new red value (0-255).</param>
    /// <returns>A new color with the specified red value and the same green, blue, and alpha components as this color.</returns>
    public ColorRgba SetRed(byte r) => new(r, G, B, A);

    /// <summary>
    /// Creates a new color with the red value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the red value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted red value and the same green, blue, and alpha components as this color.</returns>
    public ColorRgba ChangeRed(int amount) => new(Clamp((int)R + amount), G, B, A);

    /// <summary>
    /// Creates a new color with the specified green value while preserving the other components.
    /// </summary>
    /// <param name="g">The new green value (0-255).</param>
    /// <returns>A new color with the specified green value and the same red, blue, and alpha components as this color.</returns>
    public ColorRgba SetGreen(byte g) => new(R, g, B, A);

    /// <summary>
    /// Creates a new color with the green value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the green value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted green value and the same red, blue, and alpha components as this color.</returns>
    public ColorRgba ChangeGreen(int amount) => new(R, Clamp((int)G + amount), B, A);

    /// <summary>
    /// Creates a new color with the specified blue value while preserving the other components.
    /// </summary>
    /// <param name="b">The new blue value (0-255).</param>
    /// <returns>A new color with the specified blue value and the same red, green, and alpha components as this color.</returns>
    public ColorRgba SetBlue(byte b) => new(R, G, b, A);

    /// <summary>
    /// Creates a new color with the blue value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the blue value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted blue value and the same red, green, and alpha components as this color.</returns>
    public ColorRgba ChangeBlue(int amount) => new(R, G, Clamp((int)B + amount), A);
    #endregion

    #region Conversion
    /// <summary>
    /// Converts this ColorRgba to a System.Drawing.Color.
    /// </summary>
    /// <returns>A System.Drawing.Color equivalent to this ColorRgba.</returns>
    public System.Drawing.Color ToSysColor() => System.Drawing.Color.FromArgb(R, G, B, A);

    /// <summary>
    /// Converts this ColorRgba to a Raylib_cs.Color.
    /// </summary>
    /// <returns>A Raylib_cs.Color equivalent to this ColorRgba.</returns>
    public Raylib_cs.Color ToRayColor() => new (R, G, B, A);

    /// <summary>
    /// Normalizes the color components from the range [0, 255] to [0.0, 1.0].
    /// </summary>
    /// <returns>A tuple containing the normalized RGBA components as float values between 0.0 and 1.0.</returns>
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

    /// <summary>
    /// Creates a ColorRgba from normalized RGBA components.
    /// </summary>
    /// <param name="r">The normalized red component (0.0 to 1.0).</param>
    /// <param name="g">The normalized green component (0.0 to 1.0).</param>
    /// <param name="b">The normalized blue component (0.0 to 1.0).</param>
    /// <param name="a">The normalized alpha component (0.0 to 1.0).</param>
    /// <returns>A new ColorRgba with the specified normalized components converted to the range [0, 255].</returns>
    public static ColorRgba FromNormalize(float r, float g, float b, float a) => new((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f));

    /// <summary>
    /// Creates a ColorRgba from a tuple of normalized RGBA components.
    /// </summary>
    /// <param name="normalizedColor">A tuple containing the normalized RGBA components as float values between 0.0 and 1.0.</param>
    /// <returns>A new ColorRgba with the specified normalized components converted to the range [0, 255].</returns>
    public static ColorRgba FromNormalize((float r, float g, float b, float a) normalizedColor) => FromNormalize(normalizedColor.r, normalizedColor.g, normalizedColor.b, normalizedColor.a);

    /// <summary>
    /// Converts this RGB color to HSL (Hue, Saturation, Lightness) color space.
    /// </summary>
    /// <returns>A ColorHsl representing this color in the HSL color space.</returns>
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

    /// <summary>
    /// Converts this color to a hexadecimal integer representation.
    /// </summary>
    /// <returns>An integer representing the color in hexadecimal format.</returns>
    public int ToHex() => Raylib.ColorToInt(ToRayColor());

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal integer representation with full opacity.
    /// </summary>
    /// <param name="colorValue">The hexadecimal integer representation of the color.</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal value and full opacity.</returns>
    public static ColorRgba FromHex(int colorValue) => FromHex(colorValue, byte.MaxValue);

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal integer representation with the specified alpha value.
    /// </summary>
    /// <param name="colorValue">The hexadecimal integer representation of the color.</param>
    /// <param name="a">The alpha component value (0-255).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal value and the specified alpha.</returns>
    public static ColorRgba FromHex(int colorValue, byte a)
    {
        byte[] rgb = BitConverter.GetBytes(colorValue);
        if (!BitConverter.IsLittleEndian) Array.Reverse(rgb);
        return new(rgb[2], rgb[1], rgb[0], a);
    }

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal string representation with full opacity.
    /// </summary>
    /// <param name="hexColor">The hexadecimal string representation of the color (e.g., "FF0000" or "#FF0000" for red).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal string and full opacity.</returns>
    public static ColorRgba FromHex(string hexColor) => FromHex(hexColor, byte.MaxValue);

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal string representation with the specified alpha value.
    /// </summary>
    /// <param name="hexColor">The hexadecimal string representation of the color (e.g., "FF0000" or "#FF0000" for red).</param>
    /// <param name="a">The alpha component value (0-255).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal string and the specified alpha.</returns>
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

    /// <summary>
    /// Creates an array of ColorRgba objects from an array of hexadecimal integer values.
    /// </summary>
    /// <param name="colors">An array of hexadecimal integer values representing colors.</param>
    /// <returns>An array of ColorRgba objects corresponding to the provided hexadecimal values.</returns>
    public static ColorRgba[] ParseColors(params int[] colors)
    {
        ColorRgba[] palette = new ColorRgba[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            palette[i] = ColorRgba.FromHex(colors[i]);
        }
        return palette;
    }

    /// <summary>
    /// Creates an array of ColorRgba objects from an array of hexadecimal string representations.
    /// </summary>
    /// <param name="hexColors">An array of hexadecimal string representations of colors.</param>
    /// <returns>An array of ColorRgba objects corresponding to the provided hexadecimal strings.</returns>
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
    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    /// <returns>A string representation of the color in the format provided by System.Drawing.Color.</returns>
    public override string ToString() => ToSysColor().ToString();

    /// <summary>
    /// Determines whether two ColorRgba instances are equal by comparing their component values.
    /// </summary>
    /// <param name="left">The first ColorRgba to compare.</param>
    /// <param name="right">The second ColorRgba to compare.</param>
    /// <returns>True if all RGBA components of both colors are equal; otherwise, false.</returns>
    public static bool operator ==(ColorRgba left, ColorRgba right) =>
        left.A == right.A && left.R == right.R && left.G == right.G && left.B == right.B;

    /// <summary>
    /// Determines whether two ColorRgba instances are not equal.
    /// </summary>
    /// <param name="left">The first ColorRgba to compare.</param>
    /// <param name="right">The second ColorRgba to compare.</param>
    /// <returns>True if any RGBA component differs between the colors; otherwise, false.</returns>
    public static bool operator !=(ColorRgba left, ColorRgba right) => !(left == right);

    /// <summary>
    /// Determines whether the specified object is equal to the current ColorRgba.
    /// </summary>
    /// <param name="obj">The object to compare with the current ColorRgba.</param>
    /// <returns>True if the specified object is a ColorRgba and has the same component values as this ColorRgba; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorRgba other && this.Equals(other);

    /// <summary>
    /// Determines whether the specified ColorRgba is equal to the current ColorRgba.
    /// </summary>
    /// <param name="other">The ColorRgba to compare with the current ColorRgba.</param>
    /// <returns>True if the specified ColorRgba has the same component values as this ColorRgba; otherwise, false.</returns>
    public bool Equals(ColorRgba other) => this == other;

    /// <summary>
    /// Returns a hash code for this ColorRgba.
    /// </summary>
    /// <returns>A hash code for the current ColorRgba, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => ToSysColor().GetHashCode();
    #endregion

    /// <summary>
    /// Represents a predefined white color (R:255, G:255, B:255, A:255).
    /// </summary>
    public static ColorRgba White => new(255, 255, 255, 255);

    /// <summary>
    /// Represents a predefined black color (R:0, G:0, B:0, A:255).
    /// </summary>
    public static ColorRgba Black => new(0, 0, 0, 255);

    /// <summary>
    /// Represents a predefined fully transparent color (R:0, G:0, B:0, A:0).
    /// </summary>
    public static ColorRgba Clear => new(0, 0, 0, 0);

    /// <summary>
    /// Clamps an integer value to the valid color component range of 0-255.
    /// </summary>
    /// <param name="value">The integer value to clamp.</param>
    /// <returns>A byte value clamped to the range [0, 255].</returns>
    private static byte Clamp(int value) => (byte)ShapeMath.Clamp(value, 0, 255);
}