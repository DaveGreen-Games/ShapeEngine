using ShapeEngine.Random;

namespace ShapeEngine.Serialization;

/// <summary>
/// Represents a dictionary of <typeparamref name="T"/> objects, providing advanced random selection and filtering capabilities.
/// </summary>
/// <typeparam name="T">The type of objects stored in the dictionary. Must inherit from <see cref="DataObject"/> and typically provides a <c>SpawnWeight</c> property for weighted random selection.</typeparam>
/// <remarks>
/// This class extends <see cref="Dictionary{string, T}"/> and adds methods for random selection, weighted selection, and type-based filtering.
/// Useful for scenarios such as loot tables, spawn systems, or any context where random or weighted selection of objects is required.
/// </remarks>
public class DataObjectDict<T> : Dictionary<string, T> where T : DataObject
{
    #region Random
    /// <summary>
    /// Returns a random entry from the dictionary.
    /// </summary>
    /// <returns>A random <typeparamref name="T"/> entry, or <c>null</c> if the dictionary is empty.</returns>
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this.ElementAt(index).Value;
    }
    /// <summary>
    /// Returns a random entry of type <typeparamref name="TU"/> from the dictionary.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <returns>A random <typeparamref name="TU"/> entry, or <c>null</c> if none exist.</returns>
    public TU? GetRandomEntry<TU>() where TU : T
    {
        if (Count == 0) return null;
        var entries = Values.OfType<TU>().ToList();
        if (entries.Count == 0) return null;
        int index = Rng.Instance.RandI(0, entries.Count);
        return entries[index];
    }
    /// <summary>
    /// Returns a list of random entries from the dictionary.
    /// </summary>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing random entries, or <c>null</c> if the dictionary is empty or <paramref name="amount"/> is non-positive.</returns>
    public DataObjectList<T>? GetRandomEntries(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<T>();
        for (int i = 0; i < amount; i++)
        {
            int index = Rng.Instance.RandI(0, Count);
            result.Add(this.ElementAt(index).Value);
        }
        return result;
    }
    /// <summary>
    /// Returns a list of random entries of type <typeparamref name="TU"/> from the dictionary.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing random entries, or <c>null</c> if none exist or <paramref name="amount"/> is non-positive.</returns>
    public DataObjectList<TU>? GetRandomEntries<TU>(int amount) where TU : T
    {
        var entries = Values.OfType<TU>().ToList();
        if (entries.Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<TU>();
        for (int i = 0; i < amount; i++)
        {
            int index = Rng.Instance.RandI(0, entries.Count);
            result.Add(entries[index]);
        }
        return result;
    }
    /// <summary>
    /// Returns a list of unique random entries from the dictionary.
    /// </summary>
    /// <param name="amount">The number of unique entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing unique random entries, or <c>null</c> if the dictionary is empty or <paramref name="amount"/> is non-positive.</returns>
    public DataObjectList<T>? GetRandomEntriesUnique(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<T>();
        var indices = new HashSet<int>();
        while (indices.Count < Math.Min(amount, Count))
        {
            int index = Rng.Instance.RandI(0, Count);
            if (indices.Add(index))
            {
                result.Add(this.ElementAt(index).Value);
            }
        }
        return result;
    }
    /// <summary>
    /// Returns a list of unique random entries of type <typeparamref name="TU"/> from the dictionary.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <param name="amount">The number of unique entries to select.</param>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing unique random entries, or <c>null</c> if none exist or <paramref name="amount"/> is non-positive.</returns>
    public DataObjectList<TU>? GetRandomEntriesUnique<TU>(int amount) where TU : T
    {
        var entries = Values.OfType<TU>().ToList();
        if (entries.Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<TU>();
        var indices = new HashSet<int>();
        while (indices.Count < Math.Min(amount, entries.Count))
        {
            int index = Rng.Instance.RandI(0, entries.Count);
            if (indices.Add(index))
            {
                result.Add(entries[index]);
            }
        }
        return result;
    }
    /// <summary>
    /// Returns a random entry from the dictionary, selected based on the <c>SpawnWeight</c> property.
    /// </summary>
    /// <returns>A weighted random <typeparamref name="T"/> entry, or <c>null</c> if the dictionary is empty or all weights are zero.</returns>
    /// <remarks>
    /// Entries with higher <c>SpawnWeight</c> are more likely to be selected.
    /// </remarks>
    public T? PickRandomEntry()
    {
        if (Count == 0) return null;
        var entries = Values.ToList();
        int totalWeight = entries.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        int rand = Rng.Instance.RandI(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in entries)
        {
            cumulative += entry.SpawnWeight;
            if (rand < cumulative)
                return entry;
        }
        return entries.Last();
    }
    /// <summary>
    /// Returns a list of entries selected randomly based on their <c>SpawnWeight</c> property.
    /// </summary>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing weighted random entries, or <c>null</c> if the dictionary is empty or all weights are zero.</returns>
    /// <remarks>
    /// Each selection is independent and may select the same entry multiple times.
    /// </remarks>
    public DataObjectList<T>? PickRandomEntries(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        var entries = Values.ToList();
        int totalWeight = entries.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        var result = new DataObjectList<T>();
        for (int i = 0; i < amount; i++)
        {
            int rand = Rng.Instance.RandI(0, totalWeight);
            int cumulative = 0;
            foreach (var entry in entries)
            {
                cumulative += entry.SpawnWeight;
                if (rand < cumulative)
                {
                    result.Add(entry);
                    break;
                }
            }
        }
        return result;
    }
    /// <summary>
    /// Returns a list of entries selected randomly using a <see cref="ChanceList{T}"/> based on their <c>SpawnWeight</c> property.
    /// </summary>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing weighted random entries, or <c>null</c> if the dictionary is empty or all weights are zero.</returns>
    /// <remarks>
    /// Uses a <see cref="ChanceList{T}"/> for selection, which may provide more advanced randomization features.
    /// </remarks>
    public DataObjectList<T>? PickRandomEntriesChanceList(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        var chanceList = GenerateChanceList();
        if (chanceList.Count == 0) return null;
        var result = new DataObjectList<T>();
        for (int i = 0; i < amount; i++)
        {
            result.Add(chanceList.Next());
        }
        return result;
    }
    /// <summary>
    /// Returns a list of entries of type <typeparamref name="TU"/> selected randomly using a <see cref="ChanceList{TU}"/> based on their <c>SpawnWeight</c> property.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing weighted random entries, or <c>null</c> if none exist or all weights are zero.</returns>
    /// <remarks>
    /// Uses a <see cref="ChanceList{TU}"/> for selection, which may provide more advanced randomization features.
    /// </remarks>
    public DataObjectList<TU>? PickRandomEntriesChanceList<TU>(int amount) where TU : T
    {
        if (Count == 0 || amount <= 0) return null;
        var chanceList = GenerateChanceList<TU>();
        if (chanceList.Count == 0) return null;
        var result = new DataObjectList<TU>();
        for (int i = 0; i < amount; i++)
        {
            result.Add(chanceList.Next());
        }
        return result;
    }
    /// <summary>
    /// Generates a <see cref="ChanceList{T}"/> for weighted random selection based on <c>SpawnWeight</c>.
    /// </summary>
    /// <returns>A <see cref="ChanceList{T}"/> containing entries with positive <c>SpawnWeight</c>.</returns>
    public ChanceList<T> GenerateChanceList()
    {
        var entries = new List<(int amount, T value)>();
        foreach (var entry in Values)
        {
            if (entry.SpawnWeight > 0)
            {
                entries.Add((entry.SpawnWeight, entry));
            }
        }
        return new ChanceList<T>(entries);
    }
    /// <summary>
    /// Returns a random entry of type <typeparamref name="TU"/> selected based on <c>SpawnWeight</c>.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <returns>A weighted random <typeparamref name="TU"/> entry, or <c>null</c> if none exist or all weights are zero.</returns>
    /// <remarks>
    /// Entries with higher <c>SpawnWeight</c> are more likely to be selected.
    /// </remarks>
    public TU? PickRandomEntry<TU>() where TU : T
    {
        if (Count == 0) return null;
        var entries = Values.ToList();
        int totalWeight = entries.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        int rand = Rng.Instance.RandI(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in entries)
        {
            if (entry.SpawnWeight <= 0) continue;
            if (entry is TU castedEntry)
            {
                cumulative += entry.SpawnWeight;
                if (rand < cumulative) return castedEntry;
            }
            
        }
        var last =  entries.Last();
        if(last is TU castedLast) return castedLast;
        return null;
    }
    /// <summary>
    /// Returns a list of entries of type <typeparamref name="TU"/> selected randomly based on their <c>SpawnWeight</c> property.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <param name="amount">The number of entries to select.</param>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing weighted random entries, or <c>null</c> if none exist or all weights are zero.</returns>
    /// <remarks>
    /// Each selection is independent and may select the same entry multiple times.
    /// </remarks>
    public DataObjectList<TU>? PickRandomEntries<TU>(int amount) where TU : T
    {
        if (Count == 0 || amount <= 0) return null;
        var entries = Values.ToList();
        int totalWeight = entries.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        var result = new DataObjectList<TU>();
        for (int i = 0; i < amount; i++)
        {
            int rand = Rng.Instance.RandI(0, totalWeight);
            int cumulative = 0;
            foreach (var entry in entries)
            {
                if (entry.SpawnWeight <= 0) continue;
                if (entry is TU castedEntry)
                {
                    cumulative += entry.SpawnWeight;
                    if (rand < cumulative)
                    {
                        result.Add(castedEntry);
                        break;
                    }
                }
                else 
                {
                    cumulative += entry.SpawnWeight;
                }
            }
        }
        return result;
    }
    /// <summary>
    /// Generates a <see cref="ChanceList{TU}"/> for weighted random selection of entries of type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <returns>A <see cref="ChanceList{TU}"/> containing entries with positive <c>SpawnWeight</c>.</returns>
    public ChanceList<TU> GenerateChanceList<TU>() where TU : T
    {
        var entries = new List<(int amount, TU value)>();
        foreach (var entry in Values)
        {
            if (entry.SpawnWeight <= 0) continue;
            if (entry is TU castedEntry)
            {
                entries.Add((entry.SpawnWeight, castedEntry));
            }
        }
        return new ChanceList<TU>(entries);
    }
    #endregion
    #region Get
    /// <summary>
    /// Gets the entry with the specified name.
    /// </summary>
    /// <param name="name">The key of the entry to retrieve.</param>
    /// <returns>The entry associated with <paramref name="name"/>, or <c>null</c> if not found.</returns>
    public T? GetEntry(string name) => TryGetValue(name, out var value) ? value : null;
    /// <summary>
    /// Gets the entry of type <typeparamref name="TU"/> with the specified name.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to filter for.</typeparam>
    /// <param name="name">The key of the entry to retrieve.</param>
    /// <returns>The entry of type <typeparamref name="TU"/> associated with <paramref name="name"/>, or <c>null</c> if not found or not of the correct type.</returns>
    public TU? GetEntry<TU>(string name) where TU : T
    {
        if (TryGetValue(name, out var value) && value is TU castedValue)
        {
            return castedValue;
        }
        return null;
    }
    #endregion
    
    #region Add
    /// <summary>
    /// Adds an item to the dictionary.
    /// </summary>
    /// <param name="item">The <typeparamref name="T"/> item to add.</param>
    /// <returns><c>true</c> if the item was added successfully; <c>false</c> if an item with the same key already exists.</returns>
    public bool Add(T item)
    {
        return TryAdd(item.Name, item);
    }
    #endregion
    
    #region Remove
    /// <summary>
    /// Removes an item from the dictionary.
    /// </summary>
    /// <param name="item">The <typeparamref name="T"/> item to remove.</param>
    /// <returns><c>true</c> if the item was removed successfully; <c>false</c> if the item was not found.</returns>
    public bool Remove(T item)
    {
        return base.Remove(item.Name);
    }
    #endregion
    
    #region Conversion
    /// <summary>
    /// Converts the dictionary values to a <see cref="DataObjectList{T}"/>.
    /// </summary>
    /// <returns>A <see cref="DataObjectList{T}"/> containing all values in the dictionary.</returns>
    public DataObjectList<T> ToDataObjectList()
    {
        var list = new DataObjectList<T>();
        foreach (var item in Values)
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Converts the dictionary values to a <see cref="DataObjectList{TU}"/> where TU is the specified subtype of T.
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to convert to.</typeparam>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing all values of type <typeparamref name="TU"/> in the dictionary, or <c>null</c> if none exist.</returns>
    public DataObjectList<TU>? ToDataObjectList<TU>() where TU : T
    {
        DataObjectList<TU>? list = null;
        foreach (var item in Values)
        {
            if (item is not TU castedItem) continue;
            list ??= [];
            list.Add(castedItem);
        }
        return list;
    }
    #endregion
    
    #region Clone
    /// <summary>
    /// Creates a shallow copy of the dictionary.
    /// </summary>
    /// <returns>A new <see cref="DataObjectDict{T}"/> containing the same key-value pairs as this dictionary.</returns>
    public DataObjectDict<T> Clone()
    {
        var newDict = new DataObjectDict<T>();
        foreach (var (key, value) in this)
        {
            newDict[key] = value; // Shallow copy
        }
        return newDict;
    }
    #endregion
    
    #region Casting
    
    /// <summary>
    /// Casts the dictionary to a new dictionary of type <typeparamref name="TU"/>. 
    /// </summary>
    /// <typeparam name="TU">The subtype of <typeparamref name="T"/> to cast to.</typeparam>
    /// <returns>A new <see cref="DataObjectDict{TU}"/> containing the entries of this dictionary that are of type <typeparamref name="TU"/>, or <c>null</c> if none exist.</returns>
    public DataObjectDict<TU>? Cast<TU>() where TU : T
    {
        DataObjectDict<TU>? newDict = null;
        foreach (var (key, value) in this)
        {
            if (value is not TU castedValue) continue;
            newDict ??= new DataObjectDict<TU>();
            newDict[key] = castedValue;
        }
        return newDict;
    }
    
    #endregion
}