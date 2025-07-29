using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Specifies the polarity of a joystick axis.
/// </summary>
public enum AxisPolarity
{
    /// <summary>
    /// The axis operates in its normal direction.
    /// </summary>
    Normal,
    /// <summary>
    /// The axis is inverted.
    /// </summary>
    Inverted,
    /// <summary>
    /// The axis is centered (neutral position).
    /// </summary>
    Centered
}

/// <summary>
/// Represents an input type for a gamepad trigger axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadTriggerAxis : IInputType
{
    /// <summary>
    /// Creates an <see cref="InputTypeGamepadTriggerAxis"/> for the left trigger.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadTriggerAxis"/> for the left trigger.</returns>
    public static InputTypeGamepadTriggerAxis CreateLeftTrigger(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,  
        ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadTriggerAxis.LEFT, deadzone, modifierKeySet);

    /// <summary>
    /// Creates an <see cref="InputTypeGamepadTriggerAxis"/> for the right trigger.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadTriggerAxis"/> for the right trigger.</returns>
    public static InputTypeGamepadTriggerAxis CreateRightTrigger(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,  
        ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadTriggerAxis.RIGHT, deadzone, modifierKeySet);
    
    
    private readonly ShapeGamepadTriggerAxis axis;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeGamepadTriggerAxis"/> class with the specified trigger axis, deadzone, and optional modifier key set.
    /// </summary>
    /// <param name="axis">The gamepad trigger axis to use for input.</param>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    public InputTypeGamepadTriggerAxis(ShapeGamepadTriggerAxis axis, float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,  ModifierKeySet? modifierKeySet = null)
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
        sb.Append(axis.GetAxisName( shorthand));
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
    public InputState GetState(GamepadDevice? gamepad) => gamepad?.CreateInputState(axis, deadzone, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad) => gamepad?.CreateInputState(axis, prev, deadzone, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadTriggerAxis(axis, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeGamepadTriggerAxis other)
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
        if (other is InputTypeGamepadTriggerAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeGamepadTriggerAxis other && Equals(other);
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