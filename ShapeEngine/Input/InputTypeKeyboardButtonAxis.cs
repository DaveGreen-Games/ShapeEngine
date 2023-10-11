using System.Text;

namespace ShapeEngine.Input;

public class InputTypeKeyboardButtonAxis : IInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;

    public InputTypeKeyboardButtonAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }

    public IInputType Copy() => new InputTypeKeyboardButtonAxis(neg, pos);
    public virtual string GetName(bool shorthand = true)
    {
        string negName = InputTypeKeyboardButton.GetKeyboardButtonName(neg, shorthand);
        string posName = InputTypeKeyboardButton.GetKeyboardButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + 4);
        b.Append(negName);
        b.Append('/');
        b.Append(posName);
        return b.ToString();
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }

    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos, prev);
    }
    public InputDevice GetInputDevice() => InputDevice.Keyboard;
    
    private static float GetAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float axis = GetAxis(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDevice.Keyboard);
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        InputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }
    
}