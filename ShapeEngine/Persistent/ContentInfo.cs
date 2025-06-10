namespace ShapeEngine.Persistent;

/// <summary>
/// Represents content information for a resource, including its file extension and binary data.
/// </summary>
public class ContentInfo
{
    /// <summary>
    /// The file extension of the content (e.g., ".png", ".wav", ".json").
    /// </summary>
    public string extension;

    /// <summary>
    /// The binary data of the content.
    /// </summary>
    public byte[] data;

    /// <summary>
    /// Initializes a new instance of the ContentInfo class with the specified extension and data.
    /// </summary>
    /// <param name="extension">The file extension of the content.</param>
    /// <param name="data">The binary data of the content.</param>
    public ContentInfo(string extension, byte[] data) { this.extension = extension; this.data = data; }
}