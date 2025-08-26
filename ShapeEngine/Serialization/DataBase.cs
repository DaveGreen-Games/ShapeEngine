namespace ShapeEngine.Serialization;

public class DataBase<T> where T : DataObject
{
    private readonly Dictionary<Type, DataObjectDict<T>> data = new();
    
    #region Add
    
    public void Add<TU>(TU obj) where TU : T
    {
        var type = typeof(TU);
        if (!data.TryGetValue(type, out var dict))
        {
            dict = new DataObjectDict<T>();
            data[type] = dict;
        }
        dict[obj.Name] = obj;
    }
    public void AddRange<TU>(IEnumerable<TU> objs) where TU : T
    {
        var type = typeof(TU);
        if (!data.TryGetValue(type, out var dict))
        {
            dict = new DataObjectDict<T>();
            data[type] = dict;
        }
        foreach (var obj in objs)
        {
            dict[obj.Name] = obj;
        }
    }
    public void Add<TU>(DataObjectDict<TU> dict) where TU : T
    {
        var type = typeof(TU);
        if (!data.TryGetValue(type, out var existingDict))
        {
            existingDict = new DataObjectDict<T>();
            data[type] = existingDict;
        }
        foreach (var pair in dict)
        {
            existingDict[pair.Key] = pair.Value;
        }
    }
    public void Add<TU>(DataObjectList<TU> list) where TU : T
    {
        var type = typeof(TU);
        if (!data.TryGetValue(type, out var dict))
        {
            dict = new DataObjectDict<T>();
            data[type] = dict;
        }
        foreach (var obj in list)
        {
            dict[obj.Name] = obj;
        }
    }
    #endregion
    
    #region Remove
    
    public bool Remove<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.Remove(name);
        }
        return false;
    }

    public int RemoveRange<TU>(IEnumerable<string> names) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            int removed = 0;
            foreach (var name in names)
            {
                if (dict.Remove(name)) removed++;
            }
            return removed;
        }
        return 0;
    }

    public bool Remove<TU>() where TU : T
    {
        var type = typeof(TU);
        return data.Remove(type);
    }

    
    #endregion

    #region Contains

    public bool ContainsType(Type type) => data.ContainsKey(type);
    public bool ContainsType<TU>() where TU : T
    {
        var type = typeof(TU);
        return data.ContainsKey(type);
    }

    #endregion
    
    #region Get
    
    public TU? GetDataObject<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict) && dict.TryGetValue(name, out var obj))
        {
            return obj as TU;
        }
        return null;
    }
    public DataObjectDict<T>? GetDataObjectDict(Type type) => data.GetValueOrDefault(type);
    
    public DataObjectDict<TU>? GetDataObjectDictCopy<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            if (dict.Count <= 0) return null;
            
            var result = new DataObjectDict<TU>();
            foreach (var pair in dict)
            {
                if (pair.Value is TU t) result[pair.Key] = t;
            }
            return result;
        }

        return null;
    }
    public DataObjectList<TU>? GetDataObjectListCopy<TU>() where TU : T
    {
        var type = typeof(TU);
        
        if (data.TryGetValue(type, out var dict))
        {
            var result = new DataObjectList<TU>();
            foreach (var pair in dict)
            {
                if (pair.Value is TU t) result.Add(t);
            }
            return result;
        }
        return null;
    }
    public DataObjectDict<T> GetAllDataObjectsDictCopy() 
    {
        var result = new DataObjectDict<T>();
        foreach (var dict in data.Values)
        {
            foreach (var pair in dict)
            {
                result[pair.Key] = pair.Value;
            }
        }
        return result;
    }
    public DataObjectList<T> GetAllDataObjectsListCopy() 
    {
        var result = new DataObjectList<T>();
        foreach (var dict in data.Values)
        {
            foreach (var pair in dict)
            {
                result.Add(pair.Value);
            }
        }
        return result;
    }
    
    #endregion

    #region Random
    
    public TU? GetRandomEntry<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.GetRandomEntry() as TU;
        }
        return null;
    }
    public DataObjectList<T>? GetRandomEntries<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        return data.TryGetValue(type, out var dict) ? dict.GetRandomEntries(amount) : null;
    }
    public DataObjectList<T>? GetRandomEntries(Type type, int amount)
    {
        return data.TryGetValue(type, out var dict) ? dict.GetRandomEntries(amount) : null;
    }
    public DataObjectList<TU>? GetRandomEntriesCast<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            var entries = dict.GetRandomEntries(amount);
            if (entries == null) return null;
            var result = new DataObjectList<TU>();
            foreach (var e in entries)
            {
                if (e is TU t) result.Add(t);
            }
            return result;
        }
        return null;
    }
    
    public TU? PickRandomEntry<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.PickRandomEntry() as TU;
        }
        return null;
    }
    public DataObjectList<T>? PickRandomEntries<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntries(amount) : null;
    }
    public DataObjectList<T>? PickRandomEntries(Type type, int amount)
    {
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntries(amount) : null;
    }
    public DataObjectList<TU>? PickRandomEntriesCast<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            var entries = dict.PickRandomEntries(amount);
            if (entries == null) return null;
            var result = new DataObjectList<TU>();
            foreach (var e in entries)
            {
                if (e is TU t) result.Add(t);
            }
            return result;
        }
        return null;
    }
    
    #endregion
    
    #region Clone

    public DataBase<T> Clone()
    {
        var newDb = new DataBase<T>();
        foreach (var (type, dict) in data)
        {
            newDb.data[type] = dict.Clone();
        }
        return newDb;
    }
    
    #endregion
    
    #region Casting
    
    public DataBase<TU> CastTo<TU>() where TU : T
    {
        var newDb = new DataBase<TU>();
        var newType = typeof(TU);
        foreach (var (type, dict) in data)
        {
            if (!newType.IsAssignableFrom(type)) continue;
            var castedDict = new DataObjectDict<TU>();
            foreach (var pair in dict)
            {
                if (pair.Value is TU tObj)
                {
                    castedDict[pair.Key] = tObj;
                }
            }
            if (castedDict.Count > 0)
            {
                newDb.Add<TU>(castedDict);
            }
        }
        return newDb;
    }
    
    #endregion
    
    #region Debug
    
    public void Print()
    {
        Console.WriteLine($"Data Base of type {typeof(T).Name} Contents:");
        foreach (var (t, dict) in data)
        {
            Console.WriteLine($"- Entries of Type {t.Name}:");
            foreach (var entry in dict)
            {
                Console.WriteLine($"-- Name: {entry.Key}, Value: {entry.Value}");
            }
        }
    }
    
    #endregion

}