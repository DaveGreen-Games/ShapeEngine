using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a gamepad axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadAxis : IInputType
{
    private readonly ShapeGamepadAxis axis;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadAxis"/> with the specified axis, deadzone, and modifier key set.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierKeySet">The set of modifier keys to apply to this input type. Optional.</param>
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone = 0.1f,  ModifierKeySet? modifierKeySet = null)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append(GamepadDevice.GetAxisName(axis, shorthand));// GetGamepadAxisName(axis, shorthand));
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
    public InputState GetState(GamepadDevice? gamepad)
    {
        if (gamepad == null) return new();
        return modifierKeySet == null ? gamepad.CreateInputState(axis, deadzone) : gamepad.CreateInputState(axis, deadzone, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad)
    {
        if (gamepad == null) return new();
        return modifierKeySet == null ? gamepad.CreateInputState(axis, prev, deadzone) : gamepad.CreateInputState(axis, prev, deadzone, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadAxis(axis, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeGamepadAxis other)
    {
        return axis == other.axis &&
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
        if (other is InputTypeGamepadAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeGamepadAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)axis, modifierKeySet?.GetHashCode() ?? 0);
    }
}