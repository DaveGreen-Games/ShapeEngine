using System.Numerics;
using System.Text;
using Raylib_CsLo;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

/*
public class ShapeAxis
{
    private IShapeInputType inputType;
    public int GamepadIndex { get; set; } = -1;

    public ShapeAxis(ShapeKeyboardButtonAxisInput keyboardButtonAxisInput)
    {
        inputType = keyboardButtonAxisInput;
    }
    public ShapeAxis(ShapeMouseButtonAxisInput mouseButtonAxisInput)
    {
        inputType = mouseButtonAxisInput;
    }
    public ShapeAxis(ShapeGamepadButtonAxisInput gamepadButtonAxisInput, int gamepadIndex)
    {
        inputType = gamepadButtonAxisInput;
        GamepadIndex = gamepadIndex;
    }
    public ShapeAxis(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        inputType = new ShapeKeyboardButtonAxisInput(neg, pos);
    }
    public ShapeAxis(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        inputType = new ShapeMouseButtonAxisInput(neg, pos);
    }
    public ShapeAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, float deadzone = 0.2f)
    {
        inputType = new ShapeGamepadButtonAxisInput(neg, pos, deadzone);
        GamepadIndex = gamepadIndex;
    }
    public ShapeAxis(ShapeMouseWheelAxisInput mouseWheelAxisInput)
    {
        inputType = mouseWheelAxisInput;
    }
    public ShapeAxis(ShapeGamepadAxisInput gamepadAxisInput, int gamepadIndex)
    {
        inputType = gamepadAxisInput;
        GamepadIndex = gamepadIndex;
    }
    public ShapeAxis(ShapeMouseWheelAxis mouseWheelAxis)
    {
        inputType = new ShapeMouseWheelAxisInput(mouseWheelAxis);
    }
    public ShapeAxis(ShapeGamepadAxis gamepadAxis, int gamepadIndex, float deadzone = 0.2f)
    {
        inputType = new ShapeGamepadAxisInput(gamepadAxis, deadzone);
        GamepadIndex = gamepadIndex;
    }

    public ShapeInputState State => inputType.GetState();
    public void Update(float dt) => inputType.Update(dt, GamepadIndex);
}
*/


public readonly struct ShapeInputState
{
    public readonly  bool Down;
    public readonly bool Up;
    public readonly bool Released;
    public readonly bool Pressed;
    public readonly float Axis;
    public readonly int Gamepad;

    public ShapeInputState()
    {
        Down = false;
        Up = true;
        Released = false;
        Pressed = false;
        Axis = 0f;
        Gamepad = -1;
    }
    public ShapeInputState(bool down, bool up, float axis, int gamepad)
    {
        Down = down;
        Up = up;
        Released = false;
        Pressed = false;
        Axis = axis;
        Gamepad = gamepad;
    }

    public ShapeInputState(ShapeInputState prev, ShapeInputState cur)
    {
        Gamepad = cur.Gamepad;
        Axis = cur.Axis;
        Down = cur.Down;
        Up = cur.Up;
        Pressed = prev.Up && cur.Down;
        Released = prev.Down && cur.Up;
    }

    public ShapeInputState Accumulate(ShapeInputState other)
    {
        float axis = ShapeMath.Clamp(Axis + other.Axis, 0f, 1f);
        bool down = Down || other.Down;
        bool up = Up && other.Up;
        return new(down, up, axis, Gamepad);
    }
}

public interface IShapeInputType
{
    public IShapeInputType Copy();
    public void Update(float dt, int gamepadIndex);
    public ShapeInputState GetState();
    public string GetName(bool shorthand = true);
}

public class ShapeKeyboardButtonInput : IShapeInputType
{
    private ShapeKeyboardButton button;
    private ShapeInputState state = new();
    public ShapeKeyboardButtonInput(ShapeKeyboardButton button)
    {
        this.button = button;
    }

    public void Update(float dt, int gamepadIndex)
    {
        bool down = IsKeyDown((int)button);
        ShapeInputState current = new(down, !down, 0f, gamepadIndex);
        state = new(state, current);
    }
    public string GetName(bool shorthand = true) => GetKeyboardButtonName(button, shorthand);
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeKeyboardButtonInput(button);
    
    public static string GetKeyboardButtonName(ShapeKeyboardButton button, bool shortHand = true)
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
                case ShapeKeyboardButton.A: return shortHand ? "A" : "A";
                case ShapeKeyboardButton.B: return shortHand ? "B" : "B";
                case ShapeKeyboardButton.C: return shortHand ? "C" : "C";
                case ShapeKeyboardButton.D: return shortHand ? "D" : "D";
                case ShapeKeyboardButton.E: return shortHand ? "E" : "E";
                case ShapeKeyboardButton.F: return shortHand ? "F" : "F";
                case ShapeKeyboardButton.G: return shortHand ? "G" : "G";
                case ShapeKeyboardButton.H: return shortHand ? "H" : "H";
                case ShapeKeyboardButton.I: return shortHand ? "I" : "I";
                case ShapeKeyboardButton.J: return shortHand ? "J" : "J";
                case ShapeKeyboardButton.K: return shortHand ? "K" : "K";
                case ShapeKeyboardButton.L: return shortHand ? "L" : "L";
                case ShapeKeyboardButton.M: return shortHand ? "M" : "M";
                case ShapeKeyboardButton.N: return shortHand ? "N" : "N";
                case ShapeKeyboardButton.O: return shortHand ? "O" : "O";
                case ShapeKeyboardButton.P: return shortHand ? "P" : "P";
                case ShapeKeyboardButton.Q: return shortHand ? "Q" : "Q";
                case ShapeKeyboardButton.R: return shortHand ? "R" : "R";
                case ShapeKeyboardButton.S: return shortHand ? "S" : "S";
                case ShapeKeyboardButton.T: return shortHand ? "T" : "T";
                case ShapeKeyboardButton.U: return shortHand ? "U" : "U";
                case ShapeKeyboardButton.V: return shortHand ? "V" : "V";
                case ShapeKeyboardButton.W: return shortHand ? "W" : "W";
                case ShapeKeyboardButton.X: return shortHand ? "X" : "X";
                case ShapeKeyboardButton.Y: return shortHand ? "Y" : "Y";
                case ShapeKeyboardButton.Z: return shortHand ? "Z" : "Z";
                case ShapeKeyboardButton.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case ShapeKeyboardButton.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case ShapeKeyboardButton.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case ShapeKeyboardButton.GRAVE: return shortHand ? "`" : "Grave";//Check
                case ShapeKeyboardButton.SPACE: return shortHand ? "Space" : "Space";
                case ShapeKeyboardButton.ESCAPE: return shortHand ? "Esc" : "Escape";
                case ShapeKeyboardButton.ENTER: return shortHand ? "Enter" : "Enter";
                case ShapeKeyboardButton.TAB: return shortHand ? "Tab" : "Tab";
                case ShapeKeyboardButton.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case ShapeKeyboardButton.INSERT: return shortHand ? "Ins" : "Insert";
                case ShapeKeyboardButton.DELETE: return shortHand ? "Del" : "Delete";
                case ShapeKeyboardButton.RIGHT: return shortHand ? "Right" : "Right";
                case ShapeKeyboardButton.LEFT: return shortHand ? "Left" : "Left";
                case ShapeKeyboardButton.DOWN: return shortHand ? "Down" : "Down";
                case ShapeKeyboardButton.UP: return shortHand ? "Up" : "Up";
                case ShapeKeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case ShapeKeyboardButton.PAGE_DOWN: return shortHand ? "PDo" : "";
                case ShapeKeyboardButton.HOME: return shortHand ? "Home" : "Home";
                case ShapeKeyboardButton.END: return shortHand ? "End" : "End";
                case ShapeKeyboardButton.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case ShapeKeyboardButton.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case ShapeKeyboardButton.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case ShapeKeyboardButton.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case ShapeKeyboardButton.PAUSE: return shortHand ? "Pause" : "Pause";
                case ShapeKeyboardButton.F1: return shortHand ? "F1" : "F1";
                case ShapeKeyboardButton.F2: return shortHand ? "F2" : "F2";
                case ShapeKeyboardButton.F3: return shortHand ? "F3" : "F3";
                case ShapeKeyboardButton.F4: return shortHand ? "F4" : "F4";
                case ShapeKeyboardButton.F5: return shortHand ? "F5" : "F5";
                case ShapeKeyboardButton.F6: return shortHand ? "F6" : "F6";
                case ShapeKeyboardButton.F7: return shortHand ? "F7" : "F7";
                case ShapeKeyboardButton.F8: return shortHand ? "F8" : "F8";
                case ShapeKeyboardButton.F9: return shortHand ? "F9" : "F9";
                case ShapeKeyboardButton.F10: return shortHand ? "F10" : "F10";
                case ShapeKeyboardButton.F11: return shortHand ? "F11" : "F11";
                case ShapeKeyboardButton.F12: return shortHand ? "F12" : "F12";
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
                case ShapeKeyboardButton.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case ShapeKeyboardButton.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case ShapeKeyboardButton.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case ShapeKeyboardButton.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case ShapeKeyboardButton.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case ShapeKeyboardButton.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case ShapeKeyboardButton.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case ShapeKeyboardButton.BACK: return shortHand ? "Back" : "Back";
                case ShapeKeyboardButton.NULL: return shortHand ? "Null" : "Null";
                case ShapeKeyboardButton.MENU: return shortHand ? "Menu" : "Menu";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        
}
public class ShapeMouseButtonInput : IShapeInputType
    {
        private ShapeMouseButton button;
        private ShapeInputState state = new();
        public ShapeMouseButtonInput(ShapeMouseButton button) { this.button = button; }

        public void Update(float dt, int gamepadIndex)
        {
            bool down = IsDown();
            ShapeInputState current = new(down, !down, 0f, gamepadIndex);
            
            state = new(state, current);
        }
        public string GetName(bool shorthand = true) => GetMouseButtonName(button, shorthand);

        public ShapeInputState GetState() => state;

        public IShapeInputType Copy() => new ShapeMouseButtonInput(button);
        
        private bool IsDown()
        {
            var id = (int)button;
            if (id >= 10)
            {
                var value = GetMouseWheelMoveV();
                return button switch
                {
                    ShapeMouseButton.MW_LEFT => value.X < 0f,
                    ShapeMouseButton.MW_RIGHT => value.X > 0f,
                    ShapeMouseButton.MW_UP => value.Y < 0f,
                    ShapeMouseButton.MW_DOWN => value.Y > 0f,
                    _ => false
                };
            }
            
            return IsMouseButtonDown(id);
        }
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
                default: return shortHand ? "No Key" : "No Key";
            }
        }

    }
public class ShapeGamepadButtonInput : IShapeInputType
{
    private ShapeGamepadButton button;
    private ShapeInputState state = new();
    private float deadzone;

    public ShapeGamepadButtonInput(ShapeGamepadButton button, float deadzone = 0.2f)
    {
        this.button = button; 
        this.deadzone = deadzone;
    }
    
    public IShapeInputType Copy() => new ShapeGamepadButtonInput(button, deadzone);
    public string GetName(bool shorthand = true) => GetGamepadButtonName(button, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        bool down = IsDown(gamepadIndex);
        ShapeInputState current = new(down, !down, 0f, gamepadIndex);
    }
    public ShapeInputState GetState() => state;
    
    private bool IsDown(int gamepadIndex)
    {
        var id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value > 0f;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value < 0f;
        }
        
        return IsGamepadButtonDown(gamepadIndex, id);
    }
    
    
    public static string GetGamepadButtonName(ShapeGamepadButton button, bool shortHand = true)
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
                default: return shortHand ? "No Key" : "No Key";
            }
        }

}
public class ShapeKeyboardButtonAxisInput : IShapeInputType
{
    private ShapeKeyboardButton neg;
    private ShapeKeyboardButton pos;
    private ShapeInputState state = new();

    public ShapeKeyboardButtonAxisInput(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }

    public IShapeInputType Copy() => new ShapeKeyboardButtonAxisInput(neg, pos);
    public string GetName(bool shorthand = true)
    {
        string negName = ShapeKeyboardButtonInput.GetKeyboardButtonName(neg, shorthand);
        string posName = ShapeKeyboardButtonInput.GetKeyboardButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }
    public void Update(float dt, int gamepadIndex)
    {
        float axis = GetAxis();
        bool down = axis != 0f;
        ShapeInputState current = new(down, !down, axis, gamepadIndex);
        state = new(state, current);
    }
    public ShapeInputState GetState() => state;
    
    private float GetAxis()
    {
        float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
        float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
}
public class ShapeMouseButtonAxisInput : IShapeInputType
{
    private ShapeMouseButton neg;
    private ShapeMouseButton pos;
    private ShapeInputState state = new();

    public ShapeMouseButtonAxisInput(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        this.neg = neg;
        this.pos = pos;
    }

    public string GetName(bool shorthand = true)
    {
        string negName = ShapeMouseButtonInput.GetMouseButtonName(neg, shorthand);
        string posName = ShapeMouseButtonInput.GetMouseButtonName(pos, shorthand);
        StringBuilder b = new(posName.Length + negName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }
    public void Update(float dt, int gamepadIndex)
    {
        float axis = GetAxis();
        bool down = axis != 0f;
        ShapeInputState current = new(down, !down, axis, gamepadIndex);
        state = new(state, current);
    }
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeMouseButtonAxisInput(neg, pos);

    private float GetAxis()
    {
        float vNegative = GetValue(neg);
        float vPositive = GetValue(pos);
        return vPositive - vNegative;
    }
    private float GetValue(ShapeMouseButton button)
    {
        int id = (int)button;
        if (id >= 10)
        {
            Vector2 value = GetMouseWheelMoveV();
            if (button == ShapeMouseButton.MW_LEFT) return MathF.Abs(value.X);
            else if (button == ShapeMouseButton.MW_RIGHT) return value.X;
            else if (button == ShapeMouseButton.MW_UP) return MathF.Abs(value.Y);
            else if (button == ShapeMouseButton.MW_DOWN) return value.Y;
            else return 0f;
        }
        else return IsMouseButtonDown(id) ? 1f : 0f;
    }
}
public class ShapeGamepadButtonAxisInput : IShapeInputType
{
    private ShapeGamepadButton neg;
    private ShapeGamepadButton pos;
    private float deadzone;
    private ShapeInputState state = new();

    public ShapeGamepadButtonAxisInput(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
    }

    public string GetName(bool shorthand = true)
    {
        string negName = ShapeGamepadButtonInput.GetGamepadButtonName(neg, shorthand);
        string posName = ShapeGamepadButtonInput.GetGamepadButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }

    public void Update(float dt, int gamepadIndex)
    {
        float axis = GetAxis(gamepadIndex);
        bool down = axis != 0f;
        ShapeInputState current = new(down, !down, axis, gamepadIndex);
        state = new(state, current);
    }
    public ShapeInputState GetState() => state;

    public IShapeInputType Copy() => new ShapeGamepadButtonAxisInput(neg, pos, deadzone);

    private float GetAxis(int gamepadIndex)
    {
        float vNegative = GetValue(gamepadIndex, neg);
        float vPositive = GetValue(gamepadIndex, pos);
        return vPositive - vNegative;
    }
    private float GetValue(int gamepadIndex, ShapeGamepadButton button)
    {
        if (gamepadIndex < 0) return 0f;

        int id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value > 0f) return value;
            
            return 0f;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value < 0) return MathF.Abs(value);
            
            return 0f;
        }
        
        return IsGamepadButtonDown(gamepadIndex, id) ? 1f : 0f;
    }
}
public class ShapeMouseWheelAxisInput : IShapeInputType
{
    private ShapeMouseWheelAxis axis;
    private ShapeInputState state = new();

    public ShapeMouseWheelAxisInput(ShapeMouseWheelAxis axis)
    {
        this.axis = axis;
    }

    public string GetName(bool shorthand = true) => GetMouseWheelAxisName(axis, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        float axisValue = GetValue();
        bool down = axisValue != 0f;
        ShapeInputState current = new(down, !down, axisValue, gamepadIndex);
        state = new(state, current);
    }
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeMouseWheelAxisInput(axis);

    private float GetValue()
    {
        Vector2 value = GetMouseWheelMoveV();
        return axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
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
public class ShapeGamepadAxisInput : IShapeInputType
{
    private ShapeGamepadAxis axis;
    private float deadzone;
    private ShapeInputState state = new();

    public ShapeGamepadAxisInput(ShapeGamepadAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
    }

    public string GetName(bool shorthand = true) => GetGamepadAxisName(axis, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        float axisValue = GetValue(gamepadIndex);
        bool down = axisValue != 0f;
        ShapeInputState current = new(down, !down, axisValue, gamepadIndex);
        state = new(state, current);
    }
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeGamepadAxisInput(axis);

    private float GetValue(int gamepadIndex)
    {
        float value = GetGamepadAxisMovement(gamepadIndex, (int)axis);
        if (MathF.Abs(value) < deadzone) return 0f;
        return value;
    }
    
    public static string GetGamepadAxisName(ShapeGamepadAxis axis, bool shortHand = true)
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

}


public class ShapeButton
{
    public IShapeInputType InputType { get; private set; }
    
    public ShapeButton(IShapeInputType inputType)
    {
        this.InputType = inputType;
    }
    public ShapeButton(ShapeKeyboardButtonInput keyboardButton)
    {
        InputType = keyboardButton;
    }
    public ShapeButton(ShapeMouseButtonInput mouseButton)
    {
        InputType = mouseButton;
    }
    public ShapeButton(ShapeGamepadButtonInput gamepadButton)
    {
        InputType = gamepadButton;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeKeyboardButton button)
    {
        InputType = new ShapeKeyboardButtonInput(button);
    }
    public ShapeButton(ShapeMouseButton button)
    {
        InputType = new ShapeMouseButtonInput(button);
    }
    public ShapeButton(ShapeGamepadButton button, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadButtonInput(button, deadzone);
        //GamepadIndex = gamepadIndex;
    }

    public ShapeButton(ShapeKeyboardButtonAxisInput keyboardButtonAxisInput)
    {
        InputType = keyboardButtonAxisInput;
    }
    public ShapeButton(ShapeMouseButtonAxisInput mouseButtonAxisInput)
    {
        InputType = mouseButtonAxisInput;
    }
    public ShapeButton(ShapeGamepadButtonAxisInput gamepadButtonAxisInput)
    {
        InputType = gamepadButtonAxisInput;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        InputType = new ShapeKeyboardButtonAxisInput(neg, pos);
    }
    public ShapeButton(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        InputType = new ShapeMouseButtonAxisInput(neg, pos);
    }
    public ShapeButton(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadButtonAxisInput(neg, pos, deadzone);
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeMouseWheelAxisInput mouseWheelAxisInput)
    {
        InputType = mouseWheelAxisInput;
    }
    public ShapeButton(ShapeGamepadAxisInput gamepadAxisInput)
    {
        InputType = gamepadAxisInput;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeMouseWheelAxis mouseWheelAxis)
    {
        InputType = new ShapeMouseWheelAxisInput(mouseWheelAxis);
    }
    public ShapeButton(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadAxisInput(gamepadAxis, deadzone);
        //GamepadIndex = gamepadIndex;
    }
    private ShapeButton(ShapeButton button)
    {
        InputType = button.InputType.Copy();
    }

    
    public ShapeButton Copy() => new ShapeButton(this);
    public void Update(float dt, int gamepadIndex) => InputType.Update(dt, gamepadIndex);
}

public class ShapeInputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    public int GamepadIndex { get; set; } = -1;

    public ShapeInputState State { get; private set; } = new();

    public readonly List<ShapeButton> Inputs = new();

    public ShapeInputAction()
    {
        ID = ShapeID.NextID;
    }
    public ShapeInputAction(uint accessTag, uint id)
    {
        ID = id;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, int gamepadIndex)
    {
        ID = ShapeID.NextID;
        GamepadIndex = gamepadIndex;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepadIndex)
    {
        ID = id;
        GamepadIndex = gamepadIndex;
        AccessTag = accessTag;
    }
    public ShapeInputAction(params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(uint accessTag, uint id, params ShapeButton[] buttons)
    {
        ID = id;
        AccessTag = accessTag;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(buttons);
        GamepadIndex = gamepadIndex;
    }
    public ShapeInputAction(uint accessTag, int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        GamepadIndex = gamepadIndex;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = id;
        AccessTag = accessTag;
        GamepadIndex = gamepadIndex;
        Inputs.AddRange(buttons);
    }
    
    public void Update(float dt)
    {
        ShapeInputState current = new();
        foreach (var input in Inputs)
        {
            input.Update(dt, GamepadIndex);
            current = current.Accumulate(input.InputType.GetState());
        }

        State = new(State, current);
    }
}



public static class ShapeInput
{
    public static readonly uint AllAccessTag = 0;
    public static bool Locked { get; private set; } = false;
    private static List<uint> lockExceptionTags = new();

    #region Lock System
    public static void Lock()
    {
        Locked = true;
        lockExceptionTags.Clear();
    }
    public static void Lock(params uint[] exceptionTags)
    {
        Locked = true;
        lockExceptionTags.Clear();
        if(exceptionTags.Length > 0) lockExceptionTags.AddRange(exceptionTags);
    }
    public static void Unlock()
    {
        Locked = false;
        lockExceptionTags.Clear();
    }
    public static bool HasAccess(uint tag) => tag == AllAccessTag || lockExceptionTags.Contains(tag);
    #endregion
    
    #region Input Actions

    //works for all gamepad / keyboard / mouse buttons -> action is device independent
    // IsPressed(uint mapID, uint actionID) -> map has access tag for locked system
    // IsReleased
    // IsDown
    // IsUp
    
    //works for all axis
    // GetAxis

    #endregion


    #region Basic
    //GetAxis(GamepadAxis, accessTag)
    //Mouse Down/Up/Released/Pressed

    public static bool IsKeyPressed(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyPressed(key);
    }
    public static bool IsKeyReleased(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyReleased(key);
    }
    public static bool IsKeyDown(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyDown(key);
    }
    public static bool IsKeyUp(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyUp(key);
    }
    public static bool IsKeyPressed(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyPressed(key);
    }
    public static bool IsKeyReleased(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyReleased(key);
    }
    public static bool IsKeyDown(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyDown(key);
    }
    public static bool IsKeyUp(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyUp(key);
    }
    
    public static List<char> GetKeyboardStreamChar()
    {
        if (Locked) return new();
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
    public static List<char> GetKeyboardStreamChar(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
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
    public static string GetKeyboardStream(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
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
    public static string GetKeyboardStream(string curText, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
        var chars = GetKeyboardStreamChar(accessTag);
        var b = new StringBuilder(chars.Count + curText.Length);
        b.Append(curText);
        b.Append(chars);
        return b.ToString();
    }
    #endregion
}



public enum ShapeKeyboardButton
    {
        APOSTROPHE = 39,
        COMMA = 44,
        MINUS = 45,
        PERIOD = 46,
        SLASH = 47,
        ZERO = 48,
        ONE = 49,
        TWO = 50,
        THREE = 51,
        FOUR = 52,
        FIVE = 53,
        SIX = 54,
        SEVEN = 55,
        EIGHT = 56,
        NINE = 57,
        SEMICOLON = 59,
        EQUAL = 61,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LEFT_BRACKET = 91,
        BACKSLASH = 92,
        RIGHT_BRACKET = 93,
        GRAVE = 96,
        SPACE = 0x20,
        ESCAPE = 0x100,
        ENTER = 257,
        TAB = 258,
        BACKSPACE = 259,
        INSERT = 260,
        DELETE = 261,
        RIGHT = 262,
        LEFT = 263,
        DOWN = 264,
        UP = 265,
        PAGE_UP = 266,
        PAGE_DOWN = 267,
        HOME = 268,
        END = 269,
        CAPS_LOCK = 280,
        SCROLL_LOCK = 281,
        NUM_LOCK = 282,
        PRINT_SCREEN = 283,
        PAUSE = 284,
        F1 = 290,
        F2 = 291,
        F3 = 292,
        F4 = 293,
        F5 = 294,
        F6 = 295,
        F7 = 296,
        F8 = 297,
        F9 = 298,
        F10 = 299,
        F11 = 300,
        F12 = 301,
        LEFT_SHIFT = 340,
        LEFT_CONTROL = 341,
        LEFT_ALT = 342,
        LEFT_SUPER = 343,
        RIGHT_SHIFT = 344,
        RIGHT_CONTROL = 345,
        RIGHT_ALT = 346,
        RIGHT_SUPER = 347,
        KB_MENU = 348,
        KP_0 = 320,
        KP_1 = 321,
        KP_2 = 322,
        KP_3 = 323,
        KP_4 = 324,
        KP_5 = 325,
        KP_6 = 326,
        KP_7 = 327,
        KP_8 = 328,
        KP_9 = 329,
        KP_DECIMAL = 330,
        KP_DIVIDE = 331,
        KP_MULTIPLY = 332,
        KP_SUBTRACT = 333,
        KP_ADD = 334,
        KP_ENTER = 335,
        KP_EQUAL = 336,
        VOLUME_UP = 24,
        VOLUME_DOWN = 25,
        BACK = 7,
        NULL = 8,
        MENU = 9,
    }
    public enum ShapeMouseButton
    {
        LEFT = 0,
        RIGHT = 1,
        MIDDLE = 2,
        SIDE = 3,
        EXTRA = 4,
        FORWARD = 5,
        BACK = 6,
        MW_UP = 10,
        MW_DOWN = 11,
        MW_LEFT = 12,
        MW_RIGHT = 13,
    }
    public enum ShapeGamepadButton
    {
        UNKNOWN = 0,
        LEFT_FACE_UP = 1,
        LEFT_FACE_RIGHT = 2,
        LEFT_FACE_DOWN = 3,
        LEFT_FACE_LEFT = 4,
        RIGHT_FACE_UP = 5,
        RIGHT_FACE_RIGHT = 6,
        RIGHT_FACE_DOWN = 7,
        RIGHT_FACE_LEFT = 8,
        LEFT_TRIGGER_TOP = 9,
        LEFT_TRIGGER_BOTTOM = 10,
        RIGHT_TRIGGER_TOP = 11,
        RIGHT_TRIGGER_BOTTOM = 12,
        MIDDLE_LEFT = 13,
        MIDDLE = 14,
        MIDDLE_RIGHT = 15,
        LEFT_THUMB = 16,
        RIGHT_THUMB = 17,

        LEFT_STICK_RIGHT = 30,
        LEFT_STICK_LEFT = 40,
        LEFT_STICK_DOWN = 31,
        LEFT_STICK_UP = 41,

        RIGHT_STICK_RIGHT = 32,
        RIGHT_STICK_LEFT = 42,
        RIGHT_STICK_DOWN = 33,
        RIGHT_STICK_UP = 43,
    }
    public enum ShapeMouseWheelAxis
    {
        HORIZONTAL = 0,
        VERTICAL = 1,
    }
    public enum ShapeGamepadAxis
    {
        LEFT_X = 0,
        LEFT_Y = 1,
        RIGHT_X = 2,
        RIGHT_Y = 3,
        LEFT_TRIGGER = 4,
        RIGHT_TRIGGER = 5,
    }
    