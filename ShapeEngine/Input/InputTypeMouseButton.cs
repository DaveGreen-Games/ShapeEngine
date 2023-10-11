using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    private float deadzone = 0f;
    public InputTypeMouseButton(ShapeMouseButton button, float deadzone = 0f) { this.button = button; this.deadzone = deadzone; }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value) { deadzone = value; }
    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button, deadzone);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button, prev, deadzone);
    }
    public virtual string GetName(bool shorthand = true) => GetMouseButtonName(button, shorthand);
    public InputDevice GetInputDevice() => InputDevice.Mouse;

    public IInputType Copy() => new InputTypeMouseButton(button);
        
        
    public static string GetMouseButtonName(ShapeMouseButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeMouseButton.LEFT: return shortHand ? "LMB" : "Left Mouse Button";
            case ShapeMouseButton.RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
            case ShapeMouseButton.MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
            case ShapeMouseButton.SIDE: return shortHand ? "SMB" : "Side Mouse Button";
            case ShapeMouseButton.EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
            case ShapeMouseButton.FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
            case ShapeMouseButton.BACK: return shortHand ? "BMB" : "Back Mouse Button";
            case ShapeMouseButton.MW_UP: return shortHand ? "MW U" : "Mouse Wheel Up";
            case ShapeMouseButton.MW_DOWN: return shortHand ? "MW D" : "Mouse Wheel Down";
            case ShapeMouseButton.MW_LEFT: return shortHand ? "MW L" : "Mouse Wheel Left";
            case ShapeMouseButton.MW_RIGHT: return shortHand ? "MW R" : "Mouse Wheel Right";
            case ShapeMouseButton.UP_AXIS: return shortHand ? "M Up" : "Mouse Up";
            case ShapeMouseButton.DOWN_AXIS: return shortHand ? "M Dwn" : "Mouse Down";
            case ShapeMouseButton.LEFT_AXIS: return shortHand ? "M Lft" : "Mouse Left";
            case ShapeMouseButton.RIGHT_AXIS: return shortHand ? "M Rgt" : "Mouse Right";
            default: return "No Key";
        }
    }
    // private static bool IsDown(ShapeMouseButton button, float deadzone = 0f)
    // {
    //     // var id = (int)button;
    //     // if (id >= 10)
    //     // {
    //     //     var value = GetMouseWheelMoveV();
    //     //     return button switch
    //     //     {
    //     //         ShapeMouseButton.MW_LEFT => value.X < 0f,
    //     //         ShapeMouseButton.MW_RIGHT => value.X > 0f,
    //     //         ShapeMouseButton.MW_UP => value.Y < 0f,
    //     //         ShapeMouseButton.MW_DOWN => value.Y > 0f,
    //     //         _ => false
    //     //     };
    //     // }
    //     //     
    //     // return IsMouseButtonDown(id);
    //     return GetValue(button, deadzone) != 0f;
    // }
    //
    public static float GetValue(ShapeMouseButton button, float deadzone = 0f)
    {
        if (!ShapeLoop.CursorOnScreen) return 0f;
        int id = (int)button;
        if (id is >= 10 and < 20)
        {
            var value = GetMouseWheelMoveV();//.Clamp(-1f, 1f);
            
            if (button == ShapeMouseButton.MW_LEFT) return value.X < -deadzone ? MathF.Abs(value.X) : 0f;
            if (button == ShapeMouseButton.MW_RIGHT) return value.X > deadzone ? value.X : 0f;
            if (button == ShapeMouseButton.MW_UP) return value.Y < -deadzone ? MathF.Abs(value.Y) : 0f;
            if (button == ShapeMouseButton.MW_DOWN) return value.Y > deadzone ? value.Y : 0f;
            return 0f;
        }
        if (id >= 20)
        {
            var mouseDelta = GetMouseDelta();
            if (button == ShapeMouseButton.LEFT_AXIS) return mouseDelta.X < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.RIGHT_AXIS) return mouseDelta.X > deadzone ? mouseDelta.X : 0f;
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > deadzone ? mouseDelta.Y : 0f;
            return 0f;
        }
        return IsMouseButtonDown(id) ? 1f : 0f;
    }

    public static InputState GetState(ShapeMouseButton button, float deadzone = 0f)
    {
        var value = GetValue(button, deadzone);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDevice.Mouse);
    }
    public static InputState GetState(ShapeMouseButton button, InputState previousState, float deadzone = 0f)
    {
        return new(previousState, GetState(button, deadzone));
    }
}