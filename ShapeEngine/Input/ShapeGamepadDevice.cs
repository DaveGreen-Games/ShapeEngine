using System.Text;
using Raylib_CsLo;
using ShapeEngine.Core;

namespace ShapeEngine.Input;


public interface ShapeInputDevice
{
    public bool WasUsed();
    public void Update();

    public bool IsLocked();
    public void Lock();
    public void Unlock();

    public void Calibrate();

}

public sealed class ShapeKeyboardDevice : ShapeInputDevice
{
    public static readonly KeyboardKey[] AllKeyboardKeys = Enum.GetValues<KeyboardKey>();
    public static readonly ShapeKeyboardButton[] AllShapeKeyboardButtons = Enum.GetValues<ShapeKeyboardButton>();

    private bool wasUsed = false;
    private bool isLocked = false;
    
    internal ShapeKeyboardDevice() { }
    
    public bool IsLocked() => isLocked;

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }
    
    public bool WasUsed() => wasUsed;
    
    public void Update()
    {
        wasUsed = WasKeyboardUsed();
    }
    
    public void Calibrate(){}
    public List<char> GetStreamChar()
    {
        if (isLocked) return new List<char>();
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }
        return chars;
    }
    public string GetStream()
    {
        if (isLocked) return String.Empty;
        
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }

        StringBuilder b = new(chars.Count);
        b.Append(chars);
        return b.ToString();
    }
    public string GetStream(string curText)
    {
        if (isLocked) return String.Empty;
        
        var chars = GetStreamChar();
        var b = new StringBuilder(chars.Count + curText.Length);
        b.Append(curText);
        b.Append(chars);
        return b.ToString();
    }

    private bool WasKeyboardUsed() => !isLocked && Raylib.GetKeyPressed() > 0;

    #region Button

    public static string GetButtonName(ShapeKeyboardButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeKeyboardButton.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
            case ShapeKeyboardButton.COMMA: return shortHand ? "," : "Comma";
            case ShapeKeyboardButton.MINUS: return shortHand ? "-" : "Minus";
            case ShapeKeyboardButton.PERIOD: return shortHand ? "." : "Period";
            case ShapeKeyboardButton.SLASH: return shortHand ? "/" : "Slash";
            case ShapeKeyboardButton.ZERO: return shortHand ? "0" : "Zero";
            case ShapeKeyboardButton.ONE: return shortHand ? "1" : "One";
            case ShapeKeyboardButton.TWO: return shortHand ? "2" : "Two";
            case ShapeKeyboardButton.THREE: return shortHand ? "3" : "Three";
            case ShapeKeyboardButton.FOUR: return shortHand ? "4" : "Four";
            case ShapeKeyboardButton.FIVE: return shortHand ? "5" : "Five";
            case ShapeKeyboardButton.SIX: return shortHand ? "6" : "Six";
            case ShapeKeyboardButton.SEVEN: return shortHand ? "7" : "Seven";
            case ShapeKeyboardButton.EIGHT: return shortHand ? "8" : "Eight";
            case ShapeKeyboardButton.NINE: return shortHand ? "9" : "Nine";
            case ShapeKeyboardButton.SEMICOLON: return shortHand ? ";" : "Semi Colon";
            case ShapeKeyboardButton.EQUAL: return shortHand ? "=" : "Equal";
            case ShapeKeyboardButton.A: return "A";
            case ShapeKeyboardButton.B: return "B";
            case ShapeKeyboardButton.C: return "C";
            case ShapeKeyboardButton.D: return "D";
            case ShapeKeyboardButton.E: return "E";
            case ShapeKeyboardButton.F: return "F";
            case ShapeKeyboardButton.G: return "G";
            case ShapeKeyboardButton.H: return "H";
            case ShapeKeyboardButton.I: return "I";
            case ShapeKeyboardButton.J: return "J";
            case ShapeKeyboardButton.K: return "K";
            case ShapeKeyboardButton.L: return "L";
            case ShapeKeyboardButton.M: return "M";
            case ShapeKeyboardButton.N: return "N";
            case ShapeKeyboardButton.O: return "O";
            case ShapeKeyboardButton.P: return "P";
            case ShapeKeyboardButton.Q: return "Q";
            case ShapeKeyboardButton.R: return "R";
            case ShapeKeyboardButton.S: return "S";
            case ShapeKeyboardButton.T: return "T";
            case ShapeKeyboardButton.U: return "U";
            case ShapeKeyboardButton.V: return "V";
            case ShapeKeyboardButton.W: return "W";
            case ShapeKeyboardButton.X: return "X";
            case ShapeKeyboardButton.Y: return "Y";
            case ShapeKeyboardButton.Z: return "Z";
            case ShapeKeyboardButton.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
            case ShapeKeyboardButton.BACKSLASH: return shortHand ? "\\" : "Backslash";
            case ShapeKeyboardButton.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
            case ShapeKeyboardButton.GRAVE: return shortHand ? "`" : "Grave";//Check
            case ShapeKeyboardButton.SPACE: return shortHand ? "Spc" : "Space";
            case ShapeKeyboardButton.ESCAPE: return shortHand ? "Esc" : "Escape";
            case ShapeKeyboardButton.ENTER: return shortHand ? "Ent" : "Enter";
            case ShapeKeyboardButton.TAB: return shortHand ? "Tab" : "Tabulator";
            case ShapeKeyboardButton.BACKSPACE: return shortHand ? "BSpc" : "Backspace";
            case ShapeKeyboardButton.INSERT: return shortHand ? "Ins" : "Insert";
            case ShapeKeyboardButton.DELETE: return shortHand ? "Del" : "Delete";
            case ShapeKeyboardButton.RIGHT: return shortHand ? "Rgt" : "Right";
            case ShapeKeyboardButton.LEFT: return shortHand ? "Lft" : "Left";
            case ShapeKeyboardButton.DOWN: return shortHand ? "Dwn" : "Down";
            case ShapeKeyboardButton.UP: return shortHand ? "Up" : "Up";
            case ShapeKeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
            case ShapeKeyboardButton.PAGE_DOWN: return shortHand ? "PDwn" : "Page Down";
            case ShapeKeyboardButton.HOME: return shortHand ? "Hom" : "Home";
            case ShapeKeyboardButton.END: return shortHand ? "End" : "End";
            case ShapeKeyboardButton.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
            case ShapeKeyboardButton.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
            case ShapeKeyboardButton.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
            case ShapeKeyboardButton.PRINT_SCREEN: return shortHand ? "Prnt" : "Print Screen";
            case ShapeKeyboardButton.PAUSE: return shortHand ? "Pse" : "Pause";
            case ShapeKeyboardButton.F1: return shortHand ? "F1" : "Function 1";
            case ShapeKeyboardButton.F2: return shortHand ? "F2" : "Function 2";
            case ShapeKeyboardButton.F3: return shortHand ? "F3" : "Function 3";
            case ShapeKeyboardButton.F4: return shortHand ? "F4" : "Function 4";
            case ShapeKeyboardButton.F5: return shortHand ? "F5" : "Function 5";
            case ShapeKeyboardButton.F6: return shortHand ? "F6" : "Function 6";
            case ShapeKeyboardButton.F7: return shortHand ? "F7" : "Function 7";
            case ShapeKeyboardButton.F8: return shortHand ? "F8" : "Function 8";
            case ShapeKeyboardButton.F9: return shortHand ? "F9" : "Function 9";
            case ShapeKeyboardButton.F10: return shortHand ? "F10" : "Function 10";
            case ShapeKeyboardButton.F11: return shortHand ? "F11" : "Function 11";
            case ShapeKeyboardButton.F12: return shortHand ? "F12" : "Function 12";
            case ShapeKeyboardButton.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
            case ShapeKeyboardButton.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
            case ShapeKeyboardButton.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
            case ShapeKeyboardButton.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
            case ShapeKeyboardButton.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
            case ShapeKeyboardButton.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
            case ShapeKeyboardButton.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
            case ShapeKeyboardButton.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
            case ShapeKeyboardButton.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
            case ShapeKeyboardButton.KP_0: return shortHand ? "KP0" : "Keypad 0";
            case ShapeKeyboardButton.KP_1: return shortHand ? "KP1" : "Keypad 1";
            case ShapeKeyboardButton.KP_2: return shortHand ? "KP2" : "Keypad 2";
            case ShapeKeyboardButton.KP_3: return shortHand ? "KP3" : "Keypad 3";
            case ShapeKeyboardButton.KP_4: return shortHand ? "KP4" : "Keypad 4";
            case ShapeKeyboardButton.KP_5: return shortHand ? "KP5" : "Keypad 5";
            case ShapeKeyboardButton.KP_6: return shortHand ? "KP6" : "Keypad 6";
            case ShapeKeyboardButton.KP_7: return shortHand ? "KP7" : "Keypad 7";
            case ShapeKeyboardButton.KP_8: return shortHand ? "KP8" : "Keypad 8";
            case ShapeKeyboardButton.KP_9: return shortHand ? "KP9" : "Keypad 9";
            case ShapeKeyboardButton.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
            case ShapeKeyboardButton.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
            case ShapeKeyboardButton.KP_MULTIPLY: return shortHand ? "KPMul" : "Keypad Multiply";
            case ShapeKeyboardButton.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
            case ShapeKeyboardButton.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
            case ShapeKeyboardButton.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
            case ShapeKeyboardButton.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
            case ShapeKeyboardButton.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
            case ShapeKeyboardButton.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
            case ShapeKeyboardButton.BACK: return shortHand ? "Bck" : "Back";
            case ShapeKeyboardButton.NULL: return shortHand ? "Nll" : "Null";
            case ShapeKeyboardButton.MENU: return shortHand ? "Mnu" : "Menu";
            default: return "No Key";
        }
    }
    public bool IsModifierActive(ShapeKeyboardButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    public bool IsDown(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(button, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeKeyboardButton button)
    {
        return GetValue(button) != 0f;
    }
    
    public float GetValue(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        bool modifierActive = IModifierKey.IsActive(modifierOperator, modifierKeys, null);
        return (modifierActive && IsKeyDown((int)button)) ? 1f : 0f;
    }
    public float GetValue(ShapeKeyboardButton button)
    {
        if (isLocked) return 0f;
        return IsKeyDown((int)button) ? 1f : 0f;
    }

    public InputState GetState(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        bool down = IsDown(button, modifierOperator, modifierKeys);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
    }
    public InputState GetState(ShapeKeyboardButton button, InputState previousState, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(button, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeKeyboardButton button)
    {
        bool down = IsDown(button);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
    }
    
    public InputState GetState(ShapeKeyboardButton button, InputState previousState)
    {
        return new(previousState, GetState(button));
    }

    #endregion
    
    #region Button Axis

    public static string GetButtonAxisName(ShapeKeyboardButton neg, ShapeKeyboardButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    
    public bool IsDown(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        return GetValue(neg, pos) != 0f;
    }

    public float GetValue(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        
        return GetValue(neg, pos);
    }
    public float GetValue(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        if (isLocked) return 0f;
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    
    public InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Keyboard);
    }
    public InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        InputState previousState, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(neg, pos, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float axis = GetValue(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Keyboard);
    }
    public InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState)
    {
        return new(previousState, GetState(neg, pos));
    }

    #endregion
}

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

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_EXTRA)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_FORWARD)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_BACK)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_SIDE)) return true;

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
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        if (isLocked) return 0f;
        if (!ShapeLoop.CursorOnScreen) return 0f;
       
        var value = GetMouseDelta();
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
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        if (isLocked) return 0f;
        if (!ShapeLoop.CursorOnScreen) return 0f;
        
        var value = GetMouseWheelMoveV();
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
        if (!ShapeLoop.CursorOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, null)) return 0f;
        return GetValue(button, deadzone);
    }
    public float GetValue(ShapeMouseButton button, float deadzone = 0f)
    {
        if (isLocked) return 0f;
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

public sealed class ShapeGamepadDevice : ShapeInputDevice
{
    public static readonly GamepadButton[] AllGamepadButtons = Enum.GetValues<GamepadButton>();
    public static readonly GamepadAxis[] AllGamepadAxis = Enum.GetValues<GamepadAxis>();
    
    public static readonly ShapeGamepadButton[] AllShapeGamepadButtons = Enum.GetValues<ShapeGamepadButton>();
    public static readonly ShapeGamepadAxis[] AllShapeGamepadAxis = Enum.GetValues<ShapeGamepadAxis>();
    

    public readonly int Index;
    
    public bool Available { get; private set; } = true;
    public bool Connected { get; private set; }

    public string Name { get; private set; } = "No Device";
    public int AxisCount { get; private set; } = 0;

    private bool isLocked = false;
    private bool wasUsed  = false;
    
    public readonly List<ShapeGamepadButton> UsedButtons = new();
    public readonly List<ShapeGamepadAxis> UsedAxis = new();
    
    public event Action? OnConnectionChanged;
    public event Action? OnAvailabilityChanged;

    // private bool triggerFix = false;
    private readonly Dictionary<ShapeGamepadAxis, float> axisCalibrationValues = new();
    
    public ShapeGamepadDevice(int index, bool connected)
    {
        Index = index;
        
        Connected = connected;
        if (Connected)
        {
            unsafe
            {
                Name = Raylib.GetGamepadName(index)->ToString();
            }

            AxisCount = Raylib.GetGamepadAxisCount(index);
            
            Calibrate();
        }
        
    }

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
        wasUsed = false;
        if(UsedButtons.Count > 0) UsedButtons.Clear();
        if(UsedAxis.Count > 0) UsedAxis.Clear();
        
        if (!Connected || isLocked) return;
        
        var usedButtons = GetUsedGamepadButtons();
        var usedAxis = GetUsedGamepadAxis(0.25f);
        wasUsed = usedButtons.Count > 0 || usedAxis.Count > 0;
        if(usedButtons.Count > 0) UsedButtons.AddRange(usedButtons);
        if(usedAxis.Count > 0) UsedAxis.AddRange(usedAxis);
    }
    
    public void Connect()
    {
        if (Connected) return;
        Connected = true;
        unsafe
        {
            Name = Raylib.GetGamepadName(Index)->ToString();
        }

        AxisCount = Raylib.GetGamepadAxisCount(Index);
        OnConnectionChanged?.Invoke();
        
        Calibrate();
    }
    public void Disconnect()
    {
        if (!Connected) return;
        Connected = false;
        OnConnectionChanged?.Invoke();
    }
    public bool Claim()
    {
        if (!Connected || !Available) return false;
        Available = false;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
    public bool Free()
    {
        if (Available) return false;
        Available = true;
        OnAvailabilityChanged?.Invoke();
        return true;
    }

    public void Calibrate()
    {
        float leftX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_LEFT_X);
        float leftY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_LEFT_Y);
        
        float rightX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_RIGHT_X);
        float rightY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y);
        
        float triggerRight = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER);
        float triggerLeft = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER);

        axisCalibrationValues[ShapeGamepadAxis.LEFT_X] = leftX;
        axisCalibrationValues[ShapeGamepadAxis.LEFT_Y] = leftY;
        
        axisCalibrationValues[ShapeGamepadAxis.RIGHT_X] = rightX;
        axisCalibrationValues[ShapeGamepadAxis.RIGHT_Y] = rightY;
        
        axisCalibrationValues[ShapeGamepadAxis.RIGHT_TRIGGER] = triggerRight;
        axisCalibrationValues[ShapeGamepadAxis.LEFT_TRIGGER] = triggerLeft;

        // triggerFix = triggerLeft < 0 || triggerRight < 0;
        // triggerFix = false;
        //trigger fix is not necessary anymore I think...
    }

    #region Button

    public bool IsModifierActive(ShapeGamepadButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    public bool IsDown(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(button, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        return GetValue(button, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;
        return GetValue(button, deadzone);
    }
    public float GetValue(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        
        var id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetValue((ShapeGamepadAxis)id, deadzone);// GetGamepadAxisMovement(Index, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetValue((ShapeGamepadAxis)id, deadzone);// GetGamepadAxisMovement(Index, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value;
        }
        
        return IsGamepadButtonDown(Index, id) ? 1f : 0f;
    }
    
    public InputState GetState(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        bool down = IsDown(button, deadzone, modifierOperator, modifierKeys);
        return new(down, !down, down ? 1f : 0f, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadButton button, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(button, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        bool down = IsDown(button, deadzone);
        return new(down, !down, down ? 1f : 0f, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadButton button, InputState previousState, float deadzone = 0.1f)
    {
        return new(previousState, GetState(button, deadzone));
    }
    public static string GetButtonName(ShapeGamepadButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeGamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
            case ShapeGamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
            case ShapeGamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
            case ShapeGamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
            case ShapeGamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
            case ShapeGamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
            case ShapeGamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
            case ShapeGamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
            case ShapeGamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
            case ShapeGamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
            case ShapeGamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
            case ShapeGamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
            case ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
            case ShapeGamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
            case ShapeGamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
            case ShapeGamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
            case ShapeGamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
            case ShapeGamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
            case ShapeGamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
            case ShapeGamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
            case ShapeGamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
            case ShapeGamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
            case ShapeGamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
            case ShapeGamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
            case ShapeGamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
            case ShapeGamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
            default: return "No Key";
        }
    }


    #endregion
    
    #region Axis
    public bool IsDown(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadAxis axis, float deadzone)
    {
        return GetValue(axis, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;

        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeGamepadAxis axis, float deadzone)
    {
        // if (Index < 0 || isLocked || !Connected) return 0f;
        // float value = GetGamepadAxisMovement(Index, (int)axis);
        // if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        // {
        //     value = (value + 1f) / 2f;
        // }
        // return MathF.Abs(value) < deadzone ? 0f : value;
        
        if (!Connected || Index < 0 || isLocked) return 0f;
        var value = GetValue(axis);
        return MathF.Abs(value) < deadzone ? 0f : value;
    }
    public float GetValue(ShapeGamepadAxis axis)
    {
        if (!Connected || Index < 0 || isLocked) return 0f;
        
        float value = GetGamepadAxisMovement(Index, (int)axis);
        value -= axisCalibrationValues[axis];
        // if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        // {
            // if(triggerFix) value = (value + 1f) / 2f;
        // }

        return value;
    }
    
    public InputState GetState(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(axis, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeGamepadAxis axis, float deadzone = 0.1f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadAxis axis, InputState previousState, float deadzone = 0.1f)
    {
        return new(previousState, GetState(axis, deadzone));
    }
    public static string GetAxisName(ShapeGamepadAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeGamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
            case ShapeGamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
            case ShapeGamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
            case ShapeGamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
            case ShapeGamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
            case ShapeGamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
            default: return "No Key";
        }
    }


    #endregion

    #region Button Axis
    public static string GetButtonAxisName(ShapeGamepadButton neg, ShapeGamepadButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    
    public bool IsDown(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        return GetValue(neg, pos, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;
        return GetValue(neg, pos, deadzone);
    }
    public float GetValue(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        float vNegative = GetValue(neg, deadzone);
        float vPositive = GetValue(pos, deadzone);
        return vPositive - vNegative;
    }
    
    public InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, deadzone, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, GetState(neg, pos, deadzone, modifierOperator, modifierKeys));
    }
    public InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        float axis = GetValue(neg, pos, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, Index, InputDeviceType.Gamepad);
    }
    public InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, GetState(neg, pos, deadzone));
    }

    #endregion

    #region Used

    // public bool WasGamepadUsed(float deadzone = 0f)
    // {
    //     if (!Connected || Index < 0) return false;
    //     return WasGamepadButtonUsed() || WasGamepadAxisUsed(deadzone);
    // }
    private bool WasGamepadButtonUsed()
    {
        if (!Connected || Index < 0 || isLocked) return false;
        foreach (var b in  AllGamepadButtons)
        {
            if (Raylib.IsGamepadButtonDown(Index, b)) return true;
        }

        return false;
    }
    private bool WasGamepadAxisUsed(ShapeGamepadAxis axis, float deadzone = 0.25f)
    {
        if (!Connected || Index < 0 || isLocked) return false;
        return GetValue(axis, deadzone) != 0f;
    }
    private bool WasGamepadAxisUsed(float deadzone = 0.25f)
    {
        if (!Connected || Index < 0 || isLocked) return false;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_X, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_Y, deadzone)) return true;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_X, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_Y, deadzone)) return true;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_TRIGGER, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_TRIGGER, deadzone)) return true;
        // if (MathF.Abs(Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;

        return false;
    }
    
    private List<ShapeGamepadButton> GetUsedGamepadButtons(float deadzone = 0.25f)
    {
        var usedButtons = new List<ShapeGamepadButton>();
        if (!Connected || Index < 0 || isLocked) return usedButtons;
        var values = ShapeGamepadDevice.AllShapeGamepadButtons;// Enum.GetValues<ShapeGamepadButton>();
        foreach (var b in  values)
        {
            if (IsDown(b, deadzone))
            {
                usedButtons.Add(b);
            }
        }
        return usedButtons;
    }
    private List<ShapeGamepadAxis> GetUsedGamepadAxis(float deadzone = 0.25f)
    {
        var usedAxis = new List<ShapeGamepadAxis>();
        if (!Connected || Index < 0 || isLocked) return usedAxis;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_X, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_X);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_Y, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_Y);
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_X, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_X);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_Y, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_Y);
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_TRIGGER, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_TRIGGER);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_TRIGGER, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_TRIGGER);
        
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_X);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_Y);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_X);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_Y);
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.LEFT_TRIGGER);
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.RIGHT_TRIGGER);

        return usedAxis;
    }
    #endregion
    
}