using System.Numerics;
using System.Text;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input type that tracks the delta of the mouse position relative to a target position.
/// Supports deadzone threshold and optional modifier keys.
/// </summary>
/// <remarks>
/// The System uses screen coordinates for the mouse position (ui mouse position).
/// </remarks>
public sealed class InputTypeMousePositionDelta : IInputType
{
    /// <summary>
    /// Gets the current target position, including any offset applied. In screen coordinates (ui coordinates).
    /// </summary>
    public Vector2 CurTargetPosition => targetPosition + TargetPositionOffset;
    
    /// <summary>
    /// Gets the base target position without any offset. In screen coordinates (ui corrdinates).
    /// </summary>
    public Vector2 BaseTargetPosition => targetPosition;
    
    /// <summary>
    /// The offset applied to the base target position.
    /// </summary>
    /// <remarks>
    /// Change this value to adjust the target position.
    /// The target position itself can not be changed after initialization.
    /// In screen coordinates (ui coordinates).
    /// </remarks>
    public Vector2 TargetPositionOffset;
    private readonly Vector2 targetPosition;
    private float deadzone;
    private readonly ModifierKeySet? modifierKeySet;
    private readonly ShapeMouseAxis axis;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputTypeMousePositionDelta"/> class.
    /// </summary>
    /// <param name="axis">The mouse axis to track.</param>
    /// <param name="targetPosition">The base target position for the mouse input.
    /// The System uses screen coordinates for the mouse position (ui mouse position).</param>
    /// <param name="deadzone">The threshold value for the mouse position delta.</param>
    /// <param name="modifierKeySet">Optional set of modifier keys required for activation.</param>
    public InputTypeMousePositionDelta(ShapeMouseAxis axis, Vector2 targetPosition, float deadzone, ModifierKeySet? modifierKeySet = null)
    {
        this.targetPosition = targetPosition;
        this.deadzone = deadzone;
        this.modifierKeySet = modifierKeySet;
        this.axis = axis;
        TargetPositionOffset = Vector2.Zero;
    }


    
    /// <summary>
    /// Gets the current deadzone threshold value.
    /// Only absolute delta values greater than this threshold will be reported.
    /// </summary>
    public float GetDeadzone() => deadzone;
    
    /// <summary>
    /// Sets the deadzone threshold value.
    /// Only absolute delta values greater than this threshold will be reported.
    /// </summary>
    /// <param name="value">The new deadzone value.</param>
    public void SetDeadzone(float value)
    {
        deadzone = value;
    }
    /// <summary>
    /// Updates the target position offset based on the new target position.
    /// </summary>
    /// <param name="newTargetPosition">The new target position to set.
    /// The System uses screen coordinates for the mouse position (ui mouse position).
    /// </param>
    /// <remarks>
    /// Does not change the base target position, but updates the offset instead!
    /// </remarks>
    public void ChangeTargetPosition(Vector2 newTargetPosition)
    {
        var delta = newTargetPosition - targetPosition;
        TargetPositionOffset = delta;
    }
    /// <inheritdoc/>
    public string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        modifierKeySet?.AppendModifierKeyNames(sb, shorthand);
        sb.Append("[Position Delta]");
        sb.Append(MouseDevice.GetAxisName(axis, shorthand));
        return sb.ToString();
    }

    /// <inheritdoc/>
    public InputState GetState(GamepadDevice? gamepad = null) => ShapeInput.ActiveMouseDevice.CreateInputState(axis, CurTargetPosition, deadzone, modifierKeySet);

    /// <inheritdoc/>
    public InputState GetState(InputState prev, GamepadDevice? gamepad = null) => ShapeInput.ActiveMouseDevice.CreateInputState(axis, CurTargetPosition, prev, deadzone, modifierKeySet);

    /// <inheritdoc/>
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;

    /// <inheritdoc/>
    public IInputType Copy() => new InputTypeMousePositionDelta(axis, targetPosition, deadzone, modifierKeySet?.Copy());
    
    private bool Equals(InputTypeMousePositionDelta other)
    {
        return axis == other.axis && 
               targetPosition.Equals(other.targetPosition)  &&
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
        if (other is InputTypeMousePositionDelta inputType)
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
        return ReferenceEquals(this, obj) || obj is InputTypeMousePositionDelta other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)axis, targetPosition, modifierKeySet?.GetHashCode() ?? 0);
    }
}