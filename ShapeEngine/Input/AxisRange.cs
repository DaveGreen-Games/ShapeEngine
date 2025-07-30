namespace ShapeEngine.Input;

/// <summary>
/// Represents the minimum and maximum values for a gamepad axis, used for calibration.
/// </summary>
public readonly struct AxisRange
{
    /// <summary>
    /// The minimum value recorded for the axis.
    /// </summary>
    public readonly float Minimum;
    /// <summary>
    /// The maximum value recorded for the axis.
    /// </summary>
    public readonly float Maximum;

    /// <summary>
    /// The total range between minimum and maximum.
    /// </summary>
    public float TotalRange => MathF.Abs(Maximum - Minimum);

    /// <summary>
    /// Initializes a new instance of the <see cref="AxisRange"/> struct.
    /// </summary>
    public AxisRange(float minimum, float maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Updates the axis range with a new value.
    /// </summary>
    public AxisRange UpdateRange(float newValue)
    {
        if (newValue < Minimum) return new(newValue, Maximum);
        if (newValue > Maximum) return new(Minimum, newValue);
        return new(Minimum, Maximum);
    }
}