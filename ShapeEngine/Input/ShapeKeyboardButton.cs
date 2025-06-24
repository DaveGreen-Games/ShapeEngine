namespace ShapeEngine.Input;

/// <summary>
/// Represents keyboard buttons for the ShapeEngine input system.
/// The enum values correspond to their respective key codes.
/// </summary>
public enum ShapeKeyboardButton
{
    /// <summary>No key or unassigned.</summary>
    NONE = -1,
    /// <summary>Single quote / apostrophe key (').</summary>
    APOSTROPHE = 39,
    /// <summary>Comma (,).</summary>
    COMMA = 44,
    /// <summary>Minus (-).</summary>
    MINUS = 45,
    /// <summary>Period (.).</summary>
    PERIOD = 46,
    /// <summary>Slash (/).</summary>
    SLASH = 47,
    /// <summary>Number 0.</summary>
    ZERO = 48,
    /// <summary>Number 1.</summary>
    ONE = 49,
    /// <summary>Number 2.</summary>
    TWO = 50,
    /// <summary>Number 3.</summary>
    THREE = 51,
    /// <summary>Number 4.</summary>
    FOUR = 52,
    /// <summary>Number 5.</summary>
    FIVE = 53,
    /// <summary>Number 6.</summary>
    SIX = 54,
    /// <summary>Number 7.</summary>
    SEVEN = 55,
    /// <summary>Number 8.</summary>
    EIGHT = 56,
    /// <summary>Number 9.</summary>
    NINE = 57,
    /// <summary>Semicolon (;).</summary>
    SEMICOLON = 59,
    /// <summary>Equal (=).</summary>
    EQUAL = 61,
    /// <summary>Letter A.</summary>
    A = 65,
    /// <summary>Letter B.</summary>
    B = 66,
    /// <summary>Letter C.</summary>
    C = 67,
    /// <summary>Letter D.</summary>
    D = 68,
    /// <summary>Letter E.</summary>
    E = 69,
    /// <summary>Letter F.</summary>
    F = 70,
    /// <summary>Letter G.</summary>
    G = 71,
    /// <summary>Letter H.</summary>
    H = 72,
    /// <summary>Letter I.</summary>
    I = 73,
    /// <summary>Letter J.</summary>
    J = 74,
    /// <summary>Letter K.</summary>
    K = 75,
    /// <summary>Letter L.</summary>
    L = 76,
    /// <summary>Letter M.</summary>
    M = 77,
    /// <summary>Letter N.</summary>
    N = 78,
    /// <summary>Letter O.</summary>
    O = 79,
    /// <summary>Letter P.</summary>
    P = 80,
    /// <summary>Letter Q.</summary>
    Q = 81,
    /// <summary>Letter R.</summary>
    R = 82,
    /// <summary>Letter S.</summary>
    S = 83,
    /// <summary>Letter T.</summary>
    T = 84,
    /// <summary>Letter U.</summary>
    U = 85,
    /// <summary>Letter V.</summary>
    V = 86,
    /// <summary>Letter W.</summary>
    W = 87,
    /// <summary>Letter X.</summary>
    X = 88,
    /// <summary>Letter Y.</summary>
    Y = 89,
    /// <summary>Letter Z.</summary>
    Z = 90,
    /// <summary>Left bracket ([).</summary>
    LEFT_BRACKET = 91,
    /// <summary>Backslash (\).</summary>
    BACKSLASH = 92,
    /// <summary>Right bracket (]).</summary>
    RIGHT_BRACKET = 93,
    /// <summary>Grave accent (`).</summary>
    GRAVE = 96,
    /// <summary>Spacebar.</summary>
    SPACE = 0x20,
    /// <summary>Escape key.</summary>
    ESCAPE = 0x100,
    /// <summary>Enter/Return key.</summary>
    ENTER = 257,
    /// <summary>Tab key.</summary>
    TAB = 258,
    /// <summary>Backspace key.</summary>
    BACKSPACE = 259,
    /// <summary>Insert key.</summary>
    INSERT = 260,
    /// <summary>Delete key.</summary>
    DELETE = 261,
    /// <summary>Right arrow key.</summary>
    RIGHT = 262,
    /// <summary>Left arrow key.</summary>
    LEFT = 263,
    /// <summary>Down arrow key.</summary>
    DOWN = 264,
    /// <summary>Up arrow key.</summary>
    UP = 265,
    /// <summary>Page Up key.</summary>
    PAGE_UP = 266,
    /// <summary>Page Down key.</summary>
    PAGE_DOWN = 267,
    /// <summary>Home key.</summary>
    HOME = 268,
    /// <summary>End key.</summary>
    END = 269,
    /// <summary>Caps Lock key.</summary>
    CAPS_LOCK = 280,
    /// <summary>Scroll Lock key.</summary>
    SCROLL_LOCK = 281,
    /// <summary>Num Lock key.</summary>
    NUM_LOCK = 282,
    /// <summary>Print Screen key.</summary>
    PRINT_SCREEN = 283,
    /// <summary>Pause/Break key.</summary>
    PAUSE = 284,
    /// <summary>Function key F1.</summary>
    F1 = 290,
    /// <summary>Function key F2.</summary>
    F2 = 291,
    /// <summary>Function key F3.</summary>
    F3 = 292,
    /// <summary>Function key F4.</summary>
    F4 = 293,
    /// <summary>Function key F5.</summary>
    F5 = 294,
    /// <summary>Function key F6.</summary>
    F6 = 295,
    /// <summary>Function key F7.</summary>
    F7 = 296,
    /// <summary>Function key F8.</summary>
    F8 = 297,
    /// <summary>Function key F9.</summary>
    F9 = 298,
    /// <summary>Function key F10.</summary>
    F10 = 299,
    /// <summary>Function key F11.</summary>
    F11 = 300,
    /// <summary>Function key F12.</summary>
    F12 = 301,
    /// <summary>Left Shift key.</summary>
    LEFT_SHIFT = 340,
    /// <summary>Left Control key.</summary>
    LEFT_CONTROL = 341,
    /// <summary>Left Alt key.</summary>
    LEFT_ALT = 342,
    /// <summary>Left Super/Windows/Command key.</summary>
    LEFT_SUPER = 343,
    /// <summary>Right Shift key.</summary>
    RIGHT_SHIFT = 344,
    /// <summary>Right Control key.</summary>
    RIGHT_CONTROL = 345,
    /// <summary>Right Alt key.</summary>
    RIGHT_ALT = 346,
    /// <summary>Right Super/Windows/Command key.</summary>
    RIGHT_SUPER = 347,
    /// <summary>Keyboard menu key.</summary>
    KB_MENU = 348,
    /// <summary>Keypad 0.</summary>
    KP_0 = 320,
    /// <summary>Keypad 1.</summary>
    KP_1 = 321,
    /// <summary>Keypad 2.</summary>
    KP_2 = 322,
    /// <summary>Keypad 3.</summary>
    KP_3 = 323,
    /// <summary>Keypad 4.</summary>
    KP_4 = 324,
    /// <summary>Keypad 5.</summary>
    KP_5 = 325,
    /// <summary>Keypad 6.</summary>
    KP_6 = 326,
    /// <summary>Keypad 7.</summary>
    KP_7 = 327,
    /// <summary>Keypad 8.</summary>
    KP_8 = 328,
    /// <summary>Keypad 9.</summary>
    KP_9 = 329,
    /// <summary>Keypad decimal point.</summary>
    KP_DECIMAL = 330,
    /// <summary>Keypad divide (/).</summary>
    KP_DIVIDE = 331,
    /// <summary>Keypad multiply (*).</summary>
    KP_MULTIPLY = 332,
    /// <summary>Keypad subtract (-).</summary>
    KP_SUBTRACT = 333,
    /// <summary>Keypad add (+).</summary>
    KP_ADD = 334,
    /// <summary>Keypad enter.</summary>
    KP_ENTER = 335,
    /// <summary>Keypad equal (=).</summary>
    KP_EQUAL = 336,
    /// <summary>Volume up key.</summary>
    VOLUME_UP = 24,
    /// <summary>Volume down key.</summary>
    VOLUME_DOWN = 25,
    /// <summary>Back key (Android/other platforms).</summary>
    BACK = 7,
    /// <summary>Null key (Android/other platforms).</summary>
    NULL = 8,
    /// <summary>Menu key (Android/other platforms).</summary>
    MENU = 9,
}