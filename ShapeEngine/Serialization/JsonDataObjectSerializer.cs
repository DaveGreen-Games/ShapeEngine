using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

public class JsonDataObjectSerializer<T> where T : DataObject
{
    private readonly JsonSerializerOptions options;

    public JsonDataObjectSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public JsonDataObjectSerializer(JsonNamingPolicy namingPolicy, bool writeIndented = true)
    {
        options = new()
        {
            PropertyNamingPolicy = namingPolicy,
            WriteIndented = writeIndented,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public JsonDataObjectSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public string Serialize(T instance) => JsonSerializer.Serialize(instance, options);

    public List<string> Serialize(DataObjectList<T> instances)
    {
        var list = new List<string>(instances.Count);
        foreach (var instance in instances)
        {
            list.Add(JsonSerializer.Serialize(instance, options));
        }
        return list;
    }

    public T? Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);

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