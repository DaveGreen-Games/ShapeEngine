namespace ShapeEngine.Input;

/// <summary>
/// Represents an input event from a keyboard, mouse, or gamepad device.
/// </summary>
public class InputEvent
{
    /// <summary>
    /// The type of input device that generated the event.
    /// </summary>
    public readonly InputDeviceType Type;

    /// <summary>
    /// The keyboard button associated with the event, if applicable.
    /// </summary>
    public readonly ShapeKeyboardButton KeyboardButton;

    /// <summary>
    /// The mouse button associated with the event, if applicable.
    /// </summary>
    public readonly ShapeMouseButton MouseButton;

    /// <summary>
    /// The gamepad button associated with the event, if applicable.
    /// </summary>
    public readonly ShapeGamepadButton GamepadButton;

    /// <summary>
    /// The gamepad device associated with the event, if applicable.
    /// </summary>
    public readonly GamepadDevice? Gamepad;

    /// <summary>
    /// Gets a value indicating whether the event is from a keyboard.
    /// </summary>
    public bool IsKeyboard => Type == InputDeviceType.Keyboard;

    /// <summary>
    /// Gets a value indicating whether the event is from a mouse.
    /// </summary>
    public bool IsMouse => Type == InputDeviceType.Mouse;

    /// <summary>
    /// Gets a value indicating whether the event is from a gamepad.
    /// </summary>
    public bool IsGamepad => Type == InputDeviceType.Gamepad;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputEvent"/> class for a keyboard button event.
    /// </summary>
    /// <param name="button">The keyboard button associated with the event.</param>
    public InputEvent(ShapeKeyboardButton button)
    {
        Type = InputDeviceType.Keyboard;
        KeyboardButton = button;
        MouseButton = ShapeMouseButton.NONE;
        GamepadButton = ShapeGamepadButton.NONE;
        Gamepad = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputEvent"/> class for a mouse button event.
    /// </summary>
    /// <param name="button">The mouse button associated with the event.</param>
    public InputEvent(ShapeMouseButton button)
    {
        Type = InputDeviceType.Mouse;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = button;
        GamepadButton = ShapeGamepadButton.NONE;
        Gamepad = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputEvent"/> class for a gamepad button event.
    /// </summary>
    /// <param name="gamepad">The gamepad device associated with the event.</param>
    /// <param name="button">The gamepad button associated with the event.</param>
    public InputEvent(GamepadDevice gamepad, ShapeGamepadButton button)
    {
        Type = InputDeviceType.Gamepad;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = ShapeMouseButton.NONE;
        GamepadButton = button;
        Gamepad = gamepad;
    }
}