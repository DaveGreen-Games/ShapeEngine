using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a keyboard button axis (negative and positive buttons), supporting modifier keys.
/// </summary>
public class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButtonAxis"/> with specified negative and positive buttons.
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButtonAxis"/> with buttons, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButtonAxis"/> with buttons, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos);

    /// <inheritdoc/>
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        
        string negName = KeyboardDevice.GetButtonName(neg, shorthand);
        string posName = KeyboardDevice.GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public float GetDeadzone() => 0f;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos, prev, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;
}