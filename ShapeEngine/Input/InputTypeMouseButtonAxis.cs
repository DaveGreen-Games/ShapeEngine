using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse button axis (negative and positive buttons),
/// supporting deadzone and modifier keys.
/// </summary>
public class InputTypeMouseButtonAxis : IInputType
{
    private readonly ShapeMouseButton neg;
    private readonly ShapeMouseButton pos;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButtonAxis"/> with specified negative and positive buttons and deadzone.
    /// </summary>
    /// <param name="neg">The negative mouse button.</param>
    /// <param name="pos">The positive mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButtonAxis"/> with buttons, deadzone, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="neg">The negative mouse button.</param>
    /// <param name="pos">The positive mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseButtonAxis"/> with buttons, deadzone, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="neg">The negative mouse button.</param>
    /// <param name="pos">The positive mouse button.</param>
    /// <param name="deadzone">
    /// The deadzone value. Deadzone is a setting that discards input values that are below the deadzone value. MouseButtons ignore deadzone (deadzone works only with axis input types).
    /// </param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value) => deadzone = value;

    /// <inheritdoc/>
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        string negName = MouseDevice.GetButtonName(neg, shorthand);
        string posName = MouseDevice.GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
        
        
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(neg, pos, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveMouseDevice.CreateInputState(neg, pos, prev, deadzone, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseButtonAxis(neg, pos);

}