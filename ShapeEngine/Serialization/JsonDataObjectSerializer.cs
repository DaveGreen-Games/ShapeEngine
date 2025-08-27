using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides serialization and deserialization functionality for <typeparamref name="T"/> objects using System.Text.Json.
/// </summary>
/// <typeparam name="T">
/// The type of <see cref="DataObject"/> to serialize/deserialize.
/// <typeparamref name="T"/> must inherit from <see cref="DataObject"/>.
/// </typeparam>
/// <remarks>
/// This class supports custom <see cref="JsonSerializerOptions"/> and static serialization methods for flexibility.
/// Use <see cref="Serialize(T)"/> and <see cref="Deserialize(string)"/> for instance-based operations,
/// or the static methods for generic usage.
/// </remarks>
public class JsonDataObjectSerializer<T> where T : DataObject
{
    private readonly JsonSerializerOptions options;

    /// <summary>
    /// Initializes a new instance with default serialization options.
    /// </summary>
    public JsonDataObjectSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Initializes a new instance with a custom naming policy and indentation option.
    /// </summary>
    /// <param name="namingPolicy">The <see cref="JsonNamingPolicy"/> to use for property names.</param>
    /// <param name="writeIndented">Whether to write indented (pretty-printed) JSON. Default is true.</param>
    public JsonDataObjectSerializer(JsonNamingPolicy namingPolicy, bool writeIndented = true)
    {
        options = new()
        {
            PropertyNamingPolicy = namingPolicy,
            WriteIndented = writeIndented,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Initializes a new instance with custom <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for serialization and deserialization.</param>
    public JsonDataObjectSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Serializes a single <typeparamref name="T"/> instance to a JSON string.
    /// </summary>
    /// <param name="instance">The <typeparamref name="T"/> object to serialize.</param>
    /// <returns>A JSON string representing the object.</returns>
    public string Serialize(T instance) => JsonSerializer.Serialize(instance, options);

    /// <summary>
    /// Serializes a list of <typeparamref name="T"/> objects to a list of JSON strings.
    /// </summary>
    /// <param name="instances">A <see cref="DataObjectList{T}"/> containing objects to serialize.</param>
    /// <returns>A list of JSON strings, each representing an object.</returns>
    public List<string> Serialize(DataObjectList<T> instances)
    {
        var list = new List<string>(instances.Count);
        foreach (var instance in instances)
        {
            list.Add(JsonSerializer.Serialize(instance, options));
        }
        return list;
    }

    /// <summary>
    /// Deserializes a JSON string into a <typeparamref name="T"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <typeparamref name="T"/> object, or null if deserialization fails.</returns>
    public T? Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);

    /// <summary>
    /// Deserializes a list of JSON strings into a <see cref="DataObjectList{T}"/> of objects.
    /// </summary>
    /// <param name="jsons">A list of JSON strings to deserialize.</param>
    /// <returns>A <see cref="DataObjectList{T}"/> containing the deserialized objects.</returns>
    public DataObjectList<T> Deserialize(List<string> jsons)
    {
        var list = new DataObjectList<T>(jsons.Count);
        foreach (var json in jsons)
        {
            var obj = JsonSerializer.Deserialize<T>(json, options);
            if (obj != null) list.Add(obj);
        }
        return list;
    }

    /// <summary>
    /// Serializes a single <typeparamref name="TS"/> object to a JSON string using static context.
    /// </summary>
    /// <typeparam name="TS">The type of <see cref="DataObject"/> to serialize.</typeparam>
    /// <param name="instance">The object to serialize.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for serialization. If null, defaults are used.</param>
    /// <returns>A JSON string representing the object.</returns>
    public static string SerializeStatic<TS>(TS instance, JsonSerializerOptions? options = null) where TS : DataObject
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
    /// Serializes a list of <typeparamref name="TS"/> objects to a list of JSON strings using static context.
    /// </summary>
    /// <typeparam name="TS">The type of <see cref="DataObject"/> to serialize.</typeparam>
    /// <param name="instances">A <see cref="DataObjectList{TS}"/> containing objects to serialize.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for serialization. If null, defaults are used.</param>
    /// <returns>A list of JSON strings, each representing an object.</returns>
    public static List<string> SerializeStatic<TS>(DataObjectList<TS> instances, JsonSerializerOptions? options = null) where TS : DataObject
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
    /// Deserializes a JSON string into a <typeparamref name="TS"/> object using static context.
    /// </summary>
    /// <typeparam name="TS">The type of <see cref="DataObject"/> to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for deserialization. If null, defaults are used.</param>
    /// <returns>The deserialized <typeparamref name="TS"/> object, or null if deserialization fails.</returns>
    public static TS? DeserializeStatic<TS>(string json, JsonSerializerOptions? options = null) where TS : DataObject
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
    /// Deserializes a list of JSON strings into a <see cref="DataObjectList{TS}"/> of objects using static context.
    /// </summary>
    /// <typeparam name="TS">The type of <see cref="DataObject"/> to deserialize.</typeparam>
    /// <param name="jsons">A list of JSON strings to deserialize.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for deserialization. If null, defaults are used.</param>
    /// <returns>A <see cref="DataObjectList{TS}"/> containing the deserialized objects.</returns>
    public static DataObjectList<TS> DeserializeStatic<TS>(List<string> jsons, JsonSerializerOptions? options = null) where TS : DataObject
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var list = new DataObjectList<TS>(jsons.Count);
        foreach (var json in jsons)
        {
            var obj = JsonSerializer.Deserialize<TS>(json, options);
            if (obj != null) list.Add(obj);
        }
        return list;
    }
}