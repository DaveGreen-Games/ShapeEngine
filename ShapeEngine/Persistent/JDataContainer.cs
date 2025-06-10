using ShapeEngine.Random;

namespace ShapeEngine.Persistent;

/// <summary>
/// A container class that stores and manages a collection of IDataObject instances by their names.
/// </summary>
public class JDataContainer : IDataContainer
{
    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The dictionary storing data objects by their names.
    /// </summary>
    protected Dictionary<string, IDataObject> data = new();

    /// <summary>
    /// Initializes a new empty instance of the JDataContainer class.
    /// </summary>
    public JDataContainer() { }

    /// <summary>
    /// Initializes a new instance of the JDataContainer class with the specified data objects.
    /// </summary>
    /// <param name="data">The data objects to add to the container. Objects with duplicate names will be ignored.</param>
    public JDataContainer(params IDataObject[] data)
    {
        foreach (var entry in data)
        {
            if (this.data.ContainsKey(entry.GetName())) continue;
            this.data.Add(entry.GetName(), entry);
        }
    }

    /// <summary>
    /// Initializes a new instance of the JDataContainer class with the specified list of data objects.
    /// </summary>
    /// <param name="data">The list of data objects to add to the container. Objects with duplicate names will be ignored.</param>
    public JDataContainer(List<IDataObject> data)
    {
        foreach (var entry in data)
        {
            if (this.data.ContainsKey(entry.GetName())) continue;
            this.data.Add(entry.GetName(), entry);
        }
    }

    /// <summary>
    /// Gets the name of the container.
    /// </summary>
    /// <returns>The name of the container.</returns>
    public string GetName() { return Name; }
    
    /// <summary>
    /// Gets a random entry from the container.
    /// </summary>
    /// <returns>A random data object, or null if the container is empty.</returns>
    public IDataObject? GetRandomEntry()
    {
        if (data.Count <= 0) return null;
        int randIndex = Rng.Instance.RandI(data.Count);
        return data.ElementAt(randIndex).Value;
    }

    /// <summary>
    /// Gets a random entry from the container cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the entry to.</typeparam>
    /// <returns>A random data object cast to the specified type, or default(T) if the container is empty or the cast fails.</returns>
    public T? GetRandomEntry<T>()
    {
        if (data.Count <= 0) return default(T);
        int randIndex = Rng.Instance.RandI(data.Count);
        if (data.ElementAt(randIndex).Value is T t) return t;
        return default(T);
    }

    /// <summary>
    /// Gets a specific entry from the container by name.
    /// </summary>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <returns>The data object with the specified name, or null if not found.</returns>
    public IDataObject? GetEntry(string name)
    {
        return data.GetValueOrDefault(name);
    }

    /// <summary>
    /// Gets a specific entry from the container by name and cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the entry to.</typeparam>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <returns>The data object with the specified name cast to the specified type, or default(T) if not found or the cast fails.</returns>
    public T? GetEntry<T>(string name)
    {
        if (data.TryGetValue(name, out var value))
        {
            if (value is T t) return t;
        }
        return default(T);
    }

    /// <summary>
    /// Gets all data objects in the container.
    /// </summary>
    /// <returns>A list of all data objects in the container.</returns>
    public List<IDataObject> GetData() { return data.Values.ToList(); }

    /// <summary>
    /// Gets all data objects in the container cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the data objects to.</typeparam>
    /// <returns>A list of all data objects cast to the specified type.</returns>
    public List<T> GetData<T>() { return data.Values.Cast<T>().ToList(); }
    
}