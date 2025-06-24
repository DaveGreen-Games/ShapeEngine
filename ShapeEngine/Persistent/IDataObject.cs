namespace ShapeEngine.Persistent;

/// <summary>
/// Represents an object that can be identified by a name.
/// </summary>
public interface IDataObject
{
    /// <summary>
    /// Gets the name of the data object.
    /// </summary>
    /// <returns>The name that identifies this data object.</returns>
    public string GetName();
}