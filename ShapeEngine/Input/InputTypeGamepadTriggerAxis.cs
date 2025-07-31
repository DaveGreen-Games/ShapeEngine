using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;


/// <summary>
/// Represents an input type for a gamepad trigger axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadTriggerAxis : IInputType
{
    /// <summary>
    /// Creates an <see cref="InputTypeGamepadTriggerAxis"/> for the left trigger.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether the trigger axis input should be inverted. (From <c>[0 - 1]</c> to <c>[1 - 0]</c></param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadTriggerAxis"/> for the left trigger.</returns>
    public static InputTypeGamepadTriggerAxis CreateLeftTrigger(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,  
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadTriggerAxis.LEFT, deadzone, inverted, modifierKeySet);

    /// <summary>
    /// Creates an <see cref="InputTypeGamepadTriggerAxis"/> for the right trigger.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether the trigger axis input should be inverted. (From <c>[0 - 1]</c> to <c>[1 - 0]</c></param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadTriggerAxis"/> for the right trigger.</returns>
    public static InputTypeGamepadTriggerAxis CreateRightTrigger(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold,  
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadTriggerAxis.RIGHT, deadzone, inverted, modifierKeySet);
    
    
    private readonly ShapeGamepadTriggerAxis axis;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;
    private readonly bool inverted;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeGamepadTriggerAxis"/> class with the specified trigger axis, deadzone, and optional modifier key set.
    /// </summary>
    /// <param name="axis">The gamepad trigger axis to use for input.</param>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether the trigger axis input should be inverted. (From <c>[0 - 1]</c> to <c>[1 - 0]</c></param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    public InputTypeGamepadTriggerAxis(ShapeGamepadTriggerAxis axis, float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, bool inverted = false,  ModifierKeySet? modifierKeySet = null)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.inverted = inverted;
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
    public InputState GetState(GamepadDevice? gamepad) => gamepad?.CreateInputState(axis, deadzone, inverted, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad) => gamepad?.CreateInputState(axis, prev, deadzone, inverted, modifierKeySet) ?? new();

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadTriggerAxis(axis, deadzone, inverted, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeGamepadTriggerAxis other)
    {
        return axis == other.axis && inverted == other.inverted &&
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
        return HashCode.Combine((int)axis, inverted, modifierKeySet?.GetHashCode() ?? 0);
    }
}