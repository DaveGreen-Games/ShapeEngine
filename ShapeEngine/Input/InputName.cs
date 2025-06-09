namespace ShapeEngine.Input;

/// <summary>
/// Represents a named input associated with a specific input device type.
/// </summary>
public readonly struct InputName
{
    /// <summary>
    /// The name of the input.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The type of input device associated with this input.
    /// </summary>
    public readonly InputDeviceType InputDeviceType;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputName"/> struct.
    /// </summary>
    /// <param name="name">The name of the input.</param>
    /// <param name="inputDeviceType">The type of input device.</param>
    public InputName(string name, InputDeviceType inputDeviceType)
    {
        Name = name;
        InputDeviceType = inputDeviceType;
    }
}