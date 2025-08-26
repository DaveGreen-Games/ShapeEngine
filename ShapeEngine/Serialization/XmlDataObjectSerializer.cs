using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ShapeEngine.Serialization;

public class XmlDataObjectSerializer<T> where T : DataObject
{
    private readonly XmlSerializer serializer;
    private readonly XmlSerializerNamespaces namespaces;
    private readonly XmlWriterSettings settings;

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
    
    public XmlDataObjectSerializer(XmlWriterSettings settings)
    {
        serializer = new XmlSerializer(typeof(T));
        namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty); // no xmlns attribute

        this.settings = settings;
    }

    public XmlDataObjectSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
    {
        serializer = new XmlSerializer(typeof(T));
        this.namespaces = namespaces;
        this.settings = settings;
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

    public List<string> Serialize(DataObjectList<T> instances)
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
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

    public static List<string> Serialize<TC>(List<TC> instances) where TC : DataObject
    {
        List<string> result = [];
        foreach (var instance in instances)
        {
            result.Add(Serialize(instance));
        }
        return result;
    }

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