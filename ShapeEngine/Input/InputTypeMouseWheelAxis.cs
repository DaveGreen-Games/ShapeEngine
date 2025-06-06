using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse wheel axis, supporting deadzone and modifier keys.
/// </summary>
public class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseWheelAxis"/> with the specified axis and deadzone.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseWheelAxis"/> with axis, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseWheelAxis"/> with axis, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }

    /// <inheritdoc/>
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(ShapeMouseDevice.GetWheelAxisName(axis, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        return ShapeInput.MouseDevice.CreateInputState(axis, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        return ShapeInput.MouseDevice.CreateInputState(axis, prev, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

}