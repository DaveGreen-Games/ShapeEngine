using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse button, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;


/// <summary>
    /// Initializes a new instance of the <see cref="InputTypeMouseButton"/> class.
    /// </summary>
    /// <param name="button">The mouse button to represent.</param>
    /// <param name="deadzone">The deadzone threshold for input detection.</param>
    /// <param name="modifierKeySet">Optional set of modifier keys required for activation.</param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseThreshold, ModifierKeySet? modifierKeySet = null)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { deadzone = value; }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null) => ShapeInput.ActiveMouseDevice.CreateInputState(button, deadzone, deadzone,  modifierKeySet);

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null) => ShapeInput.ActiveMouseDevice.CreateInputState(button, prev, deadzone,  deadzone, modifierKeySet);
    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append(button.GetButtonName(shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseButton(button, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeMouseButton other)
    {
        return button == other.button  &&
               (modifierKeySet == null && other.modifierKeySet == null ||
                modifierKeySet != null && modifierKeySet.Equals(other.modifierKeySet));
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
        return HashCode.Combine((int)button, modifierKeySet?.GetHashCode() ?? 0);
    }
}