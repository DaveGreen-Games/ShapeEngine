namespace ShapeEngine.Input;

/// <summary>
/// Represents a modifier key that is mapped to a gamepad button.
/// </summary>
public class ModifierKeyGamepadButton : IModifierKey
{
    private readonly ShapeGamepadButton modifier;
    private readonly bool reverseModifier;
    private readonly float joyAxisDeadzone;
    private readonly float triggerAxisDeadzone;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeyGamepadButton"/> class.
    /// </summary>
    /// <param name="modifierKey">The gamepad button to use as a modifier.</param>
    /// <param name="joyAxisDeadzone">The deadzone threshold for joystick axes,
    /// used to ignore minor input noise.</param>
    /// <param name="triggerAxisDeadzone">The deadzone threshold for trigger axes,
    /// used to ignore minor input noise.</param>
    /// <param name="reverseModifier">If set to <c>true</c>, the modifier is considered active when the button is not pressed.</param>
    /// <remarks>
    /// The <paramref name="reverseModifier"/> parameter is used to prevent an input from being triggered when a modifier combination is pressed.
    /// For example, if you have two inputs: RB + A and A, configuring input A with RB as a reverse modifier ensures that pressing RB + A will not also trigger A.
    /// </remarks>
    public ModifierKeyGamepadButton(ShapeGamepadButton modifierKey, 
        float joyAxisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold,
        float triggerAxisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,
        bool reverseModifier = false)
    {
        this.modifier = modifierKey;
        this.joyAxisDeadzone = joyAxisDeadzone;
        this.triggerAxisDeadzone = triggerAxisDeadzone;
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
    public bool IsActive(GamepadDevice? gamepad) => gamepad != null && gamepad.IsDown(modifier, joyAxisDeadzone, triggerAxisDeadzone) && !reverseModifier;

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If set to <c>true</c>, returns the shorthand name.</param>
    /// <returns>The name of the modifier key, or an empty string if <c>reverseModifier</c> is <c>true</c>.</returns>
    public string GetName(bool shorthand = true)
    {
        return reverseModifier ? $"Not-{modifier.GetButtonName(shorthand)}" : modifier.GetButtonName(shorthand);
    }

    /// <summary>
    /// Creates a copy of this <see cref="ModifierKeyGamepadButton"/> instance.
    /// </summary>
    /// <returns>A new <see cref="IModifierKey"/> with the same modifier and reverseModifier values.</returns>
    public IModifierKey Copy() => new  ModifierKeyGamepadButton(modifier, joyAxisDeadzone, triggerAxisDeadzone, reverseModifier);

    /// <summary>
    /// Determines whether the specified <see cref="ModifierKeyGamepadButton"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ModifierKeyGamepadButton"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    protected bool Equals(ModifierKeyGamepadButton other)
    {
        return modifier == other.modifier && reverseModifier == other.reverseModifier;
    }

    /// <summary>
    /// Determines whether the specified <see cref="IModifierKey"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="IModifierKey"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(IModifierKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;
        return Equals((ModifierKeyGamepadButton)other);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ModifierKeyGamepadButton)obj);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)modifier, reverseModifier);
    }
}