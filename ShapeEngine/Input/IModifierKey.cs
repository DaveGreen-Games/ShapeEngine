using System.Text;

namespace ShapeEngine.Input;

public interface IModifierKey
{
    public bool IsActive(int gamepad = -1);
    public string GetName(bool shorthand = true);
    public InputDevice GetInputDevice();
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
    public static bool IsActive(ModifierKeyOperator modifierOperator, IModifierKey[] modifierKeys, int gamepad = -1)
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