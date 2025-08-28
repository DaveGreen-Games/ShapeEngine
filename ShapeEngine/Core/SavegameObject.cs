namespace ShapeEngine.Core;

/// <summary>
/// Simple base class for a savegame object that can be serialized and deserialized.
/// To properly serialize and save instances of this class, every property must have a public getter and setter.
/// </summary>
/// <remarks>
/// This class provides common properties like version and name that are useful for savegame management.
/// Derive from this class to create custom savegame data structures.
/// </remarks>
public abstract class SavegameObject
{
    /// <summary>
    /// Gets or sets the version of the savegame format,
    /// useful for handling compatibility between different game versions.
    /// </summary>
    /// <remarks>Default value is "v0.0.0".</remarks>
    public string version { get; set; } = "v0.0.0";

    /// <summary>
    /// Gets or sets the name of the savegame object, can be used as an identifier.
    /// </summary>
    public string name { get; set; } = "";
}

