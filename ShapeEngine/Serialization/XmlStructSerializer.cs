using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides XML serialization and deserialization for value types (structs) using <see cref="XmlSerializer"/>.
/// </summary>
/// <remarks>
/// This class is designed for types that are structs (value types). It omits the XML namespace declaration
/// and uses UTF-8 encoding without BOM. The class supports both instance and static usage for serialization and deserialization.
/// </remarks>
/// <typeparam name="T">The value type to serialize/deserialize. Must be a struct.</typeparam>
public class XmlStructSerializer <T> where T : struct
{
    private readonly XmlSerializer serializer;
    private readonly XmlSerializerNamespaces namespaces;
    private readonly XmlWriterSettings settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlStructSerializer{T}"/> class.
    /// </summary>
    /// <remarks>
    /// Sets up the serializer, namespaces, and writer settings for XML operations.
    /// </remarks>
    public XmlStructSerializer()
    {
        serializer = new XmlSerializer(typeof(T));
        namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty); // no xmlns attribute

        settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlStructSerializer{T}"/> class with custom <see cref="XmlWriterSettings"/>.
    /// </summary>
    /// <param name="settings">The XML writer settings to use for serialization.</param>
    /// <remarks>
    /// Uses default namespaces (no xmlns attribute).
    /// </remarks>
    public XmlStructSerializer(XmlWriterSettings settings)
    {
       serializer = new XmlSerializer(typeof(T));
       namespaces = new XmlSerializerNamespaces();
       namespaces.Add(string.Empty, string.Empty); // no xmlns attribute

       this.settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlStructSerializer{T}"/> class with custom <see cref="XmlWriterSettings"/> and <see cref="XmlSerializerNamespaces"/>.
    /// </summary>
    /// <param name="settings">The XML writer settings to use for serialization.</param>
    /// <param name="namespaces">The XML namespaces to use for serialization.</param>
    public XmlStructSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
       serializer = new XmlSerializer(typeof(T));
       this.namespaces = namespaces;
       this.settings = settings;
    }
    /// <summary>
    /// Serializes an instance of <typeparamref name="T"/> to an XML string.
    /// </summary>
    /// <param name="instance">The struct instance to serialize.</param>
    /// <returns>A string containing the XML representation of the struct.</returns>
    /// <remarks>
    /// The output XML will not include an XML namespace declaration and will use UTF-8 encoding without BOM.
    /// </remarks>
    public string Serialize(T instance)
    {
        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            serializer.Serialize(writer, instance, namespaces);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// Serializes a list of <typeparamref name="T"/> instances to a list of XML strings.
    /// </summary>
    /// <param name="instances">The list of struct instances to serialize.</param>
    /// <returns>A list of XML strings representing each struct instance.</returns>
    public List<string> Serialize(List<T> instances)
    {
        List<string> result = [];
        foreach(var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }
    
    /// <summary>
    /// Deserializes an XML string into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <returns>The deserialized struct, or <c>default</c> if deserialization fails or the XML does not match <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Returns <c>default</c> if the XML cannot be deserialized to the specified type.
    /// </remarks>
    public T Deserialize(string xml)
    {
        using var reader = new StringReader(xml);
        var result = serializer.Deserialize(reader);
        
        if(result is T t)
        {
            return t;
        }

        return default;
    }
    
    /// <summary>
    /// Deserializes a list of XML strings into a list of <typeparamref name="T"/> instances.
    /// </summary>
    /// <param name="xmls">The list of XML strings to deserialize.</param>
    /// <returns>A list of deserialized structs of type <typeparamref name="T"/>.</returns>
    public List<T> Deserialize(List<string> xmls)
    {
        List<T> result = [];
        foreach(var xml in xmls)
        {
            result.Add(Deserialize(xml));
        }
        return result;
    }
    
    /// <summary>
    /// Serializes an instance of the specified type <typeparamref name="TS"/> to an XML string.
    /// </summary>
    /// <param name="instance">The struct instance to serialize.</param>
    /// <typeparam name="TS">The value type of the object instance. Must be a struct.</typeparam>
    /// <returns>A string containing the XML representation of the struct.</returns>
    /// <remarks>
    /// The output XML will not include an XML namespace declaration and will use UTF-8 encoding without BOM.
    /// </remarks>
    public static string Serialize<TS>(TS instance) where TS : struct
    {
        var serializer = new XmlSerializer(typeof(TS));
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty); // no xmlns attribute

        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            serializer.Serialize(writer, instance, ns);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// Serializes a list of struct instances of type <typeparamref name="TS"/> to a list of XML strings.
    /// </summary>
    /// <param name="instances">The list of struct instances to serialize.</param>
    /// <typeparam name="TS">The value type of the object instances. Must be a struct.</typeparam>
    /// <returns>A list of XML strings representing each struct instance.</returns>
    public static List<string> Serialize<TS>(List<TS> instances) where TS : struct
    {
        List<string> result = [];
        foreach(var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }
    
    /// <summary>
    /// Deserializes an XML string into an instance of the specified type <typeparamref name="TS"/>.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <typeparam name="TS">The value type to deserialize the XML into. Must be a struct.</typeparam>
    /// <returns>The deserialized struct, or <c>default</c> if deserialization fails or the XML does not match <typeparamref name="TS"/>.</returns>
    /// <remarks>
    /// Returns <c>default</c> if the XML cannot be deserialized to the specified type.
    /// </remarks>
    public static TS Deserialize<TS>(string xml) where TS : struct
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        object? result = serializer.Deserialize(reader);
        
        if(result is TS s)
        {
            return s;
        }

        return default;
    }
    
    /// <summary>
    /// Deserializes a list of XML strings into a list of structs of type <typeparamref name="TS"/>.
    /// </summary>
    /// <param name="xmls">The list of XML strings to deserialize.</param>
    /// <typeparam name="TS">The value type to deserialize the XML into. Must be a struct.</typeparam>
    /// <returns>A list of deserialized structs of type <typeparamref name="TS"/>.</returns>
    public static List<TS> Deserialize<TS>(List<string> xmls) where TS : struct
    {
        List<TS> result = [];
        foreach(var xml in xmls)
        {
            result.Add(Deserialize<TS>(xml));
        }
        return result;
    }
}