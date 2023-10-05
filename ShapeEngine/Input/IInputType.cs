
using System.Numerics;

namespace ShapeEngine.Input
{
    //new names for all of that
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
    public interface IInputType
    {
        public string GetName(bool shorthand = true);

        public IInputType Copy();
        public InputState GetState(int gamepadIndex);
        public bool IsGamepad();

        public static string GetMouseButtonName(SMouseButton button, bool shortHand = true)
        {
            switch (button)
            {
                case SMouseButton.LEFT: return shortHand ? "LMB" : "Left Mouse Button";
                case SMouseButton.RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
                case SMouseButton.MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
                case SMouseButton.SIDE: return shortHand ? "SMB" : "Side Mouse Button";
                case SMouseButton.EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
                case SMouseButton.FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
                case SMouseButton.BACK: return shortHand ? "BMB" : "Back Mouse Button";
                case SMouseButton.MW_UP: return shortHand ? "MW U" : "Mouse Wheel Up";
                case SMouseButton.MW_DOWN: return shortHand ? "MW D" : "Mouse Wheel Down";
                case SMouseButton.MW_LEFT: return shortHand ? "MW L" : "Mouse Wheel Left";
                case SMouseButton.MW_RIGHT: return shortHand ? "MW R" : "Mouse Wheel Right";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        
        public static string GetKeyboardButtonName(SKeyboardButton button, bool shortHand = true)
        {
            switch (button)
            {
                case SKeyboardButton.APOSTROPHE: return shortHand ? "´" : "Apostrophe";
                case SKeyboardButton.COMMA: return shortHand ? "," : "Comma";
                case SKeyboardButton.MINUS: return shortHand ? "-" : "Minus";
                case SKeyboardButton.PERIOD: return shortHand ? "." : "Period";
                case SKeyboardButton.SLASH: return shortHand ? "/" : "Slash";
                case SKeyboardButton.ZERO: return shortHand ? "0" : "Zero";
                case SKeyboardButton.ONE: return shortHand ? "1" : "One";
                case SKeyboardButton.TWO: return shortHand ? "2" : "Two";
                case SKeyboardButton.THREE: return shortHand ? "3" : "Three";
                case SKeyboardButton.FOUR: return shortHand ? "4" : "Four";
                case SKeyboardButton.FIVE: return shortHand ? "5" : "Five";
                case SKeyboardButton.SIX: return shortHand ? "6" : "Six";
                case SKeyboardButton.SEVEN: return shortHand ? "7" : "Seven";
                case SKeyboardButton.EIGHT: return shortHand ? "8" : "Eight";
                case SKeyboardButton.NINE: return shortHand ? "9" : "Nine";
                case SKeyboardButton.SEMICOLON: return shortHand ? ";" : "Semi Colon";
                case SKeyboardButton.EQUAL: return shortHand ? "=" : "Equal";
                case SKeyboardButton.A: return shortHand ? "A" : "A";
                case SKeyboardButton.B: return shortHand ? "B" : "B";
                case SKeyboardButton.C: return shortHand ? "C" : "C";
                case SKeyboardButton.D: return shortHand ? "D" : "D";
                case SKeyboardButton.E: return shortHand ? "E" : "E";
                case SKeyboardButton.F: return shortHand ? "F" : "F";
                case SKeyboardButton.G: return shortHand ? "G" : "G";
                case SKeyboardButton.H: return shortHand ? "H" : "H";
                case SKeyboardButton.I: return shortHand ? "I" : "I";
                case SKeyboardButton.J: return shortHand ? "J" : "J";
                case SKeyboardButton.K: return shortHand ? "K" : "K";
                case SKeyboardButton.L: return shortHand ? "L" : "L";
                case SKeyboardButton.M: return shortHand ? "M" : "M";
                case SKeyboardButton.N: return shortHand ? "N" : "N";
                case SKeyboardButton.O: return shortHand ? "O" : "O";
                case SKeyboardButton.P: return shortHand ? "P" : "P";
                case SKeyboardButton.Q: return shortHand ? "Q" : "Q";
                case SKeyboardButton.R: return shortHand ? "R" : "R";
                case SKeyboardButton.S: return shortHand ? "S" : "S";
                case SKeyboardButton.T: return shortHand ? "T" : "T";
                case SKeyboardButton.U: return shortHand ? "U" : "U";
                case SKeyboardButton.V: return shortHand ? "V" : "V";
                case SKeyboardButton.W: return shortHand ? "W" : "W";
                case SKeyboardButton.X: return shortHand ? "X" : "X";
                case SKeyboardButton.Y: return shortHand ? "Y" : "Y";
                case SKeyboardButton.Z: return shortHand ? "Z" : "Z";
                case SKeyboardButton.LEFT_BRACKET: return shortHand ? "[" : "Left Bracket";
                case SKeyboardButton.BACKSLASH: return shortHand ? "\\" : "Backslash";
                case SKeyboardButton.RIGHT_BRACKET: return shortHand ? "]" : "Right Bracket";
                case SKeyboardButton.GRAVE: return shortHand ? "`" : "Grave";//Check
                case SKeyboardButton.SPACE: return shortHand ? "Space" : "Space";
                case SKeyboardButton.ESCAPE: return shortHand ? "Esc" : "Escape";
                case SKeyboardButton.ENTER: return shortHand ? "Enter" : "Enter";
                case SKeyboardButton.TAB: return shortHand ? "Tab" : "Tab";
                case SKeyboardButton.BACKSPACE: return shortHand ? "Backspc" : "Backspace";
                case SKeyboardButton.INSERT: return shortHand ? "Ins" : "Insert";
                case SKeyboardButton.DELETE: return shortHand ? "Del" : "Delete";
                case SKeyboardButton.RIGHT: return shortHand ? "Right" : "Right";
                case SKeyboardButton.LEFT: return shortHand ? "Left" : "Left";
                case SKeyboardButton.DOWN: return shortHand ? "Down" : "Down";
                case SKeyboardButton.UP: return shortHand ? "Up" : "Up";
                case SKeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
                case SKeyboardButton.PAGE_DOWN: return shortHand ? "PDo" : "";
                case SKeyboardButton.HOME: return shortHand ? "Home" : "Home";
                case SKeyboardButton.END: return shortHand ? "End" : "End";
                case SKeyboardButton.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
                case SKeyboardButton.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
                case SKeyboardButton.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
                case SKeyboardButton.PRINT_SCREEN: return shortHand ? "Print" : "Print Screen";
                case SKeyboardButton.PAUSE: return shortHand ? "Pause" : "Pause";
                case SKeyboardButton.F1: return shortHand ? "F1" : "F1";
                case SKeyboardButton.F2: return shortHand ? "F2" : "F2";
                case SKeyboardButton.F3: return shortHand ? "F3" : "F3";
                case SKeyboardButton.F4: return shortHand ? "F4" : "F4";
                case SKeyboardButton.F5: return shortHand ? "F5" : "F5";
                case SKeyboardButton.F6: return shortHand ? "F6" : "F6";
                case SKeyboardButton.F7: return shortHand ? "F7" : "F7";
                case SKeyboardButton.F8: return shortHand ? "F8" : "F8";
                case SKeyboardButton.F9: return shortHand ? "F9" : "F9";
                case SKeyboardButton.F10: return shortHand ? "F10" : "F10";
                case SKeyboardButton.F11: return shortHand ? "F11" : "F11";
                case SKeyboardButton.F12: return shortHand ? "F12" : "F12";
                case SKeyboardButton.LEFT_SHIFT: return shortHand ? "LShift" : "Left Shift";
                case SKeyboardButton.LEFT_CONTROL: return shortHand ? "LCtrl" : "Left Control";
                case SKeyboardButton.LEFT_ALT: return shortHand ? "LAlt" : "Left Alt";
                case SKeyboardButton.LEFT_SUPER: return shortHand ? "LSuper" : "Left Super";
                case SKeyboardButton.RIGHT_SHIFT: return shortHand ? "RShift" : "Right Shift";
                case SKeyboardButton.RIGHT_CONTROL: return shortHand ? "RCtrl" : "Right Control";
                case SKeyboardButton.RIGHT_ALT: return shortHand ? "RAlt" : "Right Alt";
                case SKeyboardButton.RIGHT_SUPER: return shortHand ? "RSuper" : "Right Super";
                case SKeyboardButton.KB_MENU: return shortHand ? "KBMenu" : "KB Menu";
                case SKeyboardButton.KP_0: return shortHand ? "KP0" : "Keypad 0";
                case SKeyboardButton.KP_1: return shortHand ? "KP1" : "Keypad 1";
                case SKeyboardButton.KP_2: return shortHand ? "KP2" : "Keypad 2";
                case SKeyboardButton.KP_3: return shortHand ? "KP3" : "Keypad 3";
                case SKeyboardButton.KP_4: return shortHand ? "KP4" : "Keypad 4";
                case SKeyboardButton.KP_5: return shortHand ? "KP5" : "Keypad 5";
                case SKeyboardButton.KP_6: return shortHand ? "KP6" : "Keypad 6";
                case SKeyboardButton.KP_7: return shortHand ? "KP7" : "Keypad 7";
                case SKeyboardButton.KP_8: return shortHand ? "KP8" : "Keypad 8";
                case SKeyboardButton.KP_9: return shortHand ? "KP9" : "Keypad 9";
                case SKeyboardButton.KP_DECIMAL: return shortHand ? "KPDec" : "Keypad Decimal";
                case SKeyboardButton.KP_DIVIDE: return shortHand ? "KPDiv" : "Keypad Divide";
                case SKeyboardButton.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
                case SKeyboardButton.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
                case SKeyboardButton.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
                case SKeyboardButton.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
                case SKeyboardButton.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
                case SKeyboardButton.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
                case SKeyboardButton.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
                case SKeyboardButton.BACK: return shortHand ? "Back" : "Back";
                case SKeyboardButton.NULL: return shortHand ? "Null" : "Null";
                case SKeyboardButton.MENU: return shortHand ? "Menu" : "Menu";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        
        public static string GetGamepadButtonName(SGamepadButton button, bool shortHand = true)
        {
            switch (button)
            {
                case SGamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
                case SGamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
                case SGamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
                case SGamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
                case SGamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
                case SGamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
                case SGamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
                case SGamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
                case SGamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
                case SGamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
                case SGamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
                case SGamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
                case SGamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
                case SGamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
                case SGamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
                case SGamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
                case SGamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
                case SGamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
                case SGamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
                case SGamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
                case SGamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
                case SGamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
                case SGamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
                case SGamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
                case SGamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
                case SGamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
                default: return shortHand ? "No Key" : "No Key";
            }
        }


        public static string GetGamepadAxisName(SGamepadAxis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case SGamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
                case SGamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
                case SGamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
                case SGamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
                case SGamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
                case SGamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
                default: return shortHand ? "No Key" : "No Key";
            }
        }
        
        public static string GetMouseWheelAxisName(SMouseWheelAxis axis, bool shortHand = true)
        {
            switch (axis)
            {
                case SMouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
                case SMouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
                default: return shortHand ? "No Key" : "No Key";
            }
        }


        public static IInputType Create(SKeyboardButton button) { return new KeyboardButtonInput(button); }
        public static IInputType Create(SMouseButton button) { return new MouseButtonInput(button); }
        public static IInputType Create(SGamepadButton button, float deadzone = 0.2f) { return new GamepadButtonInput(button, deadzone); }
        public static IInputType Create(SKeyboardButton neg, SKeyboardButton pos) { return new KeyboardButtonAxisInput(neg, pos); }
        public static IInputType Create(SGamepadButton neg, SGamepadButton pos, float deadzone = 0.2f) { return new GamepadButtonAxisInput(neg, pos, deadzone); }
        public static IInputType Create(SMouseButton neg, SMouseButton pos) { return new MouseButtonAxisInput(neg, pos); }
        public static IInputType Create(SMouseWheelAxis axis) { return new MouseWheelAxisInput(axis); }
        public static IInputType Create(SGamepadAxis axis, float deadzone = 0.2f) { return new GamepadAxisInput(axis, deadzone); }

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
        protected static float GetGamepadAxisButtonValue(Buttons button, int gamepadIndex, float deadzone)
        {
            if(!IsGamepadAxisButton(button)) return 0f;
            float movement = GetGamepadAxisMovement(gamepadIndex, GetButtonID(button));
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
        
        protected static float GetGamepadAxisValue(Axis axis, int gamepadIndex, float deadzone)
        {
            if (gamepadIndex < 0) return 0f;
            float movement = GetGamepadAxisMovement(gamepadIndex, GetAxisID(axis));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            return movement;
        }
        protected static float GetGamepadAxisValue(Buttons axisButton, int gamepadIndex, float deadzone)
        {
            if(gamepadIndex < 0 ) return 0f;
            float movement = GetGamepadAxisMovement(gamepadIndex, GetButtonID(axisButton));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            return movement;
        }
        */
    }
    internal class KeyboardButtonInput : IInputType
    {
        private SKeyboardButton button;

        public KeyboardButtonInput(SKeyboardButton button)
        {
            this.button = button;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetKeyboardButtonName(button, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex > 0) return new InputState();

            bool down = IsKeyDown((int)button);

            return new InputState(down, !down, 0f);
        }
        public IInputType Copy()
        {
            return new KeyboardButtonInput(button);
        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    if (gamepadIndex <= 0 && IsKeyDown((int)button)) return true;
        //    return false;
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    if (gamepadIndex <= 0 && IsKeyUp((int)button)) return true;
        //    return false;
        //}
        //public float GetAxis(int gamepadIndex) { return 0f; }

    }
    internal class MouseButtonInput : IInputType
    {
        private SMouseButton button;

        public MouseButtonInput(SMouseButton button) { this.button = button; }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetMouseButtonName(button, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex > 0) return new InputState();

            bool down = IsDown();

            return new InputState(down, !down, 0f);
        }
        public IInputType Copy()
        {
            return new MouseButtonInput(button);
        }
        private bool IsDown()
        {
            int id = (int)button;
            if (id >= 10)
            {
                Vector2 value = GetMouseWheelMoveV();
                if (button == SMouseButton.MW_LEFT) return value.X < 0f;
                else if (button == SMouseButton.MW_RIGHT) return value.X > 0f;
                else if (button == SMouseButton.MW_UP) return value.Y < 0f;
                else if (button == SMouseButton.MW_DOWN) return value.Y > 0f;
                else return false;
            }
            else return IsMouseButtonDown(id);
        }

        //public bool IsUp(int gamepadIndex)
        //{
        //    if (gamepadIndex < 0) return false;
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
        //public float GetAxis(int gamepadIndex) { return 0f; }

    }
    internal class GamepadButtonInput : IInputType
    {
        private SGamepadButton button;
        private float deadzone;

        public GamepadButtonInput(SGamepadButton button, float deadzone = 0.2f) { this.button = button; this.deadzone = deadzone; }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetGamepadButtonName(button, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex < 0) return new InputState();

            bool down = IsDown(gamepadIndex);

            return new InputState(down, !down, 0f);
        }
        private bool IsDown(int gamepadIndex)
        {
            int id = (int)button;
            if (id >= 30 && id <= 33)
            {
                id -= 30;
                float value = GetGamepadAxisMovement(gamepadIndex, id);
                if (MathF.Abs(value) < deadzone) value = 0f;
                return value > 0f;
            }
            else if (id >= 40 && id <= 43)
            {
                id -= 40;
                float value = GetGamepadAxisMovement(gamepadIndex, id);
                if (MathF.Abs(value) < deadzone) value = 0f;
                return value < 0f;
            }
            else return IsGamepadButtonDown(gamepadIndex, id);
        }
        public IInputType Copy()
        {
            return new GamepadButtonInput(button, deadzone);
        }

        //public bool IsUp(int gamepadIndex)
        //{
        //    if (gamepadIndex < 0) return false;
        //
        //    int id = (int)button;
        //    if (id >= 30 && id <= 33)
        //    {
        //        id -= 30;
        //        float value = GetGamepadAxisMovement(gamepadIndex, id);
        //        if (MathF.Abs(value) < deadzone) value = 0f;
        //        return value <= 0f;
        //    }
        //    else if (id >= 40 && id <= 43)
        //    {
        //        id -= 40;
        //        float value = GetGamepadAxisMovement(gamepadIndex, id);
        //        if (MathF.Abs(value) < deadzone) value = 0f;
        //        return value >= 0f;
        //    }
        //    else return IsGamepadButtonUp(gamepadIndex, id);
        //}
        //public float GetAxis(int gamepadIndex) { return 0f; }
    }

    internal class KeyboardButtonAxisInput : IInputType
    {
        private SKeyboardButton neg;
        private SKeyboardButton pos;

        public KeyboardButtonAxisInput(SKeyboardButton neg, SKeyboardButton pos)
        {
            this.neg = neg;
            this.pos = pos;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetKeyboardButtonName(neg, shorthand) + " <> " + IInputType.GetKeyboardButtonName(pos, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex > 0) return new InputState();
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
        public IInputType Copy()
        {
            return new KeyboardButtonAxisInput(neg, pos);
        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    if (gamepadIndex > 0) return false;
        //
        //    return IsKeyDown((int)neg) || IsKeyDown((int)pos);
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    if (gamepadIndex > 0) return false;
        //
        //    return IsKeyUp((int)neg) && IsKeyUp((int)pos);
        //}
    }
    
    internal class MouseButtonAxisInput : IInputType
    {
        private SMouseButton neg;
        private SMouseButton pos;

        public MouseButtonAxisInput(SMouseButton neg, SMouseButton pos)
        {
            this.neg = neg;
            this.pos = pos;
        }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetMouseButtonName(neg, shorthand) + " <> " + IInputType.GetMouseButtonName(pos, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex > 0) return new InputState();
            float axis = GetAxis();
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInputType Copy()
        {
            return new MouseButtonAxisInput(neg, pos);
        }

        private float GetAxis()
        {
            float vNegative = GetValue(neg);
            float vPositive = GetValue(pos);
            return vPositive - vNegative;
        }
        private float GetValue(SMouseButton button)
        {
            int id = (int)button;
            if (id >= 10)
            {
                Vector2 value = GetMouseWheelMoveV();
                if (button == SMouseButton.MW_LEFT) return MathF.Abs(value.X);
                else if (button == SMouseButton.MW_RIGHT) return value.X;
                else if (button == SMouseButton.MW_UP) return MathF.Abs(value.Y);
                else if (button == SMouseButton.MW_DOWN) return value.Y;
                else return 0f;
            }
            else return IsMouseButtonDown(id) ? 1f : 0f;
        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex, neg) > 0f || GetValue(gamepadIndex, pos) > 0f;
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex, neg) <= 0f && GetValue(gamepadIndex, pos) <= 0f;
        //}
    }
    
    internal class GamepadButtonAxisInput : IInputType
    {
        private SGamepadButton neg;
        private SGamepadButton pos;
        private float deadzone;

        public GamepadButtonAxisInput(SGamepadButton neg, SGamepadButton pos, float deadzone = 0.2f)
        {
            this.neg = neg;
            this.pos = pos;
            this.deadzone = deadzone;
        }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true)
        {
            return IInputType.GetGamepadButtonName(neg, shorthand) + " <> " + IInputType.GetGamepadButtonName(pos, shorthand);
        }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex < 0) return new InputState();
            float axis = GetAxis(gamepadIndex);
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInputType Copy()
        {
            return new GamepadButtonAxisInput(neg, pos, deadzone);
        }

        private float GetAxis(int gamepadIndex)
        {
            float vNegative = GetValue(gamepadIndex, neg);
            float vPositive = GetValue(gamepadIndex, pos);
            return vPositive - vNegative;
        }
        private float GetValue(int gamepadIndex, SGamepadButton button)
        {
            if (gamepadIndex < 0) return 0f;

            int id = (int)button;
            if (id >= 30 && id <= 33)
            {
                id -= 30;
                float value = GetGamepadAxisMovement(gamepadIndex, id);
                if (MathF.Abs(value) < deadzone) return 0f;
                if (value > 0f) return value;
                else return 0f;
            }
            else if (id >= 40 && id <= 43)
            {
                id -= 40;
                float value = GetGamepadAxisMovement(gamepadIndex, id);
                if (MathF.Abs(value) < deadzone) return 0f;
                if (value < 0) return MathF.Abs(value);
                else return 0f;
            }
            else return IsGamepadButtonDown(gamepadIndex, id) ? 1f : 0f;
        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex, neg) > 0f || GetValue(gamepadIndex, pos) > 0f;
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex, neg) <= 0f && GetValue(gamepadIndex, pos) <= 0f;
        //}
    }

    internal class MouseWheelAxisInput : IInputType
    {
        private SMouseWheelAxis axis;
        public MouseWheelAxisInput(SMouseWheelAxis axis) { this.axis = axis; }

        public bool IsGamepad() { return false; }
        public string GetName(bool shorthand = true) { return IInputType.GetMouseWheelAxisName(axis, shorthand); }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex > 0) return new InputState();
            float axis = GetValue();
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInputType Copy()
        {
            return new MouseWheelAxisInput(axis);
        }

        private float GetValue()
        {
            Vector2 value = GetMouseWheelMoveV();
            return axis == SMouseWheelAxis.VERTICAL ? value.Y : value.X;
        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex) != 0f;
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex) == 0f;
        //}
        //private float GetAxis(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex);
        //}
    }
    
    internal class GamepadAxisInput : IInputType
    {
        private SGamepadAxis axis;
        private float deadzone;

        public GamepadAxisInput(SGamepadAxis axis, float deadzone = 0.2f) { this.axis = axis; this.deadzone = deadzone; }

        public bool IsGamepad() { return true; }
        public string GetName(bool shorthand = true) { return IInputType.GetGamepadAxisName(axis, shorthand); }
        public InputState GetState(int gamepadIndex)
        {
            if (gamepadIndex < 0) return new InputState();
            float axis = GetValue(gamepadIndex);
            bool down = axis != 0f;

            return new InputState(down, !down, axis);
        }
        public IInputType Copy()
        {
            return new GamepadAxisInput(axis, deadzone);
        }

        private float GetValue(int gamepadIndex)
        {
            float value = GetGamepadAxisMovement(gamepadIndex, (int)axis);
            if (MathF.Abs(value) < deadzone) return 0f;
            else return value;

        }

        //public bool IsDown(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex) != 0f;
        //}
        //public bool IsUp(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex) == 0f;
        //}
        //public float GetAxis(int gamepadIndex)
        //{
        //    return GetValue(gamepadIndex);
        //}
    }

}
