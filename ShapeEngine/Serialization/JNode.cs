using System.Text.Json;
using System.Text.Json.Nodes;
using ShapeEngine.Random;

namespace ShapeEngine.Serialization;

public abstract record DataObject
{
    public required string Name { get; set; }//set needs to be for serialization
}
public class DataObjectList<T> : List<T> where T : DataObject
{
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this[index];
    }

    public T? GetEntry(string name) => Find(item => item.Name == name);
}
public class DataObjectDict<T> : Dictionary<string, T> where T : DataObject
{
    public T? GetRandomEntry()
    {
        if (Count == 0) return null;
        int index = Rng.Instance.RandI(0, Count);
        return this.ElementAt(index).Value;
    }

    public T? GetEntry(string name) => TryGetValue(name, out var value) ? value : null;
}

public class DataBase<T> where T : DataObject
{
    private readonly Dictionary<Type, DataObjectDict<T>> data = new();
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
    public bool Remove<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            return dict.Remove(name);
        }
        return false;
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
    
    public TU? Get<TU>(string name) where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict) && dict.TryGetValue(name, out var obj))
        {
            return obj as TU;
        }
        return null;
    }
    public DataObjectDict<TU> GetAll<TU>() where TU : T
    {
        var type = typeof(TU);
        if (data.TryGetValue(type, out var dict))
        {
            var result = new DataObjectDict<TU>();
            foreach (var pair in dict)
            {
                if (pair.Value is TU t) result[pair.Key] = t;
            }
            return result;
        }
        return new DataObjectDict<TU>();
    }
    public DataObjectList<TU> GetAllList<TU>() where TU : T
    {
        var type = typeof(TU);
        var result = new DataObjectList<TU>();
        if (data.TryGetValue(type, out var dict))
        {
            foreach (var pair in dict)
            {
                if (pair.Value is TU t) result.Add(t);
            }
        }
        return result;
    }
    
    public DataObjectDict<T> GetAll() 
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
    
    //TODO: improve
    public void Print()
    {
        foreach (var kvp in data)
        {
            Console.WriteLine($"Type: {kvp.Key.Name}");
            foreach (var objKvp in kvp.Value)
            {
                Console.WriteLine($"  Name: {objKvp.Key}, Value: {objKvp.Value}");
            }
        }
    }
}

public class DataBaseTest
{
    public record DataObject1 : DataObject
    {
        public int Value { get; set; }
    }
    public record DataObject2 : DataObject
    {
        public int Value { get; set; }
    }
    public record DataObject3 : DataObject
    {
        public int Value { get; set; }
    }
    public record DataObject4 : DataObject
    {
        public int Value { get; set; }
    }
    
    public void Test1()
    {
        var myDataBase = new DataBase<DataObject>();
        var obj1 = new DataObject1 { Name = "Object1", Value = 10 };
        var obj2 = new DataObject2 { Name = "Object2", Value = 20 };
        var obj3 = new DataObject3 { Name = "Object3", Value = 30 };
        var obj4 = new DataObject4 { Name = "Object4", Value = 40 };
        myDataBase.Add(obj1);
        myDataBase.Add(obj2);
        myDataBase.Add(obj3);
        myDataBase.Add(obj4);
        
        var obj = myDataBase.Get<DataObject1>("Object1");
        Console.WriteLine(obj?.Value); // Output: 10
    }
}



//NOTE:
// - Interface for data object (each object has a name)
// - Dictionary for holding DataObjects (name is the key)
// - DataBase is dictionary holding multiple dictionaries Dictionary<type of T, Dictionary<string, T>> -> possible?!
// - this system should not be to complicated
// - having a generic list and dictionary for data objects should be good (implemented in a way that it can be used elsewhere (serializers, file manager))
//IDEA
// public class DataBase
// {
//     private readonly Dictionary<Type, Dictionary<string, object>> data = new();
//
//     public void Add<T>(string key, T value) where T : BaseDataObject
//     {
//         var type = typeof(T);
//         if (!data.TryGetValue(type, out var dict))
//         {
//             dict = new Dictionary<string, object>();
//             data[type] = dict;
//         }
//         dict[key] = value;
//     }
//
//     public T? Get<T>(string key) where T : BaseDataObject
//     {
//         var type = typeof(T);
//         if (data.TryGetValue(type, out var dict) && dict.TryGetValue(key, out var obj))
//         {
//             return obj as T;
//         }
//         return null;
//     }
//
//     public Dictionary<string, T> GetAll<T>() where T : BaseDataObject
//     {
//         var type = typeof(T);
//         if (data.TryGetValue(type, out var dict))
//         {
//             return dict.ToDictionary(pair => pair.Key, pair => (T)pair.Value);
//         }
//         return new Dictionary<string, T>();
//     }
// }



//NOTE: I could leave this class and just remove IDataContainer usages
// - Should also be removed and cleaned up if possible

//NOTE:
// - IDataObject, IDataContainer, and JDataContainer:
//  - they are good at storing data from different sources.
//  - Maybe the system can be changed into 1 Interface and 1 generic dictionary and one generic list class?
//  - This would probably the best way for having a simple and easy to use system.
//  - JNode would be removed anyway


//!!! Deserialzing/Serializing classes could use a generic list implementation that has some convinience methods, like retrieving random items.
//
// public record BaseDataObject(string Name) // => could be an interface as well?
// {
//     public string GetName() => Name;
// }
//
// public class DataList<T> : List<T> where T : BaseDataObject //cant handle strings if done like this!
// {
//     
// }

//Note: Should be able to handle different data objects in one dictionary
// - functions for retrieving item of specific type?
// - functions for retrieving random item of specific type?
// - functions for retrieving list of items of specific type?

// public class DataDict<T> : Dictionary<string, T> where T : BaseDataObject
// {
//     
// }


//Q: Does a generic list for data objects make sense
// -> serializing json strings into AsteroidData objects that would be stored in the data list and then a random
// item could easily be retrieved?

//Q: DataBase that holds multiple dictionaries -> each dictionary holds data object of the same type?

/// <summary>
/// Provides a wrapper for working with JSON data, allowing easy access to JSON properties and arrays.
/// </summary>
public sealed class JNode
{
    private readonly JsonObject node;

    /// <summary>
    /// Gets a value indicating whether this node contains valid JSON data.
    /// </summary>
    public bool Valid { get; private set; }

    /// <summary>
    /// Initializes a new empty instance of the JNode class.
    /// </summary>
    public JNode() { this.node = new JsonObject(); }

    /// <summary>
    /// Initializes a new instance of the JNode class from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    public JNode(string json)
    {
        var obj = JsonNode.Parse(json)?.AsObject();
        if(obj != null)
        {
            Valid = true;
            node = obj;
        }
        else node = new JsonObject();
    }

    /// <summary>
    /// Initializes a new instance of the JNode class from a JsonNode.
    /// </summary>
    /// <param name="node">The JsonNode to wrap.</param>
    internal JNode(JsonNode? node)
    {
        var obj = node?.AsObject();
        if (obj != null)
        {
            Valid = true;
            this.node = obj;
        }
        else this.node = new JsonObject();
    }

    /// <summary>
    /// Gets the JSON representation of this node as a string.
    /// </summary>
    /// <returns>A JSON string representing this node.</returns>
    public string GetJsonString() { return node.ToJsonString(); }

    /// <summary>
    /// Determines whether the JSON object contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>true if the node is valid and contains the specified key; otherwise, false.</returns>
    public bool ContainsKey(string key) { return Valid && node.ContainsKey(key); }

    /// <summary>
    /// Gets a nested JSON node by key.
    /// </summary>
    /// <param name="key">The key of the nested node to retrieve.</param>
    /// <returns>A JNode containing the nested JSON object if found; otherwise, an empty JNode.</returns>
    public JNode GetNode(string key)
    {
        if (node.ContainsKey(key))
        {
            var n = node[key];
            if(n != null) return new JNode(n);
        }
        return new JNode();
    }

    /// <summary>
    /// Gets an array of JSON nodes from an array property in the JSON object.
    /// </summary>
    /// <param name="arrayKey">The key of the array property to retrieve.</param>
    /// <returns>An array of JNode objects representing the elements in the JSON array,
    /// or an empty array if the property doesn't exist or isn't a valid array.</returns>
    public JNode[] GetArray(string arrayKey)
    {
        if (!Valid) return [];   
        if(node.ContainsKey(arrayKey))
        {
            var arr = node[arrayKey]?.AsArray();
            if(arr != null)
            {
                List<JNode> result = new();
                foreach (var item in arr)
                {
                    var jn = new JNode(item);
                    if (jn.Valid) result.Add(jn);
                }
                return result.ToArray();
            }
        }
        return [];
    }
    
    /// <summary>
    /// Parses a JSON array into a list of objects of type T using a custom parser function.
    /// </summary>
    /// <typeparam name="T">The type of objects to create, must implement IDataContainer.</typeparam>
    /// <param name="arrayKey">The key of the array to parse.</param>
    /// <param name="parser">A function that converts a JNode to an object of type T.</param>
    /// <returns>A list of objects of type T created from the JSON array.</returns>
    public List<T> ParseToList<T>(string arrayKey, Func<JNode, T> parser) where T : IDataContainer
    {
        List<T> result = [];
        var arr = GetArray(arrayKey);
        foreach (var item in arr)
        {
            result.Add(parser(item));
        }
        return result;
    }

    /// <summary>
    /// Parses a JSON array into a dictionary of objects of type T using a custom parser function.
    /// The dictionary is keyed by the name of each data container.
    /// </summary>
    /// <typeparam name="T">The type of objects to create, must implement IDataContainer.</typeparam>
    /// <param name="arrayKey">The key of the array to parse.</param>
    /// <param name="parser">A function that converts a JNode to an object of type T.</param>
    /// <returns>A dictionary mapping names to objects of type T created from the JSON array.</returns>
    public Dictionary<string, T> ParseToDict<T>(string arrayKey, Func<JNode, T> parser) where T : IDataContainer
    {
        Dictionary<string, T> result = new();
        var arr = GetArray(arrayKey);
        foreach (var item in arr)
        {
            var c = parser(item);
            result.Add(c.GetName(), c);
        }
        return result;
    }

    /// <summary>
    /// Converts an array of JNode objects to a list of objects of type T using JSON deserialization.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the nodes to.</typeparam>
    /// <param name="nodes">The array of JNode objects to deserialize.</param>
    /// <returns>A list of objects of type T deserialized from the nodes.</returns>
    public static List<T> SerializeArrayToList<T>(JNode[] nodes)
    {
        List<T> result = new List<T>();
        foreach (var node in nodes)
        {
            var r = node.Deserialize<T>();
            if(r != null) result.Add(r);
        }
        return result;
    }

    /// <summary>
    /// Converts an array of JNode objects to a dictionary of objects of type T using JSON deserialization.
    /// The dictionary is keyed by the name of each data object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the nodes to, must implement IDataObject.</typeparam>
    /// <param name="nodes">The array of JNode objects to deserialize.</param>
    /// <returns>A dictionary mapping names to objects of type T deserialized from the nodes.</returns>
    public static Dictionary<string, T> SerializeArrayToDict<T>(JNode[] nodes) where T : IDataObject
    {
        Dictionary<string, T> result = new();
        foreach (var node in nodes)
        {
            var r = node.Deserialize<T>();
            if (r != null)
            {
                result.Add(r.GetName(), r);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Deserializes a JSON array property into a list of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the array elements to.</typeparam>
    /// <param name="arrayKey">The key of the array property to deserialize.</param>
    /// <returns>A list of objects of type T deserialized from the JSON array.</returns>
    public List<T> SerializeArrayToList<T>(string arrayKey)
    {
        List<T> result = [];

        if (!node.ContainsKey(arrayKey)) return result;
        
        var arr = node[arrayKey]?.AsArray();
        if (arr == null) return result;
        
        foreach (var item in arr)
        {
            var obj = item?.AsObject();
            if (obj == null) continue;
            var t = obj.Deserialize<T>();
            if (t != null) result.Add(t);
        }
        return result;
    }

    /// <summary>
    /// Deserializes a JSON array property into a dictionary of objects of type T.
    /// The dictionary is keyed by the name of each data object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the array elements to, must implement IDataObject.</typeparam>
    /// <param name="arrayKey">The key of the array property to deserialize.</param>
    /// <returns>A dictionary mapping names to objects of type T deserialized from the JSON array.</returns>
    public Dictionary<string, T> SerializeArrayToDict<T>(string arrayKey) where T : IDataObject
    {
        Dictionary<string, T> result = new();

        if (!node.ContainsKey(arrayKey)) return result;
        var arr = node[arrayKey]?.AsArray();
        
        if (arr == null) return result;
        
        foreach (var item in arr)
        {
            var obj = item?.AsObject();
            if (obj == null) continue;
            var t = obj.Deserialize<T>();
            if(t != null)result.Add(t.GetName(), t);
        }
        return result;
    }

    /// <summary>
    /// Deserializes the JSON node to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>An object of type T deserialized from the JSON node, or null if deserialization fails.</returns>
    public T? Deserialize<T>() { return node.Deserialize<T>(); }

    /// <summary>
    /// Gets a property from the JSON node and deserializes it to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the property to.</typeparam>
    /// <param name="key">The key of the property to retrieve.</param>
    /// <returns>An object of type T deserialized from the property,
    /// or default(T) if the property doesn't exist or deserialization fails.</returns>
    public T? GetProperty<T>(string key)
    {
        if (node.ContainsKey(key))
        {
            var n = node[key];
            return n.Deserialize<T>();
        }
        return default(T);
    }
    
}
   

