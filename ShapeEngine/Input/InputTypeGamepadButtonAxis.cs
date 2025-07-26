using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a gamepad button axis (negative and positive buttons), supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadButtonAxis : IInputType
{
    private readonly ShapeGamepadButton neg;
    private readonly ShapeGamepadButton pos;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeGamepadButtonAxis"/> class.
    /// </summary>
    /// <param name="neg">The negative direction gamepad button.</param>
    /// <param name="pos">The positive direction gamepad button.</param>
    /// <param name="deadzone">The deadzone threshold for axis input. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional set of modifier keys.</param>
    public InputTypeGamepadButtonAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        string negName = neg.GetButtonName(shorthand);
        string posName = pos.GetButtonName(shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
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
    public InputState GetState(GamepadDevice? gamepad) => gamepad?.CreateInputState(neg, pos, deadzone, deadzone, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad) => gamepad?.CreateInputState(neg, pos, prev, deadzone, deadzone, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadButtonAxis(neg, pos, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeGamepadButtonAxis other)
    {
        return neg == other.neg && pos == other.pos  &&
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
        if (other is InputTypeGamepadButtonAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeGamepadButtonAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)neg, (int)pos, modifierKeySet?.GetHashCode() ?? 0);
    }
}