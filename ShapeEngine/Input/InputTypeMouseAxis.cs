using System.Numerics;
using System.Text;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseAxis : IInputType
{
    private readonly ShapeMouseAxis axis;
    private float deadzone;
    private readonly ShapeKeyboardButton modifier;
    private readonly bool reverseModifier;
    public InputTypeMouseAxis(ShapeMouseAxis axis, float deadzone = 0.5f, ShapeKeyboardButton modifierKey = ShapeKeyboardButton.None, bool reverseModifier = false)
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
        if(modifier == ShapeKeyboardButton.None || reverseModifier) return GetMouseAxisName(axis, shorthand);

        StringBuilder sb = new();
        sb.Append(InputTypeKeyboardButton.GetKeyboardButtonName(modifier, shorthand));
        sb.Append('+');
        sb.Append(GetMouseAxisName(axis, shorthand));
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
    public IInputType Copy() => new InputTypeMouseAxis(axis);

    private static float GetValue(ShapeMouseAxis axis, float deadzone = 0.5f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (modifier != ShapeKeyboardButton.None)
        {
            if(IsKeyDown((int)modifier) == reverseModifier) return 0f;
        } 
        Vector2 value = GetMouseDelta();
        float returnValue = axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    
    public static InputState GetState(ShapeMouseAxis axis, float deadzone = 0.5f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        float axisValue = GetValue(axis, deadzone, modifier, reverseModifier);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDevice.Mouse);
    }
    public static InputState GetState(ShapeMouseAxis axis, InputState previousState, float deadzone = 0.5f, ShapeKeyboardButton modifier = ShapeKeyboardButton.None, bool reverseModifier = false)
    {
        return new(previousState, GetState(axis, deadzone, modifier, reverseModifier));
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