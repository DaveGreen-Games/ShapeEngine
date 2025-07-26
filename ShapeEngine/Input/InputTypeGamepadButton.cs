using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a gamepad button, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadButton : IInputType
{
    private readonly ShapeGamepadButton button;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButton"/> with the specified button, deadzone, and modifier key set.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis/trigger input types).
    /// </param>
    /// <param name="modifierKeySet">The set of modifier keys associated with this input type.</param>
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
    }

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadButton(button, deadzone, modifierKeySet?.Copy());

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append(button.GetButtonName(shorthand)); // GamepadDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad) => gamepad?.CreateInputState(button, deadzone, deadzone,  modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad)
    {
        return gamepad?.CreateInputState(button, prev, deadzone, deadzone,  modifierKeySet) ?? new();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;
    
    private bool Equals(InputTypeGamepadButton other)
    {
        return button == other.button &&
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
        if (other is InputTypeGamepadButton inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeGamepadButton other && Equals(other);
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