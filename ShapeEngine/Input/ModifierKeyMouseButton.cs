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
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public bool IsActive(int gamepad = -1) => IsKeyDown((int)modifier) != reverseModifier;

    public string GetName(bool shorthand = true) => reverseModifier ? "" : InputTypeMouseButton.GetMouseButtonName(modifier, shorthand);
}