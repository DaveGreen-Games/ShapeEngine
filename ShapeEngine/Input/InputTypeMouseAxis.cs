using System.Numerics;

namespace ShapeEngine.Input;

public class InputTypeMouseAxis : IInputType
{
    private readonly ShapeMouseAxis axis;

    public InputTypeMouseAxis(ShapeMouseAxis axis)
    {
        this.axis = axis;
    }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }
    public string GetName(bool shorthand = true) => GetMouseAxisName(axis, shorthand);
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
    public IInputType Copy() => new InputTypeMouseAxis(axis);

    private static float GetValue(ShapeMouseAxis axis)
    {
        Vector2 value = GetMouseDelta();
        return axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
    }
    
    public static InputState GetState(ShapeMouseAxis axis)
    {
        float axisValue = GetValue(axis);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static InputState GetState(ShapeMouseAxis axis, InputState previousState)
    {
        return new(previousState, GetState(axis));
    }

    public static string GetMouseAxisName(ShapeMouseAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseAxis.HORIZONTAL: return shortHand ? "Mx" : "Mouse Horizontal";
            case ShapeMouseAxis.VERTICAL: return shortHand ? "My" : "Mouse Vertical";
            default: return "No Key";
        }
    }

}