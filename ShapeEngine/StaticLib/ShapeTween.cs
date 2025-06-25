//source
//https://github.com/idbrii/cs-tween/blob/main/Easing.cs

using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides tweening (easing) functions for interpolating between values.
/// </summary>
public static class ShapeTween
{
    /// <summary>
    /// Applies the specified tweening function to the normalized time value.
    /// </summary>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Transformed value after applying the tween function.</returns>
    public static float Tween(float t, TweenType tweenType)
    {
        switch (tweenType)
        {
            case TweenType.LINEAR: return t;
            case TweenType.SINE_IN: return SineIn(t);
            case TweenType.SINE_OUT: return SineOut(t);
            case TweenType.SINE_INOUT: return SineInOut(t);
            case TweenType.CIRC_IN: return CircIn(t);
            case TweenType.CIRC_OUT: return CircOut(t);
            case TweenType.CIRC_INOUT: return CircInOut(t);
            case TweenType.CUBIC_IN: return CubicIn(t);
            case TweenType.CUBIC_OUT: return CubicOut(t);
            case TweenType.CUBIC_INOUT: return CubicInOut(t);
            case TweenType.QUAD_IN: return QuadIn(t);
            case TweenType.QUAD_OUT: return QuadOut(t);
            case TweenType.QUAD_INOUT: return QuadInOut(t);
            case TweenType.EXPO_IN: return ExpoIn(t);
            case TweenType.EXPO_OUT: return ExpoOut(t);
            case TweenType.EXPO_INOUT: return ExpoInOut(t);
            case TweenType.BACK_IN: return BackIn(t);
            case TweenType.BACK_OUT: return BackOut(t);
            case TweenType.BACK_INOUT: return BackInOut(t);
            case TweenType.BOUNCE_IN: return BounceIn(t);
            case TweenType.BOUNCE_OUT: return BounceOut(t);
            case TweenType.BOUNCE_INOUT: return BounceInOut(t);
            case TweenType.ELASTIC_IN: return ElasticIn(t);
            case TweenType.ELASTIC_OUT: return ElasticOut(t);
            case TweenType.ELASTIC_INOUT: return ElasticInOut(t);
            default: return t;
        }
    }

    /// <summary>
    /// Interpolates between two float values using the specified tweening function.
    /// </summary>
    /// <param name="from">Start value.</param>
    /// <param name="to">End value.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Interpolated float value.</returns>
    public static float Tween(float from, float to, float t, TweenType tweenType)
    {
        return ShapeMath.LerpFloat(from, to, Tween(t, tweenType));
    }

    /// <summary>
    /// Interpolates between two integer values using the specified tweening function.
    /// </summary>
    /// <param name="from">Start value.</param>
    /// <param name="to">End value.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Interpolated integer value.</returns>
    public static int Tween(int from, int to, float t, TweenType tweenType)
    {
        return ShapeMath.LerpInt(from, to, Tween(t, tweenType));
    }

    /// <summary>
    /// Interpolates between two Vector2 values using the specified tweening function.
    /// </summary>
    /// <param name="from">Start vector.</param>
    /// <param name="to">End vector.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Interpolated Vector2 value.</returns>
    public static Vector2 Tween(this Vector2 from, Vector2 to, float t, TweenType tweenType)
    {
        return from.Lerp(to, Tween(t, tweenType));// SVec.Lerp(from, to, Tween(t, tweenType));
    }

    /// <summary>
    /// Interpolates between two ColorRgba values using the specified tweening function.
    /// </summary>
    /// <param name="from">Start color.</param>
    /// <param name="to">End color.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Interpolated ColorRgba value.</returns>
    public static ColorRgba Tween(this ColorRgba from, ColorRgba to, float t, TweenType tweenType)
    {
        return from.Lerp(to, Tween(t, tweenType));
    }

    /// <summary>
    /// Interpolates between two Rect values using the specified tweening function.
    /// </summary>
    /// <param name="from">Start rectangle.</param>
    /// <param name="to">End rectangle.</param>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <param name="tweenType">Type of tweening function.</param>
    /// <returns>Interpolated Rect value.</returns>
    public static Rect Tween(this Rect from, Rect to, float t, TweenType tweenType)
    {
        return from.Lerp(to, Tween(t, tweenType)); // SColor.LerpColor(from, to, Tween(t, tweenType));
    }

    /// <summary>
    /// Quartic ease-in function.
    /// </summary>
    public static float QuartIn(float p)
    {
        return p * p * p * p;
    }

    /// <summary>
    /// Quartic ease-out function.
    /// </summary>
    public static float QuartOut(float p)
    {
        p = 1f - p;
        return 1f - (QuartIn(p));
    }

    /// <summary>
    /// Quartic ease-in-out function.
    /// </summary>
    public static float QuartInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (QuartIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (QuartIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Quintic ease-in function.
    /// </summary>
    public static float QuintIn(float p)
    {
        return p * p * p * p * p;
    }

    /// <summary>
    /// Quintic ease-out function.
    /// </summary>
    public static float QuintOut(float p)
    {
        p = 1f - p;
        return 1f - (QuintIn(p));
    }

    /// <summary>
    /// Quintic ease-in-out function.
    /// </summary>
    public static float QuintInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (QuintIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (QuintIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Cubic ease-in function.
    /// </summary>
    public static float CubicIn(float p)
    {
        return p * p * p;
    }

    /// <summary>
    /// Cubic ease-out function.
    /// </summary>
    public static float CubicOut(float p)
    {
        p = 1f - p;
        return 1f - (CubicIn(p));
    }

    /// <summary>
    /// Cubic ease-in-out function.
    /// </summary>
    public static float CubicInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (CubicIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (CubicIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Elastic ease-in function.
    /// </summary>
    public static float ElasticIn(float p)
    {
        return -(MathF.Pow(2, 10 * (p - 1f)) * MathF.Sin((p - 1.075f) * (MathF.PI * 2) / 0.3f));
    }

    /// <summary>
    /// Elastic ease-out function.
    /// </summary>
    public static float ElasticOut(float p)
    {
        p = 1f - p;
        return 1f - (ElasticIn(p));
    }

    /// <summary>
    /// Elastic ease-in-out function.
    /// </summary>
    public static float ElasticInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (ElasticIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (ElasticIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Back ease-in function (overshoots slightly at the start).
    /// </summary>
    public static float BackIn(float p)
    {
        return p * p * (2.7f * p - 1.7f);
    }

    /// <summary>
    /// Back ease-out function (overshoots slightly at the end).
    /// </summary>
    public static float BackOut(float p)
    {
        p = 1f - p;
        return 1f - (BackIn(p));
    }

    /// <summary>
    /// Back ease-in-out function (overshoots at both ends).
    /// </summary>
    public static float BackInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (BackIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (BackIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Sinusoidal ease-in function.
    /// </summary>
    public static float SineIn(float p)
    {
        return -MathF.Cos(p * (MathF.PI * 0.5f)) + 1f;
    }

    /// <summary>
    /// Sinusoidal ease-out function.
    /// </summary>
    public static float SineOut(float p)
    {
        p = 1f - p;
        return 1f - (SineIn(p));
    }

    /// <summary>
    /// Sinusoidal ease-in-out function.
    /// </summary>
    public static float SineInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (SineIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (SineIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Exponential ease-in function.
    /// </summary>
    public static float ExpoIn(float p)
    {
        return MathF.Pow(2, (10 * (p - 1f)));
    }

    /// <summary>
    /// Exponential ease-out function.
    /// </summary>
    public static float ExpoOut(float p)
    {
        p = 1f - p;
        return 1f - (ExpoIn(p));
    }

    /// <summary>
    /// Exponential ease-in-out function.
    /// </summary>
    public static float ExpoInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (ExpoIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (ExpoIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Quadratic ease-in function.
    /// </summary>
    public static float QuadIn(float p)
    {
        return p * p;
    }

    /// <summary>
    /// Quadratic ease-out function.
    /// </summary>
    public static float QuadOut(float p)
    {
        p = 1f - p;
        return 1f - (QuadIn(p));
    }

    /// <summary>
    /// Quadratic ease-in-out function.
    /// </summary>
    public static float QuadInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (QuadIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (QuadIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Circular ease-in function.
    /// </summary>
    public static float CircIn(float p)
    {
        return -(MathF.Sqrt(1f - (p * p)) - 1f);
    }

    /// <summary>
    /// Circular ease-out function.
    /// </summary>
    public static float CircOut(float p)
    {
        p = 1f - p;
        return 1f - (CircIn(p));
    }

    /// <summary>
    /// Circular ease-in-out function.
    /// </summary>
    public static float CircInOut(float p)
    {
        p = p * 2;
        if (p < 1f)
        {
            return 0.5f * (CircIn(p));
        }
        else
        {
            p = 2f - p;
            return 0.5f * (1f - (CircIn(p))) + 0.5f;
        }
    }

    /// <summary>
    /// Bounce ease-in function.
    /// </summary>
    public static float BounceIn(float p)
    {
        return 1f - BounceOut(1f - p);
    }

    /// <summary>
    /// Bounce ease-out function.
    /// </summary>
    public static float BounceOut(float p)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (p < 1f / d1)
        {
            return n1 * p * p;
        }
        else if (p < 2f / d1)
        {
            return n1 * (p -= 1.5f / d1) * p + 0.75f;
        }
        else if (p < 2.5f / d1)
        {
            return n1 * (p -= 2.25f / d1) * p + 0.9375f;
        }
        else
        {
            return n1 * (p -= 2.625f / d1) * p + 0.984375f;
        }
    }

    /// <summary>
    /// Bounce ease-in-out function.
    /// </summary>
    public static float BounceInOut(float p)
    {
        return p < 0.5f
            ? (1f - BounceOut(1f - 2f * p)) / 2f
            : (1f + BounceOut(2f * p - 1f)) / 2f;
    }

}