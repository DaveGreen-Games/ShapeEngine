using System.Numerics;
using System.Text;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

public sealed class InputTypeMouseAxis : IInputType
{
    private readonly ShapeMouseAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
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
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeMouseDevice.GetAxisName(axis, shorthand));
        return sb.ToString();
    }

    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.CreateInputState(axis, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.CreateInputState(axis, prev, deadzone, modifierOperator, modifierKeys);
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;
    public IInputType Copy() => new InputTypeMouseAxis(axis);

    

}