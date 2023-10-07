using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private float deadzone;
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }
    public virtual string GetName(bool shorthand = true) => GetMouseWheelAxisName(axis, shorthand);
    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, deadzone);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, prev, deadzone);
    }
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

    private static float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        Vector2 value = GetMouseWheelMoveV();
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    
    public static InputState GetState(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, GetState(axis, deadzone));
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