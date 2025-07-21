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
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButton"/> with the specified button and deadzone.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButton"/> with button, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = modifierKeys;
        this.modifierOperator = modifierOperator;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadButton"/> with button, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. Gamepad buttons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeGamepadButton(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = [modifierKey];
    }

    /// <inheritdoc/>
    public IInputType Copy()
    {
        var modifierKeyCopy = new IModifierKey[modifierKeys.Length];
        for (int i = 0; i < modifierKeys.Length; i++)
        {
            modifierKeyCopy[i] = modifierKeys[i].Copy();
        }
        return  new InputTypeGamepadButton(button, deadzone, modifierOperator, modifierKeyCopy);
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(GamepadDevice.GetButtonName(button, shorthand));
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
        return gamepad?.CreateInputState(button, deadzone, modifierOperator, modifierKeys) ?? new();
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad)
    {
        return gamepad?.CreateInputState(button, prev, deadzone, modifierOperator, modifierKeys) ?? new();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;
    
    private bool Equals(InputTypeGamepadButton other)
    {
        return button == other.button && modifierOperator == other.modifierOperator&& 
               modifierKeys.SequenceEqual(other.modifierKeys); //uses IEquatable implementation of IModifierKey;
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
        return HashCode.Combine((int)button, modifierKeys, (int)modifierOperator);
    }
}