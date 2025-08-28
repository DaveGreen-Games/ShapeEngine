using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ShapeEngine.Serialization;

/// <summary>
/// Provides XML serialization and deserialization for objects of type <typeparamref name="T"/>.
/// <para>
/// <typeparamref name="T"/> must inherit from <see cref="DataObject"/> and should be compatible with <see cref="XmlSerializer"/>.
/// </para>
/// </summary>
/// <typeparam name="T">
/// The type of object to serialize/deserialize. Must inherit from <see cref="DataObject"/> and be XML serializable.
/// </typeparam>
/// <remarks>
/// This class supports both instance and static methods for serialization and deserialization. The instance methods allow customization of <see cref="XmlWriterSettings"/> and <see cref="XmlSerializerNamespaces"/>.
/// The static methods provide convenience for quick serialization/deserialization without custom settings.
/// </remarks>
public class XmlDataObjectSerializer<T> where T : DataObject
{
    private readonly XmlSerializer serializer;
    private readonly XmlSerializerNamespaces namespaces;
    private readonly XmlWriterSettings settings;

    /// <summary>
    /// Initializes a new instance of <see cref="XmlDataObjectSerializer{T}"/> with default settings.
    /// </summary>
    public XmlDataObjectSerializer()
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
    /// Initializes a new instance of <see cref="XmlDataObjectSerializer{T}"/> with custom <paramref name="settings"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="XmlWriterSettings"/> to use for serialization. Allows customization of indentation, encoding, and XML declaration.
    /// </param>
    public XmlDataObjectSerializer(XmlWriterSettings settings)
    {
        serializer = new XmlSerializer(typeof(T));
        namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty); // no xmlns attribute

        this.settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="XmlDataObjectSerializer{T}"/> with custom <paramref name="settings"/> and <paramref name="namespaces"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="XmlWriterSettings"/> to use for serialization. Allows customization of indentation, encoding, and XML declaration.
    /// </param>
    /// <param name="namespaces">
    /// The <see cref="XmlSerializerNamespaces"/> to use for serialization. Allows control over XML namespace declarations.
    /// </param>
    public XmlDataObjectSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
        serializer = new XmlSerializer(typeof(T));
        this.namespaces = namespaces;
        this.settings = settings;
    }
    
    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to an XML string.
    /// </summary>
    /// <param name="instance">
    /// The object instance to serialize. Must not be null and must be compatible with <see cref="XmlSerializer"/>.
    /// </param>
    /// <returns>
    /// An XML string representation of the object.
    /// </returns>
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
    /// Serializes a list of objects of type <typeparamref name="T"/> to a list of XML strings.
    /// </summary>
    /// <param name="instances">
    /// The list of object instances to serialize. Each must be compatible with <see cref="XmlSerializer"/>.
    /// </param>
    /// <returns>
    /// A list of XML string representations for each object in <paramref name="instances"/>.
    /// </returns>
    public List<string> Serialize(DataObjectList<T> instances)
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }
    
    /// <summary>
    /// Deserializes an XML string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="xml">
    /// The XML string to deserialize. Must represent a valid XML for type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The deserialized object of type <typeparamref name="T"/>, or <c>null</c> if deserialization fails or type mismatch occurs.
    /// </returns>
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
    /// Deserializes a list of XML strings to a <see cref="DataObjectList{T}"/>.
    /// </summary>
    /// <param name="xmls">
    /// The list of XML strings to deserialize. Each must represent a valid XML for type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// A <see cref="DataObjectList{T}"/> containing deserialized objects. Invalid or mismatched XML entries are skipped.
    /// </returns>
    public DataObjectList<T> Deserialize(List<string> xmls)
    {
        DataObjectList<T> result = [];
        foreach (var xml in xmls)
        {
            var r = Deserialize(xml);
            if(r != null) result.Add(r);
        }
        return result;
    }
    
    /// <summary>
    /// Serializes an object of type <typeparamref name="TC"/> to an XML string. Static convenience method.
    /// </summary>
    /// <typeparam name="TC">
    /// The type of object to serialize. Must inherit from <see cref="DataObject"/> and be XML serializable.
    /// </typeparam>
    /// <param name="instance">
    /// The object instance to serialize. Must not be null and must be compatible with <see cref="XmlSerializer"/>.
    /// </param>
    /// <returns>
    /// An XML string representation of the object.
    /// </returns>
    public static string Serialize<TC>(TC instance) where TC : DataObject
    {
        var serializer = new XmlSerializer(typeof(TC));
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
    /// Serializes a list of objects of type <typeparamref name="TC"/> to a list of XML strings. Static convenience method.
    /// </summary>
    /// <typeparam name="TC">
    /// The type of object to serialize. Must inherit from <see cref="DataObject"/> and be XML serializable.
    /// </typeparam>
    /// <param name="instances">
    /// The list of object instances to serialize. Each must be compatible with <see cref="XmlSerializer"/>.
    /// </param>
    /// <returns>
    /// A list of XML string representations for each object in <paramref name="instances"/>.
    /// </returns>
    public static List<string> Serialize<TC>(List<TC> instances) where TC : DataObject
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }

    /// <summary>
    /// Deserializes an XML string to an object of type <typeparamref name="TC"/>. Static convenience method.
    /// </summary>
    /// <typeparam name="TC">
    /// The type of object to deserialize. Must inherit from <see cref="DataObject"/> and be XML serializable.
    /// </typeparam>
    /// <param name="xml">
    /// The XML string to deserialize. Must represent a valid XML for type <typeparamref name="TC"/>.
    /// </param>
    /// <returns>
    /// The deserialized object of type <typeparamref name="TC"/>, or <c>null</c> if deserialization fails or type mismatch occurs.
    /// </returns>
    public static TC? Deserialize<TC>(string xml) where TC : DataObject
    {
        var serializer = new XmlSerializer(typeof(TC));
        using var reader = new StringReader(xml);
        object? result = serializer.Deserialize(reader);
        
        if(result is TC c)
        {
            return c;
        }

        return null;
    }
    
    /// <summary>
    /// Deserializes a list of XML strings to a list of objects of type <typeparamref name="TC"/>. Static convenience method.
    /// </summary>
    /// <typeparam name="TC">
    /// The type of object to deserialize. Must inherit from <see cref="DataObject"/> and be XML serializable.
    /// </typeparam>
    /// <param name="xmls">
    /// The list of XML strings to deserialize. Each must represent a valid XML for type <typeparamref name="TC"/>.
    /// </param>
    /// <returns>
    /// A list of deserialized objects of type <typeparamref name="TC"/>. Invalid or mismatched XML entries are skipped.
    /// </returns>
    public static List<TC> Deserialize<TC>(List<string> xmls) where TC : DataObject
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