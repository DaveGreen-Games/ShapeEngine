using System.Numerics;

namespace ShapeEngine.Input;

public class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;

    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis)
    {
        this.axis = axis;
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }
    public string GetName(bool shorthand = true) => GetMouseWheelAxisName(axis, shorthand);
    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, prev);
    }
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

    private static float GetValue(ShapeMouseWheelAxis axis)
    {
        Vector2 value = GetMouseWheelMoveV();
        return axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
    }
    
    public static InputState GetState(ShapeMouseWheelAxis axis)
    {
        float axisValue = GetValue(axis);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, InputState previousState)
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