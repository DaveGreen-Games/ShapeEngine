using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse button, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with the specified button and deadzone.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone = 0f)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with button, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with button, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = [modifierKey];
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { deadzone = value; }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(button, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(button, prev, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(MouseDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseButton(button);
    
    private bool Equals(InputTypeMouseButton other)
    {
        return button == other.button && modifierKeys.Equals(other.modifierKeys) && modifierOperator == other.modifierOperator;
    }
    
    /// <summary>
    /// Determines whether the specified <see cref="IInputType"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The other <see cref="IInputType"/> to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    public bool Equals(IInputType? other)
    {
        if (other is InputTypeMouseButton inputType)
        {
            return Equals(inputType);       
        }
    
        return false;
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is InputTypeMouseButton other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)button, modifierKeys, (int)modifierOperator);
    }
}