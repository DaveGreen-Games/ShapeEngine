namespace ShapeEngine.Input;

/// <summary>
/// Represents a generic input device interface for the ShapeEngine input system.
/// Provides methods for device usage tracking, locking, and calibration.
/// </summary>
public abstract class InputDevice : IComparable<InputDevice>
{
    protected static uint processPriorityCounter = 0;
    private static uint idCounter = 0;
    private readonly uint id = idCounter++;
    
    /// <summary>
    /// Indicates whether the device registered any input during the last update cycle,
    /// taking into account any filters or settings applied to the device with <see cref="InputSettings"/>.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    public abstract bool WasUsed();

    /// <summary>
    /// Determines if the device registered any input during the last update cycle,
    /// ignoring any filters or settings specified by <see cref="InputSettings"/>.
    /// Useful for detecting raw device activity.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    public abstract bool WasUsedRaw();

    /// <summary>
    /// Returns whether the device is currently locked.
    /// </summary>
    /// <returns>True if the device is locked, otherwise false.</returns>
    public abstract bool IsLocked();

    /// <summary>
    /// Locks the device, preventing input from being registered.
    /// </summary>
    public abstract void Lock();

    /// <summary>
    /// Unlocks the device, allowing input to be registered.
    /// </summary>
    public abstract void Unlock();
    
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
    internal abstract void ApplyInputDeviceChangeSettings(InputSettings settings);
    
    /// <summary>
    /// Updates the device state.
    /// Receives input if another device was used as well in this frame.
    /// </summary>
    /// <returns>
    /// If this device was used this frame. Takes <see cref="InputSettings"/> into account.
    /// </returns>
    public abstract bool Update(float dt, bool otherDeviceUsed);
    
    /// <summary>
    /// Compares this input device with another based on priority and unique ID.
    /// Devices with lower priority values come first than  those with higher values.
    /// If priorities are equal, comparison falls back to the unique device ID.
    /// </summary>
    public int CompareTo(InputDevice? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        int priorityComparison = GetDeviceProcessPriority().CompareTo(other.GetDeviceProcessPriority());
        return priorityComparison != 0 ? priorityComparison : id.CompareTo(other.id);
    }
    
}