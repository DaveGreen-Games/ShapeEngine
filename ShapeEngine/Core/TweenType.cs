namespace ShapeEngine.Core;

/// <summary>
/// Specifies the type of tweening (easing) function to use for interpolation.
/// </summary>
public enum TweenType
{
    /// <summary>Linear interpolation (no easing).</summary>
    LINEAR = 0,
    /// <summary>Sinusoidal ease-in.</summary>
    SINE_IN = 4,
    /// <summary>Sinusoidal ease-out.</summary>
    SINE_OUT = 5,
    /// <summary>Sinusoidal ease-in-out.</summary>
    SINE_INOUT = 6,
    /// <summary>Circular ease-in.</summary>
    CIRC_IN = 7,
    /// <summary>Circular ease-out.</summary>
    CIRC_OUT = 8,
    /// <summary>Circular ease-in-out.</summary>
    CIRC_INOUT = 9,
    /// <summary>Cubic ease-in.</summary>
    CUBIC_IN = 10,
    /// <summary>Cubic ease-out.</summary>
    CUBIC_OUT = 11,
    /// <summary>Cubic ease-in-out.</summary>
    CUBIC_INOUT = 12,
    /// <summary>Quadratic ease-in.</summary>
    QUAD_IN = 13,
    /// <summary>Quadratic ease-out.</summary>
    QUAD_OUT = 14,
    /// <summary>Quadratic ease-in-out.</summary>
    QUAD_INOUT = 15,
    /// <summary>Exponential ease-in.</summary>
    EXPO_IN = 16,
    /// <summary>Exponential ease-out.</summary>
    EXPO_OUT = 17,
    /// <summary>Exponential ease-in-out.</summary>
    EXPO_INOUT = 18,
    /// <summary>Back ease-in (overshoots slightly at the start).</summary>
    BACK_IN = 19,
    /// <summary>Back ease-out (overshoots slightly at the end).</summary>
    BACK_OUT = 20,
    /// <summary>Back ease-in-out (overshoots at both ends).</summary>
    BACK_INOUT = 21,
    /// <summary>Bounce ease-in.</summary>
    BOUNCE_IN = 22,
    /// <summary>Bounce ease-out.</summary>
    BOUNCE_OUT = 23,
    /// <summary>Bounce ease-in-out.</summary>
    BOUNCE_INOUT = 24,
    /// <summary>Elastic ease-in.</summary>
    ELASTIC_IN = 25,
    /// <summary>Elastic ease-out.</summary>
    ELASTIC_OUT = 26,
    /// <summary>Elastic ease-in-out.</summary>
    ELASTIC_INOUT = 27
}