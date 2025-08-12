using System.Collections;
using System.Text;
using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a keyboard button, supporting modifier keys.
/// </summary>
public sealed class InputTypeKeyboardButton : IInputType
{
    private readonly ShapeKeyboardButton button;
    private readonly ModifierKeySet? modifierKeySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeKeyboardButton"/> class.
    /// </summary>
    /// <param name="button">The keyboard button to represent.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys.</param>
    public InputTypeKeyboardButton(ShapeKeyboardButton button, ModifierKeySet? modifierKeySet = null)
    {
        this.button = button;
        this.modifierKeySet = modifierKeySet;
    }

    
    /// <inheritdoc/>
    public float GetDeadzone() => 0f;

    /// <inheritdoc/>
    public void SetDeadzone(float value) { }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return modifierKeySet == null ? 
            Game.Instance.Input.Keyboard.CreateInputState(button) : 
            Game.Instance.Input.Keyboard.CreateInputState(button, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return modifierKeySet == null ? 
            Game.Instance.Input.Keyboard.CreateInputState(button, prev) : 
            Game.Instance.Input.Keyboard.CreateInputState(button, prev, modifierKeySet);
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append(button.GetButtonName(shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeKeyboardButton(button, modifierKeySet?.Copy());


    private bool Equals(InputTypeKeyboardButton other)
    {
        return button == other.button  &&
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
        if (other is InputTypeKeyboardButton inputTypeKeyboardButton)
        {
            return Equals(inputTypeKeyboardButton);       
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
        return ReferenceEquals(this, obj) || obj is InputTypeKeyboardButton other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)button, modifierKeySet?.GetHashCode() ?? 0);
    }
}