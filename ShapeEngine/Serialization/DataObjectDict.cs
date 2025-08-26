using System.Collections.Specialized;
using ShapeEngine.Random;

namespace ShapeEngine.Serialization;

public class DataObjectDict<T> : Dictionary<string, T> where T : DataObject
{
    #region Random
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this.ElementAt(index).Value;
    }
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
                
            }
        }
        return result;
    }
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
    public T? GetEntry(string name) => TryGetValue(name, out var value) ? value : null;
    #endregion
    
    #region Add
    public bool Add(T item)
    {
        return TryAdd(item.Name, item);
    }
    #endregion
    
    #region Remove
    public bool Remove(T item)
    {
        return base.Remove(item.Name);
    }
    #endregion
    
    #region Conversion
    public DataObjectList<T> ToDataObjectList()
    {
        var list = new DataObjectList<T>();
        foreach (var item in Values)
        {
            list.Add(item);
        }
        return list;
    }

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