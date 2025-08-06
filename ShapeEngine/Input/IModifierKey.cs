using System.Text;
using ShapeEngine.Core;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a modifier key (such as Shift, Ctrl, Alt) for input handling.
/// </summary>
public interface IModifierKey : ICopyable<IModifierKey>, IEquatable<IModifierKey>
{
    /// <summary>
    /// Determines if the modifier key is currently active.
    /// </summary>
    /// <param name="gamepad">Optional gamepad device to check against.</param>
    /// <returns>True if the modifier key is active; otherwise, false.</returns>
    public bool IsActive(GamepadDevice? gamepad = null);

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If true, returns a shorthand name; otherwise, a full name.</param>
    /// <returns>The name of the modifier key.</returns>
    public string GetName(bool shorthand = true);

    /// <summary>
    /// Gets the input device type associated with this modifier key.
    /// </summary>
    /// <returns>The input device type.</returns>
    public InputDeviceType GetInputDevice();
}