namespace ShapeEngine.Input;

public class InputEvent
{
    public readonly InputDeviceType Type;
    public readonly ShapeKeyboardButton KeyboardButton;
    public readonly ShapeMouseButton MouseButton;
    public readonly ShapeGamepadButton GamepadButton;
    public readonly ShapeGamepadDevice? Gamepad;

    public bool IsKeyboard => Type == InputDeviceType.Keyboard;
    public bool IsMouse => Type == InputDeviceType.Mouse;
    public bool IsGamepad => Type == InputDeviceType.Gamepad;

    public InputEvent(ShapeKeyboardButton button)
    {
        Type = InputDeviceType.Keyboard;
        KeyboardButton = button;
        MouseButton = ShapeMouseButton.NONE;
        GamepadButton = ShapeGamepadButton.NONE;
        Gamepad = null;
    }
    public InputEvent(ShapeMouseButton button)
    {
        Type = InputDeviceType.Mouse;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = button;
        GamepadButton = ShapeGamepadButton.NONE;
        Gamepad = null;
    }
    public InputEvent(ShapeGamepadDevice gamepad, ShapeGamepadButton button)
    {
        Type = InputDeviceType.Gamepad;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = ShapeMouseButton.NONE;
        GamepadButton = button;
        Gamepad = gamepad;
    }
}