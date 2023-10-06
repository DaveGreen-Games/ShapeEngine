namespace ShapeEngine.Input;

public class ShapeKeyboardButtonInput : IShapeInputType
{
    private readonly ShapeKeyboardButton button;
    public ShapeKeyboardButtonInput(ShapeKeyboardButton button)
    {
        this.button = button;
    }

    public ShapeInputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button);
    }

    public ShapeInputState GetState(ShapeInputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button, prev);
    }
    public string GetName(bool shorthand = true) => GetKeyboardButtonName(button, shorthand);
    public InputDevice GetInputDevice() => InputDevice.Keyboard;
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
            case ShapeKeyboardButton.TAB: return shortHand ? "Tab" : "Tab";
            case ShapeKeyboardButton.BACKSPACE: return shortHand ? "Bckspc" : "Backspace";
            case ShapeKeyboardButton.INSERT: return shortHand ? "Ins" : "Insert";
            case ShapeKeyboardButton.DELETE: return shortHand ? "Del" : "Delete";
            case ShapeKeyboardButton.RIGHT: return shortHand ? "Rgt" : "Right";
            case ShapeKeyboardButton.LEFT: return shortHand ? "Lft" : "Left";
            case ShapeKeyboardButton.DOWN: return shortHand ? "Dwn" : "Down";
            case ShapeKeyboardButton.UP: return shortHand ? "Up" : "Up";
            case ShapeKeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
            case ShapeKeyboardButton.PAGE_DOWN: return shortHand ? "PDwn" : "Page Down";
            case ShapeKeyboardButton.HOME: return shortHand ? "Home" : "Home";
            case ShapeKeyboardButton.END: return shortHand ? "End" : "End";
            case ShapeKeyboardButton.CAPS_LOCK: return shortHand ? "CpsL" : "Caps Lock";
            case ShapeKeyboardButton.SCROLL_LOCK: return shortHand ? "ScrL" : "Scroll Lock";
            case ShapeKeyboardButton.NUM_LOCK: return shortHand ? "NumL" : "Num Lock";
            case ShapeKeyboardButton.PRINT_SCREEN: return shortHand ? "Prnt" : "Print Screen";
            case ShapeKeyboardButton.PAUSE: return shortHand ? "Pause" : "Pause";
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
            case ShapeKeyboardButton.KP_MULTIPLY: return shortHand ? "KPMult" : "Keypad Multiply";
            case ShapeKeyboardButton.KP_SUBTRACT: return shortHand ? "KPSub" : "Keypad Subtract";
            case ShapeKeyboardButton.KP_ADD: return shortHand ? "KPAdd" : "Keypad Add";
            case ShapeKeyboardButton.KP_ENTER: return shortHand ? "KPEnt" : "Keypad Enter";
            case ShapeKeyboardButton.KP_EQUAL: return shortHand ? "KPEqual" : "Keypad Equal";
            case ShapeKeyboardButton.VOLUME_UP: return shortHand ? "Vol+" : "Volume Up";
            case ShapeKeyboardButton.VOLUME_DOWN: return shortHand ? "Vol-" : "Volume Down";
            case ShapeKeyboardButton.BACK: return shortHand ? "Bck" : "Back";
            case ShapeKeyboardButton.NULL: return shortHand ? "Null" : "Null";
            case ShapeKeyboardButton.MENU: return shortHand ? "Menu" : "Menu";
            default: return "No Key";
        }
    }

    private static bool IsDown(ShapeKeyboardButton button) => IsKeyDown((int)button);
    public static ShapeInputState GetState(ShapeKeyboardButton button)
    {
        bool down = IsDown(button);
        return new(down, !down, 0f, -1);
    }
    public static ShapeInputState GetState(ShapeKeyboardButton button, ShapeInputState previousState)
    {
        return new(previousState, GetState(button));
    }
}