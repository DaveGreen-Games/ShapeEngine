using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeGamepadAxis : IInputType
{
    private readonly ShapeGamepadAxis axis;
    private float deadzone;

    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
    }

    public virtual string GetName(bool shorthand = true) => GetGamepadAxisName(axis, shorthand);

    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }

    public InputState GetState(int gamepad = -1)
    {
        return GetState(axis, gamepad, deadzone);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        return GetState(axis, prev, gamepad, deadzone);
    }

    public InputDevice GetInputDevice() => InputDevice.Gamepad;
    public IInputType Copy() => new InputTypeGamepadAxis(axis);

    private static float GetValue(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.2f)
    {
        if (gamepad < 0) return 0f;
        float value = GetGamepadAxisMovement(gamepad, (int)axis);
        if (MathF.Abs(value) < deadzone) return 0f;
        return value;
    }
    public static InputState GetState(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.2f)
    {
        float axisValue = GetValue(axis, gamepad, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, gamepad);
    }
    public static InputState GetState(ShapeGamepadAxis axis, InputState previousState, int gamepad,
        float deadzone = 0.2f)
    {
        return new(previousState, GetState(axis, gamepad, deadzone));
    }
    public static string GetGamepadAxisName(ShapeGamepadAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeGamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
            case ShapeGamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
            case ShapeGamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
            case ShapeGamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
            case ShapeGamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
            case ShapeGamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
            default: return "No Key";
        }
    }

}