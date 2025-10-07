namespace ShapeEngine.Serialization;

/// <summary>
/// Interface for objects that can be serialized to and deserialized from CSV format.
/// </summary>
public interface ICsvSerializable
{
    /// <summary>
    /// Serializes the object to a CSV string.
    /// All values should be in a comma-separated list like: value1,value2,value3.
    /// </summary>
    /// <returns>A CSV-formatted string representing the object.</returns>
    /// <example>
    /// <code>
    /// public string ToCsv()
    /// {
    ///    return $"{Property1},{Property2},{Property3}";
    /// }
    /// public void FromCsv(string[] values)
    /// {
    ///     Name = values[0];
    ///     Age = int.TryParse(values[1], out int age) ? age : 0;
    ///     IsAlive = bool.TryParse(values[2], out bool alive) && alive;
    /// }
    /// </code>
    /// </example>
    string ToCsv();

    /// <summary>
    /// Populates the object from an array of CSV values.
    /// </summary>
    /// <param name="values">The CSV values to deserialize from.</param>
    void FromCsv(string[] values);
}