using Raylib_CsLo;

namespace ShapeInput
{
    public class InputAction
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

        private string name = "";
        private List<Keys> actionKeys = new();
        private bool disabled = false;
        //private int gamepadIndex = -1;
        private float deadzone = 0.25f;

        //private float holdInterval = 0f;
        //private float holdTimer = -1f;
        //
        //private float doubleTapInterval = 0f;
        //private float doubleTapTimer = 0f;
        //private bool doubleTapRelease = false;

        public InputAction(string name, params Keys[] keys)
        {
            this.name = name;
            foreach (var key in keys)
            {
                AddKey(key);
            }
        }
        //public InputAction(string name, float doubleTapInterval, float holdInterval, params Keys[] keys)
        //{
        //    this.name = name;
        //    this.doubleTapInterval = doubleTapInterval;
        //    this.holdInterval = holdInterval;
        //    foreach (var key in keys)
        //    {
        //        AddKey(key);
        //    }
        //}
        public InputAction(string name, float deadzone, params Keys[] keys)
        {
            this.name = name;
            this.deadzone = deadzone;
            foreach (var key in keys)
            {
                AddKey(key);
            }
        }
        //public InputAction(string name, float deadzone, float doubleTapInterval, float holdInterval, params Keys[] keys)
        //{
        //    this.name = name;
        //    this.deadzone = deadzone;
        //    this.doubleTapInterval = doubleTapInterval;
        //    this.holdInterval = holdInterval;
        //    foreach (var key in keys)
        //    {
        //        AddKey(key);
        //    }
        //}
        
        public List<Keys> GetActionKeys()
        {
            List<Keys> keys = new List<Keys>();
            keys.AddRange(actionKeys);
            return keys;
        }
        //public void Update(float dt, int gamepad, bool gamepadOnly)
        //{
        //    if(holdInterval > 0f)
        //    {
        //        if (holdTimer < 0f) // no hold in progress
        //        {
        //            if (IsPressed(gamepad, gamepadOnly))
        //            {
        //                holdTimer = holdInterval - dt; // hold started
        //            }
        //        }
        //        else if (holdTimer >= 0f) // hold in progress or finished
        //        {
        //            if (IsReleased(gamepad, gamepadOnly))
        //            {
        //                holdTimer = -1f; // hold canceled
        //            }
        //            else
        //            {
        //                if (holdTimer > 0f)
        //                {
        //                    holdTimer -= dt;
        //                    if (holdTimer <= 0f) holdTimer = 0f; //hold finished
        //                }
        //                else holdTimer = holdInterval; // restart hold
        //
        //            }
        //        }
        //    }
        //}

        //public bool HasHold() { return holdInterval > 0f; }
        //public float GetHoldF()
        //{
        //    if (holdInterval <= 0f) return -1f;
        //    if (holdTimer < 0f) return -1f;
        //    return 1.0f - ( holdTimer / holdInterval );
        //}

        //public bool IsHoldFinished()
        //{
        //    return holdTimer == 0f;
        //}
        //public bool IsDoubleTap()
        //{
        //    if(doubleTapTimer > 0f)
        //    {
        //        if (doubleTapRelease)
        //        {
        //
        //        }
        //    }
        //}

        public void Rename(string newName) { name = newName; }
        public void AddKey(Keys key)
        {
            if (actionKeys.Contains(key)) return;
            actionKeys.Add(key);
        }
        public void RemoveKey(Keys key)
        {
            actionKeys.Remove(key);
        }
        public string GetName() { return name; }
        public List<string> GetAllKeyNames(bool shorthand = true)
        {
            List<string> keyNames = new();
            foreach (var key in actionKeys)
            {
                keyNames.Add(GetKeyName(key, shorthand));
            }
            return keyNames;
        }
        public List<string> GetKeyboardKeyNames(bool shorthand = true)
        {
            List<string> keyNames = new();
            var keyboardActionKeys = actionKeys.FindAll((Keys k) => { return IsKeyboard(k); });
            foreach (var key in keyboardActionKeys)
            {
                keyNames.Add(GetKeyName(key, shorthand));
            }
            return keyNames;
        }
        public List<string> GetGamepadKeyNames(bool shorthand = true)
        {
            List<string> keyNames = new();
            var gamepadActionKeys = actionKeys.FindAll((Keys k) => { return IsGamepad(k); });
            foreach (var key in gamepadActionKeys)
            {
                keyNames.Add(GetKeyName(key, shorthand));
            }
            return keyNames;
        }
        public List<string> GetMouseKeyNames(bool shorthand = true)
        {
            List<string> keyNames = new();
            var mouseActionKeys = actionKeys.FindAll((Keys k) => { return IsMouse(k); });
            foreach (var key in mouseActionKeys)
            {
                keyNames.Add(GetKeyName(key, shorthand));
            }
            return keyNames;
        }
        public string GetKeyboardKeyName(bool shorthand = true)
        {
            var keyboardActionKeys = actionKeys.FindAll((Keys k) => { return IsKeyboard(k); });
            if (keyboardActionKeys.Count == 0) return "";
            return GetKeyName(keyboardActionKeys[0], shorthand);
        }
        public string GetMouseKeyName(bool shorthand = true)
        {
            var mouseActionKeys = actionKeys.FindAll((Keys k) => { return IsMouse(k); });
            if (mouseActionKeys.Count == 0) return "";
            return GetKeyName(mouseActionKeys[0], shorthand);
        }
        public string GetGamepadKeyName(bool shorthand = true)
        {
            var gamepadActionKeys = actionKeys.FindAll((Keys k) => { return IsGamepad(k); });
            if (gamepadActionKeys.Count == 0) return "";
            return GetKeyName(gamepadActionKeys[0], shorthand);
        }


        public (string keyboard, string mouse, string gamepad) GetKeyNames(bool shorthand = true)
        {
            return (GetKeyboardKeyName(shorthand), GetMouseKeyName(shorthand), GetGamepadKeyName(shorthand));
        }
        //public string GetKeyName(bool gamepad = false, bool shorthand = false)
        //{
        //    if (gamepad) return GetGamepadKeyName(shorthand);
        //    else return GetKeyboardKeyName(shorthand);
        //}
        public bool IsDisabled() { return disabled; }
        public void Enable() { disabled = false; }
        public void Disable() { disabled = true; }

        //public string GetInputKeyName(bool isGamepad)
        //{
        //    if (isGamepad) return gamepadKeyName;
        //    else return keyboardMouseKeyName;
        //}
        //gamepad axis released/pressed are the same as up/down right now
        public bool IsDown(int gamepad, bool gamepadOnly = false)
        {
            if (actionKeys.Count <= 0 || IsDisabled()) return false;
            foreach (var key in actionKeys)
            {
                if (IsMouse(key))
                {
                    if (!gamepadOnly && IsMouseButtonDown(TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadButton(key))
                {
                    if (IsGamepadButtonDown(gamepad, TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonPos(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) > 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonNeg(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) < 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxis(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) != 0f) return true;
                }
                else
                {
                    if (!gamepadOnly && IsKeyDown(TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsPressed(int gamepad, bool gamepadOnly = false)
        {
            if (actionKeys.Count <= 0 || IsDisabled()) return false;
            foreach (var key in actionKeys)
            {
                if (IsMouse(key))
                {
                    if (!gamepadOnly && IsMouseButtonPressed(TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadButton(key))
                {
                    if (IsGamepadButtonPressed(gamepad, TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonPos(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) > 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonNeg(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) < 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxis(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) != 0f) return true;
                }
                else
                {
                    if (!gamepadOnly && IsKeyPressed(TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsReleased(int gamepad, bool gamepadOnly = false)
        {
            if (actionKeys.Count <= 0 || IsDisabled()) return false;
            foreach (var key in actionKeys)
            {
                if (IsMouse(key))
                {
                    if (!gamepadOnly && IsMouseButtonReleased(TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadButton(key))
                {
                    if (IsGamepadButtonReleased(gamepad, TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonPos(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) <= 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonNeg(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) >= 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxis(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) == 0f) return true;
                }
                else
                {
                    if (!gamepadOnly && IsKeyReleased(TransformKeyValue(key))) return true;
                }
            }
            return false;
        }
        public bool IsUp(int gamepad, bool gamepadOnly = false)
        {
            if (actionKeys.Count <= 0 || IsDisabled()) return false;
            foreach (var key in actionKeys)
            {
                if (IsMouse(key))
                {
                    if (!gamepadOnly && IsMouseButtonUp(TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadButton(key))
                {
                    if (IsGamepadButtonUp(gamepad, TransformKeyValue(key))) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonPos(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) <= 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxisButtonNeg(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) >= 0f) return true;
                }
                else if (gamepad >= 0 && IsGamepadAxis(key))
                {
                    if (GetGamepadAxisMovement(gamepad, TransformKeyValue(key)) == 0f) return true;
                }
                else
                {
                    if (!gamepadOnly && IsKeyUp(TransformKeyValue(key))) return true;
                }
            }
            return false;
        }

        private float AxisMovement(int gamepad, Keys key)
        {
            if (gamepad < 0) return 0f;
            if (!IsGamepadAxis(key) && !IsGamepadAxisButton(key)) return 0f;
            float movement = GetGamepadAxisMovement(gamepad, TransformKeyValue(key));
            if (MathF.Abs(movement) < deadzone) movement = 0f;
            return movement;
        }
        public float GetGamepadAxis(int gamepad)
        {
            if (gamepad < 0) return 0f;
            if (actionKeys.Count <= 0 || IsDisabled()) return 0f;
            float axisValue = 0f;
            foreach (var key in actionKeys)
            {
                axisValue += AxisMovement(gamepad, key);
            }
            return Clamp(axisValue, -1f, 1f);
        }

        public static bool IsGamepad(Keys key) { return (int)key >= 500 && (int)key <= 523; }
        public static bool IsGamepadAxis(Keys key) { return (int)key >= 400 && (int)key <= 405; }
        public static bool IsGamepadButton(Keys key) { return (int)key >= 500 && (int)key <= 517; }
        public static bool IsGamepadAxisButtonPos(Keys key) { return (int)key >= 600 && (int)key <= 603; }
        public static bool IsGamepadAxisButtonNeg(Keys key) { return (int)key >= 610 && (int)key <= 613; }
        public static bool IsGamepadAxisButton(Keys key) { return (int)key >= 600 && (int)key <= 613; }
        public static bool IsMouse(Keys key) { return (int)key >= 0 && (int)key <= 6; }
        public static bool IsKeyboard(Keys key) { return (int)key > 6 && (int)key < 400; }
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

}
