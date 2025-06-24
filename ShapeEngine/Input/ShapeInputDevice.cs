namespace ShapeEngine.Input;

/// <summary>
/// Represents a generic input device interface for the ShapeEngine input system.
/// Provides methods for device usage tracking, locking, updating, and calibration.
/// </summary>
public interface ShapeInputDevice
{
    /// <summary>
    /// Returns whether the device was used in the last update.
    /// </summary>
    /// <returns>True if the device was used, otherwise false.</returns>
    bool WasUsed();

    /// <summary>
    /// Updates the device state.
    /// </summary>
    void Update();

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
