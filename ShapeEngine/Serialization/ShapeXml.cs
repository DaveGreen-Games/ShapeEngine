using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ShapeEngine.Serialization;

public class ShapeXml <T> where T : class
{
    private readonly XmlSerializer serializer;
    private readonly XmlSerializerNamespaces namespaces;
    private readonly XmlWriterSettings settings;
    
    public ShapeXml()
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
    
    
    public string Serialize(T instance)
    {
        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            serializer.Serialize(writer, instance, namespaces);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

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
}
