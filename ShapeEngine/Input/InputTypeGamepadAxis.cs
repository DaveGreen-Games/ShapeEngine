using System.Text;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeGamepadAxis : IInputType
{
    private readonly ShapeGamepadAxis axis;
    private float deadzone;
    private readonly ShapeGamepadButton modifier;
    private readonly bool reverseModifier;
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone = 0.1f, ShapeGamepadButton modifierKey = ShapeGamepadButton.NONE, bool reverseModifier = false)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }

    public virtual string GetName(bool shorthand = true)
    {
        if(modifier == ShapeGamepadButton.NONE || reverseModifier) return GetGamepadAxisName(axis, shorthand);
        StringBuilder sb = new();

        sb.Append(InputTypeGamepadButton.GetGamepadButtonName(modifier, shorthand));
        sb.Append('+');
        sb.Append(GetGamepadAxisName(axis, shorthand));
        
        return sb.ToString();
    }

    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }

    public InputState GetState(int gamepad = -1)
    {
        return GetState(axis, gamepad, deadzone, modifier, reverseModifier);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        return GetState(axis, prev, gamepad, deadzone, modifier, reverseModifier);
    }

    public InputDevice GetInputDevice() => InputDevice.Gamepad;
    public IInputType Copy() => new InputTypeGamepadAxis(axis);

    private static float GetValue(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.1f, ShapeGamepadButton modifier = ShapeGamepadButton.NONE, bool reverseModifier = false)
    {
        if (gamepad < 0) return 0f;
        if (modifier != ShapeGamepadButton.NONE)
        {
            if(InputTypeGamepadButton.IsDown(modifier, gamepad, deadzone) == reverseModifier) return 0f;
        } 
        float value = GetGamepadAxisMovement(gamepad, (int)axis);
        if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        {
            value = (value + 1f) / 2f;
        }
        return MathF.Abs(value) < deadzone ? 0f : value;
    }
    public static InputState GetState(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.1f, ShapeGamepadButton modifier = ShapeGamepadButton.NONE, bool reverseModifier = false)
    {
        float axisValue = GetValue(axis, gamepad, deadzone, modifier, reverseModifier);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, gamepad, InputDevice.Gamepad);
    }
    public static InputState GetState(ShapeGamepadAxis axis, InputState previousState, int gamepad,
        float deadzone = 0.1f, ShapeGamepadButton modifier = ShapeGamepadButton.NONE, bool reverseModifier = false)
    {
        return new(previousState, GetState(axis, gamepad, deadzone, modifier, reverseModifier));
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