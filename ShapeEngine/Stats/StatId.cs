namespace ShapeEngine.Stats;

/// <summary>
/// Represents the stable identifier of a stat.
/// </summary>
/// <remarks>
/// Stat ids are the primary way modifiers target stats. Games can define ids with constants, enums cast to uint,
/// or generated values from their own content pipeline.
/// </remarks>
public readonly struct StatId : IEquatable<StatId>
{
    /// <summary>
    /// The raw unsigned integer value of the id.
    /// </summary>
    public readonly uint Value;

    /// <summary>
    /// Creates a new stat id.
    /// </summary>
    /// <param name="value">The raw unsigned integer value.</param>
    public StatId(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Determines whether this id is equal to another id.
    /// </summary>
    /// <param name="other">The other id.</param>
    /// <returns>True if both ids contain the same raw value.</returns>
    public bool Equals(StatId other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is StatId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Converts an unsigned integer to a stat id.
    /// </summary>
    /// <param name="value">The raw id value.</param>
    /// <returns>A stat id with the supplied value.</returns>
    public static implicit operator StatId(uint value) => new(value);

    /// <summary>
    /// Converts a stat id to its raw unsigned integer value.
    /// </summary>
    /// <param name="id">The stat id.</param>
    /// <returns>The raw id value.</returns>
    public static implicit operator uint(StatId id) => id.Value;

    /// <summary>
    /// Determines whether two stat ids are equal.
    /// </summary>
    public static bool operator ==(StatId left, StatId right) => left.Equals(right);

    /// <summary>
    /// Determines whether two stat ids are not equal.
    /// </summary>
    public static bool operator !=(StatId left, StatId right) => !left.Equals(right);
}
