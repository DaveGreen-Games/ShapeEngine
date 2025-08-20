using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

public class JsonStructSerializer<T> where T : struct
{
    private readonly JsonSerializerOptions options;
    
    public JsonStructSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    
    public string Serialize(T enemy) => JsonSerializer.Serialize(enemy, options);

    public T Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);
}