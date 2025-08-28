namespace ShapeEngine.Serialization;

/// <summary>
/// Represents a generic database for storing and managing objects of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The base type of objects stored in the database. <typeparamref name="T"/> must inherit from <see cref="DataObject"/>.
/// <para>
/// This class supports storing multiple derived types of <typeparamref name="T"/>. Each derived type is managed in its own internal dictionary, allowing for type-safe retrieval and organization.
/// </para>
/// </typeparam>
/// <remarks>
/// The database organizes objects by their runtime type, enabling efficient storage and lookup for polymorphic scenarios. 
/// Use the provided Add methods to insert single objects, collections, or entire dictionaries. 
/// </remarks>
public class DataBase<T> where T : DataObject
{
    private readonly Dictionary<Type, DataObjectDict<T>> data = new();
    
    #region Add
    
    /// <summary>
    /// Adds a single object of type <typeparamref name="TU"/> to the database.
    /// </summary>
    /// <typeparam name="TU">The type of the object to add. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="obj">The object to add. Its <c>Name</c> property is used as the key in the internal dictionary.</param>
    /// <remarks>
    /// If an object with the same name already exists for the given type, it will be overwritten.
    /// </remarks>
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
    /// <summary>
    /// Adds a collection of objects of type <typeparamref name="TU"/> to the database.
    /// </summary>
    /// <typeparam name="TU">The type of the objects to add. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="objs">The collection of objects to add. Each object's <c>Name</c> property is used as the key in the internal dictionary.</param>
    /// <remarks>
    /// Existing objects with the same name will be overwritten for the given type.
    /// </remarks>
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
    /// <summary>
    /// Adds all objects from a <see cref="DataObjectDict{TU}"/> to the database.
    /// </summary>
    /// <typeparam name="TU">The type of the objects in the dictionary. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="dict">The dictionary containing objects to add. Keys are object names.</param>
    /// <remarks>
    /// Existing objects with the same name will be overwritten for the given type.
    /// </remarks>
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
    /// <summary>
    /// Adds all objects from a <see cref="DataObjectList{TU}"/> to the database.
    /// </summary>
    /// <typeparam name="TU">The type of the objects in the list. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="list">The list containing objects to add. Each object's name is used as the key in the internal dictionary.</param>
    /// <remarks>
    /// Existing objects with the same name will be overwritten for the given type.
    /// </remarks>
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
    /// <summary>
    /// Removes an object of type <typeparamref name="TU"/> with the specified name from the database.
    /// </summary>
    /// <typeparam name="TU">The type of the object to remove. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="name">The name of the object to remove.</param>
    /// <returns><c>true</c> if the object was found and removed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If the type or name does not exist, the method returns <c>false</c>.
    /// </remarks>
    public bool Remove<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.Remove(name);
        }
        return false;
    }
    /// <summary>
    /// Removes multiple objects of type <typeparamref name="TU"/> with the specified names from the database.
    /// </summary>
    /// <typeparam name="TU">The type of the objects to remove. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="names">A collection of names of the objects to remove.</param>
    /// <returns>The number of objects successfully removed.</returns>
    /// <remarks>
    /// Only objects that exist for the given type and names will be removed.
    /// </remarks>
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
    /// <summary>
    /// Removes all objects of type <typeparamref name="TU"/> from the database.
    /// </summary>
    /// <typeparam name="TU">The type of the objects to remove. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns><c>true</c> if the type was found and removed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This will remove the entire dictionary for the specified type.
    /// </remarks>
    public bool Remove<TU>() where TU : T
    {
        var type = typeof(TU);
        return data.Remove(type);
    }

    
    #endregion

    #region Contains
    /// <summary>
    /// Determines whether the database contains any objects of the specified type.
    /// </summary>
    /// <param name="type">The type to check for.</param>
    /// <returns><c>true</c> if the type exists in the database; otherwise, <c>false</c>.</returns>
    public bool ContainsType(Type type) => data.ContainsKey(type);
    /// <summary>
    /// Determines whether the database contains any objects of type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type to check for. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns><c>true</c> if the type exists in the database; otherwise, <c>false</c>.</returns>
    public bool ContainsType<TU>() where TU : T
    {
        var type = typeof(TU);
        return data.ContainsKey(type);
    }

    #endregion
    
    #region Get
    /// <summary>
    /// Retrieves an object of type <typeparamref name="TU"/> with the specified name from the database.
    /// </summary>
    /// <typeparam name="TU">The type of the object to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="name">The name of the object to retrieve.</param>
    /// <returns>The object if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if the type or name does not exist in the database.
    /// </remarks>
    public TU? GetDataObject<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict) && dict.TryGetValue(name, out var obj))
        {
            return obj as TU;
        }
        return null;
    }
    /// <summary>
    /// Retrieves the dictionary of objects for the specified type.
    /// </summary>
    /// <param name="type">The type of objects to retrieve.</param>
    /// <returns>The dictionary of objects if found as reference; otherwise, <c>null</c>.</returns>
    public DataObjectDict<T>? GetDataObjectDict(Type type) => data.GetValueOrDefault(type);
    /// <summary>
    /// Retrieves a copy of the dictionary of objects of type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type of objects to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A copy of the dictionary if found and not empty; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// The returned dictionary contains only objects of the specified type.
    /// </remarks>
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
    /// <summary>
    /// Retrieves a copy of the list of objects of type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type of objects to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A copy of the list if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// The returned list contains only objects of the specified type.
    /// </remarks>
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
    /// <summary>
    /// Retrieves a copy of all objects in the database as a dictionary.
    /// </summary>
    /// <returns>A dictionary containing all objects from all types.</returns>
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
    /// <summary>
    /// Retrieves a copy of all objects in the database as a list.
    /// </summary>
    /// <returns>A list containing all objects from all types.</returns>
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
    /// <summary>
    /// Retrieves a random object of type <typeparamref name="TU"/> from the database.
    /// </summary>
    /// <typeparam name="TU">The type of object to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A random object if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if no objects of the specified type exist.
    /// </remarks>
    public TU? GetRandomEntry<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.GetRandomEntry() as TU;
        }
        return null;
    }
    /// <summary>
    /// Retrieves a list of random objects of type <typeparamref name="TU"/> from the database.
    /// </summary>
    /// <typeparam name="TU">The type of objects to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to retrieve.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? GetRandomEntries<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        return data.TryGetValue(type, out var dict) ? dict.GetRandomEntries(amount) : null;
    }
    /// <summary>
    /// Retrieves a list of random objects of the specified type from the database.
    /// </summary>
    /// <param name="type">The type of objects to retrieve.</param>
    /// <param name="amount">The number of random objects to retrieve.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? GetRandomEntries(Type type, int amount)
    {
        return data.TryGetValue(type, out var dict) ? dict.GetRandomEntries(amount) : null;
    }
    /// <summary>
    /// Retrieves a list of random objects of type <typeparamref name="TU"/> from the database, cast to <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type of objects to retrieve. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to retrieve.</param>
    /// <returns>A list of random objects cast to <typeparamref name="TU"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Only objects that can be cast to <typeparamref name="TU"/> are included in the result.
    /// </remarks>
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
    /// <summary>
    /// Picks a random object of type <typeparamref name="TU"/> from the database.
    /// </summary>
    /// <typeparam name="TU">The type of object to pick. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A random object if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if no objects of the specified type exist.
    /// </remarks>
    public TU? PickRandomEntry<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.PickRandomEntry() as TU;
        }
        return null;
    }
    /// <summary>
    /// Picks a list of random objects of type <typeparamref name="TU"/> from the database.
    /// </summary>
    /// <typeparam name="TU">The type of objects to pick. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? PickRandomEntries<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntries(amount) : null;
    }
    /// <summary>
    /// Picks a list of random objects of the specified type from the database.
    /// </summary>
    /// <param name="type">The type of objects to pick.</param>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? PickRandomEntries(Type type, int amount)
    {
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntries(amount) : null;
    }
    
    /// <summary>
    /// Picks a list of random objects of type <typeparamref name="TU"/> from the database using a chance-based selection.
    /// </summary>
    /// <typeparam name="TU">The type of objects to pick. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? PickRandomEntriesChanceList<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntriesChanceList(amount) : null;
    }

    /// <summary>
    /// Picks a list of random objects of the specified type from the database using a chance-based selection.
    /// </summary>
    /// <param name="type">The type of objects to pick.</param>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects if found; otherwise, <c>null</c>.</returns>
    public DataObjectList<T>? PickRandomEntriesChanceList(Type type, int amount)
    {
        return data.TryGetValue(type, out var dict) ? dict.PickRandomEntriesChanceList(amount) : null;
    }

    /// <summary>
    /// Picks a list of random objects of type <typeparamref name="TU"/> from the database using a chance-based selection, cast to <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type of objects to pick. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects cast to <typeparamref name="TU"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Only objects that can be cast to <typeparamref name="TU"/> are included in the result.
    /// </remarks>
    public DataObjectList<TU>? PickRandomEntriesChanceListCast<TU>(int amount) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            var entries = dict.PickRandomEntriesChanceList(amount);
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
    /// <summary>
    /// Picks a list of random objects of type <typeparamref name="TU"/> from the database, cast to <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The type of objects to pick. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <param name="amount">The number of random objects to pick.</param>
    /// <returns>A list of random objects cast to <typeparamref name="TU"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Only objects that can be cast to <typeparamref name="TU"/> are included in the result.
    /// </remarks>
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
    /// <summary>
    /// Creates a deep copy of the database, including all stored objects and their dictionaries.
    /// </summary>
    /// <returns>A new <see cref="DataBase{T}"/> instance containing copies of all objects.</returns>
    /// <remarks>
    /// The cloned database is independent of the original and changes to one do not affect the other.
    /// </remarks>
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
    /// <summary>
    /// Casts the database to a new generic type <typeparamref name="TU"/> and returns a new database containing only objects that can be cast to <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The new base type for the database. Must inherit from <typeparamref name="T"/>.</typeparam>
    /// <returns>A new <see cref="DataBase{TU}"/> containing only objects that can be cast to <typeparamref name="TU"/>.</returns>
    /// <remarks>
    /// Only objects that are assignable to <typeparamref name="TU"/> are included in the new database.
    /// </remarks>
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
    /// <summary>
    /// Prints the contents of the database to the console for debugging purposes.
    /// </summary>
    /// <remarks>
    /// Shows all types and their stored objects with names and values.
    /// </remarks>
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