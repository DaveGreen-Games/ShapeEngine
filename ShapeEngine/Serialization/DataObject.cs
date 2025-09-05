using System.Xml.Serialization;

namespace ShapeEngine.Serialization;


/// <summary>
/// Abstract base record for serializable data objects.
/// If used for XML serialization,
/// XML attributes have to be used and added to <see cref="Name"/> and <see cref="SpawnWeight"/> members
/// using new keyword (see XML code example).
/// </summary>
/// <remarks>
/// To be serializable by <see cref="XmlDataObjectSerializer{T}"/> or <see cref="JsonDataObjectSerializer{T}"/>
/// all members have to be public and have both getter and setter.
/// If used for XML serialization, XML attributes have to be used (See code example).
/// For instance the difference between XML attributes and XML elements is:
/// <list type="bullet">
/// <item>[XmlAttribute("Name")] -> XmlAttribute maps a property to an XML attribute, appearing inside the opening tag: <Person Name="John" Age="30" /></item>
/// <item>[XmlElement("Name")] -> XmlElement maps a property to an XML element, appearing as a child node: <Person><Name>John</Name><Age>30</Age></Person></item>
/// </list>
/// </remarks>
/// <code>
/// using System.Xml.Serialization;
/// //Valid examples for XML:
/// public record MyXmlDataObject : DataObject
/// {
///     //use new here to add XML attributes
///     [XmlAttribute("Name")]
///     public new string Name { get; set; } = string.Empty;
///
///     //use new here to add XML attributes
///     [XmlAttribute("SpawnWeight")]
///     public new int SpawnWeight { get; set; }
///     
///     [XmlElement("Health")]
///     public int Health {get; set;}
/// 
///     [XmlElement("EnemyType")]
///     public EnemyTypeEnum EnemyType {get; set;}
/// 
///     [XmlElement("ResourceSpawnChances")]
///     public MyList ResourceSpawnChances {get; set;}
/// }
/// </code>
/// <code>
/// //Valid examples for JSON:
/// public record MyJsonDataObject : DataObject
/// {
///     //No new keyword required for Name and SpawnWeight, because JSON does not use attributes!
/// 
///     public int Health {get; set;}
/// 
///     public EnemyTypeEnum EnemyType {get; set;}
/// 
///     public MyList ResourceSpawnChances {get; set;}
/// }
/// </code>
public abstract record DataObject
{
    /// <summary>
    /// The name of the data object. Required for serialization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The spawn weight used for object instantiation logic. Required for serialization.
    /// </summary>
    public int SpawnWeight { get; set; } 
}

