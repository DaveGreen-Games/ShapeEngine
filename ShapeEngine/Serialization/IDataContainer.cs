namespace ShapeEngine.Serialization;

/// <summary>
/// Represents a container that holds a collection of data objects.
/// </summary>
public interface IDataContainer
{
    /// <summary>
    /// Gets the name of the container.
    /// </summary>
    /// <returns>The name that identifies this container.</returns>
    public string GetName();

    /// <summary>
    /// Gets all data objects in the container.
    /// </summary>
    /// <returns>A list of all data objects in the container.</returns>
    public List<IDataObject> GetData();

    /// <summary>
    /// Gets all data objects in the container cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the data objects to.</typeparam>
    /// <returns>A list of all data objects cast to the specified type.</returns>
    public List<T> GetData<T>() { return GetData().Cast<T>().ToList(); }

    /// <summary>
    /// Gets a random entry from the container.
    /// </summary>
    /// <returns>A random data object, or null if the container is empty.</returns>
    public IDataObject? GetRandomEntry();

    /// <summary>
    /// Gets a random entry from the container cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the entry to.</typeparam>
    /// <returns>A random data object cast to the specified type, or default(T) if the container is empty or the cast fails.</returns>
    public T? GetRandomEntry<T>();

    /// <summary>
    /// Gets a specific entry from the container by name.
    /// </summary>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <returns>The data object with the specified name, or null if not found.</returns>
    public IDataObject? GetEntry(string name);

    /// <summary>
    /// Gets a specific entry from the container by name and cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the entry to.</typeparam>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <returns>The data object with the specified name cast to the specified type, or default(T) if not found or the cast fails.</returns>
    public T? GetEntry<T>(string name);
}