using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

public class JsonClassSerializer<T> where T : class
{
    private readonly JsonSerializerOptions options;
    
    public JsonClassSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    
    public string Serialize(T enemy) => JsonSerializer.Serialize(enemy, options);

    public T? Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);
}