namespace ShapeEngine.Input;

public class ModifierKeyKeyboardButton : IModifierKey
{
    private readonly ShapeKeyboardButton modifier;
    private readonly bool reverseModifier;
    
    public ModifierKeyKeyboardButton(ShapeKeyboardButton modifierKey, bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }

    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;

    public bool IsActive(ShapeGamepadDevice? gamepad = null) => ShapeInput.KeyboardDevice.IsModifierActive(modifier, reverseModifier);

    public string GetName(bool shorthand = true) => reverseModifier ? "" : ShapeKeyboardDevice.GetButtonName(modifier, shorthand);
}