using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents the settings for an input action, including hold duration, multi-tap duration, and multi-tap target.
/// </summary>
public readonly struct InputActionSettings : IEquatable<InputActionSettings>
{
    /// <summary>
    /// The duration required to trigger a hold action (in seconds).
    /// Default is 1 second.
    /// </summary>
    public readonly float HoldDuration;
    /// <summary>
    /// The duration allowed between taps for a multi-tap action (in seconds).
    /// Default is 0.25 seconds.
    /// </summary>
    public readonly float MultiTapDuration;
    /// <summary>
    /// The number of taps required for a multi-tap action.
    /// </summary>
    public readonly int MultiTapTarget;

    /// <summary>
    /// How fast an axis moves towards the max value (1 / -1) in seconds.
    /// Used for calculating InputState.Axis values.
    /// </summary>
    public readonly float AxisSensitivity;
    
    /// <summary>
    /// How fast an axis moves towards 0 after no input is detected (in seconds).
    /// Used for calculating InputState.Axis values.
    /// </summary>
    public readonly float AxisGravitiy;
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputActionSettings"/> with default values.
    /// HoldDuration is set to 1 second, MultiTapDuration to 0.25 seconds, MultiTapTarget to 0, AxisGravity to 1, and AxisSensitivity to 1.
    /// </summary>
    public InputActionSettings()
    {
        HoldDuration = 1f;
        MultiTapDuration = 0.25f;
        MultiTapTarget = 0;
        AxisGravitiy = 1f;
        AxisSensitivity = 1f;
    }

    /// <summary>
    /// Creates an <see cref="InputActionSettings"/> instance with default values.
    /// </summary>
    public static InputActionSettings CreateDefault() => new();

    /// <summary>
    /// Creates an <see cref="InputActionSettings"/> instance configured for hold actions.
    /// </summary>
    /// <param name="holdDuration">The duration required to trigger a hold action (in seconds).</param>
    public static InputActionSettings CreateHold(float holdDuration) => new(holdDuration);

    /// <summary>
    /// Creates an <see cref="InputActionSettings"/> instance configured for multi-tap actions.
    /// </summary>
    /// <param name="tapDuration">The duration allowed between taps (in seconds).</param>
    /// <param name="multiTapTarget">The number of taps required for a multi-tap action.</param>
    public static InputActionSettings CreateMultiTap(float tapDuration, int multiTapTarget) => new(tapDuration, multiTapTarget);

    /// <summary>
    /// Creates an <see cref="InputActionSettings"/> instance configured for axis input.
    /// </summary>
    /// <param name="axisSensitivity">How fast an axis moves towards the max value (in seconds).</param>
    /// <param name="axisGravitiy">How fast an axis moves towards 0 after no input is detected (in seconds).</param>
    public static InputActionSettings CreateAxis(float axisSensitivity, float axisGravitiy) => new(1f, 0.25f, 0, axisSensitivity, axisGravitiy);

    /// <summary>
    /// Creates an <see cref="InputActionSettings"/> instance configured for both hold and multi-tap actions.
    /// </summary>
    /// <param name="holdDuration">The duration required to trigger a hold action (in seconds).</param>
    /// <param name="tapDuration">The duration allowed between taps (in seconds).</param>
    /// <param name="multiTapTarget">The number of taps required for a multi-tap action.</param>
    public static InputActionSettings CreateHoldAndMultiTap(float holdDuration, float tapDuration, int multiTapTarget) => new(holdDuration, tapDuration, multiTapTarget);

    /// <summary>
    /// Initializes a new instance of <see cref="InputActionSettings"/> with custom values.
    /// </summary>
    /// <param name="holdDuration">The duration required to trigger a hold action (in seconds).</param>
    /// <param name="tapDuration">The duration allowed between taps for a multi-tap action (in seconds).</param>
    /// <param name="multiTapTarget">The number of taps required for a multi-tap action.</param>
    /// <param name="axisSensitivity">How fast an axis moves towards the max value (in seconds).</param>
    /// <param name="axisGravitiy">How fast an axis moves towards 0 after no input is detected (in seconds).</param>
    public InputActionSettings(float holdDuration, float tapDuration, int multiTapTarget, float axisSensitivity, float axisGravitiy)
    {
        HoldDuration = MathF.Max(0f, holdDuration);
        MultiTapDuration = MathF.Max(0f, tapDuration);
        MultiTapTarget = ShapeMath.MaxInt(0, multiTapTarget);
        AxisSensitivity = MathF.Max(0f, axisSensitivity);
        AxisGravitiy = MathF.Max(0f, axisGravitiy);
    }
    private InputActionSettings(float holdDuration, float tapDuration, int multiTapTarget)
    {
        HoldDuration = MathF.Max(0f, holdDuration);
        MultiTapDuration = MathF.Max(0f, tapDuration);
        MultiTapTarget = ShapeMath.MaxInt(0, multiTapTarget);
    }
    private InputActionSettings(float tapDuration, int multiTapTarget)
    {
        HoldDuration = 0f;
        MultiTapDuration = MathF.Max(0f, tapDuration);
        MultiTapTarget = ShapeMath.MaxInt(0, multiTapTarget);
    }
    private InputActionSettings(float holdDuration)
    {
        HoldDuration = MathF.Max(0f, holdDuration);
        MultiTapDuration = 0.25f;
        MultiTapTarget = 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="InputActionSettings"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="InputActionSettings"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(InputActionSettings other)
    {
        return 
            HoldDuration.Equals(other.HoldDuration) && 
            MultiTapDuration.Equals(other.MultiTapDuration) && 
            MultiTapTarget == other.MultiTapTarget && 
            AxisSensitivity.Equals(other.AxisSensitivity) && 
            AxisGravitiy.Equals(other.AxisGravitiy);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="InputActionSettings"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is InputActionSettings other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="InputActionSettings"/>.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HoldDuration, MultiTapDuration, MultiTapTarget, AxisSensitivity, AxisGravitiy);
    }
}