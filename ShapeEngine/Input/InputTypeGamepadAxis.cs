using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a gamepad axis, supporting deadzone and modifier keys.
/// </summary>
public class InputTypeGamepadAxis : IInputType
{
    private readonly ShapeGamepadAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadAxis"/> with the specified axis and deadzone.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone = 0.1f)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadAxis"/> with axis, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeGamepadAxis"/> with axis, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeGamepadAxis(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeGamepadDevice.GetAxisName(axis, shorthand));// GetGamepadAxisName(axis, shorthand));
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
    public InputState GetState(ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.CreateInputState(axis, deadzone, modifierOperator, modifierKeys) : new();
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad)
    {
        return gamepad != null ? gamepad.CreateInputState(axis, prev, deadzone, modifierOperator, modifierKeys) : new();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Gamepad;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeGamepadAxis(axis);
}