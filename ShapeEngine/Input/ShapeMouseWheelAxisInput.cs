using System.Numerics;

namespace ShapeEngine.Input;

public class ShapeMouseWheelAxisInput : IShapeInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private ShapeInputState state = new();

    public ShapeMouseWheelAxisInput(ShapeMouseWheelAxis axis)
    {
        this.axis = axis;
    }

    public string GetName(bool shorthand = true) => GetMouseWheelAxisName(axis, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        state = GetState(axis, state);
    }
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeMouseWheelAxisInput(axis);

    private static float GetValue(ShapeMouseWheelAxis axis)
    {
        Vector2 value = GetMouseWheelMoveV();
        return axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
    }
    
    public static ShapeInputState GetState(ShapeMouseWheelAxis axis)
    {
        float axisValue = GetValue(axis);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static ShapeInputState GetState(ShapeMouseWheelAxis axis, ShapeInputState previousState)
    {
        return new(previousState, GetState(axis));
    }

    public static string GetMouseWheelAxisName(ShapeMouseWheelAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
            case ShapeMouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
            default: return "No Key";
        }
    }

}