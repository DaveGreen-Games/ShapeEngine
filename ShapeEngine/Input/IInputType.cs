namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type abstraction for various input devices (keyboard, mouse, gamepad).
/// Provides methods for copying, deadzone management, state retrieval, and device identification.
/// </summary>
public interface IInputType
{
    /// <summary>
    /// Creates a copy of this input type.
    /// </summary>
    /// <returns>A new instance of <see cref="IInputType"/> with the same configuration.</returns>
    public IInputType Copy();

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
    public InputState GetState(ShapeGamepadDevice? gamepad = null);

    /// <summary>
    /// Gets the current input state for this input type, using a previous state for comparison.
    /// </summary>
    /// <param name="prev">The previous input state.</param>
    /// <param name="gamepad">Optional gamepad device to query state from.</param>
    /// <returns>The current <see cref="InputState"/>.</returns>
    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null);

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

    /// <summary>
    /// Creates an input type for a keyboard button.
    /// </summary>
    public static IInputType Create(ShapeKeyboardButton button) => new InputTypeKeyboardButton(button);

    /// <summary>
    /// Creates an input type for a mouse button.
    /// </summary>
    public static IInputType Create(ShapeMouseButton button) => new InputTypeMouseButton(button);

    /// <summary>
    /// Creates an input type for a gamepad button.
    /// </summary>
    public static IInputType Create(ShapeGamepadButton button, float deadzone = 0.2f) => new InputTypeGamepadButton(button, deadzone);

    /// <summary>
    /// Creates an input type for a keyboard button axis (negative and positive).
    /// </summary>
    public static IInputType Create(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => new InputTypeKeyboardButtonAxis(neg, pos);

    /// <summary>
    /// Creates an input type for a mouse button axis (negative and positive).
    /// </summary>
    public static IInputType Create(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0.2f) => new InputTypeMouseButtonAxis(neg, pos, deadzone);

    /// <summary>
    /// Creates an input type for a gamepad button axis (negative and positive).
    /// </summary>
    public static IInputType Create(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f) => new InputTypeGamepadButtonAxis(neg, pos, deadzone);

    /// <summary>
    /// Creates an input type for a mouse wheel axis.
    /// </summary>
    public static IInputType Create(ShapeMouseWheelAxis mouseWheelAxis, float deadzone = 0.2f) => new InputTypeMouseWheelAxis(mouseWheelAxis, deadzone);

    /// <summary>
    /// Creates an input type for a mouse axis.
    /// </summary>
    public static IInputType Create(ShapeMouseAxis mouseAxis, float deadzone = 0.2f) => new InputTypeMouseAxis(mouseAxis, deadzone);

    /// <summary>
    /// Creates an input type for a gamepad axis.
    /// </summary>
    public static IInputType Create(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f) => new InputTypeGamepadAxis(gamepadAxis, deadzone);
}
