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
    #endregion
    
    #region Get
    public T? GetEntry(string name) => TryGetValue(name, out var value) ? value : null;
    #endregion
    
    #region Add
    public bool Add(T item)
    {
        if (ContainsKey(item.Name)) return false;
        base.Add(item.Name, item);
        return true;
    }
    #endregion
    
    #region Remove
    public bool Remove(T item) => Remove(item.Name);
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
}