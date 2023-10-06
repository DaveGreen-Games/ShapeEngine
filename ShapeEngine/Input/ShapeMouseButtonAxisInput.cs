using System.Numerics;
using System.Text;

namespace ShapeEngine.Input;

public class ShapeMouseButtonAxisInput : IShapeInputType
{
    private readonly ShapeMouseButton neg;
    private readonly ShapeMouseButton pos;

    public ShapeMouseButtonAxisInput(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }

    public string GetName(bool shorthand = true)
    {
        string negName = ShapeMouseButtonInput.GetMouseButtonName(neg, shorthand);
        string posName = ShapeMouseButtonInput.GetMouseButtonName(pos, shorthand);
        StringBuilder b = new(posName.Length + negName.Length + 4);
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
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IShapeInputType Copy() => new ShapeMouseButtonAxisInput(neg, pos);

    private static float GetAxis(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        float vNegative = GetValue(neg);
        float vPositive = GetValue(pos);
        return vPositive - vNegative;
    }
    private static float GetValue(ShapeMouseButton button)
    {
        int id = (int)button;
        if (id >= 10)
        {
            Vector2 value = GetMouseWheelMoveV();
            if (button == ShapeMouseButton.MW_LEFT) return MathF.Abs(value.X);
            if (button == ShapeMouseButton.MW_RIGHT) return value.X;
            if (button == ShapeMouseButton.MW_UP) return MathF.Abs(value.Y);
            if (button == ShapeMouseButton.MW_DOWN) return value.Y;
            return 0f;
        }
        return IsMouseButtonDown(id) ? 1f : 0f;
    }
    
    public static ShapeInputState GetState(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        float axis = GetAxis(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1);
    }
    public static ShapeInputState GetState(ShapeMouseButton neg, ShapeMouseButton pos,
        ShapeInputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }

}