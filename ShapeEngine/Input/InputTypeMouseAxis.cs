using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeMouseAxis : IInputType
{
    private readonly ShapeMouseAxis axis;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseAxis"/> with the specified axis and deadzone.
    /// </summary>
    /// <param name="axis">The mouse axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseAxis"/> with axis, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="axis">The mouse axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseAxis"/> with axis, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="axis">The mouse axis.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value.
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
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
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(MouseDevice.GetAxisName(axis, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(axis, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(axis, prev, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseAxis(axis);
}