namespace ShapeEngine.Input;

/// <summary>
/// Represents settings for input actions, including axis sensitivity and gravity.
/// Implements <see cref="IEquatable{InputActionSettings}"/> for value equality.
/// </summary>
public readonly struct InputActionSettings : IEquatable<InputActionSettings>
{
    /// <summary>
    /// How fast an axis moves towards the max value (1 / -1) in seconds.
    /// Used for calculating InputState.Axis values.
    /// </summary>
    public readonly float AxisSensitivity;
    
    /// <summary>
    /// How fast an axis moves towards 0 after no input is detected (in seconds).
    /// Used for calculating InputState.Axis values.
    /// </summary>
    public readonly float AxisGravity;
    
    /// <summary>
    /// Initializes a new instance of <c>InputActionSettings</c> with default values.
    /// <see cref="AxisGravity"/> and <see cref="AxisSensitivity"/> are both set to 1.
    /// </summary>
    public InputActionSettings()
    {
        AxisGravity = 1f;
        AxisSensitivity = 1f;
    }

    /// <summary>
    /// Initializes a new instance of <c>InputActionSettings</c> with specified sensitivity and gravity.
    /// Values are clamped to a minimum of 0.
    /// </summary>
    /// <param name="axisSensitivity">How fast the axis moves towards the max value (1 / -1) in seconds.</param>
    /// <param name="axisGravity">How fast the axis moves towards 0 after no input is detected (in seconds).</param>
    public InputActionSettings(float axisSensitivity, float axisGravity)
    {
        AxisSensitivity = MathF.Max(0f, axisSensitivity);
        AxisGravity = MathF.Max(0f, axisGravity);
    }
    
    /// <summary>
    /// Creates a new <c>InputActionSettings</c> instance with the specified axis sensitivity and gravity.
    /// </summary>
    /// <param name="axisSensitivity">How fast the axis moves towards the max value (1 / -1) in seconds.</param>
    /// <param name="axisGravity">How fast the axis moves towards 0 after no input is detected (in seconds).</param>
    /// <returns>A new <c>InputActionSettings</c> instance.</returns>
    public static InputActionSettings Axis(float axisSensitivity, float axisGravity) => new(axisSensitivity, axisGravity);
    
    
    /// <summary>
    /// Determines whether the specified <c>InputActionSettings</c> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <c>InputActionSettings</c> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified instance is equal; otherwise, <c>false</c>.</returns>
    public bool Equals(InputActionSettings other)
    {
        return 
            AxisSensitivity.Equals(other.AxisSensitivity) && 
            AxisGravity.Equals(other.AxisGravity);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <c>InputActionSettings</c> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is InputActionSettings other && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current <c>InputActionSettings</c> instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(AxisSensitivity, AxisGravity);
    }

    /// <summary>
    /// Determines whether two <c>InputActionSettings</c> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if both instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(InputActionSettings left, InputActionSettings right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <c>InputActionSettings</c> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if both instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(InputActionSettings left, InputActionSettings right)
    {
        return !(left == right);
    }
}