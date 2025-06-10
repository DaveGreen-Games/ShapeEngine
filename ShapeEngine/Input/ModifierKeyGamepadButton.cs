namespace ShapeEngine.Input;

/// <summary>
/// Represents a modifier key that is mapped to a gamepad button.
/// </summary>
public class ModifierKeyGamepadButton : IModifierKey
{
    private readonly ShapeGamepadButton modifier;
    private readonly bool reverseModifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeyGamepadButton"/> class.
    /// </summary>
    /// <remarks>
    /// The <paramref name="reverseModifier"/> parameter is used to prevent an input from being triggered when a modifier combination is pressed.
    /// For example, if you have two inputs: RB + A and A, configuring input A with RB as a reverse modifier ensures that pressing RB + A will not also trigger A.
    /// </remarks>
    /// <param name="modifierKey">The gamepad button to use as a modifier.</param>
    /// <param name="reverseModifier">If set to <c>true</c>, the modifier is considered active when the button is not pressed.</param>
    public ModifierKeyGamepadButton(ShapeGamepadButton modifierKey, bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }

    /// <summary>
    /// Gets the input device type associated with this modifier key.
    /// </summary>
    /// <returns>The <see cref="InputDeviceType.Gamepad"/> value.</returns>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <summary>
    /// Determines whether the modifier key is currently active on the specified gamepad device.
    /// </summary>
    /// <param name="gamepad">The gamepad device to check.</param>
    /// <returns><c>true</c> if the modifier is active; otherwise, <c>false</c>.</returns>
    public bool IsActive(ShapeGamepadDevice? gamepad) => gamepad != null && gamepad.IsModifierActive(modifier, reverseModifier);

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If set to <c>true</c>, returns the shorthand name.</param>
    /// <returns>The name of the modifier key, or an empty string if <c>reverseModifier</c> is <c>true</c>.</returns>
    public string GetName(bool shorthand = true) => reverseModifier ? "" : ShapeGamepadDevice.GetButtonName(modifier, shorthand);
}