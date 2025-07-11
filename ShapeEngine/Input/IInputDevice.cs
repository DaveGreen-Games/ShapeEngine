namespace ShapeEngine.Input;

//TODO: change this to an abstract class or base class -> interface makes no sense because there are only 3 supported input devices and all of them are hard coded
//there is no generic system in place


/// <summary>
/// Represents a generic input device interface for the ShapeEngine input system.
/// Provides methods for device usage tracking, locking, updating, and calibration.
/// </summary>
public interface IInputDevice
{
    /// <summary>
    /// Gets the type of this input device.
    /// </summary>
    /// <returns>The <see cref="InputDeviceType"/> representing the device type.</returns>
    InputDeviceType GetDeviceType();
    /// <summary>
    /// Applies the specified change settings to this input device,
    /// modifying how device usage is detected and processed.
    /// </summary>
    /// <param name="settings">The change settings to apply to the input device.</param>
    void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings);
    
    /// <summary>
    /// Indicates whether the device registered any input during the last update cycle,
    /// taking into account any filters or settings applied to the device with <see cref="InputDeviceUsageDetectionSettings"/>.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    bool WasUsed();

    /// <summary>
    /// Determines if the device registered any input during the last update cycle,
    /// ignoring any filters or settings specified by <see cref="InputDeviceUsageDetectionSettings"/>.
    /// Useful for detecting raw device activity.
    /// </summary>
    /// <returns><c>true</c> if the device was used in the last update; otherwise, <c>false</c>.</returns>
    bool WasUsedRaw();

    /// <summary>
    /// Updates the device state.
    /// Receives input if another device was used as well in this frame.
    /// </summary>
    /// <returns>
    /// If this device was used this frame. Takes <see cref="InputDeviceUsageDetectionSettings"/> into account.
    /// </returns>
    bool Update(float dt, bool otherDeviceUsed);

    /// <summary>
    /// Returns whether the device is currently locked.
    /// </summary>
    /// <returns>True if the device is locked, otherwise false.</returns>
    bool IsLocked();

    /// <summary>
    /// Locks the device, preventing input from being registered.
    /// </summary>
    void Lock();

    /// <summary>
    /// Unlocks the device, allowing input to be registered.
    /// </summary>
    void Unlock();

    /// <summary>
    /// Calibrates the device, if supported.
    /// </summary>
    void Calibrate();
}
