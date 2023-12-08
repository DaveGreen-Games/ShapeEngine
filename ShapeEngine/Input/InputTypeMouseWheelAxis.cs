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
        sb.Append(ShapeMouseDevice.GetWheelAxisName(axis, shorthand));
        return sb.ToString();
    }

    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.GetState(axis, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.GetState(axis, prev, deadzone, modifierOperator, modifierKeys);
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

    
}