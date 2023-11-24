using System.Text;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeGamepadAxis : IInputType
{
    private readonly ShapeGamepadAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone = 0.1f)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
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
        return GetState(axis, gamepad, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        return GetState(axis, prev, gamepad, deadzone, modifierOperator, modifierKeys);
    }

    public InputDevice GetInputDevice() => InputDevice.Gamepad;
    public IInputType Copy() => new InputTypeGamepadAxis(axis);

    private static float GetValue(ShapeGamepadAxis axis, int gamepad, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (gamepad < 0) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, gamepad)) return 0f;

        return GetValue(axis, gamepad, deadzone);
    }
    private static float GetValue(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.1f)
    {
        if (gamepad < 0) return 0f;
        float value = GetGamepadAxisMovement(gamepad, (int)axis);
        if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        {
            value = (value + 1f) / 2f;
        }
        return MathF.Abs(value) < deadzone ? 0f : value;
    }
    public static InputState GetState(ShapeGamepadAxis axis, int gamepad, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axisValue = GetValue(axis, gamepad, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, gamepad, InputDevice.Gamepad);
    }
    public static InputState GetState(ShapeGamepadAxis axis, InputState previousState, int gamepad, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(axis, gamepad, deadzone, modifierOperator, modifierKeys));
    }
    public static InputState GetState(ShapeGamepadAxis axis, int gamepad, float deadzone = 0.1f)
    {
        float axisValue = GetValue(axis, gamepad, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, gamepad, InputDevice.Gamepad);
    }
    public static InputState GetState(ShapeGamepadAxis axis, InputState previousState, int gamepad,float deadzone = 0.1f)
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