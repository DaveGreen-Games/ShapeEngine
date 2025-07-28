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
    public bool IsActive(GamepadDevice? gamepad = null) => ShapeInput.ActiveMouseDevice.IsDown(modifier) && !reverseModifier; //   ShapeInput.ActiveMouseDevice.IsModifierActive(modifier, reverseModifier);

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If set to <c>true</c>, returns the shorthand name.</param>
    /// <returns>The name of the modifier key, or an empty string if <c>reverseModifier</c> is <c>true</c>.</returns>
    public string GetName(bool shorthand = true) => reverseModifier ? "" : modifier.GetButtonName(shorthand);

    /// <summary>
    /// Creates a copy of this <see cref="ModifierKeyMouseButton"/> instance.
    /// </summary>
    /// <returns>A new <see cref="IModifierKey"/> with the same modifier and reverseModifier values.</returns>
    public IModifierKey Copy() => new ModifierKeyMouseButton(modifier, reverseModifier);
    
        /// <summary>
    /// Determines whether the specified <see cref="ModifierKeyMouseButton"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ModifierKeyMouseButton"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    protected bool Equals(ModifierKeyMouseButton other)
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
        return Equals((ModifierKeyMouseButton)other);
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