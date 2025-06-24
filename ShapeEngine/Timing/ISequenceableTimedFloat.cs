namespace ShapeEngine.Timing;

/// <summary>
/// Defines an interface for a sequenceable timed float value,
/// which can apply its value to a given total.
/// </summary>
public interface ISequenceableTimedFloat : ISequenceable
{
    /// <summary>
    /// Applies the stored float value to the provided total.
    /// </summary>
    /// <param name="total">The current total value to which the stored value will be applied.</param>
    /// <returns>The result of applying the stored value to the total.</returns>
    public float ApplyValue(float total);
}