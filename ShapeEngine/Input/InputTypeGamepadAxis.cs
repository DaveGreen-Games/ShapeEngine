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
        sb.Append(ShapeGamepadDevice.GetAxisName(axis, shorthand));// GetGamepadAxisName(axis, shorthand));
        return sb.ToString();
    }

    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }

    public InputState GetState(ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.GetState(axis, deadzone, modifierOperator, modifierKeys) : new();
        //return GetState(axis, gamepad, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.GetState(axis, prev, deadzone, modifierOperator, modifierKeys) : new();
        // return GetState(axis, prev, gamepad, deadzone, modifierOperator, modifierKeys);
    }

    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;
    public IInputType Copy() => new InputTypeGamepadAxis(axis);

}