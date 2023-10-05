using System.Text;

namespace ShapeEngine.Input;

public class ShapeKeyboardButtonAxisInput : IShapeInputType
{
    private readonly ShapeKeyboardButton neg;
    private readonly ShapeKeyboardButton pos;
    private ShapeInputState state = new();

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
    public void Update(float dt, int gamepadIndex)
    {
        state = GetState(neg, pos, state);
    }
    public ShapeInputState GetState() => state;
    
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