using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides serialization and deserialization of objects to and from JSON using customizable options.
/// </summary>
public class JsonClassSerializer<T> where T : class
{
    private readonly JsonSerializerOptions options;
    
    /// <summary>
    /// Initializes a new instance of <see cref="JsonClassSerializer{T}"/> with default options (no case transformation, indented, enum as string).
    /// </summary>
    /// <remarks>Setting <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> to null uses the default property naming behavior (no transformation).</remarks>
    public JsonClassSerializer()
    {
        options = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    /// <summary>
    /// Initializes a new instance of <see cref="JsonClassSerializer{T}"/> with a custom naming policy and indentation option.
    /// </summary>
    /// <param name="namingPolicy">The property naming policy to use.</param>
    /// <param name="writeIndented">Whether to write indented JSON.</param>
    public JsonClassSerializer(JsonNamingPolicy namingPolicy, bool writeIndented = true)
    {
        options = new()
        {
            PropertyNamingPolicy = namingPolicy,
            WriteIndented = writeIndented,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    /// <summary>
    /// Initializes a new instance of <see cref="JsonClassSerializer{T}"/> with custom <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The serializer options to use.</param>
    public JsonClassSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="enemy">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public string Serialize(T enemy) => JsonSerializer.Serialize(enemy, options);

    /// <summary>
    /// Deserializes the specified JSON string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    public T? Deserialize(string json) => JsonSerializer.Deserialize<T>(json, options);
}