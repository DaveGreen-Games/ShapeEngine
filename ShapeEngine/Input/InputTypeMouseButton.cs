using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse button, supporting deadzone and modifier keys.
/// </summary>
public class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with the specified button and deadzone.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone = 0f)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with button, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButton"/> with button, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button; 
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { deadzone = value; }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(button, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(button, prev, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(MouseDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseButton(button);
}