using System.Text;
using ShapeEngine.Screen;

namespace ShapeEngine.Input;

public class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos);
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        
        string negName = ShapeKeyboardDevice.GetButtonName(neg, shorthand);
        string posName = ShapeKeyboardDevice.GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }

    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(neg, pos, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(neg, pos, prev, modifierOperator, modifierKeys);
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;
    
    
}