namespace ShapeEngine.Input;

public class ModifierKeyMouseButton : IModifierKey
{
    private readonly ShapeMouseButton modifier;
    private readonly bool reverseModifier;
    
    public ModifierKeyMouseButton(ShapeMouseButton modifierKey, bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    public bool IsActive(ShapeGamepadDevice? gamepad = null) => ShapeInput.MouseDevice.IsModifierActive(modifier, reverseModifier);

    public string GetName(bool shorthand = true) => reverseModifier ? "" : ShapeMouseDevice.GetButtonName(modifier, shorthand);
}