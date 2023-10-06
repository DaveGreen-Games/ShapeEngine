using System.Text;

namespace ShapeEngine.Input;

public class ShapeKeyboardButtonAxisInput : IShapeInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;

    public ShapeKeyboardButtonAxisInput(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }

    public IShapeInputType Copy() => new ShapeKeyboardButtonAxisInput(neg, pos);
    public string GetName(bool shorthand = true)
    {
        string negName = ShapeKeyboardButtonInput.GetKeyboardButtonName(neg, shorthand);
        string posName = ShapeKeyboardButtonInput.GetKeyboardButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }
    public ShapeInputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(neg, pos);
    }

    public ShapeInputState GetState(ShapeInputState prev, int gamepad = -1)
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
    public static ShapeInputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float axis = GetAxis(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1);
    }
    public static ShapeInputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        ShapeInputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }
    
}