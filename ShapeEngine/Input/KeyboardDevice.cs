using System.Text;
using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a keyboard input device, providing access to keyboard buttons, state tracking,
/// character input, and utility methods for keyboard input.
/// </summary>
public sealed class KeyboardDevice : InputDevice
{
    /// <summary>
    /// All available Raylib keyboard keys.
    /// </summary>
    public static readonly KeyboardKey[] AllKeyboardKeys = Enum.GetValues<KeyboardKey>();
    /// <summary>
    /// All available Shape keyboard buttons.
    /// </summary>
    public static readonly ShapeKeyboardButton[] AllShapeKeyboardButtons = Enum.GetValues<ShapeKeyboardButton>();

    /// <summary>
    /// Gets the usage detection settings for the keyboard input device.
    /// </summary>
    public InputDeviceUsageDetectionSettings.KeyboardSettings UsageDetectionSettings { get; private set; } = new();
    
    private bool wasUsed;
    private bool wasUsedRaw;
    private bool isLocked;
    private bool isActive;
    
    private int pressedCount;
    private float pressedCountDurationTimer;
    private float usedDurationTimer;
    /// <summary>
    /// List of characters entered since the last update.
    /// </summary>
    public readonly List<char> UsedCharacters = [];
    /// <summary>
    /// List of keyboard buttons pressed since the last update.
    /// </summary>
    public readonly List<ShapeKeyboardButton> PressedButtons = [];
    
    /// <summary>
    /// List of keyboard buttons released since the last update.
    /// </summary>
    public readonly List<ShapeKeyboardButton> ReleasedButtons = [];
    
    /// <summary>
    /// List of keyboard buttons currently held down.
    /// </summary>
    public readonly List<ShapeKeyboardButton> HeldDownButtons = [];
    
    private readonly Dictionary<ShapeKeyboardButton, InputState> buttonStates = new(AllShapeKeyboardButtons.Length);
    
    /// <summary>
    /// Event triggered when a keyboard button is pressed.
    /// </summary>
    public event Action<ShapeKeyboardButton>? OnButtonPressed;
    /// <summary>
    /// Event triggered when a keyboard button is released.
    /// </summary>
    public event Action<ShapeKeyboardButton>? OnButtonReleased;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardDevice"/> class.
    /// </summary>
    internal KeyboardDevice()
    {
        foreach (var button in AllShapeKeyboardButtons)
        {
            buttonStates.Add(button, new());
        }
    }
    
    /// <summary>
    /// Gets the type of this input device, which is always <see cref="InputDeviceType.Keyboard"/>.
    /// </summary>
    public override InputDeviceType GetDeviceType() => InputDeviceType.Keyboard;
    
    /// <summary>
    /// Returns whether the keyboard device is currently locked.
    /// Locking can be used to temporarily disable input processing.
    /// This does not affect whether the device is active or not.
    /// </summary>
    public override bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the keyboard device, preventing input from being registered.
    /// </summary>
    public override void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
    }

    /// <summary>
    /// Unlocks the keyboard device, allowing input to be registered.
    /// </summary>
    public override void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
    }
    
    /// <inheritdoc cref="InputDevice.ApplyInputDeviceChangeSettings"/>
    public override void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings) => UsageDetectionSettings = settings.Keyboard;

    /// <inheritdoc cref="InputDevice.WasUsed"/>
    public override bool WasUsed() => wasUsed;
    
    /// <inheritdoc cref="InputDevice.WasUsedRaw"/>
    public override bool WasUsedRaw() => wasUsedRaw;
    
    /// <inheritdoc cref="InputDevice.Update"/>
    public override bool Update(float dt, bool wasOtherDeviceUsed)
    {
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        UsedCharacters.Clear();
        
        UpdateButtonStates();
        
        if (isLocked)
        {
            wasUsed = false;
            wasUsedRaw = false;
            return false;
        }
        
        var unicode = Raylib.GetCharPressed();
        while (unicode > 0)
        {
            UsedCharacters.Add((char)unicode);
            unicode = Raylib.GetCharPressed();
        }
        
        WasKeyboardUsed(dt, wasOtherDeviceUsed, out wasUsed, out wasUsedRaw);
        return wasUsed && !wasOtherDeviceUsed;//safety precaution
    }
    
    /// <summary>
    /// Calibrates the keyboard device. (Currently not implemented.)
    /// </summary>
    public override void Calibrate(){}
    
    /// <summary>
    /// Indicates whether the mouse device is currently active,
    /// as in being used by the <see cref="ShapeInput"/> class to generate input.
    /// </summary>
    public override bool IsActive() => isActive;
    
    /// <summary>
    /// Activates the mouse device, enabling input processing.
    /// </summary>
    public override void Activate()
    {
        if (isActive) return;
        isActive = true;
    }
    
    /// <summary>
    /// Deactivates the mouse device, disabling input processing and resetting state.
    /// </summary>
    public override void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
    }
    
    /// <summary>
    /// Gets the current input state for the specified keyboard button.
    /// </summary>
    public InputState GetButtonState(ShapeKeyboardButton button) => buttonStates[button];
    
    /// <summary>
    /// Consumes the input state for the specified keyboard button, marking it as consumed.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="button">The keyboard button to consume.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    public InputState ConsumeButtonState(ShapeKeyboardButton button, out bool valid)
    {
        valid = false;
        var state = buttonStates[button];
        if (state.Consumed) return state;

        valid = true;
        buttonStates[button] = state.Consume();
        return state;
    }
    
    /// <summary>
    /// Gets the list of characters entered since the last update.
    /// </summary>
    /// <returns>List of entered characters.</returns>
    public List<char> GetStreamChar()
    {
        if (isLocked) return new List<char>();
        return UsedCharacters.ToList();
    }
    /// <summary>
    /// Gets the string of characters entered since the last update.
    /// </summary>
    /// <returns>String of entered characters.</returns>
    public string GetStream()
    {
        if (isLocked) return string.Empty;
        return new string(UsedCharacters.ToArray());
    }
    /// <summary>
    /// Appends the entered characters to the current text.
    /// </summary>
    /// <param name="curText">Current text.</param>
    /// <returns>Updated text with entered characters appended.</returns>
    public string GetStream(string curText)
    {
        if (isLocked) return curText;
        var b = new StringBuilder(UsedCharacters.Count + curText.Length);
        b.Append(curText);
        b.Append( new string(UsedCharacters.ToArray()) );
        return b.ToString();
    }
    /// <summary>
    /// Inserts entered characters into the current text at the specified caret index.
    /// </summary>
    /// <param name="curText">Current text.</param>
    /// <param name="caretIndex">Caret index for insertion.</param>
    /// <returns>Tuple of updated text and new caret index.</returns>
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
    }

    /// <summary>
    /// Determines if the keyboard was used based on button presses.
    /// </summary>
    private void WasKeyboardUsed(float dt, bool wasOtherDeviceUsed, out bool used, out bool usedRaw)
    {
        used = false;
        usedRaw = false;
        
        if (wasOtherDeviceUsed)
        {
            usedDurationTimer = 0f;
            pressedCount = 0;
            pressedCountDurationTimer = 0f;
        }
        
        if (isLocked) return;

        usedRaw = PressedButtons.Count > 0;

        if (!UsageDetectionSettings.Detection || wasOtherDeviceUsed) return;
            
        if (UsageDetectionSettings.SpecialButtonSelectionSystemEnabled)
        {
            if 
            (
                Raylib.IsKeyDown((KeyboardKey)UsageDetectionSettings.SelectionButtonPrimary) ||
                Raylib.IsKeyDown((KeyboardKey)UsageDetectionSettings.SelectionButtonSecondary) ||
                Raylib.IsKeyDown((KeyboardKey)UsageDetectionSettings.SelectionButtonTertiary)
            )
            {
                used = true;
            }
        }
        else
        {
            var pressCountEnabled = UsageDetectionSettings.PressCountEnabled;
            var usedDurationEnabled = UsageDetectionSettings.UsedDurationEnabled;
            
            if (pressCountEnabled)
            {
                pressedCountDurationTimer += dt;
                if (pressedCountDurationTimer >= UsageDetectionSettings.MinPressInterval)
                {
                    pressedCountDurationTimer -= UsageDetectionSettings.MinPressInterval;
                    pressedCount = 0;
                }
            }
            
            if (usedDurationEnabled && HeldDownButtons.Count > 0)
            {
                usedDurationTimer += dt;
                if (usedDurationTimer > UsageDetectionSettings.MinUsedDuration)
                {
                    usedDurationTimer -= UsageDetectionSettings.MinUsedDuration;
                    used = true;
                    pressedCount = 0;
                    pressedCountDurationTimer = 0f;
                    return;
                }
            }
            
            if (pressCountEnabled && PressedButtons.Count > 0)
            {
                pressedCount++;
                if (pressedCount >= UsageDetectionSettings.MinPressCount)
                {
                    used = true;
                    pressedCountDurationTimer = 0f;
                    usedDurationTimer = 0f;
                    pressedCount = 0;
                }
            }
        }
    } 
    
    /// <summary>
    /// Updates the states of all keyboard buttons.
    /// </summary>
    private void UpdateButtonStates()
    {
        foreach (var state in buttonStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button);
            var nextState = new InputState(prevState, curState);
            buttonStates[button] = nextState;
            
            if(nextState.Down) HeldDownButtons.Add(button);

            if (nextState.Pressed)
            {
                PressedButtons.Add(button);
                OnButtonPressed?.Invoke(button);
            }
            else if (nextState.Released)
            {
                ReleasedButtons.Add(button);
                OnButtonReleased?.Invoke(button);
            }
        }
    }

    #region Button

    /// <summary>
    /// Gets the display name for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The button name.</returns>
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
            case ShapeKeyboardButton.UP: return "Up"; //shortHand ? "Up" : "Up";
            case ShapeKeyboardButton.PAGE_UP: return shortHand ? "PUp" : "Page Up";
            case ShapeKeyboardButton.PAGE_DOWN: return shortHand ? "PDwn" : "Page Down";
            case ShapeKeyboardButton.HOME: return shortHand ? "Hom" : "Home";
            case ShapeKeyboardButton.END: return shortHand ? "End" : "End Key";
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
    /// <summary>
    /// Checks if a modifier keyboard button is active, optionally reversing the logic.
    /// </summary>
    public bool IsModifierActive(ShapeKeyboardButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    /// <summary>
    /// Determines if the specified keyboard button is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeKeyboardButton button, ModifierKeySet modifierKeySet)
    {
        return GetValue(button, modifierKeySet) != 0f;
    }
    /// <summary>
    /// Determines if the specified keyboard button is "down".
    /// </summary>
    public bool IsDown(ShapeKeyboardButton button)
    {
        return GetValue(button) != 0f;
    }
    /// <summary>
    /// Gets the value of the specified keyboard button, considering modifier keys.
    /// </summary>
    public float GetValue(ShapeKeyboardButton button, ModifierKeySet modifierKeySet)
    {
        if (isLocked) return 0f;
        bool modifierActive = modifierKeySet.IsActive();//  IModifierKey.IsActive(modifierKeySet);
        return (modifierActive && Raylib.IsKeyDown((KeyboardKey)button)) ? 1f : 0f;
    }
    /// <summary>
    /// Gets the value of the specified keyboard button.
    /// </summary>
    public float GetValue(ShapeKeyboardButton button)
    {
        if (isLocked) return 0f;
        return Raylib.IsKeyDown((KeyboardKey)button) ? 1f : 0f;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton button)
    {
        bool down = IsDown(button);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button with modifier keys.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton button, ModifierKeySet modifierKeySet)
    {
        bool down = IsDown(button, modifierKeySet);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button, using a previous state and modifier keys.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton button, InputState previousState, ModifierKeySet modifierKeySet)
    {
        return new(previousState, CreateInputState(button, modifierKeySet));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton button, InputState previousState)
    {
        return new(previousState, CreateInputState(button));
    }

    #endregion
    
    #region Button Axis

    /// <summary>
    /// Gets the display name for a button axis (negative and positive button pair).
    /// </summary>
    /// <remarks>
    /// Format: "NegButtonName|PosButtonName" (e\.g\., "A|D" or "Left|Right"\)
    /// </remarks>
    /// <param name="neg">Negative direction button.</param>
    /// <param name="pos">Positive direction button.</param>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The button axis name.</returns>
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
    /// <summary>
    /// Determines if the button axis (negative/positive) is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet modifierKeySet)
    {
        return GetValue(neg, pos, modifierKeySet) != 0f;
    }
    /// <summary>
    /// Determines if the button axis (negative/positive) is "down".
    /// </summary>
    public bool IsDown(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        return GetValue(neg, pos) != 0f;
    }

    /// <summary>
    /// Gets the value of the button axis (negative/positive), considering modifier keys.
    /// </summary>
    public float GetValue(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet modifierKeySet)
    {
        if (isLocked) return 0f;
        if (!modifierKeySet.IsActive()) return 0f;
        return GetValue(neg, pos);
    }
    /// <summary>
    /// Gets the value of the button axis (negative/positive).
    /// </summary>
    public float GetValue(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        if (isLocked) return 0f;
        float vNegative = Raylib.IsKeyDown((KeyboardKey)neg) ? 1f : 0f;
        float vPositive = Raylib.IsKeyDown((KeyboardKey)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive) with modifier keys.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet modifierKeySet)
    {
        float axis = GetValue(neg, pos, modifierKeySet);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Keyboard);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state and modifier keys.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos,
        InputState previousState, ModifierKeySet modifierKeySet)
    {
        return new(previousState, CreateInputState(neg, pos, modifierKeySet));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive).
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        float axis = GetValue(neg, pos);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Keyboard);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState)
    {
        return new(previousState, CreateInputState(neg, pos));
    }

    #endregion
}