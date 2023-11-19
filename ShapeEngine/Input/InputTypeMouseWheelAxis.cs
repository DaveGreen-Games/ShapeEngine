using System.Numerics;
using System.Text;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseWheelAxis : IInputType
{
    private readonly ShapeMouseWheelAxis axis;
    private float deadzone;
    private readonly ShapeKeyboardButton modifier;
    private readonly bool reverseModifier;
    public InputTypeMouseWheelAxis(ShapeMouseWheelAxis axis, float deadzone = 0.2f, ShapeKeyboardButton modifierKey = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        this.axis = axis;
        this.deadzone = deadzone;
        this.modifier = modifierKey;
        this.reverseModifier = reverseModifier;
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value)
    {
        deadzone = ShapeMath.Clamp(value, 0f, 1f);
    }
    public virtual string GetName(bool shorthand = true)
    {
        if(modifier == ShapeKeyboardButton.None || reverseModifier) return GetMouseWheelAxisName(axis, shorthand);

        StringBuilder sb = new();
        sb.Append(InputTypeKeyboardButton.GetKeyboardButtonName(modifier, shorthand));
        sb.Append('+');
        sb.Append(GetMouseWheelAxisName(axis, shorthand));
        return sb.ToString();
    }

    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, deadzone, modifier, reverseModifier);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(axis, prev, deadzone, modifier, reverseModifier);
    }
    public InputDevice GetInputDevice() => InputDevice.Mouse;
    public IInputType Copy() => new InputTypeMouseWheelAxis(axis);

    private static float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (modifier != ShapeKeyboardButton.None)
        {
            if(IsKeyDown((int)modifier) == reverseModifier) return 0f;
        } 
        Vector2 value = GetMouseWheelMoveV();
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    
    public static InputState GetState(ShapeMouseWheelAxis axis, float deadzone = 0.2f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        
        float axisValue = GetValue(axis, deadzone, modifier, reverseModifier);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDevice.Mouse);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        return new(previousState, GetState(axis, deadzone, modifier, reverseModifier));
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