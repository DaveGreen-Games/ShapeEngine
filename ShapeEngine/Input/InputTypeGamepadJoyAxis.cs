using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;





/// <summary>
/// Represents an input type for a gamepad joystick axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeGamepadJoyAxis : IInputType
{
    /// <summary>
    /// Creates an <see cref="InputTypeGamepadJoyAxis"/> for the left joystick X axis.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadJoyAxis"/> instance for the left X axis.</returns>
    public static InputTypeGamepadJoyAxis CreateLeftX(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false,  ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadJoyAxis.LEFT_X, deadzone, inverted, modifierKeySet);

    /// <summary>
    /// Creates an <see cref="InputTypeGamepadJoyAxis"/> for the left joystick Y axis.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadJoyAxis"/> instance for the left Y axis.</returns>
    public static InputTypeGamepadJoyAxis CreateLeftY(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadJoyAxis.LEFT_Y, deadzone, inverted, modifierKeySet);

    /// <summary>
    /// Creates an <see cref="InputTypeGamepadJoyAxis"/> for the right joystick X axis.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadJoyAxis"/> instance for the right X axis.</returns>
    public static InputTypeGamepadJoyAxis CreateRightX(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadJoyAxis.RIGHT_X, deadzone, inverted, modifierKeySet);

    /// <summary>
    /// Creates an <see cref="InputTypeGamepadJoyAxis"/> for the right joystick Y axis.
    /// </summary>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    /// <returns>A new <see cref="InputTypeGamepadJoyAxis"/> instance for the right Y axis.</returns>
    public static InputTypeGamepadJoyAxis CreateRightY(float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new (ShapeGamepadJoyAxis.RIGHT_Y, deadzone, inverted, modifierKeySet);
    
    
    
    private readonly ShapeGamepadJoyAxis axis;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;
    private readonly bool inverted;
    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeGamepadJoyAxis"/> class with the specified joystick axis, deadzone, and optional modifier key set.
    /// </summary>
    /// <param name="axis">The gamepad joystick axis to use for input.</param>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="inverted">Whether the axis input is inverted. Default is false.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys to apply to this input type.</param>
    public InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis axis, float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, bool inverted = false,  ModifierKeySet? modifierKeySet = null)
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
    public IInputType Copy() => new InputTypeGamepadJoyAxis(axis, deadzone, inverted, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeGamepadJoyAxis other)
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
        if (other is InputTypeGamepadJoyAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeGamepadJoyAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)axis, inverted,  modifierKeySet?.GetHashCode() ?? 0);
    }
}