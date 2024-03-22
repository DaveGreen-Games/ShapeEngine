using System.Text;

namespace ShapeEngine.Input;

public sealed class InputTypeKeyboardButton : IInputType
{
    private readonly ShapeKeyboardButton button;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeKeyboardButton(ShapeKeyboardButton button)
    {
        this.button = button;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeKeyboardButton(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeKeyboardButton(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }
    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        //if (gamepad != null) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(button, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        //if (gamepad != null) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(button, prev, modifierOperator, modifierKeys);
    }
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeKeyboardDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;
    public IInputType Copy() => new InputTypeKeyboardButton(button);
    
    
    
}