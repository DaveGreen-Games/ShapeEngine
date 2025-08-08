/*namespace ShapeEngine.Input;

/// <summary>
/// Abstract base class for all input devices in the ShapeEngine input system.
/// Implements comparison based on device processing priority and unique device ID.
/// Includes methods for updating device state, checking if the device is active, and retrieving the device type.
/// </summary>
public abstract class InputDeviceBase : IComparable<InputDeviceBase>
{
    protected static uint processPriorityCounter = 0;
    private static uint idCounter = 0;
    private readonly uint id = idCounter++;
    
    /// <summary>
    /// Gets the priority of the input device.
    /// Devices with lower priority values are processed before those with higher values.
    /// </summary>
    /// <returns>The priority value of the device.</returns>
    public abstract uint GetDeviceProcessPriority();
    
    /// <summary>
    /// Gets the type of this input device.
    /// </summary>
    /// <returns>The <see cref="InputDeviceType"/> representing the device type.</returns>
    public abstract InputDeviceType GetDeviceType();
    
    /// <summary>
    /// Applies the specified change settings to this input device,
    /// modifying how device usage is detected and processed.
    /// </summary>
    /// <param name="settings">The change settings to apply to the input device.</param>
    public abstract void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings);
    
    /// <summary>
    /// Updates the device state.
    /// Receives input if another device was used as well in this frame.
    /// </summary>
    /// <returns>
    /// If this device was used this frame. Takes <see cref="InputDeviceUsageDetectionSettings"/> into account.
    /// </returns>
    public abstract bool Update(float dt, bool otherDeviceUsed);
    

    /// <summary>
    /// Indicates whether the input device is currently attached to <see cref="ShapeInput"/> making it available for input processing.
    /// </summary>
    public abstract bool IsAttached();
    
    /// <summary>
    /// Attaches the input device, making it available for input processing.
    /// This function is called by <see cref="ShapeInput"/> when a new device is attached.
    /// </summary>
    internal abstract void Attach();
    
    /// <summary>
    /// Detaches the input device, making it unavailable for input processing.
    /// This functions is called by <see cref="ShapeInput"/> when a currently attached device has to be detached,
    /// because a new device has been attached.
    /// </summary>
    internal abstract void Detach();
    
    /// <summary>
    /// Compares this input device with another based on priority and unique ID.
    /// Devices with lower priority values come first than  those with higher values.
    /// If priorities are equal, comparison falls back to the unique device ID.
    /// </summary>
    public int CompareTo(InputDeviceBase? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        int priorityComparison = GetDeviceProcessPriority().CompareTo(other.GetDeviceProcessPriority());
        return priorityComparison != 0 ? priorityComparison : id.CompareTo(other.id);
    }
}*/