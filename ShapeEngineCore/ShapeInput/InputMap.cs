using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using Vortice.XInput;
using static ShapeInput.InputAction;

namespace ShapeInput
{
    public struct InputState
    {
        public bool down = false;
        public bool up = false;
        public bool released = false;
        public bool pressed = false;
        public float axisValue = 0f;

        public InputState() { }
        public InputState(bool down, bool up, bool released, bool pressed, float axisValue) 
        { 
            this.down = down;
            this.up = up;
            this.released = released;
            this.pressed = pressed;
            this.axisValue = axisValue;
        }
    }
    public enum Axis 
    {
        NONE = -1,
        MW_AXIS_VERTICAL = 10,
        MW_AXIS_HORIZONTAL = 11,
        GP_AXIS_LEFT_X = 400,
        GP_AXIS_LEFT_Y = 401,
        GP_AXIS_RIGHT_X = 402,
        GP_AXIS_RIGHT_Y = 403,
        GP_AXIS_LEFT_TRIGGER = 404,
        GP_AXIS_RIGHT_TRIGGER = 405,
    }
    public enum Buttons
    {
        NONE = -1,
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
    public interface IInputValue
    {
        public (string keyboard, string mouse, string gamepad) GetNames(bool shorthand = true);

        
        public void Change(Buttons newButton);
        public void Change(Axis newAxis);
        public void Change((Buttons neg, Buttons pos) newAxis);



        public bool IsDown(int slot);
        public bool IsUp(int slot);
        public float GetAxis(int slot, float deadzone);

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

    }
    public class ButtonValue : IInputValue
    {
        private Buttons button = Buttons.NONE;
        public ButtonValue(Buttons button)
        {
            this.button = button;
        }

        public void Change(Buttons newButton)
        {
            button = newButton;
        }
        public void Change(Axis newAxis) { return; }
        public void Change((Buttons neg, Buttons pos) newAxis) { return; }

        public (string keyboard, string mouse, string gamepad) GetNames(bool shorthand = true)
        {
            if (IInputValue.IsKeyboard(button)) return (IInputValue.GetButtonName(button, shorthand), "", "");
            else if (IInputValue.IsMouse(button)) return ("", IInputValue.GetButtonName(button, shorthand), "");
            else return ("", "", IInputValue.GetButtonName(button, shorthand));
        }

        public bool IsDown(int slot)
        {
            if(button == Buttons.NONE) return false;
            int buttonID = IInputValue.GetButtonID(button);
            if (IInputValue.IsMouse(button))
            {
                if (slot <= 0 && IsMouseButtonDown(buttonID)) return true;
            }
            else if (IInputValue.IsMouseWheelAxisButton(button))
            {
                if (slot <= 0 && IInputValue.GetMouseWheelAxisButtonValue(button) > 0f) return true;
            }
            else if (IInputValue.IsGamepadButton(button))
            {
                if (slot >= 0 && IsGamepadButtonDown(slot, buttonID)) return true;
            }
            else if (IInputValue.IsGamepadAxisButton(button))
            {
                if (slot >= 0 && IInputValue.GetGamepadAxisButtonValue(button, slot, 0f) > 0f) return true;
            }
            else
            {
                if (slot <= 0 && IsKeyDown(buttonID)) return true;
            }
            return false;
        }
        public bool IsUp(int slot)
        {
            if (button == Buttons.NONE) return false;
            int buttonID = IInputValue.GetButtonID(button);
            if (IInputValue.IsMouse(button))
            {
                if (slot <= 0 && IsMouseButtonUp(buttonID)) return true;
            }
            else if (IInputValue.IsMouseWheelAxisButton(button))
            {
                if (slot <= 0 && IInputValue.GetMouseWheelAxisButtonValue(button) <= 0f) return true;
            }
            else if (IInputValue.IsGamepadButton(button))
            {
                if (slot >= 0 && IsGamepadButtonUp(slot, buttonID)) return true;
            }
            else if (IInputValue.IsGamepadAxisButton(button))
            {
                if (slot >= 0 && IInputValue.GetGamepadAxisButtonValue(button, slot, 0f) <= 0f) return true;
            }
            else
            {
                if (slot <= 0 && IsKeyUp(buttonID)) return true;
            }
            return false;
        }
        public float GetAxis(int slot, float deadzone)
        {
            return IsDown(slot) ? 1f : 0f;
        }
        
    }
    public class AxisValue : IInputValue
    {
        private Axis axis = Axis.NONE;
        private (Buttons neg, Buttons pos) axisButtons = (Buttons.NONE, Buttons.NONE);
        
        public AxisValue(Axis axis)
        {
            this.axis = axis;
        }
        public AxisValue((Buttons neg, Buttons pos) axis)
        {
            this.axisButtons = axis;
        }

        public void Change(Buttons newButton) { return; }
        public void Change(Axis newAxis)
        {
            axis = newAxis;
        }
        public void Change((Buttons neg, Buttons pos) newAxis)
        {
            axisButtons = newAxis;
        }

        public (string keyboard, string mouse, string gamepad) GetNames(bool shorthand = true)
        {
            if (axis != Axis.NONE)
            {
                if (IInputValue.IsGamepadAxis(axis)) return ("", "", IInputValue.GetAxisName(axis, shorthand));
                else return ("", IInputValue.GetAxisName(axis, shorthand), "");
            }
            else
            {
                if (axisButtons.neg != Buttons.NONE && axisButtons.pos != Buttons.NONE)
                {
                    string name = IInputValue.GetButtonName(axisButtons.neg, shorthand) + "/" + IInputValue.GetButtonName(axisButtons.pos, shorthand);
                    if (IInputValue.IsKeyboard(axisButtons.neg)) return (name, "", "");
                    else if (IInputValue.IsMouse(axisButtons.neg)) return ("", name, "");
                    else return ("", "", name);
                }
                else return ("", "", "");
            }
        }

        public bool IsDown(int slot)
        {
            if(axis != Axis.NONE)
            {
                if (IInputValue.IsGamepadAxis(axis))
                {
                    return IInputValue.GetGamepadAxisValue(axis, slot, 0f) != 0f;
                }
                else if (IInputValue.IsMouseWheelAxis(axis))
                {
                    return IInputValue.GetMouseWheelAxisValue(axis) != 0f;
                }
            }
            else
            {
                if (axisButtons.neg != Buttons.NONE && axisButtons.pos != Buttons.NONE)
                {
                    if (IInputValue.IsMouseWheelAxisButton(axisButtons.neg) && IInputValue.IsMouseWheelAxisButton(axisButtons.pos))
                    {
                        float neg = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.neg);
                        float pos = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.pos);
                        return (pos - neg) != 0;
                    }
                    else if (IInputValue.IsGamepadAxisButton(axisButtons.neg) && IInputValue.IsGamepadAxisButton(axisButtons.pos))
                    {
                        return IInputValue.GetGamepadAxisValue(axisButtons.neg, slot, 0f) != 0f;
                    }
                }
            }
            
            return false;
        }
        public bool IsUp(int slot)
        {
            if (axis != Axis.NONE)
            {
                if (IInputValue.IsGamepadAxis(axis))
                {
                    return IInputValue.GetGamepadAxisValue(axis, slot, 0f) == 0f;
                }
                else if (IInputValue.IsMouseWheelAxis(axis))
                {
                    return IInputValue.GetMouseWheelAxisValue(axis) == 0f;
                }
            }
            else
            {
                if (axisButtons.neg != Buttons.NONE && axisButtons.pos != Buttons.NONE)
                {
                    if (IInputValue.IsMouseWheelAxisButton(axisButtons.neg) && IInputValue.IsMouseWheelAxisButton(axisButtons.pos))
                    {
                        float neg = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.neg);
                        float pos = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.pos);
                        return (pos - neg) == 0f;
                    }
                    else if (IInputValue.IsGamepadAxisButton(axisButtons.neg) && IInputValue.IsGamepadAxisButton(axisButtons.pos))
                    {
                        return IInputValue.GetGamepadAxisValue(axisButtons.neg, slot, 0f) == 0f;
                    }
                }
            }

            return false;
        }
        public float GetAxis(int slot, float deadzone)
        {
            if (axis != Axis.NONE)
            {
                if (IInputValue.IsGamepadAxis(axis))
                {
                    return IInputValue.GetGamepadAxisValue(axis, slot, deadzone);
                }
                else if (IInputValue.IsMouseWheelAxis(axis))
                {
                    return IInputValue.GetMouseWheelAxisValue(axis); // implement deadzone
                }
            }
            else
            {
                if (axisButtons.neg != Buttons.NONE && axisButtons.pos != Buttons.NONE)
                {
                    if (IInputValue.IsMouseWheelAxisButton(axisButtons.neg) && IInputValue.IsMouseWheelAxisButton(axisButtons.pos))
                    {
                        //implement deadzone
                        float neg = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.neg);
                        float pos = IInputValue.GetMouseWheelAxisButtonValue(axisButtons.pos);
                        return (pos - neg);
                    }
                    else if (IInputValue.IsGamepadAxisButton(axisButtons.neg) && IInputValue.IsGamepadAxisButton(axisButtons.pos))
                    {
                        return IInputValue.GetGamepadAxisValue(axisButtons.neg, slot, deadzone);
                    }
                    else
                    {
                        //gamepad or keyboard buttons as axis
                    }
                }
            }

            return 0f;
        }
    }
    public class InputCondition
    {
        public int ID { get; protected set; } = -1;
        private List<Buttons> keys = new();
        public InputCondition(int id, params Buttons[] keys)
        {
            this.keys = keys.ToList();
            this.ID = id;
        }

        public void ReplaceKeys(params Buttons[] newKeys)
        {
            keys = newKeys.ToList();
        }
        public bool ChangeKey(Buttons newKey, int index)
        {
            if (index >= 0 && index < keys.Count)
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
        private float GetMouseWheelAxisMovement(Buttons key)
        {
            if (InputKeys.IsMouseWheelAxis(key) || InputKeys.IsMouseWheelButton(key))
            {
                if (key == Buttons.MW_AXIS_HORIZONTAL || key == Buttons.MW_LEFT || key == Buttons.MW_RIGHT)
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
        private bool IsMouseWheelDown(Buttons key)
        {
            Vector2 mw = GetMouseWheelMovementV();
            if (key == Buttons.MW_UP && mw.Y < 0) return true;
            else if (key == Buttons.MW_DOWN && mw.Y > 0) return true;
            else if (key == Buttons.MW_LEFT && mw.X < 0) return true;
            else if (key == Buttons.MW_RIGHT && mw.X > 0) return true;
            else return false;
        }
        private bool IsMouseWheelReleased(Buttons key)
        {
            Vector2 mw = GetMouseWheelMovementV();
            if (key == Buttons.MW_UP && mw.Y >= 0) return true;
            else if (key == Buttons.MW_DOWN && mw.Y <= 0) return true;
            else if (key == Buttons.MW_LEFT && mw.X >= 0) return true;
            else if (key == Buttons.MW_RIGHT && mw.X <= 0) return true;
            else return false;
        }
        private float AxisMovement(Buttons key, int slot, float deadzone)
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
            List<Buttons> keyboard = new();
            foreach (var key in keys) if (InputKeys.IsKeyboard(key)) keyboard.Add(key);
            if (keyboard.Count <= 0) return "";
            return InputKeys.GetKeyName(keyboard[0], shorthand);
        }
        private string GetMouseKeyName(bool shorthand = true)
        {
            List<Buttons> mouse = new();
            foreach (var key in keys) if (InputKeys.IsMouse(key)) mouse.Add(key);
            if (mouse.Count <= 0) return "";
            return InputKeys.GetKeyName(mouse[0], shorthand);
        }
        private string GetGamepadKeyName(bool shorthand = true)
        {
            List<Buttons> gamepad = new();
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
