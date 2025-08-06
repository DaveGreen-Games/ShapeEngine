namespace ShapeEngine.Input;

/// <summary>
/// Represents a generic input device interface for the ShapeEngine input system.
/// Provides methods for device usage tracking, locking, and calibration.
/// </summary>
public abstract class InputDevice : InputDeviceBase
{
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
    
}