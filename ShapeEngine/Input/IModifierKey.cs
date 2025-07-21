using System.Text;
using ShapeEngine.Core;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a modifier key (such as Shift, Ctrl, Alt) for input handling.
/// </summary>
public interface IModifierKey : ICopyable<IModifierKey>
{
    /// <summary>
    /// Determines if the modifier key is currently active.
    /// </summary>
    /// <param name="gamepad">Optional gamepad device to check against.</param>
    /// <returns>True if the modifier key is active; otherwise, false.</returns>
    public bool IsActive(GamepadDevice? gamepad = null);

    /// <summary>
    /// Gets the display name of the modifier key.
    /// </summary>
    /// <param name="shorthand">If true, returns a shorthand name; otherwise, a full name.</param>
    /// <returns>The name of the modifier key.</returns>
    public string GetName(bool shorthand = true);

    /// <summary>
    /// Gets the input device type associated with this modifier key.
    /// </summary>
    /// <returns>The input device type.</returns>
    public InputDeviceType GetInputDevice();

    /// <summary>
    /// Gets a combined string of modifier key names using the specified operator.
    /// </summary>
    /// <param name="modifierKeys">Array of modifier keys.</param>
    /// <param name="modifierOperator">Operator (And/Or) to combine names.</param>
    /// <param name="shorthand">If true, uses shorthand names.</param>
    /// <returns>A combined string of modifier key names.</returns>
    public static string GetModifierKeyNames(IModifierKey[] modifierKeys, ModifierKeyOperator modifierOperator, bool shorthand = true)
    {
        if (modifierKeys.Length <= 0) return string.Empty;
        
        StringBuilder sb = new();
        foreach (var key in modifierKeys)
        {
            var name = key.GetName(shorthand);
            if (name.Length > 0)
            {
                if (modifierOperator == ModifierKeyOperator.And)
                {
                    sb.Append($"{name} + ");
                }
                else
                {
                    sb.Append($"{name}, ");
                }
            }
        }
        
        return sb.ToString();

    }

    /// <summary>
    /// Appends combined modifier key names to the provided StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="modifierKeys">Array of modifier keys.</param>
    /// <param name="modifierOperator">Operator (And/Or) to combine names.</param>
    /// <param name="shorthand">If true, uses shorthand names.</param>
    public static void GetModifierKeyNames(StringBuilder sb, IModifierKey[] modifierKeys, ModifierKeyOperator modifierOperator, bool shorthand = true)
    {
        if (modifierKeys.Length <= 0) return;
        if (modifierKeys.Length == 1)
        {
            var name = modifierKeys[0].GetName(shorthand);
            if (name.Length > 0)
            {
                sb.Append($"{name} + ");
            }
        }
        else
        {
            for (int i = 0; i < modifierKeys.Length; i++)
            {
                var key = modifierKeys[i];
                var name = key.GetName(shorthand);
                if (name.Length > 0)
                {
                    if (modifierOperator == ModifierKeyOperator.And)
                    {
                        sb.Append($"{name} + ");
                    }
                    else// or
                    {
                        
                        if (i == modifierKeys.Length - 1) sb.Append($"{name} + ");//last
                        else sb.Append($"{name}/");//not last
                    }
                }
            }
        }
        
        
    }

    /// <summary>
    /// Determines if the specified modifier keys are active based on the operator.
    /// </summary>
    /// <param name="modifierOperator">Operator (And/Or) to evaluate.</param>
    /// <param name="modifierKeys">Array of modifier keys.</param>
    /// <param name="gamepad">Optional gamepad device to check against.</param>
    /// <returns>True if the modifier keys are active according to the operator; otherwise, false.</returns>
    public static bool IsActive(ModifierKeyOperator modifierOperator, IModifierKey[] modifierKeys, GamepadDevice? gamepad = null)
    {
        if (modifierKeys.Length <= 0) return true;
        
        foreach (var key in modifierKeys)
        {
            if (modifierOperator == ModifierKeyOperator.And)
            {
                if (!key.IsActive(gamepad)) return false;
            }
            else
            {
                if (key.IsActive(gamepad))
                {
                    return true;
                }
            }
        }
        
        return modifierOperator == ModifierKeyOperator.And;
    }
}