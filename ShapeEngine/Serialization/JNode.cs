using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShapeEngine.Serialization;

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
    public List<T> ParseToList<T>(string arrayKey, Func<JNode, T> parser)
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
       // /// <summary>
    // /// Deserializes a JSON array property into a dictionary of objects of type T.
    // /// The dictionary is keyed by the name of each data object.
    // /// </summary>
    // /// <typeparam name="T">The type to deserialize the array elements to, must implement IDataObject.</typeparam>
    // /// <param name="arrayKey">The key of the array property to deserialize.</param>
    // /// <returns>A dictionary mapping names to objects of type T deserialized from the JSON array.</returns>
    // public Dictionary<string, T> SerializeArrayToDict<T>(string arrayKey)
    // {
    //     Dictionary<string, T> result = new();
    //
    //     if (!node.ContainsKey(arrayKey)) return result;
    //     var arr = node[arrayKey]?.AsArray();
    //     
    //     if (arr == null) return result;
    //     
    //     foreach (var item in arr)
    //     {
    //         var obj = item?.AsObject();
    //         if (obj == null) continue;
    //         var t = obj.Deserialize<T>();
    //         if(t != null)result.Add(t.Name, t);
    //     }
    //     return result;
    // }
    // /// <summary>
    // /// Converts an array of JNode objects to a dictionary of objects of type T using JSON deserialization.
    // /// The dictionary is keyed by the name of each data object.
    // /// </summary>
    // /// <typeparam name="T">The type to deserialize the nodes to, must implement IDataObject.</typeparam>
    // /// <param name="nodes">The array of JNode objects to deserialize.</param>
    // /// <returns>A dictionary mapping names to objects of type T deserialized from the nodes.</returns>
    // public static Dictionary<string, T> SerializeArrayToDict<T>(JNode[] nodes)
    // {
    //     Dictionary<string, T> result = new();
    //     foreach (var node in nodes)
    //     {
    //         var r = node.Deserialize<T>();
    //         if (r != null)
    //         {
    //             result.Add(r.GetName(), r);
    //         }
    //     }
    //     return result;
    // }
    // /// <summary>
    // /// Parses a JSON array into a dictionary of objects of type T using a custom parser function.
    // /// The dictionary is keyed by the name of each data container.
    // /// </summary>
    // /// <typeparam name="T">The type of objects to create, must implement IDataContainer.</typeparam>
    // /// <param name="arrayKey">The key of the array to parse.</param>
    // /// <param name="parser">A function that converts a JNode to an object of type T.</param>
    // /// <returns>A dictionary mapping names to objects of type T created from the JSON array.</returns>
    // public Dictionary<string, T> ParseToDict<T>(string arrayKey, Func<JNode, T> parser) where T : DataObjectDict<T>
    // {
    //     Dictionary<string, T> result = new();
    //     var arr = GetArray(arrayKey);
    //     foreach (var item in arr)
    //     {
    //         var c = parser(item);
    //         result.Add(c.GetName(), c);
    //     }
    //     return result;
    // }

