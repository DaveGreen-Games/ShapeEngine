using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseAxis : IInputType
{
    private readonly ShapeMouseAxis axis;
    private float deadzone;
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        this.axis = axis;
        this.deadzone = deadzone;
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }
    public string GetName(bool shorthand = true) => GetMouseAxisName(axis, shorthand);
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
    public IInputType Copy() => new InputTypeMouseAxis(axis);

    private static float GetValue(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        Vector2 value = GetMouseDelta();
        float returnValue = axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    
    public static InputState GetState(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static InputState GetState(ShapeMouseAxis axis, InputState previousState, float deadzone = 0.5f)
    {
        return new(previousState, GetState(axis, deadzone));
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