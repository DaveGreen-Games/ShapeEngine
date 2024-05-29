namespace ShapeEngine.Input;

public class InputEvent
{
    public readonly InputDeviceType Type;
    public readonly ShapeKeyboardButton KeyboardButton;
    public readonly ShapeMouseButton MouseButton;
    public readonly ShapeGamepadButton GamepadButton;
    public readonly ShapeGamepadDevice? Gamepad;

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
        Type = InputDeviceType.Keyboard;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = button;
        GamepadButton = ShapeGamepadButton.NONE;
        Gamepad = null;
    }
    public InputEvent(ShapeGamepadDevice gamepad, ShapeGamepadButton button)
    {
        Type = InputDeviceType.Keyboard;
        KeyboardButton = ShapeKeyboardButton.NONE;
        MouseButton = ShapeMouseButton.NONE;
        GamepadButton = button;
        Gamepad = gamepad;
    }
}