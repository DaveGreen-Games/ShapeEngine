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
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    public bool IsActive(ShapeGamepadDevice? gamepad) => gamepad != null && gamepad.IsModifierActive(modifier, reverseModifier);

    public string GetName(bool shorthand = true) => reverseModifier ? "" : ShapeGamepadDevice.GetButtonName(modifier, shorthand);
}