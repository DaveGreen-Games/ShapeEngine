using ShapeEngine.Random;

namespace ShapeEngine.Serialization;

public class DataObjectList<T> : List<T> where T : DataObject
{
    
    #region Get

    public T? GetEntry(string name) => Find(item => item.Name == name);

    #endregion
    
    #region Random
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this[index];
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
                result.Add(this[index]);
            }
        }
        return result;
        
        
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
}