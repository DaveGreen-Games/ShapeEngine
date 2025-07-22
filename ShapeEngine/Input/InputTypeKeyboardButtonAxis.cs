using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a keyboard button axis (negative and positive buttons), supporting modifier keys.
/// </summary>
public sealed class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private readonly ModifierKeySet? modifierKeySet;


    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeKeyboardButtonAxis"/> class.
    /// </summary>
    /// <param name="neg">The negative direction keyboard button.</param>
    /// <param name="pos">The positive direction keyboard button.</param>
    /// <param name="modifierKeySet">Optional set of modifier keys.</param>
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet? modifierKeySet = null)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierKeySet = modifierKeySet;
    }

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos, modifierKeySet?.Copy());

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        
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
        return modifierKeySet == null ? 
            ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos) : 
            ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return modifierKeySet == null ? 
            ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos, prev) : 
            ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos, prev, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Keyboard;
    
    private bool Equals(InputTypeKeyboardButtonAxis other)
    {
        return neg == other.neg && pos == other.pos  &&
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
        if (other is InputTypeKeyboardButtonAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeKeyboardButtonAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)neg, (int)pos, modifierKeySet?.GetHashCode() ?? 0);
    }
}