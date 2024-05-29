namespace ShapeEngine.Input;

public interface IInputType
{
    public IInputType Copy();
    public float GetDeadzone();
    public void SetDeadzone(float value);
    /*public float GetSensitivity();
    /// <summary>
    /// How fast the axis goes to max value (-1/1) when input is detected.
    /// </summary>
    /// <param name="seconds">How many seconds it takes.</param>
    public void SetSensitivity(float seconds);
    public float GetGravitiy();
    /// <summary>
    /// How fast the axis goes back to 0 when no input is detected anymore.
    /// </summary>
    /// <param name="seconds">How many seconds it takes.</param>
    public void SetGravitiy(float seconds);*/
    public InputState GetState(ShapeGamepadDevice? gamepad = null);
    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null);
    public string GetName(bool shorthand = true);
    public InputDeviceType GetInputDevice();

    public static IInputType Create(ShapeKeyboardButton button) => new InputTypeKeyboardButton(button);
    public static IInputType Create(ShapeMouseButton button) => new InputTypeMouseButton(button);
    public static IInputType Create(ShapeGamepadButton button, float deadzone = 0.2f) => new InputTypeGamepadButton(button, deadzone);
    public static IInputType Create(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => new InputTypeKeyboardButtonAxis(neg, pos);
    public static IInputType Create(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0.2f) => new InputTypeMouseButtonAxis(neg, pos, deadzone);
    public static IInputType Create(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f) => new InputTypeGamepadButtonAxis(neg, pos, deadzone);
    public static IInputType Create(ShapeMouseWheelAxis mouseWheelAxis, float deadzone = 0.2f) => new InputTypeMouseWheelAxis(mouseWheelAxis, deadzone);
    public static IInputType Create(ShapeMouseAxis mouseAxis, float deadzone = 0.2f) => new InputTypeMouseAxis(mouseAxis, deadzone);
    public static IInputType Create(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f) => new InputTypeGamepadAxis(gamepadAxis, deadzone);
}
