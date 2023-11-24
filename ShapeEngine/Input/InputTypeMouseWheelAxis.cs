using System.Numerics;
using System.Text;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(GetMouseWheelAxisName(axis, shorthand));
        return sb.ToString();
    }

    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, prev, deadzone, modifierOperator, modifierKeys);
    }
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

    private static float GetValue(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, -1)) return 0f;
        return GetValue(axis, deadzone);
    }
    private static float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        if (!ShapeLoop.CursorOnScreen) return 0f;
        
        Vector2 value = GetMouseWheelMoveV();
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDevice.Mouse);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(axis, deadzone, modifierOperator, modifierKeys));
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDevice.Mouse);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, GetState(axis, deadzone));
    }
    public static string GetMouseWheelAxisName(ShapeMouseWheelAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
            case ShapeMouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
            default: return "No Key";
        }
    }

}