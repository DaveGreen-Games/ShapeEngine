using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides serialization and deserialization of value types (structs) to and from JSON using customizable options.
/// </summary>
public class JsonStructSerializer<T> where T : struct
{
    private readonly JsonSerializerOptions options;
    
    /// <summary>
    /// Initializes a new instance of <see cref="JsonStructSerializer{T}"/> with default options (no case transformation, indented, enum as string).
    /// </summary>
    /// <remarks>Setting <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> to null uses the default property naming behavior (no transformation).</remarks>
    public JsonStructSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    /// <summary>
    /// Initializes a new instance of <see cref="JsonStructSerializer{T}"/> with a custom naming policy and indentation option.
    /// </summary>
    /// <param name="namingPolicy">The property naming policy to use.</param>
    /// <param name="writeIndented">Whether to write indented JSON.</param>
    public JsonStructSerializer(JsonNamingPolicy namingPolicy, bool writeIndented = true)
    {
        options = new()
        {
            PropertyNamingPolicy = namingPolicy,
            WriteIndented = writeIndented,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    /// <summary>
    /// Initializes a new instance of <see cref="JsonStructSerializer{T}"/> with custom <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The serializer options to use.</param>
    public JsonStructSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }
    
    /// <summary>
    /// Serializes the specified value type to a JSON string.
    /// </summary>
    /// <param name="instance">The value type to serialize.</param>
    /// <returns>A JSON string representation of the value type.</returns>
    public string Serialize(T instance) => JsonSerializer.Serialize(instance, options);
    /// <summary>
    /// Deserializes the specified JSON string to a value type of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized value type.</returns>
    public T Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);
   
    /// <summary>
    /// Serializes a list of objects to a list of JSON strings.
    /// </summary>
    /// <param name="instances">The list of objects to serialize.</param>
    /// <returns>A list of JSON string representations of the objects.</returns>
    public List<string> Serialize(List<T> instances)
    {
        var list = new List<string>(instances.Count);
        foreach (var instance in instances)
        {
            list.Add(JsonSerializer.Serialize(instance, options));
        }
        return list;
    }
    /// <summary>
    /// Deserializes a list of JSON strings to a list of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="jsons">The list of JSON strings to deserialize.</param>
    /// <returns>A list of deserialized objects of type <typeparamref name="T"/>.</returns>
    public List<T> Deserialize(List<string> jsons)
    {
        var list = new List<T>(jsons.Count);
        foreach (var json in jsons)
        {
             list.Add(JsonSerializer.Deserialize<T>(json, options));
        }
        return list;
    }
    
    /// <summary>
    /// Serializes the specified object to a JSON string using optional serializer options.
    /// </summary>
    /// <typeparam name="TS">The type of the object to serialize.</typeparam>
    /// <param name="instance">The object to serialize.</param>
    /// <param name="options">Optional serializer options. If null, default options are used.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeStatic<TS>(TS instance, JsonSerializerOptions? options = null) where TS : struct
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        return JsonSerializer.Serialize(instance, options);
    }
    /// <summary>
    /// Serializes a list of objects to a list of JSON strings using optional serializer options.
    /// </summary>
    /// <typeparam name="TS">The type of the objects to serialize.</typeparam>
    /// <param name="instances">The list of objects to serialize.</param>
    /// <param name="options">Optional serializer options. If null, default options are used.</param>
    /// <returns>A list of JSON string representations of the objects.</returns>
    public static List<string> SerializeStatic<TS>(List<TS> instances, JsonSerializerOptions? options = null) where TS : struct
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var list = new List<string>(instances.Count);
        foreach (var instance in instances)
        {
            list.Add(JsonSerializer.Serialize(instance, options));
        }
        return list;
    }
    
    /// <summary>
    /// Deserializes the specified JSON string to an object of type <typeparamref name="TS"/> using optional serializer options.
    /// </summary>
    /// <typeparam name="TS">The type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional serializer options. If null, default options are used.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    public static TS? DeserializeStatic<TS>(string json, JsonSerializerOptions? options = null) where TS : struct
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        return JsonSerializer.Deserialize<TS>(json, options);
    }
    /// <summary>
    /// Deserializes a list of JSON strings to a list of objects of type <typeparamref name="TS"/> using optional serializer options.
    /// </summary>
    /// <typeparam name="TS">The type of the objects to deserialize.</typeparam>
    /// <param name="jsons">The list of JSON strings to deserialize.</param>
    /// <param name="options">Optional serializer options. If null, default options are used.</param>
    /// <returns>A list of deserialized objects of type <typeparamref name="TS"/>.</returns>
    public static List<TS> SerializeStatic<TS>(List<string> jsons, JsonSerializerOptions? options = null) where TS : struct
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        
        var list = new List<TS>(jsons.Count);
        foreach (var json in jsons)
        {
            list.Add(JsonSerializer.Deserialize<TS>(json, options));
        }
        return list;
    }
}