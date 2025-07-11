namespace ShapeEngine.Input;

/// <summary>
/// Represents a generic input device interface for the ShapeEngine input system.
/// Provides methods for device usage tracking, locking, updating, and calibration.
/// </summary>
public abstract class InputDevice
{
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
    /// Indicates whether the device registered any input during the last update cycle,
    /// taking into account any filters or settings applied to the device with <see cref="InputDeviceUsageDetectionSettings"/>.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    public abstract bool WasUsed();

    /// <summary>
    /// Determines if the device registered any input during the last update cycle,
    /// ignoring any filters or settings specified by <see cref="InputDeviceUsageDetectionSettings"/>.
    /// Useful for detecting raw device activity.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    public abstract bool WasUsedRaw();

    /// <summary>
    /// Updates the device state.
    /// Receives input if another device was used as well in this frame.
    /// </summary>
    /// <returns>
    /// If this device was used this frame. Takes <see cref="InputDeviceUsageDetectionSettings"/> into account.
    /// </returns>
    public abstract bool Update(float dt, bool otherDeviceUsed);

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
    /// Calibrates the device, if supported.
    /// </summary>
    public abstract void Calibrate();

    /// <summary>
    /// Indicates whether the input device is currently active and able to process input.
    /// </summary>
    public abstract bool IsActive();
    /// <summary>
    /// Called when the input device is being activated.
    /// </summary>
    public abstract void Activate();

    /// <summary>
    /// Called when the input device is being deactivated.
    /// </summary>
    public abstract void Deactivate();
}
