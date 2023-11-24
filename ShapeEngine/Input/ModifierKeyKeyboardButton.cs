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

    public InputDevice GetInputDevice() => InputDevice.Keyboard;
    public bool IsActive(int gamepad = -1) => IsKeyDown((int)modifier) != reverseModifier;

    public string GetName(bool shorthand = true) => reverseModifier ? "" : InputTypeKeyboardButton.GetKeyboardButtonName(modifier, shorthand);
}