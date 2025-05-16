using System.Numerics;
using System.Text;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

public class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    private float deadzone = 0f;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    public InputTypeMouseButton(ShapeMouseButton button, float deadzone = 0f)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value) { deadzone = value; }
    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.CreateInputState(button, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.CreateInputState(button, prev, deadzone, modifierOperator, modifierKeys);
    }
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeMouseDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    public IInputType Copy() => new InputTypeMouseButton(button);
        
        
    
}