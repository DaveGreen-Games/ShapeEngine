namespace ShapeEngine.Input;

public class ModifierKeyGamepadButton : IModifierKey
{
    private readonly ShapeGamepadButton modifier;
    private readonly bool reverseModifier;
    
    public ModifierKeyGamepadButton(ShapeGamepadButton modifierKey, bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }
    public InputDevice GetInputDevice() => InputDevice.Gamepad;
    public bool IsActive(int gamepad) => InputTypeGamepadButton.IsDown(modifier, gamepad) != reverseModifier;

    public string GetName(bool shorthand = true) => reverseModifier ? "" : InputTypeGamepadButton.GetGamepadButtonName(modifier, shorthand);
}