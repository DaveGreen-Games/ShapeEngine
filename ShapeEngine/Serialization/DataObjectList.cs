using System.Reflection.Metadata.Ecma335;
using ShapeEngine.Random;

namespace ShapeEngine.Serialization;

public class DataObjectList<T> : List<T> where T : DataObject
{
    #region Get

    public T? GetEntry(string name) => Find(item => item.Name == name);

    public TU? GetEntry<TU>(string name) where TU : T => GetEntry(name) as TU;

    #endregion
    
    #region Random
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this[index];
    }
    public TU? GetRandomEntry<TU>() where TU : T
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        var entry = this[index];
        return entry is TU castedEntry ? castedEntry : null;
    }

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
    public DataObjectDict<T> ToDataObjectDict()
    {
        var dict = new DataObjectDict<T>();
        foreach (var item in this)
        {
            dict.TryAdd(item.Name, item);
        }
        return dict;
    }

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