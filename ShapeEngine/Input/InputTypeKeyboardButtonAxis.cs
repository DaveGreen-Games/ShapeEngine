using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a keyboard button axis (negative and positive buttons), supporting modifier keys.
/// </summary>
public sealed class InputTypeKeyboardButtonAxis : IInputType
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
        this.modifierKeys = [modifierKey];
    }

    /// <inheritdoc/>
    public IInputType Copy()
    { 
        var modifierKeyCopy = new IModifierKey[modifierKeys.Length];
        for (int i = 0; i < modifierKeys.Length; i++)
        {
            modifierKeyCopy[i] = modifierKeys[i].Copy();
        }
        return  new InputTypeKeyboardButtonAxis(neg, pos, modifierOperator, modifierKeyCopy);
    }

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
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
    
    private bool Equals(InputTypeKeyboardButtonAxis other)
    {
        return neg == other.neg && pos == other.pos &&  modifierKeys.Equals(other.modifierKeys) && modifierOperator == other.modifierOperator;
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
        return HashCode.Combine((int)neg, (int)pos,  modifierKeys, (int)modifierOperator);
    }
}