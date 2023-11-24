using System.Text;
using ShapeEngine.Screen;

namespace ShapeEngine.Input;

public class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos);
    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        
        string negName = InputTypeKeyboardButton.GetKeyboardButtonName(neg, shorthand);
        string posName = InputTypeKeyboardButton.GetKeyboardButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
        
        // string mod = (modifier != ShapeKeyboardButton.None && !reverseModifier)
        //     ? InputTypeKeyboardButton.GetKeyboardButtonName(modifier, shorthand)
        //     : "";
        // string negName = InputTypeKeyboardButton.GetKeyboardButtonName(neg, shorthand);
        // string posName = InputTypeKeyboardButton.GetKeyboardButtonName(pos, shorthand);
        // StringBuilder b = new(negName.Length + posName.Length + mod.Length + 2);
        // if (mod.Length > 0)
        // {
        //     b.Append(mod);
        //     b.Append('+');
        // }
        // b.Append(negName);
        // b.Append('|');
        // b.Append(posName);
        // return b.ToString();
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }

    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos, prev, modifierOperator, modifierKeys);
    }
    public InputDevice GetInputDevice() => InputDevice.Keyboard;
    
    private static float GetAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, -1)) return 0f;
        
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    private static float GetAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetAxis(neg, pos, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDevice.Keyboard);
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        InputState previousState, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(neg, pos, modifierOperator, modifierKeys));
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float axis = GetAxis(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDevice.Keyboard);
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }
}