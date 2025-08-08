using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse button axis (negative and positive buttons),
/// supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeMouseButtonAxis : IInputType
{
    private readonly ShapeMouseButton neg;
    private readonly ShapeMouseButton pos;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;

    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeMouseButtonAxis"/> class.
    /// </summary>
    /// <param name="neg">The mouse button representing the negative axis.</param>
    /// <param name="pos">The mouse button representing the positive axis.</param>
    /// <param name="deadzone">The deadzone value for axis input.</param>
    /// <param name="modifierKeySet">Optional set of modifier keys required for activation.</param>
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = InputSettings.MouseSettings.DefaultMouseThreshold, ModifierKeySet? modifierKeySet = null)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
    }


    /// <inheritdoc/>
    public float GetDeadzone() => deadzone;

    /// <inheritdoc/>
    public void SetDeadzone(float value) => deadzone = value;

    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        string negName = neg.GetButtonName(shorthand);
        string posName = pos.GetButtonName(shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
        
        
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null) => ShapeInput.Mouse.CreateInputState(neg, pos, deadzone, deadzone, modifierKeySet);

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null) => ShapeInput.Mouse.CreateInputState(neg, pos, prev, deadzone, deadzone, modifierKeySet);

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseButtonAxis(neg, pos, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeMouseButtonAxis other)
    {
        return neg == other.neg && pos == other.pos &&
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
        if (other is InputTypeMouseButtonAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeMouseButtonAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)neg, (int)pos,  modifierKeySet?.GetHashCode() ?? 0);
    }

}