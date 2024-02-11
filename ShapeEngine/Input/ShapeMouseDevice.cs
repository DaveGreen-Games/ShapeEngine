using System.Text;
using Raylib_cs;
using ShapeEngine.Core;

namespace ShapeEngine.Input;

public sealed class ShapeMouseDevice : ShapeInputDevice
{ 
    public static readonly MouseButton[] AllMouseButtons = Enum.GetValues<MouseButton>();
    public static readonly ShapeMouseButton[] AllShapeMouseButtons = Enum.GetValues<ShapeMouseButton>();

    public float MoveThreshold = 0.5f;
    public float MouseWheelThreshold = 0.25f;

    private bool wasUsed = false;
    private bool isLocked = false;

    internal ShapeMouseDevice() { }
    
    public bool WasUsed() => wasUsed;
    public bool IsLocked() => isLocked;

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public void Update()
    {
        wasUsed = WasMouseUsed(MoveThreshold, MouseWheelThreshold);
    }
    public void Calibrate(){ }
    private bool WasMouseUsed(float moveThreshold = 0.5f, float mouseWheelThreshold = 0.25f)
    {
        if (isLocked) return false;
        
        var mouseDelta = Raylib.GetMouseDelta();
        if (mouseDelta.LengthSquared() > moveThreshold * moveThreshold) return true;
        var mouseWheel = Raylib.GetMouseWheelMoveV();
        if (mouseWheel.LengthSquared() > mouseWheelThreshold * mouseWheelThreshold) return true;

        if (Raylib.IsMouseButtonDown(MouseButton.Left)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Right)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Middle)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Extra)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Forward)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Back)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.Side)) return true;

        return false;
    }

   
    #region Axis
    public static string GetAxisName(ShapeMouseAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseAxis.HORIZONTAL: return shortHand ? "Mx" : "Mouse Horizontal";
            case ShapeMouseAxis.VERTICAL: return shortHand ? "My" : "Mouse Vertical";
            default: return "No Key";
        }
    }

    public bool IsDown(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        return GetValue(axis, deadzone) != 0f;
    }

    public bool IsDown(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator,
        params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0;
    }
    
    public float GetValue(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
       
        var value = Raylib.GetMouseDelta();
        float returnValue = axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    public InputState GetState(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(axis, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseAxis axis, InputState previousState, float deadzone = 0.5f)
    {
        return new(previousState, GetState(axis, deadzone));
    }
    #endregion

    #region Wheel Axis

    public bool IsDown(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        return GetValue(axis, deadzone) != 0f;
    }
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
        
        var value = Raylib.GetMouseWheelMoveV();
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    public InputState GetState(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(axis, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, GetState(axis, deadzone));
    }
    
    public static string GetWheelAxisName(ShapeMouseWheelAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
            case ShapeMouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
            default: return "No Key";
        }
    }

    

    #endregion
    
    #region Button
    public static string GetButtonName(ShapeMouseButton button, bool shortHand = true)
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
    public bool IsModifierActive(ShapeMouseButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    public bool IsDown(ShapeMouseButton button, float deadzone = 0f)
    {
        return GetValue(button, deadzone) != 0;
    }

    public bool IsDown(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator,
        params IModifierKey[] modifierKeys)
    {
        return GetValue(button, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public float GetValue(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(button, deadzone);
    }
    public float GetValue(ShapeMouseButton button, float deadzone = 0f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.IsMouseOnScreen) return 0f;
        int id = (int)button;
        if (id is >= 10 and < 20)
        {
            var value = Raylib.GetMouseWheelMoveV();//.Clamp(-1f, 1f);
            
            if (button == ShapeMouseButton.MW_LEFT) return value.X < -deadzone ? MathF.Abs(value.X) : 0f;
            if (button == ShapeMouseButton.MW_RIGHT) return value.X > deadzone ? value.X : 0f;
            if (button == ShapeMouseButton.MW_UP) return value.Y < -deadzone ? MathF.Abs(value.Y) : 0f;
            if (button == ShapeMouseButton.MW_DOWN) return value.Y > deadzone ? value.Y : 0f;
            return 0f;
        }
        if (id >= 20)
        {
            var mouseDelta = Raylib.GetMouseDelta();
            if (button == ShapeMouseButton.LEFT_AXIS) return mouseDelta.X < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.RIGHT_AXIS) return mouseDelta.X > deadzone ? mouseDelta.X : 0f;
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > deadzone ? mouseDelta.Y : 0f;
            return 0f;
        }
        return Raylib.IsMouseButtonDown((MouseButton)id) ? 1f : 0f;
    }
    public InputState GetState(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        var value = GetValue(button, deadzone, modifierOperator, modifierKeys);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseButton button, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(button, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeMouseButton button, float deadzone = 0f)
    {
        var value = GetValue(button, deadzone);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseButton button, InputState previousState, float deadzone = 0f)
    {
        return new(previousState, GetState(button, deadzone));
    }
    #endregion

    #region ButtonAxis

    public static string GetButtonAxisName(ShapeMouseButton neg, ShapeMouseButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }

    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, deadzone, modifierOperator, modifierKeys) != 0f;
    }

    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        return GetValue(neg, pos, deadzone) != 0f;
    }
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(neg, pos, deadzone);
    }
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        if (isLocked) return 0f;
        float vNegative = GetValue(neg, deadzone);
        float vPositive = GetValue(pos, deadzone);
        return vPositive - vNegative;
    }
    public InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, deadzone, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(neg, pos, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        float axis = GetValue(neg, pos, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    public InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float deadzone = 0f)
    {
        return new(previousState, GetState(neg, pos, deadzone));
    }
    

    #endregion
}