using System.Text;
using ShapeEngine.Screen;

namespace ShapeEngine.Input;

public class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private readonly ShapeKeyboardButton modifier;
    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ShapeKeyboardButton modifier = ShapeKeyboardButton.None)
    {
        this.neg = neg;
        this.pos = pos;
        this.modifier = modifier;
    }

    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos);
    public virtual string GetName(bool shorthand = true)
    {
        string mod = modifier != ShapeKeyboardButton.None
            ? InputTypeKeyboardButton.GetKeyboardButtonName(modifier, shorthand)
            : "";
        string negName = InputTypeKeyboardButton.GetKeyboardButtonName(neg, shorthand);
        string posName = InputTypeKeyboardButton.GetKeyboardButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + mod.Length + 2);
        if (mod.Length > 0)
        {
            b.Append(mod);
            b.Append('+');
        }
        b.Append(negName);
        b.Append('|');
        b.Append(posName);
        return b.ToString();
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }

    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos, modifier);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos, prev, modifier);
    }
    public InputDevice GetInputDevice() => InputDevice.Keyboard;
    
    private static float GetAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ShapeKeyboardButton modifier = ShapeKeyboardButton.None)
    {
        if (modifier != ShapeKeyboardButton.None && !IsKeyDown((int)modifier)) return 0f;
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ShapeKeyboardButton modifier = ShapeKeyboardButton.None)
    {
        float axis = GetAxis(neg, pos, modifier);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDevice.Keyboard);
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        InputState previousState, ShapeKeyboardButton modifier = ShapeKeyboardButton.None)
    {
        return new(previousState, GetState(neg, pos, modifier));
    }
    
}