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
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButtonAxis"/> with specified negative and positive buttons and deadzone.
    /// </summary>
    /// <param name="neg">The negative gamepad button.</param>
    /// <param name="pos">The positive gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    public InputTypeGamepadButtonAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButtonAxis"/> with buttons, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="neg">The negative gamepad button.</param>
    /// <param name="pos">The positive gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeGamepadButtonAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButtonAxis"/> with buttons, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="neg">The negative gamepad button.</param>
    /// <param name="pos">The positive gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeGamepadButtonAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = [modifierKey];
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        string negName = GamepadDevice.GetButtonName(neg, shorthand);
        string posName = GamepadDevice.GetButtonName(pos, shorthand);
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
    public InputState GetState(GamepadDevice? gamepad)
    {
        return gamepad?.CreateInputState(neg, pos, deadzone, modifierOperator, modifierKeys) ?? new();
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad)
    {
        return gamepad?.CreateInputState(neg, pos, prev, deadzone, modifierOperator, modifierKeys) ?? new();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadButtonAxis(neg, pos, deadzone);
    
    private bool Equals(InputTypeGamepadButtonAxis other)
    {
        return neg == other.neg && pos == other.pos &&  modifierKeys.Equals(other.modifierKeys) && modifierOperator == other.modifierOperator;
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
        return HashCode.Combine((int)neg, (int)pos,  modifierKeys, (int)modifierOperator);
    }
}