using Raylib_CsLo;
using System.Numerics;
using Vortice.XInput;
using static ShapeInput.InputAction;

namespace ShapeInput
{





    public enum Keys
    {
        MB_LEFT = 0,
        MB_RIGHT = 1,
        MB_MIDDLE = 2,
        MB_SIDE = 3,
        MB_EXTRA = 4,
        MB_FORWARD = 5,
        MB_BACK = 6,
        MW_UP = 20,
        MW_DOWN = 21,
        MW_LEFT = 22,
        MW_RIGHT = 23,
        MW_AXIS_VERTICAL = 29,
        MW_AXIS_HORIZONTAL = 30,
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
        GP_AXIS_LEFT_X = 400,
        GP_AXIS_LEFT_Y = 401,
        GP_AXIS_RIGHT_X = 402,
        GP_AXIS_RIGHT_Y = 403,
        GP_AXIS_LEFT_TRIGGER = 404,
        GP_AXIS_RIGHT_TRIGGER = 405,
        GP_BUTTON_UNKNOWN = 500,
        GP_BUTTON_LEFT_FACE_UP = 501,
        GP_BUTTON_LEFT_FACE_RIGHT = 502,
        GP_BUTTON_LEFT_FACE_DOWN = 503,
        GP_BUTTON_LEFT_FACE_LEFT = 504,
        GP_BUTTON_RIGHT_FACE_UP = 505,
        GP_BUTTON_RIGHT_FACE_RIGHT = 506,
        GP_BUTTON_RIGHT_FACE_DOWN = 507,
        GP_BUTTON_RIGHT_FACE_LEFT = 508,
        GP_BUTTON_LEFT_TRIGGER_TOP = 509,
        GP_BUTTON_LEFT_TRIGGER_BOTTOM = 510,
        GP_BUTTON_RIGHT_TRIGGER_TOP = 511,
        GP_BUTTON_RIGHT_TRIGGER_BOTTOM = 512,
        GP_BUTTON_MIDDLE_LEFT = 513,
        GP_BUTTON_MIDDLE = 514,
        GP_BUTTON_MIDDLE_RIGHT = 515,
        GP_BUTTON_LEFT_THUMB = 516,
        GP_BUTTON_RIGHT_THUMB = 517,

        GP_BUTTON_LSTICK_RIGHT = 600,
        GP_BUTTON_LSTICK_LEFT = 610,
        GP_BUTTON_LSTICK_DOWN = 601,
        GP_BUTTON_LSTICK_UP = 611,

        GP_BUTTON_RSTICK_RIGHT = 602,
        GP_BUTTON_RSTICK_LEFT = 612,
        GP_BUTTON_RSTICK_DOWN = 603,
        GP_BUTTON_RSTICK_UP = 613,
    }
    public static class InputKeys
    {
        public static bool IsButton(Keys key) { return IsGamepadButton(key) || IsGamepadAxisButton(key) || IsGamepadAxisButtonNeg(key) || IsGamepadAxisButtonPos(key) || IsKeyboard(key) || IsMouse(key) || IsMouseWheelButton(key); }
        public static bool IsAxis(Keys key) { return IsGamepadAxis(key) || IsMouseWheelAxis(key); }
        public static bool IsGamepad(Keys key) { return (int)key >= 500 && (int)key <= 523; }
        public static bool IsGamepadAxis(Keys key) { return (int)key >= 400 && (int)key <= 405; }
        public static bool IsGamepadButton(Keys key) { return (int)key >= 500 && (int)key <= 517; }
        public static bool IsGamepadAxisButtonPos(Keys key) { return (int)key >= 600 && (int)key <= 603; }
        public static bool IsGamepadAxisButtonNeg(Keys key) { return (int)key >= 610 && (int)key <= 613; }
        public static bool IsGamepadAxisButton(Keys key) { return (int)key >= 600 && (int)key <= 613; }
        public static bool IsMouse(Keys key) { return ( (int)key >= 0 && (int)key <= 6 ) || IsMouseWheel(key); }
        public static bool IsMouseWheel(Keys key) { return IsMouseWheelAxis(key) || IsMouseWheelButton(key); }
        public static bool IsMouseWheelButton(Keys key) { return (int)key >= 20 && (int)key <= 23; }
        public static bool IsMouseWheelAxis(Keys key) { return (int)key == 29 || (int)key == 30; }
        public static bool IsKeyboard(Keys key) { return (int)key > 6 && (int)key < 400 && !IsMouseWheel(key); }
        public static int TransformKeyValue(Keys key)
        {
            if (key == Keys.BACK)
            {
                return (int)KeyboardKey.KEY_BACK;
            }
            else if (key == Keys.NULL)
            {
                return (int)KeyboardKey.KEY_NULL;
            }
            else if (key == Keys.MENU)
            {
                return (int)KeyboardKey.KEY_MENU;
            }
            else if (IsGamepadAxis(key)) return (int)key - 400;
            else if (IsGamepadButton(key)) return (int)key - 500;
            else if (IsGamepadAxisButtonPos(key)) return (int)key - 600;
            else if (IsGamepadAxisButtonNeg(key)) return (int)key - 610;
            return (int)key;
        }
        public static string GetKeyName(Keys key, bool shortHand = true)
        {
            switch (key)
            {
                case Keys.MB_LEFT: return shortHand ? "LMB" : "Left Mouse Button";
                case Keys.MB_RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
                case Keys.MB_MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
                case Keys.MB_SIDE: return shortHand ? "SMB" : "Side Mouse Button";
                case Keys.MB_EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
                case Keys.MB_FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
                case Keys.MB_BACK: return shortHand ? "BMB" : "Back Mouse Button";
                case Keys.MW_UP: return shortHand ? "MWU" : "Mouse Wheel Up";
                case Keys.MW_DOWN: return shortHand ? "MWD" : "Mouse Wheel Down";
                case Keys.MW_LEFT: return shortHand ? "MWL" : "Mouse Wheel Left";
                case Keys.MW_RIGHT: return shortHand ? "MWR" : "Mouse Wheel Right";
                case Keys.MW_AXIS_HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                case Keys.MW_AXIS_VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                case Keys.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
                case Keys.COMMA: return shortHand ? "," : "Comma";
                case Keys.MINUS: return shortHand ? "-" : "Minus";
                case Keys.PERIOD: return shortHand ? "." : "Period";
                case Keys.SLASH: return shortHand ? "/" : "Slash";
                case Keys.ZERO: return shortHand ? "0" : "Zero";
                case Keys.ONE: return shortHand ? "1" : "One";
                case Keys.TWO: return shortHand ? "2" : "Two";
                case Keys.THREE: return shortHand ? "3" : "Three";
                case Keys.FOUR: return shortHand ? "4" : "Four";
                case Keys.FIVE: return shortHand ? "5" : "Five";
                case Keys.SIX: return shortHand ? "6" : "Six";
                case Keys.SEVEN: return shortHand ? "7" : "Seven";
                case Keys.EIGHT: return shortHand ? "8" : "Eight";
                case Keys.NINE: return shortHand ? "9" : "Nine";
                case Keys.SEMICOLON: return shortHand ? ";" : "Semi Colon";
                case Keys.EQUAL: return shortHand ? "=" : "Equal";
                case Keys.A: return shortHand ? "A" : "A";
                case Keys.B: return shortHand ? "B" : "B";
                case Keys.C: return shortHand ? "C" : "C";
                case Keys.D: return shortHand ? "D" : "D";
                case Keys.E: return shortHand ? "E" : "E";
                case Keys.F: return shortHand ? "F" : "F";
                case Keys.G: return shortHand ? "G" : "G";
                case Keys.H: return shortHand ? "H" : "H";
                case Keys.I: return shortHand ? "I" : "I";
                case Keys.J: return shortHand ? "J" : "J";
                case Keys.K: return shortHand ? "K" : "K";
                case Keys.L: return shortHand ? "L" : "L";
                case Keys.M: return shortHand ? "M" : "M";
                case Keys.N: return shortHand ? "N" : "N";
                case Keys.O: return shortHand ? "O" : "O";
                case Keys.P: return shortHand ? "P" : "P";
                case Keys.Q: return shortHand ? "Q" : "Q";
                case Keys.R: return shortHand ? "R" : "R";
                case Keys.S: return shortHand ? "S" : "S";
                case Keys.T: return shortHand ? "T" : "T";
                case Keys.U: return shortHand ? "U" : "U";
                case Keys.V: return shortHand ? "V" : "V";
                case Keys.W: return shortHand ? "W" : "W";
                case Keys.X: return shortHand ? "X" : "X";
                case Keys.Y: return shortHand ? "Y" : "Y";
                case Keys.Z: return shortHand ? "Z" : "Z";
                case Keys.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case Keys.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case Keys.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case Keys.GRAVE: return shortHand ? "`" : "Grave";//Check
                case Keys.SPACE: return shortHand ? "Space" : "Space";
                case Keys.ESCAPE: return shortHand ? "Esc" : "Escape";
                case Keys.ENTER: return shortHand ? "Enter" : "Enter";
                case Keys.TAB: return shortHand ? "Tab" : "Tab";
                case Keys.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case Keys.INSERT: return shortHand ? "Ins" : "Insert";
                case Keys.DELETE: return shortHand ? "Del" : "Delete";
                case Keys.RIGHT: return shortHand ? "Right" : "Right";
                case Keys.LEFT: return shortHand ? "Left" : "Left";
                case Keys.DOWN: return shortHand ? "Down" : "Down";
                case Keys.UP: return shortHand ? "Up" : "Up";
                case Keys.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case Keys.PAGE_DOWN: return shortHand ? "PDo" : "";
                case Keys.HOME: return shortHand ? "Home" : "Home";
                case Keys.END: return shortHand ? "End" : "End";
                case Keys.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case Keys.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case Keys.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case Keys.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case Keys.PAUSE: return shortHand ? "Pause" : "Pause";
                case Keys.F1: return shortHand ? "F1" : "F1";
                case Keys.F2: return shortHand ? "F2" : "F2";
                case Keys.F3: return shortHand ? "F3" : "F3";
                case Keys.F4: return shortHand ? "F4" : "F4";
                case Keys.F5: return shortHand ? "F5" : "F5";
                case Keys.F6: return shortHand ? "F6" : "F6";
                case Keys.F7: return shortHand ? "F7" : "F7";
                case Keys.F8: return shortHand ? "F8" : "F8";
                case Keys.F9: return shortHand ? "F9" : "F9";
                case Keys.F10: return shortHand ? "F10" : "F10";
                case Keys.F11: return shortHand ? "F11" : "F11";
                case Keys.F12: return shortHand ? "F12" : "F12";
                case Keys.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
                case Keys.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
                case Keys.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
                case Keys.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
                case Keys.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
                case Keys.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
                case Keys.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
                case Keys.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
                case Keys.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
                case Keys.KP_0: return shortHand ? "KP0" : "Keypad 0";
                case Keys.KP_1: return shortHand ? "KP1" : "Keypad 1";
                case Keys.KP_2: return shortHand ? "KP2" : "Keypad 2";
                case Keys.KP_3: return shortHand ? "KP3" : "Keypad 3";
                case Keys.KP_4: return shortHand ? "KP4" : "Keypad 4";
                case Keys.KP_5: return shortHand ? "KP5" : "Keypad 5";
                case Keys.KP_6: return shortHand ? "KP6" : "Keypad 6";
                case Keys.KP_7: return shortHand ? "KP7" : "Keypad 7";
                case Keys.KP_8: return shortHand ? "KP8" : "Keypad 8";
                case Keys.KP_9: return shortHand ? "KP9" : "Keypad 9";
                case Keys.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
                case Keys.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
                case Keys.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case Keys.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case Keys.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case Keys.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case Keys.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case Keys.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case Keys.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case Keys.BACK: return shortHand ? "Back" : "Back";
                case Keys.NULL: return shortHand ? "Null" : "Null";
                case Keys.MENU: return shortHand ? "Menu" : "Menu";
                case Keys.GP_AXIS_LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                case Keys.GP_AXIS_LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                case Keys.GP_AXIS_RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                case Keys.GP_AXIS_RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                case Keys.GP_AXIS_RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                case Keys.GP_AXIS_LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                case Keys.GP_BUTTON_UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
                case Keys.GP_BUTTON_LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
                case Keys.GP_BUTTON_LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
                case Keys.GP_BUTTON_LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
                case Keys.GP_BUTTON_LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
                case Keys.GP_BUTTON_RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
                case Keys.GP_BUTTON_RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
                case Keys.GP_BUTTON_RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
                case Keys.GP_BUTTON_RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
                case Keys.GP_BUTTON_LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
                case Keys.GP_BUTTON_LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
                case Keys.GP_BUTTON_RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
                case Keys.GP_BUTTON_RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
                case Keys.GP_BUTTON_MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
                case Keys.GP_BUTTON_MIDDLE: return shortHand ? "Home" : "GP Button Home";
                case Keys.GP_BUTTON_MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
                case Keys.GP_BUTTON_LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
                case Keys.GP_BUTTON_RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
                default: return shortHand ? "No Key" : "No Key";
            }
        }

    }
    public class InputCondition
    {
        public int ID { get; protected set; } = -1;
        private List<Keys> keys = new();
        public InputCondition(int id, params Keys[] keys)
        {
            this.keys = keys.ToList();
            this.ID = id;
        }

        public void ReplaceKeys(params Keys[] newKeys)
        {
            keys = newKeys.ToList();
        }
        public bool ChangeKey(Keys newKey, int index)
        {
            if(index >= 0 && index < keys.Count)
            {
                if (!keys.Contains(newKey))
                {
                    keys[index] = newKey;
                    return true;
                }
            }
            return false;
        }
        
        public (string keyboard, string mouse, string gamepad) GetKeyNames(bool shorthand = true)
        {
            return (GetKeyboardKeyName(shorthand), GetMouseKeyName(shorthand), GetGamepadKeyName(shorthand));
        }
        
        public bool IsDown(int slot)
        {
            if (keys.Count <= 0) return false;
            foreach (var key in keys)
            {
                if (InputKeys.IsMouse(key))
                {
                    if (slot <= 0 && IsMouseButtonDown(InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsMouseWheelButton(key))//!!!!!!!!!!!
                {
                    if (slot <= 0) return IsMouseWheelDown(key);
                }
                else if (InputKeys.IsMouseWheelAxis(key))//!!!!!!!!!!!!
                {
                    if (slot <= 0 && GetMouseWheelAxisMovement(key) != 0f) return true;
                }
                else if (InputKeys.IsGamepadButton(key))
                {
                    if (slot >= 0 && IsGamepadButtonDown(slot, InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonPos(key))//!!!!!!!!!!!!!!!!!
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) > 0f) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonNeg(key))//!!!!!!!!!!!!!!!!!!!!
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) < 0f) return true;
                }
                else if (InputKeys.IsGamepadAxis(key))//!!!!!!!!!!!!!!!!!!!
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) != 0f) return true;
                }
                else
                {
                    if (slot <= 0 && IsKeyDown(InputKeys.TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsPressed(int slot)
        {
            if (keys.Count <= 0) return false;
            foreach (var key in keys)
            {
                if (InputKeys.IsMouse(key))
                {
                    if (slot <= 0 && IsMouseButtonPressed(InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsMouseWheelButton(key))
                {
                    if (slot <= 0) return IsMouseWheelDown(key);
                }
                else if (InputKeys.IsMouseWheelAxis(key))
                {
                    if (slot <= 0 && GetMouseWheelAxisMovement(key) != 0f) return true;
                }
                else if (InputKeys.IsGamepadButton(key))
                {
                    if (slot >= 0 && IsGamepadButtonPressed(slot, InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonPos(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) > 0f) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonNeg(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) < 0f) return true;
                }
                else if (InputKeys.IsGamepadAxis(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) != 0f) return true;
                }
                else
                {
                    if (slot <= 0 && IsKeyPressed(InputKeys.TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsReleased(int slot)
        {
            if (keys.Count <= 0) return false;
            foreach (var key in keys)
            {
                if (InputKeys.IsMouse(key))
                {
                    if (slot <= 0 && IsMouseButtonReleased(InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsMouseWheelButton(key))
                {
                    if (slot <= 0) return IsMouseWheelReleased(key);
                }
                else if (InputKeys.IsMouseWheelAxis(key))
                {
                    if (slot <= 0 && GetMouseWheelAxisMovement(key) == 0f) return true;
                }
                else if (InputKeys.IsGamepadButton(key))
                {
                    if (slot >= 0 && IsGamepadButtonReleased(slot, InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonPos(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) <= 0f) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonNeg(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) >= 0f) return true;
                }
                else if (InputKeys.IsGamepadAxis(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) == 0f) return true;
                }
                else
                {
                    if (slot <= 0 && IsKeyReleased(InputKeys.TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsUp(int slot)
        {
            if (keys.Count <= 0) return false;
            foreach (var key in keys)
            {
                if (InputKeys.IsMouse(key))
                {
                    if (slot <= 0 && IsMouseButtonUp(InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsMouseWheelButton(key))
                {
                    if (slot <= 0) return IsMouseWheelReleased(key);
                }
                else if (InputKeys.IsMouseWheelAxis(key))
                {
                    if (slot <= 0 && GetMouseWheelAxisMovement(key) == 0f) return true;
                }
                else if (InputKeys.IsGamepadButton(key))
                {
                    if (slot >= 0 && IsGamepadButtonUp(slot, InputKeys.TransformKeyValue(key))) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonPos(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) <= 0f) return true;
                }
                else if (InputKeys.IsGamepadAxisButtonNeg(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) >= 0f) return true;
                }
                else if (InputKeys.IsGamepadAxis(key))
                {
                    if (slot >= 0 && GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key)) == 0f) return true;
                }
                else
                {
                    if (slot <= 0 && IsKeyUp(InputKeys.TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        

        //implement new axis system -> just 1 function to get axis value
        //new functions and caching to know if axis was released or pressed
        //should not count axis more than once
        public float GetAxis(int slot, float deadzone)
        {
            return 0f;
        }
        //----------deprecate!------------------------------------
        public float GetGamepadAxis(int slot, float deadzone)
        {
            if (slot < 0) return 0f;
            float axisValue = 0f;
            foreach (var key in keys)
            {
                if (InputKeys.IsGamepadAxis(key) || InputKeys.IsGamepadAxisButton(key))
                {
                    axisValue += AxisMovement(key, slot, deadzone);
                }
            }
            return Clamp(axisValue, -1f, 1f);
        }
        public float GetMouseWheelAxis()
        {
            float axisValue = 0f;
            Vector2 mw = GetMouseWheelMovementV();
            foreach (var key in keys)
            {
                if (InputKeys.IsMouseWheelAxis(key) || InputKeys.IsMouseWheelButton(key))
                {
                    axisValue += GetMouseWheelAxisMovement(key);
                }
            }
            return Clamp(axisValue, -1f, 1f);
        }

        private Vector2 GetMouseWheelMovementV(bool inverted = false)
        {
            Vector2 movement = GetMouseWheelMoveV();

            if (inverted) return -movement;
            return movement;
        }
        private float GetMouseWheelAxisMovement(Keys key)
        {
            if (InputKeys.IsMouseWheelAxis(key) || InputKeys.IsMouseWheelButton(key))
            {
                if(key == Keys.MW_AXIS_HORIZONTAL || key == Keys.MW_LEFT || key == Keys.MW_RIGHT)
                {
                    return GetMouseWheelMovementV().X;
                }
                else
                {
                    return GetMouseWheelMovementV().Y;

                }
            }
            return 0f;
        }
        private bool IsMouseWheelDown(Keys key)
        {
            Vector2 mw = GetMouseWheelMovementV();
            if (key == Keys.MW_UP && mw.Y < 0) return true;
            else if (key == Keys.MW_DOWN && mw.Y > 0) return true;
            else if (key == Keys.MW_LEFT && mw.X < 0) return true;
            else if (key == Keys.MW_RIGHT && mw.X > 0) return true;
            else return false;
        }
        private bool IsMouseWheelReleased(Keys key)
        {
            Vector2 mw = GetMouseWheelMovementV();
            if (key == Keys.MW_UP && mw.Y >= 0) return true;
            else if (key == Keys.MW_DOWN && mw.Y <= 0) return true;
            else if (key == Keys.MW_LEFT && mw.X >= 0) return true;
            else if (key == Keys.MW_RIGHT && mw.X <= 0) return true;
            else return false;
        }
        private float AxisMovement(Keys key, int slot, float deadzone)
        {
            if (slot < 0) return 0f;
            if (InputKeys.IsGamepadAxis(key) || InputKeys.IsGamepadAxisButton(key))
            {
                float movement = GetGamepadAxisMovement(slot, InputKeys.TransformKeyValue(key));
                if (MathF.Abs(movement) < deadzone) movement = 0f;
                return movement;
            }
            return 0f;
        }
        //--------------------------------------------------------



        private string GetKeyboardKeyName(bool shorthand = true)
        {
            List<Keys> keyboard = new();
            foreach (var key in keys) if (InputKeys.IsKeyboard(key)) keyboard.Add(key);
            if (keyboard.Count <= 0) return "";
            return InputKeys.GetKeyName(keyboard[0], shorthand);
        }
        private string GetMouseKeyName(bool shorthand = true)
        {
            List<Keys> mouse = new();
            foreach (var key in keys) if (InputKeys.IsMouse(key)) mouse.Add(key);
            if (mouse.Count <= 0) return "";
            return InputKeys.GetKeyName(mouse[0], shorthand);
        }
        private string GetGamepadKeyName(bool shorthand = true)
        {
            List<Keys> gamepad = new();
            foreach (var key in keys) if (InputKeys.IsGamepad(key)) gamepad.Add(key);
            if (gamepad.Count <= 0) return "";
            return InputKeys.GetKeyName(gamepad[0], shorthand);
        }

    }
    public class InputMap2
    {
        private Dictionary<int, InputCondition> conditions = new();
        public int Slot { get; protected set; } = -1;
        public int ID { get; protected set; } = -1;
        public float Deadzone { get; set; } = 0.2f;
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

        public bool IsDown(int id)
        {
            if (conditions.ContainsKey(id))
            {
                return conditions[id].IsDown(Slot);
            }
            return false;
        }
        public bool IsPressed(int id)
        {
            if (conditions.ContainsKey(id))
            {
                return conditions[id].IsPressed(Slot);
            }
            return false;
        }
        public bool IsReleased(int id)
        {
            if (conditions.ContainsKey(id))
            {
                return conditions[id].IsReleased(Slot);
            }
            return false;
        }
        public bool IsUp(int id)
        {
            if (conditions.ContainsKey(id))
            {
                return conditions[id].IsUp(Slot);
            }
            return false;
        }
        
        public float GetAxis(int idNegative, int idPositive)
        {
            if (!conditions.ContainsKey(idNegative) || !conditions.ContainsKey(idPositive)) return 0f;
            float p = conditions[idPositive].IsDown(Slot) ? 1f : 0f;
            float n = conditions[idNegative].IsDown(Slot) ? 1f : 0f;
            return p - n;
        }
        public Vector2 GetAxis(int idLeft, int idRight, int idUp, int idDown)
        {
            return new(GetAxis(idLeft, idRight), GetAxis(idUp, idDown));
        }
        public float GetGamepadAxis(int id)
        {
            if (!conditions.ContainsKey(id)) return 0f;
            return conditions[id].GetGamepadAxis(Slot, Deadzone);
        }
        public Vector2 GetGamepadAxis(int gamepadAxisHorID, int gamepadAxisVerID)
        {
            return new(GetGamepadAxis(gamepadAxisHorID), GetGamepadAxis(gamepadAxisVerID));
        }
        
        
        

        public (string keyboard, string mouse, string gamepad) GetKeyNames(int id, bool shorthand = false)
        {
            if(conditions.ContainsKey(id)) return conditions[id].GetKeyNames(shorthand);
            return ("", "", "");
        }
    }

    public static class InputHandler2
    {

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

}
