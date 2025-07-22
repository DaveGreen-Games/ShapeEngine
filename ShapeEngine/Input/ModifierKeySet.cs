using System.Text;
using ShapeEngine.Core;

namespace ShapeEngine.Input;


public class ModifierKeySet : IEquatable<ModifierKeySet>, ICopyable<ModifierKeySet>
{
    public readonly IModifierKey[] ModifierKeys;

    public readonly ModifierKeyOperator Operator;

    public ModifierKeySet(ModifierKeyOperator keyOperator, params IModifierKey[] modifierKeys)
    {
        ModifierKeys = modifierKeys;
        Operator = keyOperator;
    }
    
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
    
    public ModifierKeySet Copy()
    {
        var modifierKeyCopy = new IModifierKey[ModifierKeys.Length];
        for (int i = 0; i < ModifierKeys.Length; i++)
        {
            modifierKeyCopy[i] = ModifierKeys[i].Copy();
        }
        return new ModifierKeySet(Operator, modifierKeyCopy);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ModifierKeySet)obj);
    }
    
    public override int GetHashCode() => HashCode.Combine(Operator, ModifierKeys);
    
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
    
    public  void GetModifierKeyNames(StringBuilder sb, bool shorthand = true)
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
    
    
    
    public static ModifierKeySet CreateAndOperator(params IModifierKey[] modifierKeys) => new(ModifierKeyOperator.And, modifierKeys);

    public static ModifierKeySet CreateOrOperator(params IModifierKey[] modifierKeys) => new(ModifierKeyOperator.Or, modifierKeys);
}