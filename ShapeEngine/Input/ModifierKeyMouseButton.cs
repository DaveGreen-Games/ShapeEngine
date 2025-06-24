namespace ShapeEngine.Input;

/// <summary>
/// Represents a modifier key that is mapped to a mouse button.
/// </summary>
public class ModifierKeyMouseButton : IModifierKey
{
    private readonly ShapeMouseButton modifier;
    private readonly bool reverseModifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeyMouseButton"/> class.
    /// </summary>
    /// <remarks>
    /// The <paramref name="reverseModifier"/> parameter is used to prevent an input from being triggered when a modifier combination is pressed.
    /// For example, if you have two inputs: LMB + Space and Space, configuring input Space with LMB as a reverse modifier ensures that pressing LMB + Space will not also trigger Space.
    /// </remarks>
    /// <param name="modifierKey">The mouse button to use as a modifier.</param>
    /// <param name="reverseModifier">If set to <c>true</c>, the modifier is considered active when the button is not pressed.</param>
    public ModifierKeyMouseButton(ShapeMouseButton modifierKey, bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }

    /// <summary>
    /// Gets the input device type associated with this modifier key.
    /// </summary>
    /// <returns>The <see cref="InputDeviceType.Mouse"/> value.</returns>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <summary>
    /// Determines whether the modifier key is currently active on the mouse device.
    /// </summary>
    /// <param name="gamepad">Unused. Present for interface compatibility.</param>
    /// <returns><c>true</c> if the modifier is active; otherwise, <c>false</c>.</returns>
    public bool IsActive(ShapeGamepadDevice? gamepad = null) => ShapeInput.MouseDevice.IsModifierActive(modifier, reverseModifier);

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If set to <c>true</c>, returns the shorthand name.</param>
    /// <returns>The name of the modifier key, or an empty string if <c>reverseModifier</c> is <c>true</c>.</returns>
    public string GetName(bool shorthand = true) => reverseModifier ? "" : ShapeMouseDevice.GetButtonName(modifier, shorthand);
}