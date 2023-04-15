
using ShapeCore;
using ShapeLib;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using Vortice.XInput;

namespace ShapeInput
{
    public struct InputState
    {
        public bool down = false;
        public bool up = true;
        public float axis = 0f;

        public InputState() { }
        public InputState(bool down, bool up, float axis)
        {
            this.down = down;
            this.up = up;
            this.axis = axis;
        }
    }
    public struct InputConditionState
    {
        public bool down = false;
        public bool up = true;
        public bool released = false;
        public bool pressed = false;
        public float axisValue = 0f;

        public InputConditionState() { }
        public InputConditionState(bool down, bool up, bool released, bool pressed, float axisValue) 
        { 
            this.down = down;
            this.up = up;
            this.released = released;
            this.pressed = pressed;
            this.axisValue = axisValue;
        }

        public static InputConditionState Generate(InputConditionState state, bool down, bool up, float axis)
        {
            bool released = state.down && up;
            bool pressed = state.up && down;

            return new InputConditionState(down, up, released, pressed, axis);
        }
    }
    
    
    public enum KeyboardButton
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
    public enum MouseButton
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
    public enum GamepadButton
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
    public enum MouseWheelAxis
    {
        HORIZONTAL = 0,
        VERTICAL = 1,
    }
    public enum GamepadAxis
    {
        LEFT_X = 0,
        LEFT_Y = 1,
        RIGHT_X = 2,
        RIGHT_Y = 3,
        LEFT_TRIGGER = 4,
        RIGHT_TRIGGER = 5,
    }


    //new names for all of that
    public interface IInput
    {
        public string GetName(bool shorthand = true);

        public IInput Copy();
        public InputState GetState(int slot);
        public bool IsGamepad();

        public static string GetMouseButtonName(MouseButton button, bool shortHand = true)
        {
            switch (button)
            {
                case MouseButton.LEFT: return shortHand ? "LMB" : "Left Mouse Button";
                case MouseButton.RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
                case MouseButton.MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
                case MouseButton.SIDE: return shortHand ? "SMB" : "Side Mouse Button";
                case MouseButton.EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
                case MouseButton.FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
                case MouseButton.BACK: return shortHand ? "BMB" : "Back Mouse Button";
                case MouseButton.MW_UP: return shortHand ? "MW U" : "Mouse Wheel Up";
                case MouseButton.MW_DOWN: return shortHand ? "MW D" : "Mouse Wheel Down";
                case MouseButton.MW_LEFT: return shortHand ? "MW L" : "Mouse Wheel Left";
                case MouseButton.MW_RIGHT: return shortHand ? "MW R" : "Mouse Wheel Right";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        public static string GetKeyboardButtonName(KeyboardButton button, bool shortHand = true)
        {
            switch (button)
            {
                case KeyboardButton.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
                case KeyboardButton.COMMA: return shortHand ? "," : "Comma";
                case KeyboardButton.MINUS: return shortHand ? "-" : "Minus";
                case KeyboardButton.PERIOD: return shortHand ? "." : "Period";
                case KeyboardButton.SLASH: return shortHand ? "/" : "Slash";
                case KeyboardButton.ZERO: return shortHand ? "0" : "Zero";
                case KeyboardButton.ONE: return shortHand ? "1" : "One";
                case KeyboardButton.TWO: return shortHand ? "2" : "Two";
                case KeyboardButton.THREE: return shortHand ? "3" : "Three";
                case KeyboardButton.FOUR: return shortHand ? "4" : "Four";
                case KeyboardButton.FIVE: return shortHand ? "5" : "Five";
                case KeyboardButton.SIX: return shortHand ? "6" : "Six";
                case KeyboardButton.SEVEN: return shortHand ? "7" : "Seven";
                case KeyboardButton.EIGHT: return shortHand ? "8" : "Eight";
                case KeyboardButton.NINE: return shortHand ? "9" : "Nine";
                case KeyboardButton.SEMICOLON: return shortHand ? ";" : "Semi Colon";
                case KeyboardButton.EQUAL: return shortHand ? "=" : "Equal";
                case KeyboardButton.A: return shortHand ? "A" : "A";
                case KeyboardButton.B: return shortHand ? "B" : "B";
                case KeyboardButton.C: return shortHand ? "C" : "C";
                case KeyboardButton.D: return shortHand ? "D" : "D";
                case KeyboardButton.E: return shortHand ? "E" : "E";
                case KeyboardButton.F: return shortHand ? "F" : "F";
                case KeyboardButton.G: return shortHand ? "G" : "G";
                case KeyboardButton.H: return shortHand ? "H" : "H";
                case KeyboardButton.I: return shortHand ? "I" : "I";
                case KeyboardButton.J: return shortHand ? "J" : "J";
                case KeyboardButton.K: return shortHand ? "K" : "K";
                case KeyboardButton.L: return shortHand ? "L" : "L";
                case KeyboardButton.M: return shortHand ? "M" : "M";
                case KeyboardButton.N: return shortHand ? "N" : "N";
                case KeyboardButton.O: return shortHand ? "O" : "O";
                case KeyboardButton.P: return shortHand ? "P" : "P";
                case KeyboardButton.Q: return shortHand ? "Q" : "Q";
                case KeyboardButton.R: return shortHand ? "R" : "R";
                case KeyboardButton.S: return shortHand ? "S" : "S";
                case KeyboardButton.T: return shortHand ? "T" : "T";
                case KeyboardButton.U: return shortHand ? "U" : "U";
                case KeyboardButton.V: return shortHand ? "V" : "V";
                case KeyboardButton.W: return shortHand ? "W" : "W";
                case KeyboardButton.X: return shortHand ? "X" : "X";
                case KeyboardButton.Y: return shortHand ? "Y" : "Y";
                case KeyboardButton.Z: return shortHand ? "Z" : "Z";
                case KeyboardButton.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case KeyboardButton.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case KeyboardButton.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case KeyboardButton.GRAVE: return shortHand ? "`" : "Grave";//Check
                case KeyboardButton.SPACE: return shortHand ? "Space" : "Space";
                case KeyboardButton.ESCAPE: return shortHand ? "Esc" : "Escape";
                case KeyboardButton.ENTER: return shortHand ? "Enter" : "Enter";
                case KeyboardButton.TAB: return shortHand ? "Tab" : "Tab";
                case KeyboardButton.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case KeyboardButton.INSERT: return shortHand ? "Ins" : "Insert";
                case KeyboardButton.DELETE: return shortHand ? "Del" : "Delete";
                case KeyboardButton.RIGHT: return shortHand ? "Right" : "Right";
                case KeyboardButton.LEFT: return shortHand ? "Left" : "Left";
                case KeyboardButton.DOWN: return shortHand ? "Down" : "Down";
                case KeyboardButton.UP: return shortHand ? "Up" : "Up";
                case KeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case KeyboardButton.PAGE_DOWN: return shortHand ? "PDo" : "";
                case KeyboardButton.HOME: return shortHand ? "Home" : "Home";
                case KeyboardButton.END: return shortHand ? "End" : "End";
                case KeyboardButton.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case KeyboardButton.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case KeyboardButton.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case KeyboardButton.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case KeyboardButton.PAUSE: return shortHand ? "Pause" : "Pause";
                case KeyboardButton.F1: return shortHand ? "F1" : "F1";
                case KeyboardButton.F2: return shortHand ? "F2" : "F2";
                case KeyboardButton.F3: return shortHand ? "F3" : "F3";
                case KeyboardButton.F4: return shortHand ? "F4" : "F4";
                case KeyboardButton.F5: return shortHand ? "F5" : "F5";
                case KeyboardButton.F6: return shortHand ? "F6" : "F6";
                case KeyboardButton.F7: return shortHand ? "F7" : "F7";
                case KeyboardButton.F8: return shortHand ? "F8" : "F8";
                case KeyboardButton.F9: return shortHand ? "F9" : "F9";
                case KeyboardButton.F10: return shortHand ? "F10" : "F10";
                case KeyboardButton.F11: return shortHand ? "F11" : "F11";
                case KeyboardButton.F12: return shortHand ? "F12" : "F12";
                case KeyboardButton.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
                case KeyboardButton.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
                case KeyboardButton.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
                case KeyboardButton.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
                case KeyboardButton.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
                case KeyboardButton.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
                case KeyboardButton.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
                case KeyboardButton.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
                case KeyboardButton.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
                case KeyboardButton.KP_0: return shortHand ? "KP0" : "Keypad 0";
                case KeyboardButton.KP_1: return shortHand ? "KP1" : "Keypad 1";
                case KeyboardButton.KP_2: return shortHand ? "KP2" : "Keypad 2";
                case KeyboardButton.KP_3: return shortHand ? "KP3" : "Keypad 3";
                case KeyboardButton.KP_4: return shortHand ? "KP4" : "Keypad 4";
                case KeyboardButton.KP_5: return shortHand ? "KP5" : "Keypad 5";
                case KeyboardButton.KP_6: return shortHand ? "KP6" : "Keypad 6";
                case KeyboardButton.KP_7: return shortHand ? "KP7" : "Keypad 7";
                case KeyboardButton.KP_8: return shortHand ? "KP8" : "Keypad 8";
                case KeyboardButton.KP_9: return shortHand ? "KP9" : "Keypad 9";
                case KeyboardButton.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
                case KeyboardButton.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
                case KeyboardButton.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case KeyboardButton.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case KeyboardButton.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case KeyboardButton.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case KeyboardButton.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case KeyboardButton.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case KeyboardButton.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case KeyboardButton.BACK: return shortHand ? "Back" : "Back";
                case KeyboardButton.NULL: return shortHand ? "Null" : "Null";
                case KeyboardButton.MENU: return shortHand ? "Menu" : "Menu";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        public static string GetGamepadButtonName(GamepadButton button, bool shortHand = true)
        {
            switch (button)
            {
                case GamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
                case GamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
                case GamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
                case GamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
                case GamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
                case GamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
                case GamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
                case GamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
                case GamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
                case GamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
                case GamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
                case GamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
                case GamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
                case GamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
                case GamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
                case GamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
                case GamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
                case GamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
                case GamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
                case GamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
                case GamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
                case GamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
                case GamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
                case GamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
                case GamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
                case GamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        
        
        public static string GetGamepadAxisName(GamepadAxis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case GamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                case GamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                case GamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                case GamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                case GamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                case GamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        public static string GetMouseWheelAxisName(MouseWheelAxis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case MouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                case MouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                default: return shortHand ? "No Key" : "No Key";
            }
        }

        
        public static IInput Create(KeyboardButton button) { return new KeyboardButtonInput(button); }
        public static IInput Create(MouseButton button) { return new MouseButtonInput(button); }
        public static IInput Create(GamepadButton button, float deadzone = 0.2f) { return new GamepadButtonInput(button, deadzone); }
        public static IInput Create(KeyboardButton neg, KeyboardButton pos) { return new KeyboardButtonAxisInput(neg, pos); }
        public static IInput Create(GamepadButton neg, GamepadButton pos, float deadzone = 0.2f) { return new GamepadButtonAxisInput(neg, pos, deadzone); }
        public static IInput Create(MouseButton neg, MouseButton pos) { return new MouseButtonAxisInput(neg, pos); }
        public static IInput Create(MouseWheelAxis axis) { return new MouseWheelAxisInput(axis); }
        public static IInput Create(GamepadAxis axis, float deadzone = 0.2f) { return new GamepadAxisInput(axis, deadzone); }

        /*
        protected static int GetButtonID(Buttons button)
        {
            if (button == Buttons.BACK)
            {
                return (int)KeyboardKey.KEY_BACK;
            }
            else if (button == Buttons.NULL)
            {
                return (int)KeyboardKey.KEY_NULL;
            }
            else if (button == Buttons.MENU)
            {
                return (int)KeyboardKey.KEY_MENU;
            }
            //else if (IsGamepadAxis(key)) return (int)key - 400;
            else if (IsGamepadButton(button)) return (int)button - 500;
            else if (IsGamepadAxisButtonPos(button)) return (int)button - 600;
            else if (IsGamepadAxisButtonNeg(button)) return (int)button - 610;
            return (int)button;
        }
        protected static int GetAxisID(Axis axis)
        {
            if (IsGamepadAxis(axis)) return (int)axis - 400;
            else return (int)axis;
        }

        protected static string GetButtonName(Buttons button, bool shortHand = true)
        {
            switch (button)
            {
                case Buttons.MB_LEFT: return shortHand ? "LMB" : "Left Mouse Button";
                case Buttons.MB_RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
                case Buttons.MB_MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
                case Buttons.MB_SIDE: return shortHand ? "SMB" : "Side Mouse Button";
                case Buttons.MB_EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
                case Buttons.MB_FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
                case Buttons.MB_BACK: return shortHand ? "BMB" : "Back Mouse Button";
                case Buttons.MW_UP: return shortHand ? "MWU" : "Mouse Wheel Up";
                case Buttons.MW_DOWN: return shortHand ? "MWD" : "Mouse Wheel Down";
                case Buttons.MW_LEFT: return shortHand ? "MWL" : "Mouse Wheel Left";
                case Buttons.MW_RIGHT: return shortHand ? "MWR" : "Mouse Wheel Right";
                //case Buttons.MW_AXIS_HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                //case Buttons.MW_AXIS_VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                case Buttons.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
                case Buttons.COMMA: return shortHand ? "," : "Comma";
                case Buttons.MINUS: return shortHand ? "-" : "Minus";
                case Buttons.PERIOD: return shortHand ? "." : "Period";
                case Buttons.SLASH: return shortHand ? "/" : "Slash";
                case Buttons.ZERO: return shortHand ? "0" : "Zero";
                case Buttons.ONE: return shortHand ? "1" : "One";
                case Buttons.TWO: return shortHand ? "2" : "Two";
                case Buttons.THREE: return shortHand ? "3" : "Three";
                case Buttons.FOUR: return shortHand ? "4" : "Four";
                case Buttons.FIVE: return shortHand ? "5" : "Five";
                case Buttons.SIX: return shortHand ? "6" : "Six";
                case Buttons.SEVEN: return shortHand ? "7" : "Seven";
                case Buttons.EIGHT: return shortHand ? "8" : "Eight";
                case Buttons.NINE: return shortHand ? "9" : "Nine";
                case Buttons.SEMICOLON: return shortHand ? ";" : "Semi Colon";
                case Buttons.EQUAL: return shortHand ? "=" : "Equal";
                case Buttons.A: return shortHand ? "A" : "A";
                case Buttons.B: return shortHand ? "B" : "B";
                case Buttons.C: return shortHand ? "C" : "C";
                case Buttons.D: return shortHand ? "D" : "D";
                case Buttons.E: return shortHand ? "E" : "E";
                case Buttons.F: return shortHand ? "F" : "F";
                case Buttons.G: return shortHand ? "G" : "G";
                case Buttons.H: return shortHand ? "H" : "H";
                case Buttons.I: return shortHand ? "I" : "I";
                case Buttons.J: return shortHand ? "J" : "J";
                case Buttons.K: return shortHand ? "K" : "K";
                case Buttons.L: return shortHand ? "L" : "L";
                case Buttons.M: return shortHand ? "M" : "M";
                case Buttons.N: return shortHand ? "N" : "N";
                case Buttons.O: return shortHand ? "O" : "O";
                case Buttons.P: return shortHand ? "P" : "P";
                case Buttons.Q: return shortHand ? "Q" : "Q";
                case Buttons.R: return shortHand ? "R" : "R";
                case Buttons.S: return shortHand ? "S" : "S";
                case Buttons.T: return shortHand ? "T" : "T";
                case Buttons.U: return shortHand ? "U" : "U";
                case Buttons.V: return shortHand ? "V" : "V";
                case Buttons.W: return shortHand ? "W" : "W";
                case Buttons.X: return shortHand ? "X" : "X";
                case Buttons.Y: return shortHand ? "Y" : "Y";
                case Buttons.Z: return shortHand ? "Z" : "Z";
                case Buttons.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case Buttons.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case Buttons.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case Buttons.GRAVE: return shortHand ? "`" : "Grave";//Check
                case Buttons.SPACE: return shortHand ? "Space" : "Space";
                case Buttons.ESCAPE: return shortHand ? "Esc" : "Escape";
                case Buttons.ENTER: return shortHand ? "Enter" : "Enter";
                case Buttons.TAB: return shortHand ? "Tab" : "Tab";
                case Buttons.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case Buttons.INSERT: return shortHand ? "Ins" : "Insert";
                case Buttons.DELETE: return shortHand ? "Del" : "Delete";
                case Buttons.RIGHT: return shortHand ? "Right" : "Right";
                case Buttons.LEFT: return shortHand ? "Left" : "Left";
                case Buttons.DOWN: return shortHand ? "Down" : "Down";
                case Buttons.UP: return shortHand ? "Up" : "Up";
                case Buttons.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case Buttons.PAGE_DOWN: return shortHand ? "PDo" : "";
                case Buttons.HOME: return shortHand ? "Home" : "Home";
                case Buttons.END: return shortHand ? "End" : "End";
                case Buttons.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case Buttons.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case Buttons.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case Buttons.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case Buttons.PAUSE: return shortHand ? "Pause" : "Pause";
                case Buttons.F1: return shortHand ? "F1" : "F1";
                case Buttons.F2: return shortHand ? "F2" : "F2";
                case Buttons.F3: return shortHand ? "F3" : "F3";
                case Buttons.F4: return shortHand ? "F4" : "F4";
                case Buttons.F5: return shortHand ? "F5" : "F5";
                case Buttons.F6: return shortHand ? "F6" : "F6";
                case Buttons.F7: return shortHand ? "F7" : "F7";
                case Buttons.F8: return shortHand ? "F8" : "F8";
                case Buttons.F9: return shortHand ? "F9" : "F9";
                case Buttons.F10: return shortHand ? "F10" : "F10";
                case Buttons.F11: return shortHand ? "F11" : "F11";
                case Buttons.F12: return shortHand ? "F12" : "F12";
                case Buttons.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
                case Buttons.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
                case Buttons.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
                case Buttons.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
                case Buttons.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
                case Buttons.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
                case Buttons.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
                case Buttons.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
                case Buttons.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
                case Buttons.KP_0: return shortHand ? "KP0" : "Keypad 0";
                case Buttons.KP_1: return shortHand ? "KP1" : "Keypad 1";
                case Buttons.KP_2: return shortHand ? "KP2" : "Keypad 2";
                case Buttons.KP_3: return shortHand ? "KP3" : "Keypad 3";
                case Buttons.KP_4: return shortHand ? "KP4" : "Keypad 4";
                case Buttons.KP_5: return shortHand ? "KP5" : "Keypad 5";
                case Buttons.KP_6: return shortHand ? "KP6" : "Keypad 6";
                case Buttons.KP_7: return shortHand ? "KP7" : "Keypad 7";
                case Buttons.KP_8: return shortHand ? "KP8" : "Keypad 8";
                case Buttons.KP_9: return shortHand ? "KP9" : "Keypad 9";
                case Buttons.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
                case Buttons.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
                case Buttons.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case Buttons.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case Buttons.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case Buttons.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case Buttons.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case Buttons.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case Buttons.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case Buttons.BACK: return shortHand ? "Back" : "Back";
                case Buttons.NULL: return shortHand ? "Null" : "Null";
                case Buttons.MENU: return shortHand ? "Menu" : "Menu";
                //case Buttons.GP_AXIS_LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                //case Buttons.GP_AXIS_LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                //case Buttons.GP_AXIS_RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                //case Buttons.GP_AXIS_RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                //case Buttons.GP_AXIS_RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                //case Buttons.GP_AXIS_LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                case Buttons.GP_BUTTON_UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
                case Buttons.GP_BUTTON_LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
                case Buttons.GP_BUTTON_LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
                case Buttons.GP_BUTTON_LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
                case Buttons.GP_BUTTON_LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
                case Buttons.GP_BUTTON_RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
                case Buttons.GP_BUTTON_RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
                case Buttons.GP_BUTTON_RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
                case Buttons.GP_BUTTON_RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
                case Buttons.GP_BUTTON_LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
                case Buttons.GP_BUTTON_LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
                case Buttons.GP_BUTTON_RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
                case Buttons.GP_BUTTON_RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
                case Buttons.GP_BUTTON_MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
                case Buttons.GP_BUTTON_MIDDLE: return shortHand ? "Home" : "GP Button Home";
                case Buttons.GP_BUTTON_MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
                case Buttons.GP_BUTTON_LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
                case Buttons.GP_BUTTON_RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        protected static string GetAxisName(Axis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case Axis.MW_AXIS_HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                case Axis.MW_AXIS_VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                case Axis.GP_AXIS_LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                case Axis.GP_AXIS_LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                case Axis.GP_AXIS_RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                case Axis.GP_AXIS_RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                case Axis.GP_AXIS_RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                case Axis.GP_AXIS_LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        */

        /*
        protected static bool IsGamepad(Buttons button) { return (int)button >= 500 && (int)button <= 523; }
        protected static bool IsMouse(Buttons button) { return ((int)button >= 0 && (int)button <= 6) || IsMouseWheelAxisButton(button); }
        protected static bool IsMouseWheelAxisButton(Buttons button) { return (int)button >= 20 && (int)button <= 23; }
        protected static bool IsKeyboard(Buttons button) { return (int)button > 6 && (int)button < 400 && !IsMouseWheelAxisButton(button); }
        protected static bool IsGamepadButton(Buttons button) { return (int)button >= 500 && (int)button <= 517; }
        protected static bool IsGamepadAxisButtonPos(Buttons button) { return (int)button >= 600 && (int)button <= 603; }
        protected static bool IsGamepadAxisButtonNeg(Buttons button) { return (int)button >= 610 && (int)button <= 613; }
        protected static bool IsGamepadAxisButton(Buttons button) { return (int)button >= 600 && (int)button <= 613; }
        protected static bool IsGamepadAxis(Axis axis) { return (int)axis >= 400 && (int)axis <= 405; }
        protected static bool IsMouseWheelAxis(Axis axis) { return (int)axis == 10 || (int)axis == 11; }

        protected static Vector2 GetMouseWheelMovementV(bool inverted = false)
        {
            Vector2 movement = GetMouseWheelMoveV();

            if (inverted) return -movement;
            return movement;
        }
        protected static float GetMouseWheelAxisValue(Axis axis)
        {
            Vector2 mw = GetMouseWheelMovementV();
            if (axis == Axis.MW_AXIS_HORIZONTAL) return mw.X;
            else if (axis == Axis.MW_AXIS_VERTICAL) return mw.Y;
            else return 0f;
        }
        protected static float GetMouseWheelAxisButtonValue(Buttons button)
        {
            if (!IsMouseWheelAxisButton(button)) return 0f;
            Vector2 mw = GetMouseWheelMovementV();
            if (button == Buttons.MW_UP && mw.Y < 0) return MathF.Abs(mw.Y);
            else if (button == Buttons.MW_DOWN && mw.Y > 0) return mw.Y;
            else if (button == Buttons.MW_LEFT && mw.X < 0) return MathF.Abs(mw.X);
            else if(button == Buttons.MW_RIGHT && mw.X > 0) return mw.X;
            else return 0f;
        }
        protected static float GetGamepadAxisButtonValue(Buttons button, int slot, float deadzone)
        {
            if(!IsGamepadAxisButton(button)) return 0f;
            float movement = GetGamepadAxisMovement(slot, GetButtonID(button));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            if (IsGamepadAxisButtonPos(button) && movement > 0f)
            {
                return movement;
            }
            else if (IsGamepadAxisButtonNeg(button) && movement < 0f)
            {
                return MathF.Abs(movement);
            }
            else return 0f;
        }
        
        protected static float GetGamepadAxisValue(Axis axis, int slot, float deadzone)
        {
            if (slot < 0) return 0f;
            float movement = GetGamepadAxisMovement(slot, GetAxisID(axis));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            return movement;
        }
        protected static float GetGamepadAxisValue(Buttons axisButton, int slot, float deadzone)
        {
            if(slot < 0 ) return 0f;
            float movement = GetGamepadAxisMovement(slot, GetButtonID(axisButton));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            return movement;
        }
        */
    }
    internal class KeyboardButtonInput : IInput
    {
        private KeyboardButton button;
        
        public KeyboardButtonInput(KeyboardButton button)
        {
            this.button = button;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetKeyboardButtonName(button, shorthand);
        }
        public InputState GetState(int slot)
        {
           if(slot > 0) return new InputState();

            bool down = IsKeyDown((int)button);

            return new InputState(down, !down, 0f);
        }
        public IInput Copy()
        {
            return new KeyboardButtonInput(button);
        }

        //public bool IsDown(int slot)
        //{
        //    if (slot <= 0 && IsKeyDown((int)button)) return true;
        //    return false;
        //}
        //public bool IsUp(int slot)
        //{
        //    if (slot <= 0 && IsKeyUp((int)button)) return true;
        //    return false;
        //}
        //public float GetAxis(int slot) { return 0f; }
        
    }
    internal class MouseButtonInput : IInput
    {
        private MouseButton button;
        
        public MouseButtonInput(MouseButton button) { this.button = button; }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetMouseButtonName(button, shorthand);
        }
        public InputState GetState(int slot)
        {
            if (slot > 0) return new InputState();

            bool down = IsDown();

            return new InputState(down, !down, 0f);
        }
        public IInput Copy()
        {
            return new MouseButtonInput(button);
        }
        private bool IsDown()
        {
            int id = (int)button;
            if (id >= 10)
            {
                Vector2 value = GetMouseWheelMoveV();
                if (button == MouseButton.MW_LEFT) return value.X < 0f;
                else if (button == MouseButton.MW_RIGHT) return value.X > 0f;
                else if (button == MouseButton.MW_UP) return value.Y < 0f;
                else if (button == MouseButton.MW_DOWN) return value.Y > 0f;
                else return false;
            }
            else return IsMouseButtonDown(id);
        }
        
        //public bool IsUp(int slot)
        //{
        //    if (slot < 0) return false;
        //
        //    int id = (int)button;
        //    if (id >= 10)
        //    {
        //        Vector2 value = GetMouseWheelMoveV();
        //        if (button == MouseButton.MW_LEFT) return value.X >= 0f;
        //        else if (button == MouseButton.MW_RIGHT) return value.X <= 0f;
        //        else if (button == MouseButton.MW_UP) return value.Y >= 0f;
        //        else if (button == MouseButton.MW_DOWN) return value.Y <= 0f;
        //        else return false;
        //    }
        //    else return IsMouseButtonUp(id);
        //}
        //public float GetAxis(int slot) { return 0f; }

    }
    internal class GamepadButtonInput : IInput
    {
        private GamepadButton button;
        private float deadzone;
        
        public GamepadButtonInput(GamepadButton button, float deadzone = 0.2f) { this.button = button; this.deadzone = deadzone; }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetGamepadButtonName(button, shorthand);
        }
        public InputState GetState(int slot)
        {
            if (slot < 0) return new InputState();

            bool down = IsDown(slot);

            return new InputState(down, !down, 0f);
        }
        private bool IsDown(int slot)
        {
            int id = (int)button;
            if (id >= 30 && id <= 33)
            {
                id -= 30;
                float value = GetGamepadAxisMovement(slot, id);
                if (MathF.Abs(value) < deadzone) value = 0f;
                return value > 0f;
            }
            else if (id >= 40 && id <= 43)
            {
                id -= 40;
                float value = GetGamepadAxisMovement(slot, id);
                if (MathF.Abs(value) < deadzone) value = 0f;
                return value < 0f;
            }
            else return IsGamepadButtonDown(slot, id);
        }
        public IInput Copy()
        {
            return new GamepadButtonInput(button, deadzone);
        }

        //public bool IsUp(int slot)
        //{
        //    if (slot < 0) return false;
        //
        //    int id = (int)button;
        //    if (id >= 30 && id <= 33)
        //    {
        //        id -= 30;
        //        float value = GetGamepadAxisMovement(slot, id);
        //        if (MathF.Abs(value) < deadzone) value = 0f;
        //        return value <= 0f;
        //    }
        //    else if (id >= 40 && id <= 43)
        //    {
        //        id -= 40;
        //        float value = GetGamepadAxisMovement(slot, id);
        //        if (MathF.Abs(value) < deadzone) value = 0f;
        //        return value >= 0f;
        //    }
        //    else return IsGamepadButtonUp(slot, id);
        //}
        //public float GetAxis(int slot) { return 0f; }
    }
    
    internal class KeyboardButtonAxisInput : IInput
    {
        private KeyboardButton neg;
        private KeyboardButton pos;
        
        public KeyboardButtonAxisInput(KeyboardButton neg, KeyboardButton pos)
        {
            this.neg = neg;
            this.pos = pos;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetKeyboardButtonName(neg, shorthand) + " <> " + IInput.GetKeyboardButtonName(pos, shorthand);
        }
        public InputState GetState(int slot)
        {
            if (slot > 0) return new InputState();
            float axis = GetAxis();
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        private float GetAxis()
        {
            float vNegative = IsKeyDown((int)neg) ? 1f : 0f;
            float vPositive = IsKeyDown((int)pos) ? 1f : 0f;
            return vPositive - vNegative;
        }
        public IInput Copy()
        {
            return new KeyboardButtonAxisInput(neg, pos);
        }

        //public bool IsDown(int slot)
        //{
        //    if (slot > 0) return false;
        //
        //    return IsKeyDown((int)neg) || IsKeyDown((int)pos);
        //}
        //public bool IsUp(int slot)
        //{
        //    if (slot > 0) return false;
        //
        //    return IsKeyUp((int)neg) && IsKeyUp((int)pos);
        //}
    }
    internal class MouseButtonAxisInput : IInput
    {
        private MouseButton neg;
        private MouseButton pos;
       
        public MouseButtonAxisInput(MouseButton neg, MouseButton pos)
        {
            this.neg = neg;
            this.pos = pos;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetMouseButtonName(neg, shorthand) + " <> " + IInput.GetMouseButtonName(pos, shorthand);
        }
        public InputState GetState(int slot)
        {
            if (slot > 0) return new InputState();
            float axis = GetAxis();
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInput Copy()
        {
            return new MouseButtonAxisInput(neg, pos);
        }

        private float GetAxis()
        {
            float vNegative = GetValue(neg);
            float vPositive = GetValue(pos);
            return vPositive - vNegative;
        }
        private float GetValue(MouseButton button)
        {
            int id = (int)button;
            if (id >= 10)
            {
                Vector2 value = GetMouseWheelMoveV();
                if (button == MouseButton.MW_LEFT) return MathF.Abs(value.X);
                else if (button == MouseButton.MW_RIGHT) return value.X;
                else if (button == MouseButton.MW_UP) return MathF.Abs(value.Y);
                else if (button == MouseButton.MW_DOWN) return value.Y;
                else return 0f;
            }
            else return IsMouseButtonDown(id) ? 1f : 0f;
        }
        
        //public bool IsDown(int slot)
        //{
        //    return GetValue(slot, neg) > 0f || GetValue(slot, pos) > 0f;
        //}
        //public bool IsUp(int slot)
        //{
        //    return GetValue(slot, neg) <= 0f && GetValue(slot, pos) <= 0f;
        //}
    }
    internal class GamepadButtonAxisInput : IInput
    {
        private GamepadButton neg;
        private GamepadButton pos;
        private float deadzone;
        
        public GamepadButtonAxisInput(GamepadButton neg, GamepadButton pos, float deadzone = 0.2f)
        {
            this.neg = neg;
            this.pos = pos;
            this.deadzone = deadzone;
        }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true)
        {
            return IInput.GetGamepadButtonName(neg, shorthand) + " <> " + IInput.GetGamepadButtonName(pos, shorthand);
        }
        public InputState GetState(int slot)
        {
            if (slot < 0) return new InputState();
            float axis = GetAxis(slot);
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInput Copy()
        {
            return new GamepadButtonAxisInput(neg, pos, deadzone);
        }

        private float GetAxis(int slot)
        {
            float vNegative = GetValue(slot, neg);
            float vPositive = GetValue(slot, pos);
            return vPositive - vNegative;
        }
        private float GetValue(int slot, GamepadButton button)
        {
            if (slot < 0) return 0f;

            int id = (int)button;
            if (id >= 30 && id <= 33)
            {
                id -= 30;
                float value = GetGamepadAxisMovement(slot, id);
                if (MathF.Abs(value) < deadzone) return 0f;
                if (value > 0f) return value;
                else return 0f;
            }
            else if (id >= 40 && id <= 43)
            {
                id -= 40;
                float value = GetGamepadAxisMovement(slot, id);
                if (MathF.Abs(value) < deadzone) return 0f;
                if (value < 0) return MathF.Abs(value);
                else return 0f;
            }
            else return IsGamepadButtonDown(slot, id) ? 1f : 0f;
        }
        
        //public bool IsDown(int slot)
        //{
        //    return GetValue(slot, neg) > 0f || GetValue(slot, pos) > 0f;
        //}
        //public bool IsUp(int slot)
        //{
        //    return GetValue(slot, neg) <= 0f && GetValue(slot, pos) <= 0f;
        //}
    }
    
    internal class MouseWheelAxisInput : IInput
    {
        private MouseWheelAxis axis;
        public MouseWheelAxisInput(MouseWheelAxis axis) { this.axis = axis; }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true) { return IInput.GetMouseWheelAxisName(axis, shorthand); }
        public InputState GetState(int slot)
        {
            if (slot > 0) return new InputState();
            float axis = GetValue();
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInput Copy()
        {
            return new MouseWheelAxisInput(axis);
        }

        private float GetValue()
        {
            Vector2 value = GetMouseWheelMoveV();
            return axis == MouseWheelAxis.VERTICAL ? value.Y : value.X;
        }

        //public bool IsDown(int slot)
        //{
        //    return GetValue(slot) != 0f;
        //}
        //public bool IsUp(int slot)
        //{
        //    return GetValue(slot) == 0f;
        //}
        //private float GetAxis(int slot)
        //{
        //    return GetValue(slot);
        //}
    }
    internal class GamepadAxisInput : IInput
    {
        private GamepadAxis axis;
        private float deadzone;
        
        public GamepadAxisInput(GamepadAxis axis, float deadzone = 0.2f) { this.axis = axis; this.deadzone = deadzone; }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true) { return IInput.GetGamepadAxisName(axis, shorthand); }
        public InputState GetState(int slot)
        {
            if (slot < 0) return new InputState();
            float axis = GetValue(slot);
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInput Copy()
        {
            return new GamepadAxisInput(axis, deadzone);
        }
        
        private float GetValue(int slot)
        {
            float value = GetGamepadAxisMovement(slot, (int)axis);
            if (MathF.Abs(value) < deadzone) return 0f;
            else return value;
            
        }
    
        //public bool IsDown(int slot)
        //{
        //    return GetValue(slot) != 0f;
        //}
        //public bool IsUp(int slot)
        //{
        //    return GetValue(slot) == 0f;
        //}
        //public float GetAxis(int slot)
        //{
        //    return GetValue(slot);
        //}
    }

    public class InputCondition
    {
        public int ID { get; protected set; } = -1;
        private List<IInput> inputs = new();
        
        public InputConditionState State { get; protected set; } = new();

        public InputCondition(int id, params IInput[] inputs)
        {
            this.inputs = inputs.ToList();
            this.ID = id;
        }

        public InputCondition Copy()
        {
            IInput[] copy = new IInput[inputs.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = inputs[i].Copy();
            }
            return new(ID, copy);
        }
        public void Update(int slot, float dt)
        {
            bool down = false;
            bool up = true;
            float axis = 0f;
            foreach (var input in inputs)
            {
                var state = input.GetState(slot);
                axis += state.axis;
                down = down || state.down;
                up = up && state.up;
            }
            axis = Clamp(axis, -1f, 1f);

            State = InputConditionState.Generate(State, down, up, axis);
        }
        public string GetInputName(bool gamepad, bool shorthand = true)
        {
            foreach (var input in inputs)
            {
                if (input.IsGamepad() == gamepad) return input.GetName(shorthand);
            }
            return "";
        }
        public void Replace(params IInput[] newInputs)
        {
            State = new();
            inputs = newInputs.ToList();
        }
        public bool ChangeInput(int index, IInput newInput)
        {
            if (index >= 0 && index < inputs.Count)
            {
                if (!inputs.Contains(newInput))
                {
                    inputs[index] = newInput;
                    return true;
                }
            }
            return false;
        }
    }

    public class InputMap2
    {
        private Dictionary<int, InputCondition> conditions = new();
        public int Slot { get; protected set; } = -1;
        public int ID { get; protected set; } = -1;
        public bool Disabled { get; set; } = false;
        public InputMap2(int id, int slot, params InputCondition[] conditions)
        {
            foreach (var condition in conditions)
            {
                if (this.conditions.ContainsKey(condition.ID)) continue;
                this.conditions.Add(condition.ID, condition);
            }
            this.Slot = slot;
            this.ID = id;
        }

        public void Update(float dt)
        {
            if(Disabled) return;
            foreach (var condition in conditions.Values)
            {
                condition.Update(Slot, dt);
            }
        }
        public InputConditionState GetConditionState(int id)
        {
            if (!Disabled && conditions.ContainsKey(id))
            {
                return conditions[id].State;
            }
            return new();
        }
    }
    
    //IInputDevice interface?
    //input device class -> keyboard/mouse/gamepad
    //gamepad device supports vibration
    //last used input device = cur input device
    public class GamepadSlot
    {
        public int gamepadIndex = -1;
        public bool disabled = false;

        private List<GamepadVibration> gamepadVibrationStack = new();
        public GamepadSlot(int gamepadIndex)
        {
            this.gamepadIndex = gamepadIndex;
        }

        public void AddVibration(float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if (!InputHandler.IsGamepadConnected(gamepadIndex) || InputHandler.GAMEPAD_VIBRATION_STRENGTH <= 0f) return;
            gamepadVibrationStack.Add(new(name, duration, leftMotor, rightMotor));
        }
        public void RemoveVibration(string name)
        {
            if (name == "" || gamepadVibrationStack.Count <= 0) return;
            gamepadVibrationStack.RemoveAll(item => item.name == name);
        }
        public void StopVibration()
        {
            if (ShapeEngine.IsWindows()) XInput.SetVibration(gamepadIndex, 0f, 0f);

            gamepadVibrationStack.Clear();
        }
        public void UpdateVibration(float dt)
        {
            if (InputHandler.IsGamepadConnected(gamepadIndex))
            {
                float maxLeftMotor = 0f;
                float maxRightMotor = 0f;
                for (int i = gamepadVibrationStack.Count - 1; i >= 0; i--)
                {
                    var stack = gamepadVibrationStack[i];
                    if (stack.duration > 0)
                    {
                        if (stack.timer > 0)
                        {
                            stack.timer -= dt;
                            maxLeftMotor += stack.leftMotor;
                            maxRightMotor += stack.rightMotor;
                        }
                        else gamepadVibrationStack.RemoveAt(i);
                    }
                    else
                    {
                        maxLeftMotor += stack.leftMotor;
                        maxRightMotor += stack.rightMotor;
                    }
                }

                if (ShapeEngine.IsWindows())
                    XInput.SetVibration(gamepadIndex, Clamp(maxLeftMotor * InputHandler.GAMEPAD_VIBRATION_STRENGTH, 0f, 1f), Clamp(maxRightMotor * InputHandler.GAMEPAD_VIBRATION_STRENGTH, 0f, 1f));
            }
        }
    }
    public static class InputHandler2
    {
        public static readonly int MAX_GAMEPADS = 8;


        private static Dictionary<int, GamepadSlot> gamepadSlots = new();

        public static float GAMEPAD_VIBRATION_STRENGTH = 1.0f;
        private static List<int> connectedGamepads = new();
        public static int CUR_GAMEPAD
        {
            get { return gamepadSlots[0].gamepadIndex; }
            set { gamepadSlots[0].gamepadIndex = value; }
        }
        public static ref readonly List<int> GetConnectedGamepads() { return ref connectedGamepads; }
        public static int GetConnectedGamepadCount() { return connectedGamepads.Count; }



        private static InputType CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
        public static InputType GetCurInputType() { return CUR_INPUT_TYPE; }

        private static bool mouseUsed = false;
        private static bool keyboardUsed = false;
        private static bool keyboardOnlyMode = false;
        public static int gamepadUsed = -1;

        public static event Action<InputType>? OnInputChanged;
        public delegate void GamepadConnectionChanged(int gamepad, bool connected, int curGamepad);
        public static event GamepadConnectionChanged? OnGamepadConnectionChanged;


        private static bool inputDisabled = false;
        public static void DisableInput()
        {
            if (inputDisabled) return;
            inputDisabled = true;
            foreach (var slot in gamepadSlots.Keys)
            {
                StopVibration(slot);
            }
        }
        public static void EnableInput()
        {
            if (!inputDisabled) return;
            inputDisabled = false;
        }

        private static void OnInputTypeChanged(InputType newInputType)
        {
            //if (newInputType == InputType.KEYBOARD_MOUSE) { CursorHandler.Show(); }
            //else { CursorHandler.Hide(); }
            OnInputChanged?.Invoke(newInputType);
        }
        private static void OnControllerConnectionChanged(int gamepad, bool connected, int curGamepad)
        {
            if (!connected && gamepad == curGamepad) StopVibration(gamepad);
            OnGamepadConnectionChanged?.Invoke(gamepad, connected, curGamepad);
        }


        public static void Initialize()
        {
            CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
            for (int i = 0; i < MAX_GAMEPADS; i++)
            {
                if (IsGamepadAvailable(i)) AddGamepadSlot(i);
            }
            
        }
        public static void Update(float dt)
        {
            CheckGamepadConnection();
            CheckInputType();

            foreach (var inputSlot in gamepadSlots.Values)
            {
                inputSlot.UpdateVibration(dt);
            }
        }
        public static void Close()
        {
            gamepadSlots.Clear();
        }


        public static void AddGamepadSlot(int gamepadIndex)
        {
            if (!gamepadSlots.ContainsKey(gamepadIndex)) gamepadSlots.Add(gamepadIndex, new(gamepadIndex));
        }
        public static void RemoveInputSlot(int gamepadIndex)
        {
            gamepadSlots.Remove(gamepadIndex);
        }
        public static void AddVibration(int gamepadIndex, float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if (inputDisabled) return;
            if (!gamepadSlots.ContainsKey(gamepadIndex)) return;
            if (gamepadIndex == 0 && !IsGamepad()) return;

            gamepadSlots[gamepadIndex].AddVibration(leftMotor, rightMotor, duration, name);

        }
        public static void RemoveVibration(int gamepadIndex, string name)
        {
            if (!gamepadSlots.ContainsKey(gamepadIndex)) return;
            gamepadSlots[gamepadIndex].RemoveVibration(name);
        }
        public static void StopVibration(int gamepadIndex)
        {
            if (!gamepadSlots.ContainsKey(gamepadIndex)) return;
            gamepadSlots[gamepadIndex].StopVibration();
        }


        public static bool IsMouse() { return mouseUsed && !keyboardOnlyMode; }
        public static bool IsKeyboardOnlyMode() { return keyboardOnlyMode; }
        public static bool IsKeyboard() { return keyboardUsed; }
        public static bool IsKeyboardMouse() { return CUR_INPUT_TYPE == InputType.KEYBOARD_MOUSE; }
        public static bool IsGamepad() { return CUR_INPUT_TYPE == InputType.GAMEPAD; }
        public static bool IsTouch() { return CUR_INPUT_TYPE == InputType.TOUCH; }

        private static void CheckGamepadConnection()
        {
            for (int i = 0; i < 4; i++)
            {
                bool contains = connectedGamepads.Contains(i);
                if (IsGamepadAvailable(i))
                {
                    if (!contains)
                    {
                        connectedGamepads.Add(i);
                        OnControllerConnectionChanged(i, true, CUR_GAMEPAD);
                        if (CUR_INPUT_TYPE != InputType.GAMEPAD)
                        {
                            CUR_INPUT_TYPE = InputType.GAMEPAD;
                            OnInputTypeChanged(CUR_INPUT_TYPE);
                        }
                        if (gamepadSlots.Count == 1 && CUR_GAMEPAD < 0) CUR_GAMEPAD = i;
                    }
                }
                else
                {
                    if (contains)
                    {
                        connectedGamepads.Remove(i);
                        OnControllerConnectionChanged(i, false, CUR_GAMEPAD);

                        if (gamepadSlots.Count == 1)
                        {
                            if (connectedGamepads.Count <= 0)
                            {
                                CUR_GAMEPAD = -1;
                                if (CUR_INPUT_TYPE == InputType.GAMEPAD)
                                {
                                    CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                                    OnInputTypeChanged(CUR_INPUT_TYPE);
                                }
                            }
                            else//other gamepads remaining
                            {
                                CUR_GAMEPAD = connectedGamepads[0];
                                if (CUR_INPUT_TYPE == InputType.GAMEPAD)
                                {
                                    CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                                    OnInputTypeChanged(CUR_INPUT_TYPE);
                                }
                            }
                        }
                        else
                        {
                            if (CUR_GAMEPAD == i) CUR_GAMEPAD = -1;
                        }
                    }
                }
            }

            if (gamepadSlots.Count == 1)
            {
                gamepadUsed = GetGamepadUsed();
                if (gamepadUsed >= 0 && gamepadUsed != CUR_GAMEPAD)
                {
                    if (ShapeEngine.IsWindows()) XInput.SetVibration(CUR_GAMEPAD, 0f, 0f);
                    CUR_GAMEPAD = gamepadUsed;
                    if (CUR_INPUT_TYPE != InputType.GAMEPAD)
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                }
            }
        }
        private static void CheckInputType()
        {
            mouseUsed = false;
            keyboardUsed = false;
            //check if new input type was used and raise event
            switch (CUR_INPUT_TYPE)
            {
                case InputType.KEYBOARD_MOUSE:

                    if (WasAnyGamepadUsed())
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasTouchUsed())
                    {
                        CUR_INPUT_TYPE = InputType.TOUCH;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else
                    {
                        mouseUsed = WasMouseUsed();
                        keyboardUsed = WasKeyboardUsed();
                    }
                    break;

                case InputType.GAMEPAD:
                    mouseUsed = WasMouseUsed();
                    keyboardUsed = WasKeyboardUsed();
                    if (mouseUsed || keyboardUsed)
                    {
                        StopVibration(0);
                        CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasTouchUsed())
                    {
                        StopVibration(0);
                        CUR_INPUT_TYPE = InputType.TOUCH;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    break;

                case InputType.TOUCH:
                    mouseUsed = WasMouseUsed();
                    keyboardUsed = WasKeyboardUsed();
                    if (mouseUsed || keyboardUsed)
                    {
                        CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasAnyGamepadUsed())
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    break;

                default:
                    break;
            }

        }

        private static bool WasKeyboardUsed()
        {
            return GetKeyPressed() > 0;
        }
        private static bool WasMouseUsed()
        {
            bool mouseMovement = Vector2LengthSqr(GetMouseMovement()) > 0.0f;
            if (mouseMovement) return true;

            bool wheelMovement = MathF.Abs(GetMouseWheelMovement()) > 0;
            if (wheelMovement) return true;

            bool mouseButton =
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_SIDE) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_EXTRA) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_FORWARD) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_BACK);

            return mouseButton;
        }

        private static int GetGamepadUsed()
        {
            if (connectedGamepads.Count <= 0) return -1;
            foreach (int gamepad in connectedGamepads)
            {
                //if (gamepad == CUR_GAMEPAD) continue;
                if (CheckGamepadUsed(gamepad)) return gamepad;
            }
            return -1; // CUR_GAMEPAD;
        }
        private static bool CheckGamepadUsed(int gamepad)
        {
            if (!IsGamepadConnected(gamepad)) return false;

            if (GetGamepadButton(gamepad)) return true;

            bool axis =
                Vector2LengthSqr(GetGamepadAxisLeft(gamepad, 0.5f, false)) > 0.0f ||
                Vector2LengthSqr(GetGamepadAxisRight(gamepad, 0.5f, false)) > 0.0f ||
                GetGamepadLeftTrigger(gamepad, 0.5f, false) > 0.0f ||
                GetGamepadRightTrigger(gamepad, 0.5f, false) > 0.0f;
            return axis;
        }
        private static bool WasAnyGamepadUsed()
        {
            return gamepadUsed >= 0;
        }
        private static bool WasTouchUsed()
        {
            return GetTouchPointCount() > 0;
        }
        public static bool IsGamepadConnected(int gamepadIndex)
        {
            return connectedGamepads.Contains(gamepadIndex);
        }
        public static bool IsCurGamepadConnected()
        {
            return connectedGamepads.Contains(CUR_GAMEPAD);
        }

        public static bool GetGamepadButton(int gamepad)
        {
            foreach (var button in Enum.GetValues(typeof(GamepadButton)).Cast<int>())
            {
                if (IsGamepadButtonDown(gamepad, button)) return true;
            }
            return false;
        }
        public static Vector2 GetGamepadAxisLeft(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (inputDisabled) return new(0f);
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X),
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static Vector2 GetGamepadAxisRight(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (inputDisabled) return new(0f);
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X),
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static float GetGamepadLeftTrigger(int gamepad, float deadzone = 0.25f, bool inverted = false)
        {
            if (inputDisabled) return inverted ? 1f : 0f;
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1.0f) * 0.5f;
            if (MathF.Abs(axis) < deadzone)
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }

            if (inverted) return 1.0f - axis;
            return axis;
        }
        public static float GetGamepadRightTrigger(int gamepad, float deadzone = 0.25f, bool inverted = false)
        {
            if (inputDisabled) return inverted ? 1f : 0f;
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1.0f) * 0.5f;
            if (MathF.Abs(axis) < deadzone)
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }

            if (inverted) return 1.0f - axis;
            return axis;
        }


        public static Vector2 GetMouseMovement(float deadzone = 5.0f, bool normalized = false)
        {
            var movement = GetMouseDelta();
            if (Vector2LengthSqr(movement) < deadzone * deadzone) return new(0.0f, 0.0f);
            if (normalized) return Vector2Normalize(movement);
            return movement;
        }
        public static float GetMouseWheelMovement(bool inverted = false)
        {
            float movement = GetMouseWheelMove();

            if (inverted) return -movement;
            return movement;
        }
        public static Vector2 GetMouseWheelMovementV(bool inverted = false)
        {
            Vector2 movement = GetMouseWheelMoveV();

            if (inverted) return -movement;
            return movement;
        }


    }


    public class InputMap
    {
        private int id = -1;
        public string DisplayName { get; private set; }
        //private int gamepad = -1;
        private Dictionary<int, InputAction> inputActions = new();
        //private bool disabled = false;

        public InputMap(int id, string displayName, params InputAction[] actions)
        {
            this.id = id;
            this.DisplayName = displayName;
            foreach (var action in actions)
            {
                AddAction(action.GetID(), action);
            }
        }

        //public void Update(float dt, int gamepad, bool gamepadOnly)
        //{
        //    foreach (var inputAction in inputActions.Values)
        //    {
        //        inputAction.Update(dt, gamepad, gamepadOnly);
        //    }
        //}
        //public int GamepadIndex { get { return gamepad; } }
        //public bool HasGamepad { get { return gamepad >= 0; } }
        //public void Rename(string newName) { id = newName; }
        public int GetID() { return id; }
        public void AddActions(List<InputAction> actions)
        {
            foreach (var inputAction in actions)
            {
                AddAction(inputAction);
            }
        }
        public void AddAction(InputAction action)
        {
            int id = action.GetID();
            if (inputActions.ContainsKey(id))
            {
                inputActions[id] = action;
            }
            else
            {
                inputActions.Add(id, action);
            }
        }
        public void AddAction(int id, InputAction action)
        {
            if (inputActions.ContainsKey(id))
            {
                inputActions[id] = action;
            }
            else
            {
                inputActions.Add(id, action);
            }
        }
        public void AddAction(int id, params InputAction.Keys[] keys)
        {
            AddAction(id, new InputAction(id, keys));
        }
        public void RemoveAction(int id)
        {
            inputActions.Remove(id);
        }
        public InputAction? GetAction(int id)
        {
            if (!inputActions.ContainsKey(id)) return null;
            return inputActions[id];
        }
        public List<string> GetKeyNames(int id)
        {
            if (!inputActions.ContainsKey(id)) return new();
            return inputActions[id].GetAllKeyNames();
        }
        
        //public bool IsDisabled() { return disabled; }
        //public void Enable() { disabled = false; }
        //public void Disable() { disabled = true; }

        public float GetAxis(int gamepad, int idNegative, int idPositive)
        {
            if (!inputActions.ContainsKey(idNegative) || !inputActions.ContainsKey(idPositive)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            float p = inputActions[idPositive].IsDown(gamepad) ? 1f : 0f;
            float n = inputActions[idNegative].IsDown(gamepad) ? 1f : 0f;
            return p - n;
        }
        public Vector2 GetAxis(int gamepad, int idLeft, int idRight, int idUp, int idDown)
        {
            return new(GetAxis(gamepad, idLeft, idRight), GetAxis(gamepad, idUp, idDown));
        }
        public float GetGamepadAxis(int gamepad, int id)
        {
            if (!inputActions.ContainsKey(id)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[id].GetGamepadAxis(gamepad);
        }
        public Vector2 GetGamepadAxis(int gamepad,int gamepadAxisHorID, int gamepadAxisVerID)
        {
            return new(GetGamepadAxis(gamepad, gamepadAxisHorID), GetGamepadAxis(gamepad, gamepadAxisVerID));
        }

        //public float GetHoldF(int gamepad, string actionName, bool gamepadOnly = false)
        //{
        //    if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return -1f;
        //    return inputActions[actionName].GetHoldF();
        //}
        public bool IsDown(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsDown(gamepad, gamepadOnly);
        }
        public bool IsPressed(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsPressed(gamepad, gamepadOnly);
        }
        public bool IsReleased(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsReleased(gamepad, gamepadOnly);
        }
        public bool IsUp(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsUp(gamepad, gamepadOnly);
        }

        public (string keyboard, string mouse, string gamepad) GetKeyNames(int actionID, bool shorthand = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return ("", "", "");
            return inputActions[actionID].GetKeyNames(shorthand);
        }
        //public List<string> GetKeyNames(string actionName, bool shorthand = true)
        //{
        //    if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return new();
        //    return inputActions[actionName].GetKeyNames(shorthand);
        //}
    }

    /*
    public abstract class InputValue
    {
        public bool IsAxis { get; private set; } = false;

        private Buttons button = Buttons.NONE;
        private Axis axis = Axis.NONE;
        private (Buttons neg, Buttons pos) axisButtons = (Buttons.NONE, Buttons.NONE);


        public InputValue(Buttons button)
        {
            this.button = button;
        }
        
        public InputValue(Axis axis)
        {
            this.axis = axis;
            this.IsAxis = true;
        }  
        public InputValue((Buttons neg, Buttons pos) axis)
        {
            this.axisButtons = axis;
            this.IsAxis = true;
        }

        public (string keyboard, string mouse, string gamepad) GetNames(bool shorthand = true)
        {
            if (IsAxis)
            {
                if (axis != Axis.NONE)
                {
                    if (IsGamepadAxis(axis)) return ("", "", GetAxisName(axis, shorthand));
                    else return ("", GetAxisName(axis, shorthand), "");
                }
                else
                {
                    if (axisButtons.neg != Buttons.NONE && axisButtons.pos != Buttons.NONE)
                    {
                        string name = GetButtonName(axisButtons.neg, shorthand) + "/" + GetButtonName(axisButtons.pos, shorthand);
                        if (IsKeyboard(axisButtons.neg)) return (name, "", "");
                        else if (IsMouse(axisButtons.neg)) return ("", name, "");
                        else return ("", "", name);
                    }
                    else return ("", "", "");
                }
            }
            else
            {
                if (IsKeyboard(button)) return (GetButtonName(button, shorthand), "", "");
                else if(IsMouse(button)) return ("", GetButtonName(button, shorthand), "");
                else return ("", "", GetButtonName(button, shorthand));
            }
        }

        public void Change(Buttons newButton)
        {
            button = newButton;
            if (IsAxis)
            {
                IsAxis = false;
                axis = Axis.NONE;
                axisButtons = (Buttons.NONE, Buttons.NONE);
            }
        }
        public void Change(Axis newAxis)
        {
            axis = newAxis;
            axisButtons = (Buttons.NONE, Buttons.NONE);
            if (!IsAxis)
            {
                IsAxis = true;
                button = Buttons.NONE;
            }
        }
        public void Change((Buttons neg, Buttons pos) newAxis)
        {
            axisButtons = newAxis;
            axis = Axis.NONE;
            if (!IsAxis)
            {
                IsAxis = true;
                button = Buttons.NONE;
            }
        }



        public bool IsDown(int slot)
        {
            return false;
        }
        public bool IsPressed(int slot)
        {
            return false;
        }
        public bool IsReleased(int slot)
        {
            return false;
        }
        public bool IsUp(int slot)
        {
            return false;
        }
        public float GetAxis(int slot, float deadzone)
        {
            return 0f;
        }


        private static int TransformButtonValue(Buttons key)
        {
            if (key == Buttons.BACK)
            {
                return (int)KeyboardKey.KEY_BACK;
            }
            else if (key == Buttons.NULL)
            {
                return (int)KeyboardKey.KEY_NULL;
            }
            else if (key == Buttons.MENU)
            {
                return (int)KeyboardKey.KEY_MENU;
            }
            //else if (IsGamepadAxis(key)) return (int)key - 400;
            else if (IsGamepadButton(key)) return (int)key - 500;
            else if (IsGamepadAxisButtonPos(key)) return (int)key - 600;
            else if (IsGamepadAxisButtonNeg(key)) return (int)key - 610;
            return (int)key;
        }
        private static int TransformAxisValue(Axis axis)
        {
            if (IsGamepadAxis(axis)) return (int)axis - 400;
            else return (int)axis;
        }
        
        public static string GetButtonName(Buttons key, bool shortHand = true)
        {
            switch (key)
            {
                case Buttons.MB_LEFT: return shortHand ? "LMB" : "Left Mouse Button";
                case Buttons.MB_RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
                case Buttons.MB_MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
                case Buttons.MB_SIDE: return shortHand ? "SMB" : "Side Mouse Button";
                case Buttons.MB_EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
                case Buttons.MB_FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
                case Buttons.MB_BACK: return shortHand ? "BMB" : "Back Mouse Button";
                case Buttons.MW_UP: return shortHand ? "MWU" : "Mouse Wheel Up";
                case Buttons.MW_DOWN: return shortHand ? "MWD" : "Mouse Wheel Down";
                case Buttons.MW_LEFT: return shortHand ? "MWL" : "Mouse Wheel Left";
                case Buttons.MW_RIGHT: return shortHand ? "MWR" : "Mouse Wheel Right";
                //case Buttons.MW_AXIS_HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                //case Buttons.MW_AXIS_VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                case Buttons.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
                case Buttons.COMMA: return shortHand ? "," : "Comma";
                case Buttons.MINUS: return shortHand ? "-" : "Minus";
                case Buttons.PERIOD: return shortHand ? "." : "Period";
                case Buttons.SLASH: return shortHand ? "/" : "Slash";
                case Buttons.ZERO: return shortHand ? "0" : "Zero";
                case Buttons.ONE: return shortHand ? "1" : "One";
                case Buttons.TWO: return shortHand ? "2" : "Two";
                case Buttons.THREE: return shortHand ? "3" : "Three";
                case Buttons.FOUR: return shortHand ? "4" : "Four";
                case Buttons.FIVE: return shortHand ? "5" : "Five";
                case Buttons.SIX: return shortHand ? "6" : "Six";
                case Buttons.SEVEN: return shortHand ? "7" : "Seven";
                case Buttons.EIGHT: return shortHand ? "8" : "Eight";
                case Buttons.NINE: return shortHand ? "9" : "Nine";
                case Buttons.SEMICOLON: return shortHand ? ";" : "Semi Colon";
                case Buttons.EQUAL: return shortHand ? "=" : "Equal";
                case Buttons.A: return shortHand ? "A" : "A";
                case Buttons.B: return shortHand ? "B" : "B";
                case Buttons.C: return shortHand ? "C" : "C";
                case Buttons.D: return shortHand ? "D" : "D";
                case Buttons.E: return shortHand ? "E" : "E";
                case Buttons.F: return shortHand ? "F" : "F";
                case Buttons.G: return shortHand ? "G" : "G";
                case Buttons.H: return shortHand ? "H" : "H";
                case Buttons.I: return shortHand ? "I" : "I";
                case Buttons.J: return shortHand ? "J" : "J";
                case Buttons.K: return shortHand ? "K" : "K";
                case Buttons.L: return shortHand ? "L" : "L";
                case Buttons.M: return shortHand ? "M" : "M";
                case Buttons.N: return shortHand ? "N" : "N";
                case Buttons.O: return shortHand ? "O" : "O";
                case Buttons.P: return shortHand ? "P" : "P";
                case Buttons.Q: return shortHand ? "Q" : "Q";
                case Buttons.R: return shortHand ? "R" : "R";
                case Buttons.S: return shortHand ? "S" : "S";
                case Buttons.T: return shortHand ? "T" : "T";
                case Buttons.U: return shortHand ? "U" : "U";
                case Buttons.V: return shortHand ? "V" : "V";
                case Buttons.W: return shortHand ? "W" : "W";
                case Buttons.X: return shortHand ? "X" : "X";
                case Buttons.Y: return shortHand ? "Y" : "Y";
                case Buttons.Z: return shortHand ? "Z" : "Z";
                case Buttons.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case Buttons.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case Buttons.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case Buttons.GRAVE: return shortHand ? "`" : "Grave";//Check
                case Buttons.SPACE: return shortHand ? "Space" : "Space";
                case Buttons.ESCAPE: return shortHand ? "Esc" : "Escape";
                case Buttons.ENTER: return shortHand ? "Enter" : "Enter";
                case Buttons.TAB: return shortHand ? "Tab" : "Tab";
                case Buttons.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case Buttons.INSERT: return shortHand ? "Ins" : "Insert";
                case Buttons.DELETE: return shortHand ? "Del" : "Delete";
                case Buttons.RIGHT: return shortHand ? "Right" : "Right";
                case Buttons.LEFT: return shortHand ? "Left" : "Left";
                case Buttons.DOWN: return shortHand ? "Down" : "Down";
                case Buttons.UP: return shortHand ? "Up" : "Up";
                case Buttons.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case Buttons.PAGE_DOWN: return shortHand ? "PDo" : "";
                case Buttons.HOME: return shortHand ? "Home" : "Home";
                case Buttons.END: return shortHand ? "End" : "End";
                case Buttons.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case Buttons.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case Buttons.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case Buttons.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case Buttons.PAUSE: return shortHand ? "Pause" : "Pause";
                case Buttons.F1: return shortHand ? "F1" : "F1";
                case Buttons.F2: return shortHand ? "F2" : "F2";
                case Buttons.F3: return shortHand ? "F3" : "F3";
                case Buttons.F4: return shortHand ? "F4" : "F4";
                case Buttons.F5: return shortHand ? "F5" : "F5";
                case Buttons.F6: return shortHand ? "F6" : "F6";
                case Buttons.F7: return shortHand ? "F7" : "F7";
                case Buttons.F8: return shortHand ? "F8" : "F8";
                case Buttons.F9: return shortHand ? "F9" : "F9";
                case Buttons.F10: return shortHand ? "F10" : "F10";
                case Buttons.F11: return shortHand ? "F11" : "F11";
                case Buttons.F12: return shortHand ? "F12" : "F12";
                case Buttons.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
                case Buttons.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
                case Buttons.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
                case Buttons.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
                case Buttons.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
                case Buttons.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
                case Buttons.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
                case Buttons.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
                case Buttons.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
                case Buttons.KP_0: return shortHand ? "KP0" : "Keypad 0";
                case Buttons.KP_1: return shortHand ? "KP1" : "Keypad 1";
                case Buttons.KP_2: return shortHand ? "KP2" : "Keypad 2";
                case Buttons.KP_3: return shortHand ? "KP3" : "Keypad 3";
                case Buttons.KP_4: return shortHand ? "KP4" : "Keypad 4";
                case Buttons.KP_5: return shortHand ? "KP5" : "Keypad 5";
                case Buttons.KP_6: return shortHand ? "KP6" : "Keypad 6";
                case Buttons.KP_7: return shortHand ? "KP7" : "Keypad 7";
                case Buttons.KP_8: return shortHand ? "KP8" : "Keypad 8";
                case Buttons.KP_9: return shortHand ? "KP9" : "Keypad 9";
                case Buttons.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
                case Buttons.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
                case Buttons.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case Buttons.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case Buttons.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case Buttons.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case Buttons.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case Buttons.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case Buttons.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case Buttons.BACK: return shortHand ? "Back" : "Back";
                case Buttons.NULL: return shortHand ? "Null" : "Null";
                case Buttons.MENU: return shortHand ? "Menu" : "Menu";
                //case Buttons.GP_AXIS_LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                //case Buttons.GP_AXIS_LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                //case Buttons.GP_AXIS_RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                //case Buttons.GP_AXIS_RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                //case Buttons.GP_AXIS_RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                //case Buttons.GP_AXIS_LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                case Buttons.GP_BUTTON_UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
                case Buttons.GP_BUTTON_LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
                case Buttons.GP_BUTTON_LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
                case Buttons.GP_BUTTON_LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
                case Buttons.GP_BUTTON_LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
                case Buttons.GP_BUTTON_RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
                case Buttons.GP_BUTTON_RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
                case Buttons.GP_BUTTON_RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
                case Buttons.GP_BUTTON_RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
                case Buttons.GP_BUTTON_LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
                case Buttons.GP_BUTTON_LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
                case Buttons.GP_BUTTON_RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
                case Buttons.GP_BUTTON_RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
                case Buttons.GP_BUTTON_MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
                case Buttons.GP_BUTTON_MIDDLE: return shortHand ? "Home" : "GP Button Home";
                case Buttons.GP_BUTTON_MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
                case Buttons.GP_BUTTON_LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
                case Buttons.GP_BUTTON_RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        public static string GetAxisName(Axis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case Axis.MW_AXIS_HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                case Axis.MW_AXIS_VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                case Axis.GP_AXIS_LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                case Axis.GP_AXIS_LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                case Axis.GP_AXIS_RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                case Axis.GP_AXIS_RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                case Axis.GP_AXIS_RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                case Axis.GP_AXIS_LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                default: return shortHand ? "No Key" : "No Key";
            }
        }

        public static bool IsGamepad(Buttons key) { return (int)key >= 500 && (int)key <= 523; }
        public static bool IsMouse(Buttons key) { return ((int)key >= 0 && (int)key <= 6) || IsMouseWheel(key); }
        public static bool IsMouseWheel(Buttons key) { return (int)key >= 20 && (int)key <= 23; }
        public static bool IsKeyboard(Buttons key) { return (int)key > 6 && (int)key < 400 && !IsMouseWheel(key); }
        public static bool IsGamepadButton(Buttons key) { return (int)key >= 500 && (int)key <= 517; }
        public static bool IsGamepadAxisButtonPos(Buttons key) { return (int)key >= 600 && (int)key <= 603; }
        public static bool IsGamepadAxisButtonNeg(Buttons key) { return (int)key >= 610 && (int)key <= 613; }
        public static bool IsGamepadAxisButton(Buttons key) { return (int)key >= 600 && (int)key <= 613; }
        public static bool IsGamepadAxis(Axis axis) { return (int)axis >= 400 && (int)axis <= 405; }
        public static bool IsMouseWheelAxis(Axis axis) { return (int)axis == 10 || (int)axis == 11; }
        




        //public static bool IsButton(Buttons key) { return IsGamepadButton(key) || IsGamepadAxisButton(key) || IsGamepadAxisButtonNeg(key) || IsGamepadAxisButtonPos(key) || IsKeyboard(key) || IsMouse(key) || IsMouseWheelButton(key); }
        //public static bool IsAxis(Buttons key) { return IsGamepadAxis(key) || IsMouseWheelAxis(key); }
        //public static bool IsMouseWheelButton(Buttons key) { return (int)key >= 20 && (int)key <= 23; }
        //public static bool IsMouseWheelAxis(Buttons key) { return (int)key == 29 || (int)key == 30; }

    }
    */
}
