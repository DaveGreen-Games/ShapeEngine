using ShapeEngine.Random;

namespace ShapeEngine.Serialization;


/// <summary>
/// Represents a strongly-typed list of <typeparamref name="T"/> objects, where <typeparamref name="T"/> is a subclass of <see cref="DataObject"/>.
/// Provides methods for retrieving entries by name and for selecting random entries, including type-specific and unique selections.
/// </summary>
/// <typeparam name="T">The type of objects in the list. Must inherit from <see cref="DataObject"/>. Each <typeparamref name="T"/> must have a unique <c>Name</c> property for retrieval.</typeparam>
/// <remarks>
/// This class extends <see cref="List{T}"/> and adds convenience methods for common data object operations, such as random selection and type filtering.
/// </remarks>
public class DataObjectList<T> : List<T> where T : DataObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataObjectList{T}"/> class.
    /// </summary>
    public DataObjectList(){}
    /// <summary>
    /// Initializes a new instance of the <see cref="DataObjectList{T}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the list can initially store.</param>
    public DataObjectList(int capacity) : base(capacity){}
    
    
    #region Get
    /// <summary>
    /// Retrieves the first entry with the specified name.
    /// </summary>
    /// <param name="name">The name of the entry to retrieve. This should match the <c>Name</c> property of the <typeparamref name="T"/> object.</param>
    /// <returns>The entry with the specified name, or <c>null</c> if not found.</returns>
    public T? GetEntry(string name) => Find(item => item.Name == name);

    /// <summary>
    /// Retrieves the first entry with the specified name and casts it to the specified type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to cast the entry to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <returns>The entry cast to <typeparamref name="TU"/>, or <c>null</c> if not found or the cast fails.</returns>
    public TU? GetEntry<TU>(string name) where TU : T => GetEntry(name) as TU;

    #endregion
    
    #region Random
    /// <summary>
    /// Retrieves a random entry from the list.
    /// </summary>
    /// <returns>A random entry, or <c>null</c> if the list is empty.</returns>
    /// <remarks>
    /// Uses <see cref="Rng.Instance"/> for random selection.
    /// </remarks>
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this[index];
    }
    /// <summary>
    /// Retrieves a random entry from the list and casts it to the specified type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to cast the entry to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A random entry cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty or the cast fails.</returns>
    /// <remarks>
    /// Uses <see cref="Rng.Instance"/> for random selection.
    /// </remarks>
    public TU? GetRandomEntry<TU>() where TU : T
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        var entry = this[index];
        return entry is TU castedEntry ? castedEntry : null;
    }
    /// <summary>
    /// Retrieves a list of random entries from the list.
    /// </summary>
    /// <param name="amount">The number of random entries to retrieve.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing the random entries, or <c>null</c> if the list is empty or <paramref name="amount"/> is less than or equal to zero.</returns>
    /// <remarks>
    /// Entries may be repeated.
    /// </remarks>
    public DataObjectList<T>? GetRandomEntries(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<T>();
        for (int i = 0; i < amount; i++)
        {
            int index = Rng.Instance.RandI(0, Count);
            result.Add(this[index]);
        }
        return result;
    }
    /// <summary>
    /// Retrieves a list of random entries from the list, casts them to the specified type <typeparamref name="TU"/>, and returns them.
    /// </summary>
    /// <param name="amount">The number of random entries to retrieve.</param>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing the random entries cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty or <paramref name="amount"/> is less than or equal to zero.</returns>
    /// <remarks>
    /// Entries may be repeated.
    /// </remarks>
    public DataObjectList<TU>? GetRandomEntries<TU>(int amount) where TU : T
    {
        if (Count == 0 || amount <= 0) return null;
        var result = new DataObjectList<TU>();
        for (int i = 0; i < amount; i++)
        {
            int index = Rng.Instance.RandI(0, Count);
            var entry = this[index];
            if (entry is TU castedEntry)
            {
                result.Add(castedEntry);
            }
        }
        return result;
    }

    /// <summary>
    /// Retrieves a list of unique random entries from the list.
    /// </summary>
    /// <param name="amount">The number of unique random entries to retrieve.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing the unique random entries, or <c>null</c> if the list is empty, <paramref name="amount"/> is less than or equal to zero, or if unique entries cannot be guaranteed.</returns>
    /// <remarks>
    /// Uses a hash set to ensure uniqueness of entries. The operation may fail to find enough unique entries if <paramref name="amount"/> is too large relative to the list size.
    /// </remarks>
    public DataObjectList<T>? GetRandomEntriesUnique(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        int safeGuard = Count * 100;
        var result = new DataObjectList<T>();
        var indices = new HashSet<int>();
        while (indices.Count < Math.Min(amount, Count) && safeGuard > 0)
        {
            safeGuard--;
            
            int index = Rng.Instance.RandI(0, Count);
            if (indices.Add(index))
            {
                result.Add(this[index]);
            }
        }
        return result;
    }
    /// <summary>
    /// Retrieves a list of unique random entries from the list, casts them to the specified type <typeparamref name="TU"/>, and returns them.
    /// </summary>
    /// <param name="amount">The number of unique random entries to retrieve.</param>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing the unique random entries cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty, <paramref name="amount"/> is less than or equal to zero, or if unique entries cannot be guaranteed.</returns>
    /// <remarks>
    /// Uses a hash set to ensure uniqueness of entries. The operation may fail to find enough unique entries if <paramref name="amount"/> is too large relative to the list size.
    /// </remarks>
    public DataObjectList<TU>? GetRandomEntriesUnique<TU>(int amount) where TU : T
    {
        if (Count == 0 || amount <= 0) return null;
        int safeGuard = Count * 100;
        var result = new DataObjectList<TU>();
        var indices = new HashSet<int>();
        while (indices.Count < Math.Min(amount, Count) && safeGuard > 0)
        {
            safeGuard--;
            
            int index = Rng.Instance.RandI(0, Count);
            if (indices.Contains(index)) continue;
            var entry = this[index];
            indices.Add(index); // Add index regardless of type match to avoid infinite loop - if not castable to TU index should not be tried again
            if (entry is not TU castedEntry) continue;
            result.Add(castedEntry);
        }
        return result;
    }

    /// <summary>
    /// Selects a random entry from the list based on the spawn weight of each entry.
    /// </summary>
    /// <returns>A randomly selected entry, or <c>null</c> if the list is empty or all weights are non-positive.</returns>
    /// <remarks>
    /// The selection is weighted by the <c>SpawnWeight</c> property of each entry. Uses <see cref="Rng.Instance"/> for random selection.
    /// </remarks>
    public T? PickRandomEntry()
    {
        if (Count == 0) return null;
        int totalWeight = this.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        int rand = Rng.Instance.RandI(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in this)
        {
            cumulative += entry.SpawnWeight;
            if (rand < cumulative)
                return entry;
        }
        return this.Last();
    }
    /// <summary>
    /// Selects a random entry from the list based on the spawn weight of each entry, and casts it to the specified type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to cast the entry to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A randomly selected entry cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty, all weights are non-positive, or the cast fails.</returns>
    /// <remarks>
    /// The selection is weighted by the <c>SpawnWeight</c> property of each entry. Uses <see cref="Rng.Instance"/> for random selection.
    /// </remarks>
    public TU? PickRandomEntry<TU>() where TU : T
    {
        if (Count == 0) return null;
        int totalWeight = this.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        int rand = Rng.Instance.RandI(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in this)
        {
            if (entry.SpawnWeight <= 0) continue;
            if (entry is TU castedEntry)
            {
                cumulative += entry.SpawnWeight;
                if (rand < cumulative)
                    return castedEntry;
            }
            else
            {
                cumulative += entry.SpawnWeight;
            }
        }
        var last = this.Last();
        return last as TU;
    }

    /// <summary>
    /// Selects a specified number of random entries from the list based on the spawn weight of each entry.
    /// </summary>
    /// <param name="amount">The number of random entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing the randomly selected entries, or <c>null</c> if the list is empty, the amount is less than or equal to zero, or if all weights are non-positive.</returns>
    /// <remarks>
    /// Each selected entry is added to the result list immediately after being selected. The result list may contain duplicate entries.
    /// </remarks>
    public DataObjectList<T>? PickRandomEntries(int amount)
    {
        if (Count == 0 || amount <= 0) return null;
        int totalWeight = this.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        var result = new DataObjectList<T>();
        for (int i = 0; i < amount; i++)
        {
            int rand = Rng.Instance.RandI(0, totalWeight);
            int cumulative = 0;
            foreach (var entry in this)
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
    /// Selects a specified number of random entries from the list based on the spawn weight of each entry, casts them to the specified type <typeparamref name="TU"/>, and returns them.
    /// </summary>
    /// <param name="amount">The number of random entries to select.</param>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing the randomly selected entries cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty, the amount is less than or equal to zero, or if all weights are non-positive.</returns>
    /// <remarks>
    /// Each selected entry is added to the result list immediately after being selected. The result list may contain duplicate entries.
    /// </remarks>
    public DataObjectList<TU>? PickRandomEntries<TU>(int amount) where TU : T
    {
        if (Count == 0 || amount <= 0) return null;
        int totalWeight = this.Sum(e => e.SpawnWeight);
        if (totalWeight <= 0) return null;
        var result = new DataObjectList<TU>();
        for (int i = 0; i < amount; i++)
        {
            int rand = Rng.Instance.RandI(0, totalWeight);
            int cumulative = 0;
            foreach (var entry in this)
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
    /// Selects a specified number of random entries from the list using a generated <see cref="ChanceList{T}"/>.
    /// </summary>
    /// <param name="amount">The number of random entries to select.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing the randomly selected entries, or <c>null</c> if the list is empty, the amount is less than or equal to zero, or if all weights are non-positive.</returns>
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
    /// Selects a specified number of random entries from the list using a generated <see cref="ChanceList{TU}"/>, casts them to the specified type <typeparamref name="TU"/>, and returns them.
    /// </summary>
    /// <param name="amount">The number of random entries to select.</param>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="DataObjectList{TU}"/> containing the randomly selected entries cast to <typeparamref name="TU"/>, or <c>null</c> if the list is empty, the amount is less than or equal to zero, or if all weights are non-positive.</returns>
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
    /// Generates a <see cref="ChanceList{T}"/> from the list, where each entry's chance is proportional to its spawn weight.
    /// </summary>
    /// <returns>A <see cref="ChanceList{T}"/> containing the entries of the list with their respective chances, or an empty <see cref="ChanceList{T}"/> if the list is empty or all weights are non-positive.</returns>
    /// <remarks>
    /// Entries with a spawn weight less than or equal to zero are not included in the chance list.
    /// </remarks>
    public ChanceList<T> GenerateChanceList()
    {
        var entries = new List<(int amount, T value)>();
        foreach (var entry in this)
        {
            if (entry.SpawnWeight > 0)
            {
                entries.Add((entry.SpawnWeight, entry));
            }
        }
        return new ChanceList<T>(entries);
    }
    /// <summary>
    /// Generates a <see cref="ChanceList{TU}"/> from the list, where each entry's chance is proportional to its spawn weight, and casts the entries to the specified type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="ChanceList{TU}"/> containing the entries of the list cast to <typeparamref name="TU"/> with their respective chances, or an empty <see cref="ChanceList{TU}"/> if the list is empty, all weights are non-positive, or the cast fails.</returns>
    /// <remarks>
    /// Entries with a spawn weight less than or equal to zero are not included in the chance list.
    /// </remarks>
    public ChanceList<TU> GenerateChanceList<TU>() where TU : T
    {
        var entries = new List<(int amount, TU value)>();
        foreach (var entry in this)
        {
            if (entry.SpawnWeight <= 0) continue;
            if (entry is TU castedEntry)
            {
                entries.Add((entry.SpawnWeight, castedEntry));
            }
        }
        return new ChanceList<TU>(entries);
    }
    
    /// <summary>
    /// Generates a <see cref="ChanceListIndices"/> from the list, where each entry's chance is proportional to its spawn weight, and provides the indices of the entries.
    /// </summary>
    /// <returns>A <see cref="ChanceListIndices"/> containing the indices of the entries of the list with their respective chances, or an empty <see cref="ChanceListIndices"/> if the list is empty or all weights are non-positive.</returns>
    /// <remarks>
    /// This can be used to randomly select entries by index with the given chances.
    /// </remarks>
    public ChanceListIndices GenerateChanceListIndices()
    {
        var entries = new List<(int amount, int index)>();

        for (var i = 0; i < Count; i++)
        {
            var entry = this[i];
            if (entry.SpawnWeight > 0)
            {
                entries.Add((entry.SpawnWeight, i));
            }
        }
        return new ChanceListIndices(entries);
    }
    #endregion
    
    #region Conversion
    /// <summary>
    /// Converts the list to a <see cref="DataObjectDict{T}"/>, using the <c>Name</c> property of each entry as the key.
    /// </summary>
    /// <returns>A <see cref="DataObjectDict{T}"/> containing all entries of the list, or an empty <see cref="DataObjectDict{T}"/> if the list is empty.</returns>
    public DataObjectDict<T> ToDataObjectDict()
    {
        var dict = new DataObjectDict<T>();
        foreach (var item in this)
        {
            dict.TryAdd(item.Name, item);
        }
        return dict;
    }

    /// <summary>
    /// Converts the list to a <see cref="DataObjectDict{TU}"/>, using the <c>Name</c> property of each entry as the key, and casts the entries to the specified type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to cast the entries to. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A <see cref="DataObjectDict{TU}"/> containing all entries of the list cast to <typeparamref name="TU"/>, or an empty <see cref="DataObjectDict{TU}"/> if the list is empty or the cast fails.</returns>
    public DataObjectDict<TU>? ToDataObjectDict<TU>() where TU : T
    {
        DataObjectDict<TU>? dict = null;
        foreach (var item in this)
        {
            if (item is not TU castedItem) continue;
            dict ??= new DataObjectDict<TU>();
            dict.TryAdd(castedItem.Name, castedItem);
        }
        return dict;
    }
    #endregion
    
    #region Clone
    /// <summary>
    /// Creates a shallow copy of the list.
    /// </summary>
    /// <returns>A new <see cref="DataObjectList{T}"/> containing copies of the entries in the list.</returns>
    public DataObjectList<T> Clone()
    {
        var newList = new DataObjectList<T>();
        foreach (var item in this)
        {
            newList.Add(item); // Shallow copy
        }
        return newList;
    }
    #endregion
    
    #region Casting
    
    /// <summary>
    /// Creates a new list by casting each entry in the original list to the specified type <typeparamref name="TU"/>, if possible.
    /// </summary>
    /// <typeparam name="TU">The target type to cast the entries to. Must be a subclass of <typeparamref name="T"/>.</typeparam>
    /// <returns>A new <see cref="DataObjectList{TU}"/> containing the casted entries, or <c>null</c> if the cast fails for all entries.</returns>
    /// <remarks>
    /// This method attempts to cast each entry to the specified type <typeparamref name="TU"/> and collects successfully casted entries into a new list.
    /// </remarks>
    public DataObjectList<TU>? Cast<TU>() where TU : T
    {
        DataObjectList<TU>? newList = null;
        foreach (var entry in this)
        {
            if (entry is not TU castedEntry) continue;
            newList ??= [];
            newList.Add(castedEntry);
        }
        return newList;
    }
    
    #endregion
}