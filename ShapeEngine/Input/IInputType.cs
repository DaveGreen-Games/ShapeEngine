using ShapeEngine.Core;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type abstraction for various input devices (keyboard, mouse, gamepad).
/// Provides methods for copying, deadzone management, state retrieval, and device identification.
/// </summary>
public interface IInputType : IEquatable<IInputType>, ICopyable<IInputType>
{
    
    /// <summary>
    /// Gets the deadzone value for this input type.
    /// </summary>
    /// <returns>The deadzone as a float.</returns>
    public float GetDeadzone();

    /// <summary>
    /// Sets the deadzone value for this input type.
    /// </summary>
    /// <param name="value">The deadzone value to set.</param>
    public void SetDeadzone(float value);

    /// <summary>
    /// Gets the current input state for this input type.
    /// </summary>
    /// <param name="gamepad">Optional gamepad device to query state from.</param>
    /// <returns>The current <see cref="InputState"/>.</returns>
    public InputState GetState(GamepadDevice? gamepad = null);

    /// <summary>
    /// Gets the current input state for this input type, using a previous state for comparison.
    /// </summary>
    /// <param name="prev">The previous input state.</param>
    /// <param name="gamepad">Optional gamepad device to query state from.</param>
    /// <returns>The current <see cref="InputState"/>.</returns>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null);

    /// <summary>
    /// Gets the name of this input type.
    /// </summary>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The name as a string.</returns>
    public string GetName(bool shorthand = true);

    /// <summary>
    /// Gets the input device type associated with this input type.
    /// </summary>
    /// <returns>The <see cref="InputDeviceType"/>.</returns>
    public InputDeviceType GetInputDevice();

}
