using System.Text;
using ShapeEngine.Core;

namespace ShapeEngine.Input;

/// <summary>
/// A set of modifier keys that are evaluated together based on a specified operator.
/// </summary>
/// <remarks>
/// This class is used to define combinations of modifier keys (like Shift, Ctrl, Alt)
/// that must be active for an input to be triggered.
/// The keys can be combined
/// using either an <c>AND</c> or an <c>OR</c> logic.
/// </remarks>
/// <example>
/// <para>
/// Any combination of modifier keys is allowed. However, gamepad-related modifier keys only function with gamepad-related input.
/// </para>
/// <para>
/// For example, while <c>"[Shift] + Right Bumper"</c> is technically valid, the order and context matter: <c>"[Right Bumper] + Shift"</c> does not work because gamepad modifiers require a gamepad input context.
/// Combinations like <c>"[Shift] + Left Mouse Button"</c> or <c>"[Middle Mouse Button] + Space"</c> are valid, as they involve keyboard or mouse modifiers.
/// Always ensure that gamepad modifiers are paired with gamepad inputs for correct behavior.
/// </para>
///<para>
/// <c>So you can pair any modifier type with gamepad input, but gamepad modifiers will only work with gamepad input.</c>
/// </para>
/// </example>
public class ModifierKeySet : IEquatable<ModifierKeySet>, ICopyable<ModifierKeySet>
{
    /// <summary>
    /// The collection of modifier keys in this set.
    /// </summary>
    public readonly IModifierKey[] ModifierKeys;

    /// <summary>
    /// The logical operator used to evaluate the modifier keys.
    /// </summary>
    /// <seealso cref="ModifierKeyOperator"/>
    public readonly ModifierKeyOperator Operator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeySet"/> class with multiple modifier keys.
    /// </summary>
    /// <param name="keyOperator">The logical operator to apply to the modifier keys.</param>
    /// <param name="modifierKeys">An array of modifier keys.</param>
    public ModifierKeySet(ModifierKeyOperator keyOperator, params IModifierKey[] modifierKeys)
    {
        ModifierKeys = modifierKeys;
        Operator = keyOperator;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeySet"/> class with a single modifier key.
    /// </summary>
    /// <param name="keyOperator">The logical operator to apply to the modifier key.</param>
    /// <param name="modifierKey">A single modifier key.</param>
    public ModifierKeySet(ModifierKeyOperator keyOperator, IModifierKey modifierKey)
    {
        ModifierKeys = [modifierKey];
        Operator = keyOperator;
    }
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
    public bool Equals(ModifierKeySet? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Operator != other.Operator) return false;
        if(ModifierKeys.Length != other.ModifierKeys.Length) return false;
        for (int i = 0; i < ModifierKeys.Length; i++)
        {
            if (!ModifierKeys[i].Equals(other.ModifierKeys[i]))
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Creates a deep copy of the current <see cref="ModifierKeySet"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ModifierKeySet"/> that is a copy of this instance.</returns>
    public ModifierKeySet Copy()
    {
        var modifierKeyCopy = new IModifierKey[ModifierKeys.Length];
        for (int i = 0; i < ModifierKeys.Length; i++)
        {
            modifierKeyCopy[i] = ModifierKeys[i].Copy();
        }
        return new ModifierKeySet(Operator, modifierKeyCopy);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ModifierKeySet)obj);
    }
    
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(Operator, ModifierKeys);
    
    /// <summary>
    /// Gets a string representing the names of the modifier keys in this set.
    /// </summary>
    /// <param name="shorthand">If <c>true</c>, uses abbreviated names for the keys. Default is <c>true</c>.</param>
    /// <returns>A formatted string of modifier key names.</returns>
    /// <remarks>
    /// For <see cref="ModifierKeyOperator.And"/>, names are joined by " + ".
    /// For <see cref="ModifierKeyOperator.Or"/>, names are joined by ", ".
    /// </remarks>
    public string GetModifierKeyNames(bool shorthand = true)
    {
        if (ModifierKeys.Length <= 0) return string.Empty;
        
        StringBuilder sb = new();
        foreach (var key in ModifierKeys)
        {
            var name = key.GetName(shorthand);
            if (name.Length > 0)
            {
                if (Operator == ModifierKeyOperator.And)
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
    /// Appends the names of the modifier keys to a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append the names to.</param>
    /// <param name="shorthand">If <c>true</c>, uses abbreviated names for the keys. Default is <c>true</c>.</param>
    /// <remarks>
    /// For a single key or <see cref="ModifierKeyOperator.And"/>, names are appended with " + ".
    /// For <see cref="ModifierKeyOperator.Or"/>, names are separated by "/" and the last one is appended with " + ".
    /// </remarks>
    public  void AppendModifierKeyNames(StringBuilder sb, bool shorthand = true)
    {
        if (ModifierKeys.Length <= 0) return;
        if (ModifierKeys.Length == 1)
        {
            var name = ModifierKeys[0].GetName(shorthand);
            if (name.Length > 0)
            {
                sb.Append($"{name} + ");
            }
        }
        else
        {
            for (int i = 0; i < ModifierKeys.Length; i++)
            {
                var key = ModifierKeys[i];
                var name = key.GetName(shorthand);
                if (name.Length > 0)
                {
                    if (Operator == ModifierKeyOperator.And)
                    {
                        sb.Append($"{name} + ");
                    }
                    else// or
                    {
                        
                        if (i == ModifierKeys.Length - 1) sb.Append($"{name} + ");//last
                        else sb.Append($"{name}/");//not last
                    }
                }
            }
        }
        
        
    }
    
    /// <summary>
    /// Checks if the modifier key set is currently active.
    /// </summary>
    /// <param name="gamepad">The gamepad device to check against. If <c>null</c>, only keyboard/mouse modifiers are considered. Default is <c>null</c>.</param>
    /// <returns><c>true</c> if the conditions of the modifier set are met; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// If the set is empty, this method returns <c>true</c>.
    /// (No modifier keys means no restrictions, so input should be allowed.)
    /// </para>
    /// For <see cref="ModifierKeyOperator.And"/>, all keys must be active.
    /// For <see cref="ModifierKeyOperator.Or"/>, at least one key must be active.
    /// </remarks>
    public bool IsActive(GamepadDevice? gamepad = null)
    {
        if (ModifierKeys.Length <= 0) return true;
        
        foreach (var key in ModifierKeys)
        {
            if (Operator == ModifierKeyOperator.And)
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
        
        return Operator == ModifierKeyOperator.And;
    }
    
    
    
    /// <summary>
    /// Creates a new <see cref="ModifierKeySet"/> that uses the <see cref="ModifierKeyOperator.And"/> operator.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys to include in the set.</param>
    /// <returns>A new <see cref="ModifierKeySet"/> with the <c>AND</c> operator.</returns>
    public static ModifierKeySet CreateAndOperator(params IModifierKey[] modifierKeys) => new(ModifierKeyOperator.And, modifierKeys);

    /// <summary>
    /// Creates a new <see cref="ModifierKeySet"/> that uses the <see cref="ModifierKeyOperator.Or"/> operator.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys to include in the set.</param>
    /// <returns>A new <see cref="ModifierKeySet"/> with the <c>OR</c> operator.</returns>
    public static ModifierKeySet CreateOrOperator(params IModifierKey[] modifierKeys) => new(ModifierKeyOperator.Or, modifierKeys);
}