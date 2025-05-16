using System.Reflection.Metadata;
using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

public class InputTypeGamepadButton : IInputType
{
    private readonly ShapeGamepadButton button;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = modifierKeys;
        this.modifierOperator = modifierOperator;
    }
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public IInputType Copy() => new InputTypeGamepadButton(button, deadzone);
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeGamepadDevice.GetButtonName(button, shorthand)); // GetGamepadButtonName(button, shorthand));
        return sb.ToString();
    }

    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }
    public InputState GetState(ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.CreateInputState(button, deadzone, modifierOperator, modifierKeys) : new();
        // return GetState(button, gamepad, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.CreateInputState(button, prev, deadzone, modifierOperator, modifierKeys) : new();
        // return GetState(button, prev, gamepad, deadzone, modifierOperator, modifierKeys);
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;
    
}