using System.Numerics;
using System.Text;

namespace ShapeEngine.Input;

public class InputTypeMouseButtonAxis : IInputType
{
    private readonly ShapeMouseButton neg;
    private readonly ShapeMouseButton pos;

    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }
    public string GetName(bool shorthand = true)
    {
        string negName = InputTypeMouseButton.GetMouseButtonName(neg, shorthand);
        string posName = InputTypeMouseButton.GetMouseButtonName(pos, shorthand);
        StringBuilder b = new(posName.Length + negName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }
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
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IInputType Copy() => new InputTypeMouseButtonAxis(neg, pos);

    private static float GetAxis(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        float vNegative = GetValue(neg);
        float vPositive = GetValue(pos);
        return vPositive - vNegative;
    }
    private static float GetValue(ShapeMouseButton button)
    {
        int id = (int)button;
        if (id is >= 10 and < 20)
        {
            //TODO is this working or do I need to change it to axis system?
            Vector2 value = GetMouseWheelMoveV();
            if (button == ShapeMouseButton.MW_LEFT) return MathF.Abs(value.X);
            if (button == ShapeMouseButton.MW_RIGHT) return value.X;
            if (button == ShapeMouseButton.MW_UP) return MathF.Abs(value.Y);
            if (button == ShapeMouseButton.MW_DOWN) return value.Y;
            return 0f;
        }
        if (id >= 20)
        {
            Vector2 mouseDelta = GetMouseDelta();
            if (button == ShapeMouseButton.LEFT_AXIS) return mouseDelta.X < 0f ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.RIGHT_AXIS) return mouseDelta.X > 0f ? mouseDelta.X : 0f;
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < 0f ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > 0f ? mouseDelta.Y : 0f;
            return 0f;
        }
        return IsMouseButtonDown(id) ? 1f : 0f;
    }
    
    public static InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        float axis = GetAxis(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1);
    }
    public static InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos,
        InputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }

}