using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides XML serialization and deserialization for reference types using <see cref="XmlSerializer"/>.
/// </summary>
/// <remarks>
/// This class is designed for types that are classes (reference types). It omits the XML namespace declaration
/// and uses UTF-8 encoding without BOM. The class supports both instance and static usage for serialization and deserialization.
/// </remarks>
/// <typeparam name="T">The reference type to serialize/deserialize. Must be a class.</typeparam>
public class XmlClassSerializer <T> where T : class
{
    private readonly XmlSerializer serializer;
    private readonly XmlSerializerNamespaces namespaces;
    private readonly XmlWriterSettings settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlClassSerializer{T}"/> class.
    /// </summary>
    /// <remarks>
    /// Sets up the serializer, namespaces, and writer settings for XML operations.
    /// </remarks>
    public XmlClassSerializer()
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
    /// Initializes a new instance of the <see cref="XmlClassSerializer{T}"/> class with custom <see cref="XmlWriterSettings"/>.
    /// </summary>
    /// <param name="settings">The XML writer settings to use for serialization.</param>
    /// <remarks>
    /// Uses default namespaces (no xmlns attribute).
    /// </remarks>
    public XmlClassSerializer(XmlWriterSettings settings)
    {
        serializer = new XmlSerializer(typeof(T));
        namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty); // no xmlns attribute

        this.settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlClassSerializer{T}"/> class with custom <see cref="XmlWriterSettings"/> and <see cref="XmlSerializerNamespaces"/>.
    /// </summary>
    /// <param name="settings">The XML writer settings to use for serialization.</param>
    /// <param name="namespaces">The XML namespaces to use for serialization.</param>
    public XmlClassSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
        serializer = new XmlSerializer(typeof(T));
        this.namespaces = namespaces;
        this.settings = settings;
    }
    
    /// <summary>
    /// Serializes an instance of <typeparamref name="T"/> to an XML string.
    /// </summary>
    /// <param name="instance">The object instance to serialize. Must not be null.</param>
    /// <returns>A string containing the XML representation of the object.</returns>
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
    /// <param name="instances">The list of object instances to serialize. Must not be null.</param>
    /// <returns>A list of strings containing the XML representations of the objects.</returns>
    public List<string> Serialize(List<T> instances)
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }

    /// <summary>
    /// Deserializes an XML string into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <returns>The deserialized object, or <c>null</c> if deserialization fails or the XML does not match <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if the XML cannot be deserialized to the specified type.
    /// </remarks>
    public T? Deserialize(string xml)
    {
        using var reader = new StringReader(xml);
        var result = serializer.Deserialize(reader);
        
        if(result is T t)
        {
            return t;
        }

        return null;
    }
    /// <summary>
    /// Deserializes a list of XML strings into a list of <typeparamref name="T"/> instances.
    /// </summary>
    /// <param name="xmls">The list of XML strings to deserialize.</param>
    /// <returns>A list of deserialized objects of type <typeparamref name="T"/>.</returns>
    public List<T> Deserialize(List<string> xmls)
    {
        List<T> result = [];
        foreach (var xml in xmls)
        {
            var r = Deserialize(xml);
            if(r != null) result.Add(r);
        }
        return result;
    }
    
    /// <summary>
    /// Serializes an instance of the specified type <typeparamref name="TC"/> to an XML string.
    /// </summary>
    /// <param name="instance">The object instance to serialize. Must not be null.</param>
    /// <typeparam name="TC">The type of the object instance. Must be a class.</typeparam>
    /// <returns>A string containing the XML representation of the object.</returns>
    /// <remarks>
    /// The output XML will not include an XML namespace declaration and will use UTF-8 encoding without BOM.
    /// </remarks>
    public static string Serialize<TC>(TC instance) where TC : class
    {
        var serializer = new XmlSerializer(typeof(T));
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
    /// Serializes a list of instances of the specified type <typeparamref name="TC"/> to a list of XML strings.
    /// </summary>
    /// <typeparam name="TC">The type of the object instances. Must be a class.</typeparam>
    /// <param name="instances">The list of object instances to serialize. Must not be null.</param>
    /// <returns>A list of strings containing the XML representations of the objects.</returns>
    public static List<string> Serialize<TC>(List<TC> instances) where TC : class
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }
    
    /// <summary>
    /// Deserializes an XML string into an instance of the specified type <typeparamref name="TC"/>.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <typeparam name="TC">The type to deserialize the XML into. Must be a class.</typeparam>
    /// <returns>The deserialized object, or <c>null</c> if deserialization fails or the XML does not match <typeparamref name="TC"/>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if the XML cannot be deserialized to the specified type.
    /// </remarks>
    public static TC? Deserialize<TC>(string xml) where TC : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        object? result = serializer.Deserialize(reader);
        
        if(result is TC c)
        {
            return c;
        }

        return null;
    }
    
    /// <summary>
    /// Deserializes a list of XML strings into a list of instances of the specified type <typeparamref name="TC"/>.
    /// </summary>
    /// <typeparam name="TC">The type to deserialize the XML into. Must be a class.</typeparam>
    /// <param name="xmls">The list of XML strings to deserialize.</param>
    /// <returns>A list of deserialized objects of type <typeparamref name="TC"/>.</returns>
    public static List<TC> Deserialize<TC>(List<string> xmls) where TC : class
    {
        List<TC> result = [];
        foreach (var xml in xmls)
        {
            var r = Deserialize<TC>(xml);
            if(r != null) result.Add(r);
        }
        return result;
    }
}
