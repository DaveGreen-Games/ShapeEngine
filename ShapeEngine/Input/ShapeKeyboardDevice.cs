using System.Text;
using Raylib_cs;

namespace ShapeEngine.Input;


public sealed class ShapeKeyboardDevice : ShapeInputDevice
{
    public static readonly KeyboardKey[] AllKeyboardKeys = Enum.GetValues<KeyboardKey>();
    public static readonly ShapeKeyboardButton[] AllShapeKeyboardButtons = Enum.GetValues<ShapeKeyboardButton>();

    private bool wasUsed = false;
    private bool isLocked = false;

    public readonly List<char> UsedCharacters = new();
    public readonly List<ShapeKeyboardButton> UsedButtons = new();
    
    // private readonly Dictionary<ShapeKeyboardButton, InputState> buttonStates = new();
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
        UsedButtons.Clear();
        var keycode = Raylib.GetKeyPressed();
        while (keycode > 0)
        {
            UsedButtons.Add((ShapeKeyboardButton)keycode);
            keycode = Raylib.GetKeyPressed();
        }
        
        UsedCharacters.Clear();
        var unicode = Raylib.GetCharPressed();
        while (unicode > 0)
        {
            UsedCharacters.Add((char)unicode);
            unicode = Raylib.GetCharPressed();
        }
        
        wasUsed = WasKeyboardUsed();
        // foreach (var buttonStatePair in buttonStates)
        // {
        //     var previous = buttonStatePair.Value;
        //     var current = GetCurrentButtonState(buttonStatePair.Key);
        //     buttonStates[buttonStatePair.Key] = new InputState(previous, current);
        // }
    }
    
    public void Calibrate(){}
    
    public List<char> GetStreamChar()
    {
        if (isLocked) return new List<char>();
        return UsedCharacters.ToList();
        // int unicode = Raylib.GetCharPressed();
        // List<char> chars = new();
        // while (unicode != 0)
        // {
        //     var c = (char)unicode;
        //     chars.Add(c);
        //
        //     unicode = Raylib.GetCharPressed();
        // }
        // return chars;
    }
    public string GetStream()
    {
        if (isLocked) return string.Empty;
        return new string(UsedCharacters.ToArray());
        // int unicode = Raylib.GetCharPressed();
        // List<char> chars = new();
        // while (unicode != 0)
        // {
        //     var c = (char)unicode;
        //     chars.Add(c);
        //
        //     unicode = Raylib.GetCharPressed();
        // }
        // return new string(chars.ToArray());
    }
    public string GetStream(string curText)
    {
        if (isLocked) return curText;;
        var b = new StringBuilder(UsedCharacters.Count + curText.Length);
        b.Append(curText);
        b.Append( new string(UsedCharacters.ToArray()) );
        return b.ToString();
        // var chars = GetStreamChar();
        // var b = new StringBuilder(chars.Count + curText.Length);
        // b.Append(curText);
        // b.Append( new string(chars.ToArray()) );
        // return b.ToString();
    }
    public (string text, int caretIndex) GetStream(string curText, int caretIndex)
    {
        if (isLocked) return (curText, caretIndex);
        var characters = curText.ToList();
        for (int i = 0; i < UsedCharacters.Count; i++)
        {
            var c = characters[i];
            if (caretIndex < 0 || caretIndex >= characters.Count) characters.Add(c);
            else
            {
                characters.Insert(caretIndex, c);

            }
            caretIndex++;
        }
        return (new string(characters.ToArray()), caretIndex);
        
        
        // var characters = curText.ToList();
        // int unicode = Raylib.GetCharPressed();
        // while (unicode != 0)
        // {
        //     var c = (char)unicode;
        //     if (caretIndex < 0 || caretIndex >= characters.Count) characters.Add(c);
        //     else
        //     {
        //         characters.Insert(caretIndex, c);
        //
        //     }
        //     caretIndex++;
        //     unicode = Raylib.GetCharPressed();
        // }
        //
        // return (new string(characters.ToArray()), caretIndex);
    }

    private bool WasKeyboardUsed() => !isLocked && UsedButtons.Count > 0;// Raylib.GetKeyPressed() > 0;

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
        return (modifierActive && Raylib.IsKeyDown((KeyboardKey)button)) ? 1f : 0f;
    }
    public float GetValue(ShapeKeyboardButton button)
    {
        if (isLocked) return 0f;
        return Raylib.IsKeyDown((KeyboardKey)button) ? 1f : 0f;
    }

    
    // /// <summary>
    // /// This function takes the button state from last frame into account.
    // /// </summary>
    // /// <param name="button"></param>
    // /// <returns> Returns the current button state for the specified keyboard button. </returns>
    // public InputState GetCurrentButtonState(ShapeKeyboardButton button)
    // {
    //     if (!buttonStates.ContainsKey(button))
    //     {
    //         buttonStates.Add(button, GetState(button));
    //         return new();
    //     }
    //
    //     return buttonStates[button];
    // }
    //
    public InputState GetState(ShapeKeyboardButton button)
    {
        bool down = IsDown(button);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
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
        float vNegative = Raylib.IsKeyDown((KeyboardKey)neg) ? 1f : 0f;
        float vPositive = Raylib.IsKeyDown((KeyboardKey)pos) ? 1f : 0f;
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



/*
//Raylib key pressed system
// Check if a key has been pressed once
   bool IsKeyPressed(int key)
   {
   
       bool pressed = false;
   
       if ((key > 0) && (key < MAX_KEYBOARD_KEYS))
       {
           if ((CORE.Input.Keyboard.previousKeyState[key] == 0) && (CORE.Input.Keyboard.currentKeyState[key] == 1)) pressed = true;
       }
   
       return pressed;
   }
   
   // Check if a key has been pressed again
   bool IsKeyPressedRepeat(int key)
   {
       bool repeat = false;
   
       if ((key > 0) && (key < MAX_KEYBOARD_KEYS))
       {
           if (CORE.Input.Keyboard.keyRepeatInFrame[key] == 1) repeat = true;
       }
   
       return repeat;
   }
   
   // Check if a key is being pressed (key held down)
   bool IsKeyDown(int key)
   {
       bool down = false;
   
       if ((key > 0) && (key < MAX_KEYBOARD_KEYS))
       {
           if (CORE.Input.Keyboard.currentKeyState[key] == 1) down = true;
       }
   
       return down;
   }
   
   // Check if a key has been released once
   bool IsKeyReleased(int key)
   {
       bool released = false;
   
       if ((key > 0) && (key < MAX_KEYBOARD_KEYS))
       {
           if ((CORE.Input.Keyboard.previousKeyState[key] == 1) && (CORE.Input.Keyboard.currentKeyState[key] == 0)) released = true;
       }
   
       return released;
   }
   
   // Check if a key is NOT being pressed (key not held down)
   bool IsKeyUp(int key)
   {
       bool up = false;
   
       if ((key > 0) && (key < MAX_KEYBOARD_KEYS))
       {
           if (CORE.Input.Keyboard.currentKeyState[key] == 0) up = true;
       }
   
       return up;
   }
   
   // Get the last key pressed
   int GetKeyPressed(void)
   {
       int value = 0;
   
       if (CORE.Input.Keyboard.keyPressedQueueCount > 0)
       {
           // Get character from the queue head
           value = CORE.Input.Keyboard.keyPressedQueue[0];
   
           // Shift elements 1 step toward the head
           for (int i = 0; i < (CORE.Input.Keyboard.keyPressedQueueCount - 1); i++)
               CORE.Input.Keyboard.keyPressedQueue[i] = CORE.Input.Keyboard.keyPressedQueue[i + 1];
   
           // Reset last character in the queue
           CORE.Input.Keyboard.keyPressedQueue[CORE.Input.Keyboard.keyPressedQueueCount - 1] = 0;
           CORE.Input.Keyboard.keyPressedQueueCount--;
       }
   
       return value;
   }
   
   // Get the last char pressed
   int GetCharPressed(void)
   {
       int value = 0;
   
       if (CORE.Input.Keyboard.charPressedQueueCount > 0)
       {
           // Get character from the queue head
           value = CORE.Input.Keyboard.charPressedQueue[0];
   
           // Shift elements 1 step toward the head
           for (int i = 0; i < (CORE.Input.Keyboard.charPressedQueueCount - 1); i++)
               CORE.Input.Keyboard.charPressedQueue[i] = CORE.Input.Keyboard.charPressedQueue[i + 1];
   
           // Reset last character in the queue
           CORE.Input.Keyboard.charPressedQueue[CORE.Input.Keyboard.charPressedQueueCount - 1] = 0;
           CORE.Input.Keyboard.charPressedQueueCount--;
       }
   
       return value;
   }
   
   // Set a custom key to exit program
   // NOTE: default exitKey is set to ESCAPE
   void SetExitKey(int key)
   {
       CORE.Input.Keyboard.exitKey = key;
   }
*/