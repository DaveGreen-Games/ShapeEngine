using System.Text;
using ShapeEngine.Core.GameDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type for a mouse wheel axis, supporting deadzone and modifier keys.
/// </summary>
public sealed class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;
    

    /// <summary>
    /// Initializes a new instance of <see cref="InputTypeMouseWheelAxis"/> with the specified axis, deadzone, and optional modifier key set.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to use.</param>
    /// <param name="deadzone">The deadzone value. Input values below this threshold are ignored.</param>
    /// <param name="modifierKeySet">An optional set of modifier keys required for activation.</param>
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone = InputSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
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
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append(axis.GetWheelAxisName(shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null)
    {
        return modifierKeySet == null ? 
            Game.Instance.Input.Mouse.CreateInputState(axis, deadzone) : 
            Game.Instance.Input.Mouse.CreateInputState(axis, deadzone, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null)
    {
        return modifierKeySet == null ? 
            Game.Instance.Input.Mouse.CreateInputState(axis, prev, deadzone) : 
            Game.Instance.Input.Mouse.CreateInputState(axis, prev, deadzone, modifierKeySet);
    }

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeMouseWheelAxis other)
    {
        return axis == other.axis &&
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
        if (other is InputTypeMouseWheelAxis inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeMouseWheelAxis other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)axis, modifierKeySet?.GetHashCode() ?? 0);
    }

}