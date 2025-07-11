using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a keyboard button, supporting modifier keys.
/// </summary>
public sealed class InputTypeKeyboardButton : IInputType
{
    private readonly ShapeKeyboardButton button;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButton"/> with the specified button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    public InputTypeKeyboardButton(ShapeKeyboardButton button)
    {
        this.button = button;
        this.modifierKeys = [];
        this.modifierOperator = ModifierKeyOperator.And;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButton"/> with button, modifier operator, and modifier keys.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKeys">The modifier keys.</param>
    public InputTypeKeyboardButton(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.button = button;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeKeyboardButton"/> with button, modifier operator, and a single modifier key.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="modifierOperator">The modifier key operator.</param>
    /// <param name="modifierKey">The modifier key.</param>
    public InputTypeKeyboardButton(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.button = button;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }

    /// <inheritdoc/>
    public float GetDeadzone() => 0f;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(button, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(button, prev, modifierOperator, modifierKeys);
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        sb.Append(KeyboardDevice.GetButtonName(button, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeKeyboardButton(button);
}